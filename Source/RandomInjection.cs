using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Assembly_CSharp.TasInfo.mm.Source.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Assembly_CSharp.TasInfo.mm.Source {
    public static class RandomInjection {
        private static MethodInfo _rangeFloat;
        private static MethodInfo _rangeInt;
        private static MethodInfo _getRwi;
        private static List<Dictionary<string, PlaybackState>> _playback;
        private static List<Dictionary<string, List<float>>> _recording;
        private static List<string> _detailLog;
        private static List<string> _sceneNames;
        private static object _lock;
        private static int _sceneIndex;
        private static int _nextSceneRollCount;
        private static bool _useLegacyRngSync;

        public static bool EnablePlayback;
        public static bool EnableRecording;
        public static bool EnableDetailLogging;

        public static int SceneIndex => _sceneIndex;

        private const BindingFlags allFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static void Init() {
            _lock = new object();
            _sceneNames = new List<string>();
            EnableRecording = true;
            EnableDetailLogging = false;
            EnablePlayback = true;
            _useLegacyRngSync = ConfigManager.UseLegacyRngSync;
            _sceneIndex = 0;
            _sceneNames.Add("Start");

            if (EnableDetailLogging)
                _detailLog = new List<string>();

            var ueRandom = typeof(UnityEngine.Random);
            _rangeFloat = ueRandom.GetMethod("Range", BindingFlags.Static | BindingFlags.Public, null, new[] {typeof(float), typeof(float)}, null);
            _rangeInt = ueRandom.GetMethod("Range", BindingFlags.Static | BindingFlags.Public, null, new[] {typeof(int), typeof(int)}, null);
            _getRwi = typeof(ActionHelpers).GetMethod("GetRandomWeightedIndex", BindingFlags.Static | BindingFlags.Public);

            var targetMethods = new List<MethodInfo>();
            AddRandomCalls(targetMethods);
            foreach (var method in targetMethods.Where(m => m != null)) {
                if (typeof(FsmStateAction).IsAssignableFrom(method.DeclaringType)) {
                    InjectOnRangeFsm(method);
                } else {
                    InjectOnRange(method);
                }
            }

            if (targetMethods.Any(m => m == null))
                Debug.Log("One or more Random-using methods failed to bind");

            var rwiMethods = new[] {
                typeof(AudioPlayerOneShot).GetMethod("DoPlayRandomClip", allFlags),
                typeof(AudioPlayRandom).GetMethod("DoPlayRandomClip", allFlags),
                typeof(PlayRandomAnimation).GetMethod("DoPlayRandomAnimation", allFlags),
                typeof(PlayRandomSound).GetMethod("DoPlayRandomClip", allFlags),
                typeof(SelectRandomColor).GetMethod("DoSelectRandomColor", allFlags),
                typeof(SelectRandomGameObject).GetMethod("DoSelectRandomGameObject", allFlags),
                typeof(SelectRandomString).GetMethod("DoSelectRandomString", allFlags),
                typeof(SelectRandomVector3).GetMethod("DoSelectRandomColor", allFlags), //This is a copy/paste error in the game code; intentional
                typeof(SendRandomEvent).GetMethod("OnEnter", allFlags),
                typeof(SendRandomEventV2).GetMethod("OnEnter", allFlags),
                typeof(SendRandomEventV3).GetMethod("OnEnter", allFlags),
            };
            foreach (var method in rwiMethods.Where(m => m != null)) {
                InjectGetRwiFsm(method);
            }

            if (rwiMethods.Any(m => m == null))
                Debug.Log("One or more RWI-using methods failed to bind");

            _playback = new List<Dictionary<string, PlaybackState>>();
            _recording = new List<Dictionary<string, List<float>>>();

            if (EnablePlayback)
                LoadPlaybackFiles();
        }

        private static void AddRandomCalls(List<MethodInfo> targetMethods) {
#if V1221
            Add(targetMethods, typeof(WaterDrip).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(WaterDrip).GetMethod("Update", allFlags));
            Add(targetMethods, typeof(PushableRubble).GetMethod("OnEnable", allFlags));
#endif
#if V1221 || V1432
            Add(targetMethods, typeof(Breakable).GetMethod("Break", allFlags));
            Add(targetMethods, typeof(Breakable).GetMethod("SpawnNailHitEffect", allFlags));
            Add(targetMethods, typeof(BreakableInfectedVine).GetMethod("OnTriggerEnter2D", allFlags));
            Add(targetMethods, typeof(BreakableInfectedVine).GetMethod("SpawnSpatters", allFlags));
            Add(targetMethods, typeof(BreakableObject.FlingObject).GetMethod("Fling", allFlags));
            Add(targetMethods, typeof(BreakableObject).GetMethod("OnTriggerEnter2D", allFlags));
            Add(targetMethods, typeof(BreakablePole).GetMethod("TakeDamage", allFlags));
            Add(targetMethods, typeof(BreakablePoleSimple).GetMethod("OnTriggerEnter2D", allFlags));
            Add(targetMethods, typeof(BreakableWithExternalDebris).GetMethod("Spawn", allFlags));
            Add(targetMethods, typeof(CrystalPieceSize).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(DebrisParticle).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(DebrisPiece).GetMethod("Launch", allFlags));
            Add(targetMethods, typeof(DebrisPiece).GetMethod("Spin", allFlags));
            Add(targetMethods, typeof(HealthCocoon).GetMethod("FlingObjects", allFlags));
            Add(targetMethods, typeof(HealthCocoon).GetMethod("OnTriggerEnter2D", allFlags));
            Add(targetMethods, typeof(FlingFlashingGeo).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(WalkLeftRight).GetMethod("SetupStartingDirection", allFlags));
            Add(targetMethods, typeof(IdleBuzzing).GetMethod("Buzz", allFlags));
            Add(targetMethods, typeof(InfectedBurstLarge).GetMethod("OnTriggerEnter2D", allFlags));
            Add(targetMethods, typeof(InfectedBurstLarge).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(InfectedBurstSmall).GetMethod("OnTriggerEnter2D", allFlags));
            Add(targetMethods, typeof(InfectedBurstSmall).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(LiftPlatform).GetMethod("OnCollisionEnter2D", allFlags));
            Add(targetMethods, typeof(Probability).GetMethod("GetRandomGameObjectByProbability", allFlags));
            Add(targetMethods, typeof(PushableRubble).GetMethod("Push", allFlags));
            Add(targetMethods, typeof(RandomAudioClipTable).GetMethod("SelectClip", allFlags));
            Add(targetMethods, typeof(RandomAudioClipTable).GetMethod("SelectPitch", allFlags));
            Add(targetMethods, typeof(RecycleAfter2dtkAnimation).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(SetRandomSpriteId).GetMethod("Init", allFlags));
            Add(targetMethods, typeof(SpatterOrange).GetMethod("Impact", allFlags));
            Add(targetMethods, typeof(SpatterOrange).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(SpinSelfSimple).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(SplashAnimator).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(StalactiteControl).GetMethod("FlingObjects", allFlags));
            Add(targetMethods, typeof(TinkEffect).GetMethod("OnTriggerEnter2D", allFlags));
            Add(targetMethods, typeof(TownGrass).GetMethod("OnTriggerEnter2D", allFlags));
#endif
#if V1221
            Add(targetMethods, typeof(PlayFromRandomFrameMecanim).GetNestedType("<DelayStart>c__IteratorA", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(HeroController).GetNestedType("<CheckForTerrainThunk>c__Iterator1C", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
#endif
#if V1432
            Add(targetMethods, typeof(PlayFromRandomFrameMecanim).GetNestedType("<DelayStart>c__Iterator0", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(HeroController).GetNestedType("<CheckForTerrainThunk>c__IteratorB", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(BigCentipede).GetMethod("Awake", allFlags));
            Add(targetMethods, typeof(BossStatue).GetNestedType("<Jitter>c__Iterator1", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(BounceShroom).GetNestedType("<Idle>c__Iterator0", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(BounceShroom).GetMethod("<Start>m__0", allFlags));
            Add(targetMethods, typeof(Breakable.FlingObject).GetMethod("Fling", allFlags));
            Add(targetMethods, typeof(Corpse).GetNestedType("<DropThroughFloor>c__Iterator0", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(CorpseDeathStun).GetNestedType("<Jitter>c__Iterator1", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(CorpseDeathStunChunker).GetNestedType("<Jitter>c__Iterator1", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(CorpseFungusExplode).GetNestedType("<Jitter>c__Iterator1", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(CorpseGoopExplode).GetNestedType("<Jitter>c__Iterator1", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(CorpseHatcher).GetMethod("Smash", allFlags));
            Add(targetMethods, typeof(CorpseZomHive).GetMethod("LandEffects", allFlags));
            Add(targetMethods, typeof(EnemyBullet).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(EnemyDeathEffects).GetMethod("EmitCorpse", allFlags));
            Add(targetMethods, typeof(EnemyDeathEffects).GetMethod("EmitEssence", allFlags));
            Add(targetMethods, typeof(EnemyHitEffectsShade).GetMethod("RecieveHitEffect", allFlags));
            Add(targetMethods, typeof(EnemySpawner).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(FakeBat).GetNestedType("<SendOutRoutine>c__Iterator0", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(FakeBat).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(FlingUtils).GetMethod("FlingChildren", allFlags));
            Add(targetMethods, typeof(FlingUtils).GetMethod("FlingObject", allFlags));
            Add(targetMethods, typeof(FlingUtils).GetMethod("SpawnAndFling", allFlags));
            Add(targetMethods, typeof(FlipPlatform).GetNestedType("<Idle>c__Iterator0", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(GeoControl).GetNestedType("<Getter>c__Iterator1", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(GeoControl).GetMethod("OnCollisionEnter2D", allFlags));
            Add(targetMethods, typeof(GeoControl).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(GeoControl).GetMethod("PlayCollectSound", allFlags));
            Add(targetMethods, typeof(GrimmballControl).GetNestedType("<DoFire>c__Iterator0", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(GrubBGControl).GetNestedType("<Idle>c__Iterator0", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(HealthManager).GetMethod("TakeDamage", allFlags));
            Add(targetMethods, typeof(HeroController).GetMethod("TakeDamage", allFlags));
            Add(targetMethods, typeof(ShakePosition).GetMethod("UpdateShaking", allFlags));
            Add(targetMethods, typeof(KnightHatchling).GetNestedType("<Spawn>c__Iterator0", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(KnightHatchling).GetMethod("DoBuzz", allFlags));
            Add(targetMethods, typeof(KnightHatchling).GetMethod("FixedUpdate", allFlags));
            Add(targetMethods, typeof(KnightHatchling).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(KnightHatchling).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(MegaJellyZap).GetNestedType("<MultiZapSequence>c__Iterator1", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(PaintBullet).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(ScuttlerControl).GetNestedType("<Bounce>c__Iterator2", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(ScuttlerControl).GetMethod("Hit", allFlags));
            Add(targetMethods, typeof(ScuttlerControl).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(SetWalkerFacing).GetMethod("Apply", allFlags));
            Add(targetMethods, typeof(SpawnJarControl).GetNestedType("<Behaviour>c__Iterator0", BindingFlags.NonPublic)?.GetMethod("MoveNext", allFlags));
            Add(targetMethods, typeof(SpellFluke).GetMethod("<Start>m__1", allFlags));
            Add(targetMethods, typeof(SpellFluke).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(SpellGetOrb).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(SpellGetOrb).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(SpinSelf).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(Walker).GetMethod("BeginStopped", allFlags));
            Add(targetMethods, typeof(Walker).GetMethod("BeginWalking", allFlags));
            Add(targetMethods, typeof(Walker).GetMethod("EndStopping", allFlags));
            Add(targetMethods, typeof(Walker).GetMethod("StartMoving", allFlags));
            Add(targetMethods, typeof(WeaverlingEnemyList).GetMethod("GetTarget", allFlags));
#endif
#if V1221 || V1028 || V1028_KRYTHOM || V1037
            Add(targetMethods, typeof(UnityEngine.ParticleEmitter).GetMethod("Emit", allFlags, null, new Type[] { }, null));
#endif
            Add(targetMethods, typeof(AudioSourcePitchRandomizer).GetMethod("Awake", allFlags));
            Add(targetMethods, typeof(DropCrystal).GetMethod("OnCollisionEnter2D", allFlags));
            Add(targetMethods, typeof(DropCrystal).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(DropCrystal).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(GameManager).GetMethod("TimePasses", allFlags));
            Add(targetMethods, typeof(HeroAudioController).GetMethod("RandomizePitch", allFlags));
            Add(targetMethods, typeof(HeroController).GetMethod("TakeDamage", allFlags));
            Add(targetMethods, typeof(AnimatorFollow).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(ArrayGetRandom).GetMethod("DoGetRandomValue", allFlags));
            Add(targetMethods, typeof(ArrayListGetRandom).GetMethod("GetRandomItem", allFlags));
            Add(targetMethods, typeof(ArrayListShuffle).GetMethod("DoArrayListShuffle", allFlags));
            Add(targetMethods, typeof(ArrayShuffle).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(AudioPlayerOneShot).GetMethod("DoPlayRandomClip", allFlags));
            Add(targetMethods, typeof(AudioPlayerOneShotSingle).GetMethod("DoPlayRandomClip", allFlags));
            Add(targetMethods, typeof(AudioPlayRandom).GetMethod("DoPlayRandomClip", allFlags));
            Add(targetMethods, typeof(AudioPlayRandomSingle).GetMethod("DoPlayRandomClip", allFlags));
            Add(targetMethods, typeof(ChaseObject).GetMethod("DoBuzz", allFlags));
            Add(targetMethods, typeof(CreatePoolObjects).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(DistanceWalk).GetMethod("DoWalk", allFlags));
            Add(targetMethods, typeof(FireAtTarget).GetMethod("DoSetVelocity", allFlags));
            Add(targetMethods, typeof(Flicker).GetMethod("OnUpdate", allFlags));
            Add(targetMethods, typeof(FlingObject).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(FlingObjects).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(FlingObjectsFromGlobalPool).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(FlingObjectsFromGlobalPoolTime).GetMethod("OnUpdate", allFlags));
            Add(targetMethods, typeof(FlingObjectsFromGlobalPoolVel).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(GetRandomChild).GetMethod("DoGetRandomChild", allFlags));
            Add(targetMethods, typeof(GetRandomObject).GetMethod("DoGetRandomObject", allFlags));
            Add(targetMethods, typeof(IdleBuzz).GetMethod("DoBuzz", allFlags));
            Add(targetMethods, typeof(IdleBuzzV2).GetMethod("DoBuzz", allFlags));
            Add(targetMethods, typeof(IdleBuzzV3).GetMethod("DoBuzz", allFlags));
            Add(targetMethods, typeof(ObjectBounce).GetMethod("OnCollisionEnter2D", allFlags));
            Add(targetMethods, typeof(ObjectJitter).GetMethod("DoTranslate", allFlags));
            Add(targetMethods, typeof(ObjectJitterLocal).GetMethod("DoTranslate", allFlags));
            Add(targetMethods, typeof(RandomBool).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(RandomEvent).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(RandomEvent).GetMethod("GetRandomEvent", allFlags));
            Add(targetMethods, typeof(RandomFloat).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(RandomFloatEither).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(RandomFloatV2).GetMethod("Randomise", allFlags));
            Add(targetMethods, typeof(RandomInt).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(RandomlyFlipFloat).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(RandomWait).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(SetRandomMaterial).GetMethod("DoSetRandomMaterial", allFlags));
            Add(targetMethods, typeof(SetRandomRotation).GetMethod("DoRandomRotation", allFlags));
            Add(targetMethods, typeof(SpawnFromPool).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(SpawnFromPoolV2).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(SpawnObjectFromGlobalPoolOverTimeV2).GetMethod("OnUpdate", allFlags));
            Add(targetMethods, typeof(SpawnRandomObjects).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(SpawnRandomObjectsOverTime).GetMethod("DoSpawn", allFlags));
            Add(targetMethods, typeof(SpawnRandomObjectsOverTimeV2).GetMethod("DoSpawn", allFlags));
            Add(targetMethods, typeof(SpawnRandomObjectsV2).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(SpawnRandomObjectsVelocity).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(Tk2dSpriteSetIdRandom).GetMethod("DoSetSpriteID", allFlags));
            Add(targetMethods, typeof(WaitRandom).GetMethod("OnEnter", allFlags));
            Add(targetMethods, typeof(iTween).GetMethod("ApplyShakePositionTargets", allFlags));
            Add(targetMethods, typeof(iTween).GetMethod("ApplyShakeRotationTargets", allFlags));
            Add(targetMethods, typeof(iTween).GetMethod("ApplyShakeScaleTargets", allFlags));
            Add(targetMethods, typeof(iTween).GetMethod("GenerateID", allFlags));
            Add(targetMethods, typeof(ObjectBounce).GetMethod("OnCollisionEnter2D", allFlags));
            Add(targetMethods, typeof(PlayFromRandomFrame).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(RandomRotation).GetMethod("RandomRotate", allFlags));
            Add(targetMethods, typeof(RandomScale).GetMethod("onEnable", allFlags));
            Add(targetMethods, typeof(RandomScale).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(SceneryTriggerCircle).GetMethod("RandomizePitch", allFlags));
            Add(targetMethods, typeof(SetZ).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(SetZRandom).GetMethod("OnEnable", allFlags));
            Add(targetMethods, typeof(SimpleRock).GetMethod("OnTriggerEnter", allFlags));
            Add(targetMethods, typeof(SimpleRock).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(SpinSelf).GetMethod("Start", allFlags));
            Add(targetMethods, typeof(tk2dSpriteAnimator).GetMethod("Play", allFlags, null, new[] {typeof(tk2dSpriteAnimationClip), typeof(float), typeof(float)}, null));
        }

        private static void Add(List<MethodInfo> targetMethods, MethodInfo method) {
            if (method != null) {
                targetMethods.Add(method);
                Debug.Log($"Hooked Random in {method.Name}");
            } else {
                Debug.Log("Failed to hook Random as could not locate method");
            }
        }

        private static void LoadPlaybackFiles() {
            string playbackPath = "./Playback/RNG";
            if (Directory.Exists(playbackPath)) {
                var files = Directory.GetFiles(playbackPath);
                EnablePlayback = files.Length > 0;
                foreach (var file in files.OrderBy(f => f, StringComparer.InvariantCulture)) {
                    LoadPlaybackFile(file);
                }
            }
        }

        private static void LoadPlaybackFile(string file) {
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream)) {
                _playback.Add(new Dictionary<string, PlaybackState>());
                var playback = _playback[_playback.Count - 1];
                string line;
                while ((line = reader.ReadLine()) != null) {
                    if (line.Length > 0 && line[0] != '#') {
                        int indexFirst = line.IndexOf(',');
                        if (indexFirst > -1) {
                            var name = line.Substring(0, indexFirst).Trim();
                            var values = StringUtils.ParseFloats(line.Substring(indexFirst + 1));
                            if (!playback.TryGetValue(name, out var playbackState)) {
                                playbackState = new PlaybackState();
                                playback.Add(name, playbackState);
                            }

                            playbackState.Values.AddRange(values);
                        }
                    }
                }
            }
        }

        public static void DumpLogs() {
            lock (_lock) {
                if (!Directory.Exists("./Recording"))
                    Directory.CreateDirectory("./Recording");
                string recordingPath = "./Recording/RNG";
                if (!Directory.Exists(recordingPath))
                    Directory.CreateDirectory(recordingPath);

                for (int i = 0; i < _recording.Count; i++) {
                    var recording = _recording[i];
                    var sceneName = i < _sceneNames.Count ? _sceneNames[i] : "Unknown";
                    using (var stream = File.Open($"{recordingPath}/S{i:00000}_{sceneName}.csv", FileMode.Create, FileAccess.Write))
                    using (var writer = new StreamWriter(stream)) {
                        
                        foreach (var name in recording.Keys) {
                            writer.WriteLine($"{name},{StringUtils.Join(",", recording[name])}");
                        }
                    }
                }

                if (EnableDetailLogging && _detailLog != null) {
                    using (var stream = File.Open("./HK_Rng_Details.csv", FileMode.Create, FileAccess.Write))
                    using (var writer = new StreamWriter(stream)) {
                        foreach (var entry in _detailLog) {
                            writer.WriteLine(entry);
                        }
                    }
                }
            }
        }

        public static void RollRngNextScene() {
            _nextSceneRollCount++;
        }

        //public static void NotifyBeginScene(string destScene) {
        //    if (!_leftScene) {
        //        _sceneIndex++;
        //    }
        //    _leftScene = false;

        //    lock (_lock) {
        //        _sceneNames.Add(destScene);
        //        if (EnableDetailLogging && _detailLog != null) {
        //            _detailLog.Add("");
        //            _detailLog.Add("");
        //            _detailLog.Add("### Scene: " + destScene);
        //            _detailLog.Add("");
        //        }
        //    }
        //}

        //public static void OnLeftScene() {
        //    _sceneIndex++;
        //    _leftScene = true;
        //    Debug.Log("Left scene");
        //    if (_nextSceneRollCount > 0) {
        //        //Repeatedly call Random to increment the seed
        //        for (int i = 0; i < _nextSceneRollCount; i++) {
        //            var discard = UnityEngine.Random.Range(0, 1);
        //        }
        //        _nextSceneRollCount = 0;

        //        //Disable playback for just this scene
        //        if (_sceneIndex < _playback.Count-1) {
        //            _playback[_sceneIndex+1].Clear();
        //        }
        //    }
        //}

        public static void OnChangeScene(Scene scene) {
            lock (_lock) {
                _sceneIndex++;
                var name = scene.name;
                _sceneNames.Add(name);
                if (EnableDetailLogging && _detailLog != null) {
                    _detailLog.Add("");
                    _detailLog.Add("");
                    _detailLog.Add("### Scene: " + name);
                    _detailLog.Add("");
                }
            }

            if (_nextSceneRollCount > 0) {
                //Repeatedly call Random to increment the seed
                for (int i = 0; i < _nextSceneRollCount; i++) {
                    var discard = UnityEngine.Random.Range(0, 1);
                }
                _nextSceneRollCount = 0;

                //Disable playback for just this scene
                if (_sceneIndex < _playback.Count) {
                    _playback[_sceneIndex].Clear();
                }
            }
        }

        private static void InjectOnRange(MethodInfo method) {
            HookEndpointManager.Modify(method, (Action<ILContext>)InjectOnRange);
        }

        private static void InjectOnRangeFsm(MethodInfo method) {
            HookEndpointManager.Modify(method, (Action<ILContext>)InjectOnRangeFsm);
        }

        private static void InjectGetRwiFsm(MethodInfo method) {
            HookEndpointManager.Modify(method, (Action<ILContext>)InjectGetRwiFsm);
        }
        
        private static void InjectOnRange(ILContext il) {
            var name = TrimNamespace(il.Method.Name);
            var c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.Before, x => x.MatchCall(_rangeFloat))) {
                c.Remove();
                c.Emit(OpCodes.Ldstr, name);
                c.Emit(OpCodes.Call, typeof(RandomInjection).GetMethod("OnRangeFloat", BindingFlags.Public | BindingFlags.Static));
            }

            c.Goto(0);
            while (c.TryGotoNext(MoveType.Before, x => x.MatchCall(_rangeInt))) {
                c.Remove();
                c.Emit(OpCodes.Ldstr, name);
                c.Emit(OpCodes.Call, typeof(RandomInjection).GetMethod("OnRangeInt", BindingFlags.Public | BindingFlags.Static));
            }
        }

        private static void InjectOnRangeFsm(ILContext il) {
            var name = TrimNamespace(il.Method.Name);
            var c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.Before, x => x.MatchCall(_rangeFloat))) {
                c.Remove();
                c.Emit(OpCodes.Ldstr, name);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Castclass, typeof(FsmStateAction));
                c.Emit(OpCodes.Call, typeof(RandomInjection).GetMethod("OnRangeFloatFsm", BindingFlags.Public | BindingFlags.Static));
            }

            c.Goto(0);
            while (c.TryGotoNext(MoveType.Before, x => x.MatchCall(_rangeInt))) {
                c.Remove();
                c.Emit(OpCodes.Ldstr, name);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Castclass, typeof(FsmStateAction));
                c.Emit(OpCodes.Call, typeof(RandomInjection).GetMethod("OnRangeIntFsm", BindingFlags.Public | BindingFlags.Static));
            }
        }

        private static void InjectGetRwiFsm(ILContext il) {
            var name = TrimNamespace(il.Method.Name);
            var c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.Before, x => x.MatchCall(_getRwi))) {
                c.Remove();
                c.Emit(OpCodes.Ldstr, name);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Castclass, typeof(FsmStateAction));
                c.Emit(OpCodes.Call, typeof(RandomInjection).GetMethod("OnGetRwiFsm", BindingFlags.Public | BindingFlags.Static));
            }
        }

        private static string TrimNamespace(string name) {
            var playMakerNs = "HutongGames.PlayMaker.Actions.";
            if (name.StartsWith(playMakerNs))
                name = name.Substring(playMakerNs.Length);

            return name;
        }

        public static float OnRangeFloat(float min, float max, string name) {
            lock (_lock) {
                CheckScene();
                name = $"{name}({min}:{max})";
                float result;
                if (EnablePlayback && TryGetPlayback(name, out var playbackState) && playbackState.Index < playbackState.Values.Count) {
                    playbackState.Index++;
                    result = playbackState.Values[playbackState.Index - 1];
                    if (!_useLegacyRngSync) {
                        //Improve edge case sync by calling Random so that seed progression should match even when using playback ideally
                        var discard = UnityEngine.Random.Range(min, max);
                    }
                } else {
                    result = UnityEngine.Random.Range(min, max);
                }

                if (EnableRecording) {
                    var list = GetRecording(name);
                    list.Add(result);
                }

                if (_detailLog != null) {
                    _detailLog.Add($"{name}: {result}");
                }

                return result;
            }
        }

        public static int OnRangeInt(int min, int max, string name) {
            lock (_lock) {
                CheckScene();
                name = $"{name}({min}:{max})";
                int result;
                if (EnablePlayback && TryGetPlayback(name, out var playbackState) && playbackState.Index < playbackState.Values.Count) {
                    playbackState.Index++;
                    result = (int)playbackState.Values[playbackState.Index - 1];
                    if (!_useLegacyRngSync) {
                        //Improve edge case sync by calling Random so that seed progression should match even when using playback ideally
                        var discard = UnityEngine.Random.Range(min, max);
                    }
                } else {
                    result = UnityEngine.Random.Range(min, max);
                }

                if (EnableRecording) {
                    var list = GetRecording(name);
                    list.Add(result);
                }

                if (_detailLog != null) {
                    _detailLog.Add($"{name}: {result}");
                }

                return result;
            }
        }

        public static float OnRangeFloatFsm(float min, float max, string name, FsmStateAction action) {
            return OnRangeFloat(min, max, $"[{action.Fsm?.GameObjectName ?? ""}/{action.Fsm?.Name ?? ""}/{action.State?.Name ?? ""}]{name}");
        }

        public static int OnRangeIntFsm(int min, int max, string name, FsmStateAction action) {
            return OnRangeInt(min, max, $"[{action.Fsm?.GameObjectName ?? ""}/{action.Fsm?.Name ?? ""}/{action.State?.Name ?? ""}]{name}");
        }

        public static int OnGetRwiFsm(FsmFloat[] weights, string name, FsmStateAction action) {
            lock (_lock) {
                CheckScene();
                var compName = $"[{action.Fsm?.GameObjectName ?? ""}/{action.Fsm?.Name ?? ""}/{action.State?.Name ?? ""}]{name}";
                int result;
                if (EnablePlayback && TryGetPlayback(compName, out var playbackState) && playbackState.Index < playbackState.Values.Count) {
                    playbackState.Index++;
                    result = (int)playbackState.Values[playbackState.Index - 1];
                    if (!_useLegacyRngSync) {
                        //Improve edge case sync by calling Random so that seed progression should match even when using playback ideally
                        var discard = ActionHelpers.GetRandomWeightedIndex(weights);
                    }
                } else {
                    result = ActionHelpers.GetRandomWeightedIndex(weights);
                }

                if (EnableRecording) {
                    var list = GetRecording(compName);
                    list.Add(result);
                }

                if (_detailLog != null) {
                    _detailLog.Add($"{compName}: {result}");
                }

                return result;
            }
        }

        private static void CheckScene() {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != _sceneNames[_sceneIndex]) {
                OnChangeScene(scene);
            }
        }

        private static bool TryGetPlayback(string name, out PlaybackState state) {
            state = null;
            if (_sceneIndex >= _playback.Count)
                return false;

            return _playback[_sceneIndex].TryGetValue(name, out state);
        }

        private static List<float> GetRecording(string name) {
            while (_sceneIndex >= _recording.Count) {
                _recording.Add(new Dictionary<string, List<float>>());
            }

            if (!_recording[_sceneIndex].TryGetValue(name, out var result)) {
                result = new List<float>();
                _recording[_sceneIndex].Add(name, result);
            }

            return result;
        }

        private sealed class PlaybackState {
            public PlaybackState() {
                Values = new List<float>();
            }

            public int Index { get; set; }

            public List<float> Values { get; }
        }
    }
}
