using Assembly_CSharp.TasInfo.mm.Source.Extensions;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Assembly_CSharp.TasInfo.mm.Source {
    public struct Split {
        private string _splitTitle;
        private float _splitTime;
        private float _totalTime;
        private SplitName _splitTrigger;



        public Split(string splitTitle, SplitName splitTrigger) {
            _splitTitle = splitTitle;
            _splitTime = 0f;
            _totalTime = 0f;
            _splitTrigger = splitTrigger;
        }
        public string SplitTitle => _splitTitle;
        public float SplitTime => _splitTime;
        public float TotalTime => _totalTime;
        public SplitName SplitTrigger => _splitTrigger;

        public void StartSplitTimer(float time) {
            _totalTime = time;
        }

        public void IncreaseTimer(float time) {
            _splitTime += time;
            _totalTime += time;
        }
    }

    public class SplitClass {
        public Split Splt;

        public SplitClass() {
            Splt = new Split("", SplitName.ManualSplit);
        }

        public SplitClass(string splitTitle, SplitName splitTrigger) {
            Splt = new Split(splitTitle, splitTrigger);
        }

        public ref Split SplitRef => ref Splt;

        public ref Split GetSplitStruct() {
            return ref Splt;
        }
    }

    internal class AutoSplit {

        private static List<string> menuingSceneNames = new List<string> { "Menu_Title", "Quit_To_Menu", "PermaDeath" };
        private static readonly FieldInfo TeleportingFieldInfo = typeof(CameraController).GetFieldInfo("teleporting");
        private static readonly FieldInfo TilemapDirtyFieldInfo = typeof(GameManager).GetFieldInfo("tilemapDirty");
        private static int currentSplitIndex = 0;
        private static HollowKnightStoredData store;
        private static bool timeStart = false;
        private static bool timeEnd = false;
        private static float inGameTime = 0f;
        private static GameState lastGameState;
        private static bool lookForTeleporting;
        private static bool isPaused = false;
        private static readonly int minorVersion = int.Parse(Constants.GAME_VERSION.Substring(2, 1));
        public static bool SplitLastSplit = false;

        private static string FormattedTime {
            get {
                string previousSplitFormat = null;
                if (currentSplitIndex > 0) {
                    ref Split previousSplit = ref SplitReader.SplitList.ElementAt(currentSplitIndex - 1).SplitRef;
                    float previousSplitTime = previousSplit.SplitTime;
                    string previousSplitTitle = previousSplit.SplitTitle;
                    float previousTotalTime = previousSplit.TotalTime;
                    if (previousSplitTime < 60) {
                        previousSplitFormat = $"{previousSplitTitle}: {previousSplitTime.ToString("F2").PadLeft(5, '0')},";
                    } else if (previousSplitTime < 3600) {
                        int minute = (int)(previousSplitTime / 60);
                        float second = previousSplitTime - minute * 60;
                        previousSplitFormat = $"{previousSplitTitle}: {minute}:{second.ToString("F2").PadLeft(5, '0')},";
                    } else {
                        int hour = (int)(previousSplitTime / 3600);
                        int minute = (int)((previousSplitTime - hour * 3600) / 60);
                        float second = previousSplitTime - hour * 3600 - minute * 60;
                        previousSplitFormat = $"{previousSplitTitle}: {hour}:{minute.ToString().PadLeft(2, '0')}:{second.ToString("F2").PadLeft(5, '0')},";
                    }
                    if (previousTotalTime < 60) {
                        previousSplitFormat += $" {previousTotalTime.ToString("F2").PadLeft(5, '0')}\n";
                    } else if (previousTotalTime < 3600) {
                        int minute = (int)(previousTotalTime / 60);
                        float second = previousTotalTime - minute * 60;
                        previousSplitFormat+= $" {minute}:{second.ToString("F2").PadLeft(5, '0')}\n";
                    } else {
                        int hour = (int)(previousSplitTime / 3600);
                        int minute = (int)((previousSplitTime - hour * 3600) / 60);
                        float second = previousSplitTime - hour * 3600 - minute * 60;
                        previousSplitFormat += $" {hour}:{minute.ToString().PadLeft(2, '0')}:{second.ToString("F2").PadLeft(5, '0')}\n";
                    }
                }
                ref Split currentSplit = ref SplitReader.SplitList.ElementAt(currentSplitIndex).SplitRef;
                float splitTime = currentSplit.SplitTime;
                string splitTitle = currentSplit.SplitTitle;
                float splitTotalTime = currentSplit.TotalTime;
                string formattedSplits;
                if (splitTime < 60) {
                    formattedSplits = $"{previousSplitFormat}{splitTitle}: {splitTime.ToString("F2").PadLeft(5, '0')},";
                } else if (splitTime < 3600) {
                    int minute = (int)(splitTime / 60);
                    float second = splitTime - minute * 60;
                    formattedSplits = $"{previousSplitFormat}{splitTitle}: {minute}:{second.ToString("F2").PadLeft(5, '0')},";
                } else {
                    int hour = (int)(splitTime / 3600);
                    int minute = (int)((splitTime - hour * 3600) / 60);
                    float second = splitTime - hour * 3600 - minute * 60;
                    formattedSplits = $"{previousSplitFormat}{splitTitle}: {hour}:{minute.ToString().PadLeft(2, '0')}:{second.ToString("F2").PadLeft(5, '0')},";
                }
                if (splitTotalTime == 0) {
                    return string.Empty;
                } else if (splitTotalTime < 60) {
                    return $"{formattedSplits} {splitTotalTime.ToString("F2").PadLeft(5, '0')}\n";
                } else if (splitTime < 3600) {
                    int minute = (int)(splitTotalTime / 60);
                    float second = splitTotalTime - minute * 60;
                    return $"{formattedSplits} {minute}:{second.ToString("F2").PadLeft(5, '0')}\n";
                } else {
                    int hour = (int)(splitTotalTime / 3600);
                    int minute = (int)((splitTotalTime - hour * 3600) / 60);
                    float second = splitTotalTime - hour * 3600 - minute * 60;
                    return $"{formattedSplits} {hour}:{minute.ToString().PadLeft(2, '0')}:{second.ToString("F2").PadLeft(5, '0')}";
                }
            }
        }

        private static bool ShouldSplitTransition(string nextScene, string sceneName) {
            if (nextScene != sceneName) {
                return !(
                    string.IsNullOrEmpty(sceneName) ||
                    string.IsNullOrEmpty(nextScene) ||
                    menuingSceneNames.Contains(sceneName) ||
                    menuingSceneNames.Contains(nextScene)
                );
            }
            return false;
        }

        private static bool CheckSplit(GameManager gameManager, SplitName split, string nextScene, string sceneName) {
            bool shouldSplit = false;
            bool shouldSkip = false;

            switch (split) {
                case SplitName.Abyss:
                    shouldSplit = gameManager.playerData.visitedAbyss;
                    break;
                case SplitName.AbyssShriek:
                    shouldSplit = gameManager.playerData.screamLevel == 2;
                    break;
                case SplitName.Aluba:
                    shouldSplit = gameManager.playerData.killedLazyFlyer;
                    break;
                case SplitName.AncestralMound:
                    shouldSplit = nextScene.Equals("Crossroads_ShamanTemple") && nextScene != sceneName;
                    break;
                case SplitName.AspidHunter:
                    shouldSplit = gameManager.playerData.killsSpitter == 17;
                    break;
                case SplitName.BaldurShell:
                    shouldSplit = gameManager.playerData.gotCharm_5;
                    break;
                case SplitName.BeastsDenTrapBench:
                    shouldSplit = gameManager.playerData.spiderCapture;
                    break;
                case SplitName.BlackKnight:
                    shouldSplit = gameManager.playerData.killedBlackKnight;
                    break;
                case SplitName.BrettaRescued:
                    shouldSplit = gameManager.playerData.brettaRescued;
                    break;
                case SplitName.BrummFlame:
                    shouldSplit = gameManager.playerData.gotBrummsFlame;
                    break;
                case SplitName.BrokenVessel:
                    shouldSplit = gameManager.playerData.killedInfectedKnight;
                    break;
                case SplitName.BroodingMawlek:
                    shouldSplit = gameManager.playerData.killedMawlek;
                    break;
                case SplitName.CityOfTears:
                    shouldSplit = gameManager.playerData.visitedRuins;
                    break;
                case SplitName.Collector:
                    shouldSplit = gameManager.playerData.collectorDefeated;
                    break;
                case SplitName.TransCollector:
                    shouldSplit = gameManager.playerData.collectorDefeated && sceneName.StartsWith("Ruins2_11") && nextScene != sceneName;
                    break;
                case SplitName.Colosseum:
                    shouldSplit = gameManager.playerData.seenColosseumTitle;
                    break;
                case SplitName.ColosseumBronze:
                    shouldSplit = gameManager.playerData.colosseumBronzeCompleted;
                    break;
                case SplitName.ColosseumGold:
                    shouldSplit = gameManager.playerData.colosseumGoldCompleted;
                    break;
                case SplitName.ColosseumSilver:
                    shouldSplit = gameManager.playerData.colosseumSilverCompleted;
                    break;
                case SplitName.CrossroadsStation:
                    shouldSplit = gameManager.playerData.openedCrossroads;
                    break;
                case SplitName.CrystalGuardian1:
                    shouldSplit = gameManager.playerData.defeatedMegaBeamMiner;
                    break;
                case SplitName.CrystalGuardian2:
                    shouldSplit = gameManager.playerData.killsMegaBeamMiner == 0;
                    break;
                case SplitName.CrystalHeart:
                    shouldSplit = gameManager.playerData.hasSuperDash;
                    break;
                case SplitName.CrystalPeak:
                    shouldSplit = gameManager.playerData.visitedMines;
                    break;
                case SplitName.CycloneSlash:
                    shouldSplit = gameManager.playerData.hasCyclone;
                    break;
                case SplitName.Dashmaster:
                    shouldSplit = gameManager.playerData.gotCharm_31;
                    break;
                case SplitName.DashSlash:
                    shouldSplit = gameManager.playerData.hasUpwardSlash;
                    break;
                case SplitName.DeepFocus:
                    shouldSplit = gameManager.playerData.gotCharm_34;
                    break;
                case SplitName.Deepnest:
                    shouldSplit = gameManager.playerData.visitedDeepnest;
                    break;
                case SplitName.DeepnestSpa:
                    shouldSplit = gameManager.playerData.visitedDeepnestSpa;
                    break;
                case SplitName.DeepnestStation:
                    shouldSplit = gameManager.playerData.openedDeepnest;
                    break;
                case SplitName.DefendersCrest:
                    shouldSplit = gameManager.playerData.gotCharm_10;
                    break;
                case SplitName.DescendingDark:
                    shouldSplit = gameManager.playerData.quakeLevel == 2;
                    break;
                case SplitName.DesolateDive:
                    shouldSplit = gameManager.playerData.quakeLevel == 1;
                    break;
                case SplitName.Dirtmouth:
                    shouldSplit = gameManager.playerData.visitedDirtmouth;
                    break;
                case SplitName.Dreamer1:
                    shouldSplit = gameManager.playerData.guardiansDefeated == 1;
                    break;
                case SplitName.Dreamer2:
                    shouldSplit = gameManager.playerData.guardiansDefeated == 2;
                    break;
                case SplitName.Dreamer3:
                    shouldSplit = gameManager.playerData.guardiansDefeated == 3;
                    break;
                case SplitName.DreamNail:
                    shouldSplit = gameManager.playerData.hasDreamNail;
                    break;
                case SplitName.DreamNail2:
                    shouldSplit = gameManager.playerData.dreamNailUpgraded;
                    break;
                case SplitName.DreamGate:
                    shouldSplit = gameManager.playerData.hasDreamGate;
                    break;
                case SplitName.Dreamshield:
                    shouldSplit = gameManager.playerData.gotCharm_38;
                    break;
                case SplitName.DreamWielder:
                    shouldSplit = gameManager.playerData.gotCharm_30;
                    break;
                case SplitName.DungDefender:
                    shouldSplit = gameManager.playerData.killedDungDefender;
                    break;
                case SplitName.ElderbugFlower:
                    shouldSplit = gameManager.playerData.elderbugGaveFlower;
                    break;
                case SplitName.ElderHu:
                    shouldSplit = gameManager.playerData.killedGhostHu;
                    break;
                case SplitName.ElegantKey:
                    shouldSplit = gameManager.playerData.hasWhiteKey;
                    break;
#if V1432
                case SplitName.EternalOrdealAchieved:
                    shouldSplit = gameManager.playerData.ordealAchieved;
                    break;
                case SplitName.EternalOrdealUnlocked:
                    shouldSplit = gameManager.playerData.zoteStatueWallBroken;
                    break;
#endif
                case SplitName.FailedKnight:
                    shouldSplit = gameManager.playerData.falseKnightDreamDefeated;
                    break;
                case SplitName.FalseKnight:
                    shouldSplit = gameManager.playerData.killedFalseKnight;
                    break;
                case SplitName.Flukemarm:
                    shouldSplit = gameManager.playerData.killedFlukeMother;
                    break;
                case SplitName.Flukenest:
                    shouldSplit = gameManager.playerData.gotCharm_11;
                    break;
                case SplitName.FogCanyon:
                    shouldSplit = gameManager.playerData.visitedFogCanyon;
                    break;
                case SplitName.ForgottenCrossroads:
                    shouldSplit = gameManager.playerData.visitedCrossroads;
                    shouldSkip = !sceneName.StartsWith("Crossroads_"); // in most cases it will cause the Split to Skip if this Split is triggered by the splits file getting Reset from incompatible splits
                    break;
                case SplitName.FragileGreed:
                    shouldSplit = gameManager.playerData.gotCharm_24;
                    break;
                case SplitName.FragileHeart:
                    shouldSplit = gameManager.playerData.gotCharm_23;
                    break;
                case SplitName.FragileStrength:
                    shouldSplit = gameManager.playerData.gotCharm_25;
                    break;
                case SplitName.FungalWastes:
                    shouldSplit = gameManager.playerData.visitedFungus;
                    break;
                case SplitName.FuryOfTheFallen:
                    shouldSplit = gameManager.playerData.gotCharm_6;
                    break;
                case SplitName.Galien:
                    shouldSplit = gameManager.playerData.killedGhostGalien;
                    break;
                case SplitName.GatheringSwarm:
                    shouldSplit = gameManager.playerData.gotCharm_1;
                    break;
                case SplitName.GlowingWomb:
                    shouldSplit = gameManager.playerData.gotCharm_22;
                    break;
#if V1432
                case SplitName.Godhome:
                    shouldSplit = gameManager.playerData.visitedGodhome;
                    break;
                case SplitName.GodTuner:
                    shouldSplit = gameManager.playerData.hasGodfinder;
                    break;
#endif
                case SplitName.GodTamer:
                    shouldSplit = gameManager.playerData.killedLobsterLancer;
                    break;
                case SplitName.Gorb:
                    shouldSplit = gameManager.playerData.killedGhostAladar;
                    break;
                case SplitName.GorgeousHusk:
                    shouldSplit = gameManager.playerData.killedGorgeousHusk;
                    break;
                case SplitName.GreatSlash:
                    shouldSplit = gameManager.playerData.hasDashSlash;
                    break;
                case SplitName.Greenpath:
                    shouldSplit = gameManager.playerData.visitedGreenpath;
                    break;
                case SplitName.GreenpathStation:
                    shouldSplit = gameManager.playerData.openedGreenpath;
                    break;
                case SplitName.Grimmchild:
                    shouldSplit = gameManager.playerData.gotCharm_40;
                    break;
                case SplitName.Grimmchild2:
                    shouldSplit = gameManager.playerData.grimmChildLevel == 2;
                    break;
                case SplitName.Grimmchild3:
                    shouldSplit = gameManager.playerData.grimmChildLevel == 3;
                    break;
                case SplitName.Grimmchild4:
                    shouldSplit = gameManager.playerData.grimmChildLevel == 4;
                    break;
                case SplitName.GrubberflysElegy:
                    shouldSplit = gameManager.playerData.gotCharm_35;
                    break;
                case SplitName.Grubsong:
                    shouldSplit = gameManager.playerData.gotCharm_3;
                    break;
                case SplitName.GreatHopper:
                    shouldSplit = gameManager.playerData.killedGiantHopper;
                    break;
                case SplitName.GreyPrince:
                    shouldSplit = gameManager.playerData.killedGreyPrince;
                    break;
                case SplitName.GruzMother:
                    shouldSplit = gameManager.playerData.killedBigFly;
                    break;
                case SplitName.HeavyBlow:
                    shouldSplit = gameManager.playerData.gotCharm_15;
                    break;
                case SplitName.Hegemol:
                    shouldSplit = gameManager.playerData.maskBrokenHegemol;
                    break;
                case SplitName.HegemolDreamer:
                    shouldSplit = gameManager.playerData.hegemolDefeated;
                    break;
                case SplitName.HiddenStationStation:
                    shouldSplit = gameManager.playerData.openedHiddenStation;
                    break;
                case SplitName.Hive:
                    shouldSplit = gameManager.playerData.visitedHive;
                    break;
                case SplitName.Hiveblood:
                    shouldSplit = gameManager.playerData.gotCharm_29;
                    break;
                case SplitName.HollowKnightDreamnail:
                    shouldSplit = nextScene.Equals("Dream_Final_Boss", StringComparison.OrdinalIgnoreCase);
                    shouldSkip = gameManager.playerData.killedHollowKnight;
                    break;
                case SplitName.HollowKnightBoss:
                    shouldSplit = gameManager.playerData.killedHollowKnight;
                    break;
                case SplitName.RadianceBoss:
                    shouldSplit = gameManager.playerData.killedFinalBoss;
                    break;
                case SplitName.Hornet1:
                    shouldSplit = gameManager.playerData.killedHornet;
                    break;
                case SplitName.Hornet2:
                    shouldSplit = gameManager.playerData.hornetOutskirtsDefeated;
                    break;
                case SplitName.HowlingWraiths:
                    shouldSplit = gameManager.playerData.screamLevel == 1;
                    break;
                case SplitName.HuntersMark:
                    shouldSplit = gameManager.playerData.killedHunterMark;
                    break;
                case SplitName.HuskMiner:
                    shouldSplit = store.CheckIncreasedBy(Offset.killsZombieMiner, -1, gameManager.playerData.killsZombieMiner);
                    break;
                case SplitName.InfectedCrossroads:
                    shouldSplit = gameManager.playerData.crossroadsInfected && gameManager.playerData.visitedCrossroads;
                    break;
                case SplitName.IsmasTear:
                    shouldSplit = gameManager.playerData.hasAcidArmour;
                    break;
                case SplitName.JonisBlessing:
                    shouldSplit = gameManager.playerData.gotCharm_27;
                    break;
                case SplitName.KingdomsEdge:
                    shouldSplit = gameManager.playerData.visitedOutskirts;
                    break;
                case SplitName.KingsBrand:
                    shouldSplit = gameManager.playerData.hasKingsBrand;
                    break;
                case SplitName.Kingsoul:
                    shouldSplit = gameManager.playerData.charmCost_36 == 5 && gameManager.playerData.royalCharmState == 3;
                    break;
                case SplitName.KingsStationStation:
                    shouldSplit = gameManager.playerData.openedRuins2;
                    break;
                //case SplitName.Lemm1: shouldSplit = gameManager.playerData.metRelicDealer); break;
                case SplitName.Lemm2:
                    shouldSplit = gameManager.playerData.metRelicDealerShop;
                    break;
                case SplitName.LifebloodCore:
                    shouldSplit = gameManager.playerData.gotCharm_9;
                    break;
                case SplitName.LifebloodHeart:
                    shouldSplit = gameManager.playerData.gotCharm_8;
                    break;
                case SplitName.LittleFool:
                    shouldSplit = gameManager.playerData.littleFoolMet;
                    break;
                case SplitName.Longnail:
                    shouldSplit = gameManager.playerData.gotCharm_18;
                    break;
                case SplitName.LostKin:
                    shouldSplit = gameManager.playerData.infectedKnightDreamDefeated;
                    break;
                case SplitName.LoveKey:
                    shouldSplit = gameManager.playerData.hasLoveKey;
                    break;
                case SplitName.LumaflyLantern:
                    shouldSplit = gameManager.playerData.hasLantern;
                    break;
                case SplitName.Lurien:
                    shouldSplit = gameManager.playerData.maskBrokenLurien;
                    break;
                case SplitName.LurienDreamer:
                    shouldSplit = gameManager.playerData.lurienDefeated;
                    break;
                case SplitName.MantisClaw:
                    shouldSplit = gameManager.playerData.hasWalljump;
                    break;
                case SplitName.MantisLords:
                    shouldSplit = gameManager.playerData.defeatedMantisLords;
                    break;
                case SplitName.MarkOfPride:
                    shouldSplit = gameManager.playerData.gotCharm_13;
                    break;
                case SplitName.Markoth:
                    shouldSplit = gameManager.playerData.killedGhostMarkoth;
                    break;
                case SplitName.Marmu:
                    shouldSplit = gameManager.playerData.killedGhostMarmu;
                    break;
                case SplitName.MaskFragment1:
                    shouldSplit = gameManager.playerData.maxHealthBase == 5 && gameManager.playerData.heartPieces == 1;
                    break;
                case SplitName.MaskFragment2:
                    shouldSplit = gameManager.playerData.maxHealthBase == 5 && gameManager.playerData.heartPieces == 2;
                    break;
                case SplitName.MaskFragment3:
                    shouldSplit = gameManager.playerData.maxHealthBase == 5 && gameManager.playerData.heartPieces == 3;
                    break;
                case SplitName.Mask1:
                    shouldSplit = gameManager.playerData.maxHealthBase == 6;
                    break;
                case SplitName.MaskFragment5:
                    shouldSplit = gameManager.playerData.heartPieces == 5 || (gameManager.playerData.maxHealthBase == 6 && gameManager.playerData.heartPieces == 1);
                    break;
                case SplitName.MaskFragment6:
                    shouldSplit = gameManager.playerData.heartPieces == 6 || (gameManager.playerData.maxHealthBase == 6 && gameManager.playerData.heartPieces == 2);
                    break;
                case SplitName.MaskFragment7:
                    shouldSplit = gameManager.playerData.heartPieces == 7 || (gameManager.playerData.maxHealthBase == 6 && gameManager.playerData.heartPieces == 3);
                    break;
                case SplitName.Mask2:
                    shouldSplit = gameManager.playerData.maxHealthBase == 7;
                    break;
                case SplitName.MaskFragment9:
                    shouldSplit = gameManager.playerData.heartPieces == 9 || (gameManager.playerData.maxHealthBase == 7 && gameManager.playerData.heartPieces == 1);
                    break;
                case SplitName.MaskFragment10:
                    shouldSplit = gameManager.playerData.heartPieces == 10 || (gameManager.playerData.maxHealthBase == 7 && gameManager.playerData.heartPieces == 2);
                    break;
                case SplitName.MaskFragment11:
                    shouldSplit = gameManager.playerData.heartPieces == 11 || (gameManager.playerData.maxHealthBase == 7 && gameManager.playerData.heartPieces == 3);
                    break;
                case SplitName.Mask3:
                    shouldSplit = gameManager.playerData.maxHealthBase == 8;
                    break;
                case SplitName.MaskFragment13:
                    shouldSplit = gameManager.playerData.heartPieces == 13 || (gameManager.playerData.maxHealthBase == 8 && gameManager.playerData.heartPieces == 1);
                    break;
                case SplitName.MaskFragment14:
                    shouldSplit = gameManager.playerData.heartPieces == 14 || (gameManager.playerData.maxHealthBase == 8 && gameManager.playerData.heartPieces == 2);
                    break;
                case SplitName.MaskFragment15:
                    shouldSplit = gameManager.playerData.heartPieces == 15 || (gameManager.playerData.maxHealthBase == 8 && gameManager.playerData.heartPieces == 3);
                    break;
                case SplitName.Mask4:
                    shouldSplit = gameManager.playerData.maxHealthBase == 9;
                    break;
#if V1432
                case SplitName.MatoOroNailBros:
                    shouldSplit = gameManager.playerData.killedNailBros;
                    break;
#endif
                case SplitName.MegaMossCharger:
                    shouldSplit = gameManager.playerData.megaMossChargerDefeated;
                    break;
                case SplitName.MenderBug:
                    shouldSplit = gameManager.playerData.killedMenderBug;
                    break;
                case SplitName.MonarchWings:
                    shouldSplit = gameManager.playerData.hasDoubleJump;
                    break;
                case SplitName.Monomon:
                    shouldSplit = gameManager.playerData.maskBrokenMonomon;
                    break;
                case SplitName.MonomonDreamer:
                    shouldSplit = gameManager.playerData.monomonDefeated;
                    break;
                case SplitName.MossKnight:
                    shouldSplit = gameManager.playerData.killedMossKnight;
                    break;
                case SplitName.MothwingCloak:
                    shouldSplit = gameManager.playerData.hasDash;
                    break;
                case SplitName.MrMushroom1:
                    shouldSplit = gameManager.playerData.mrMushroomState == 2;
                    break;
                case SplitName.MrMushroom2:
                    shouldSplit = gameManager.playerData.mrMushroomState == 3;
                    break;
                case SplitName.MrMushroom3:
                    shouldSplit = gameManager.playerData.mrMushroomState == 4;
                    break;
                case SplitName.MrMushroom4:
                    shouldSplit = gameManager.playerData.mrMushroomState == 5;
                    break;
                case SplitName.MrMushroom5:
                    shouldSplit = gameManager.playerData.mrMushroomState == 6;
                    break;
                case SplitName.MrMushroom6:
                    shouldSplit = gameManager.playerData.mrMushroomState == 7;
                    break;
                case SplitName.MrMushroom7:
                    shouldSplit = gameManager.playerData.mrMushroomState == 8;
                    break;
                case SplitName.MushroomBrawler:
                    shouldSplit = gameManager.playerData.killsMushroomBrawler == 6;
                    break;
                case SplitName.NailmastersGlory:
                    shouldSplit = gameManager.playerData.gotCharm_26;
                    break;
                case SplitName.NailUpgrade1:
                    shouldSplit = gameManager.playerData.nailSmithUpgrades == 1;
                    break;
                case SplitName.NailUpgrade2:
                    shouldSplit = gameManager.playerData.nailSmithUpgrades == 2;
                    break;
                case SplitName.NailUpgrade3:
                    shouldSplit = gameManager.playerData.nailSmithUpgrades == 3;
                    break;
                case SplitName.NailUpgrade4:
                    shouldSplit = gameManager.playerData.nailSmithUpgrades == 4;
                    break;
                case SplitName.NightmareKingGrimm:
                    shouldSplit = gameManager.playerData.killedNightmareGrimm;
                    break;
                case SplitName.NightmareLantern:
                    shouldSplit = gameManager.playerData.nightmareLanternLit;
                    break;
                case SplitName.NightmareLanternDestroyed:
                    shouldSplit = gameManager.playerData.destroyedNightmareLantern;
                    break;
                case SplitName.NoEyes:
                    shouldSplit = gameManager.playerData.killedGhostNoEyes;
                    break;
                case SplitName.Nosk:
                    shouldSplit = gameManager.playerData.killedMimicSpider;
                    break;
                case SplitName.NotchFogCanyon:
                    shouldSplit = gameManager.playerData.notchFogCanyon;
                    break;
                case SplitName.NotchSalubra1:
                    shouldSplit = gameManager.playerData.salubraNotch1;
                    break;
                case SplitName.NotchSalubra2:
                    shouldSplit = gameManager.playerData.salubraNotch2;
                    break;
                case SplitName.NotchSalubra3:
                    shouldSplit = gameManager.playerData.salubraNotch3;
                    break;
                case SplitName.NotchSalubra4:
                    shouldSplit = gameManager.playerData.salubraNotch4;
                    break;
                case SplitName.NotchShrumalOgres:
                    shouldSplit = gameManager.playerData.notchShroomOgres;
                    break;
                case SplitName.PaleOre:
                    shouldSplit = gameManager.playerData.ore > 0;
                    break;
#if V1432
                case SplitName.PaleLurkerKey:
                    shouldSplit = gameManager.playerData.gotLurkerKey;
                    break;
                case SplitName.Pantheon1:
                    shouldSplit = gameManager.playerData.bossDoorStateTier1.completed;
                    break;
                case SplitName.Pantheon2:
                    shouldSplit = gameManager.playerData.bossDoorStateTier2.completed;
                    break;
                case SplitName.Pantheon3:
                    shouldSplit = gameManager.playerData.bossDoorStateTier3.completed;
                    break;
                case SplitName.Pantheon4:
                    shouldSplit = gameManager.playerData.bossDoorStateTier4.completed;
                    break;
                case SplitName.Pantheon5:
                    shouldSplit = gameManager.playerData.bossDoorStateTier5.completed;
                    break;
                case SplitName.PathOfPain:
                    shouldSplit = gameManager.playerData.newDataBindingSeal;
                    break;
                case SplitName.PureVessel:
                    shouldSplit = gameManager.playerData.killedHollowKnightPrime;
                    break;
#endif
                case SplitName.QueensGardens:
                    shouldSplit = gameManager.playerData.visitedRoyalGardens;
                    break;
                case SplitName.QueensGardensStation:
                    shouldSplit = gameManager.playerData.openedRoyalGardens;
                    break;
                case SplitName.QueensStationStation:
                    shouldSplit = gameManager.playerData.openedFungalWastes;
                    break;
                case SplitName.QuickSlash:
                    shouldSplit = gameManager.playerData.gotCharm_32;
                    break;
                case SplitName.QuickFocus:
                    shouldSplit = gameManager.playerData.gotCharm_7;
                    break;
                case SplitName.RestingGrounds:
                    shouldSplit = gameManager.playerData.visitedRestingGrounds;
                    break;
                case SplitName.RestingGroundsStation:
                    shouldSplit = gameManager.playerData.openedRestingGrounds;
                    break;
                case SplitName.RoyalWaterways:
                    shouldSplit = gameManager.playerData.visitedWaterways;
                    break;
                case SplitName.SalubrasBlessing:
                    shouldSplit = gameManager.playerData.salubraBlessing;
                    break;
                case SplitName.SeerDeparts:
                    shouldSplit = gameManager.playerData.mothDeparted;
                    break;
                case SplitName.ShadeCloak:
                    shouldSplit = gameManager.playerData.hasShadowDash;
                    break;
                case SplitName.ShadeSoul:
                    shouldSplit = gameManager.playerData.fireballLevel == 2;
                    break;
                case SplitName.ShamanStone:
                    shouldSplit = gameManager.playerData.gotCharm_19;
                    break;
                case SplitName.ShapeOfUnn:
                    shouldSplit = gameManager.playerData.gotCharm_28;
                    break;
                case SplitName.SharpShadow:
                    shouldSplit = gameManager.playerData.gotCharm_16;
                    break;
#if V1432
                case SplitName.SheoPaintmaster:
                    shouldSplit = gameManager.playerData.killedPaintmaster;
                    break;
#endif
                case SplitName.SimpleKey:
                    shouldSplit = gameManager.playerData.simpleKeys > 0;
                    break;
                case SplitName.SlyKey:
                    shouldSplit = gameManager.playerData.hasSlykey;
                    break;
#if V1432
                case SplitName.SlyNailsage:
                    shouldSplit = gameManager.playerData.killedNailsage;
                    break;
#endif
                case SplitName.SoulCatcher:
                    shouldSplit = gameManager.playerData.gotCharm_20;
                    break;
                case SplitName.SoulEater:
                    shouldSplit = gameManager.playerData.gotCharm_21;
                    break;
                case SplitName.SoulMaster:
                    shouldSplit = gameManager.playerData.killedMageLord;
                    break;
                case SplitName.SoulTyrant:
                    shouldSplit = gameManager.playerData.mageLordDreamDefeated;
                    break;
                case SplitName.SpellTwister:
                    shouldSplit = gameManager.playerData.gotCharm_33;
                    break;
                case SplitName.SporeShroom:
                    shouldSplit = gameManager.playerData.gotCharm_17;
                    break;
                case SplitName.SpiritGladeOpen:
                    shouldSplit = gameManager.playerData.gladeDoorOpened;
                    break;
                case SplitName.Sprintmaster:
                    shouldSplit = gameManager.playerData.gotCharm_37;
                    break;
                case SplitName.StalwartShell:
                    shouldSplit = gameManager.playerData.gotCharm_4;
                    break;
                case SplitName.StagnestStation:
                    shouldSplit = nextScene.Equals("Cliffs_03", StringComparison.OrdinalIgnoreCase)
                                                              && gameManager.playerData.travelling
                                                              && gameManager.playerData.openedStagNest;
                    break;
                case SplitName.SteadyBody:
                    shouldSplit = gameManager.playerData.gotCharm_14;
                    break;
                case SplitName.StoreroomsStation:
                    shouldSplit = gameManager.playerData.openedRuins1;
                    break;
                case SplitName.TeachersArchive:
                    shouldSplit = sceneName.Equals("Fungus3_archive", StringComparison.OrdinalIgnoreCase);
                    break;
                case SplitName.ThornsOfAgony:
                    shouldSplit = gameManager.playerData.gotCharm_12;
                    break;
                case SplitName.TraitorLord:
                    shouldSplit = gameManager.playerData.killedTraitorLord;
                    break;
                case SplitName.TramPass:
                    shouldSplit = gameManager.playerData.hasTramPass;
                    break;
                case SplitName.TroupeMasterGrimm:
                    shouldSplit = gameManager.playerData.killedGrimm;
                    break;
                case SplitName.UnbreakableGreed:
                    shouldSplit = gameManager.playerData.fragileGreed_unbreakable;
                    break;
                case SplitName.UnbreakableHeart:
                    shouldSplit = gameManager.playerData.fragileHealth_unbreakable;
                    break;
                case SplitName.UnbreakableStrength:
                    shouldSplit = gameManager.playerData.fragileStrength_unbreakable;
                    break;
                case SplitName.UnchainedHollowKnight:
                    shouldSplit = gameManager.playerData.unchainedHollowKnight;
                    break;
                case SplitName.Uumuu:
                    shouldSplit = gameManager.playerData.killedMegaJellyfish;
                    break;
                case SplitName.UumuuEncountered:
                    shouldSplit = gameManager.playerData.encounteredMegaJelly;
                    break;
                case SplitName.VengefulSpirit:
                    shouldSplit = gameManager.playerData.fireballLevel == 1;
                    break;
                case SplitName.TransVS:
                    shouldSplit = gameManager.playerData.fireballLevel == 1 && nextScene != sceneName;
                    break;
                case SplitName.VesselFragment1:
                    shouldSplit = gameManager.playerData.MPReserveMax == 0 && gameManager.playerData.vesselFragments == 1;
                    break;
                case SplitName.VesselFragment2:
                    shouldSplit = gameManager.playerData.MPReserveMax == 0 && gameManager.playerData.vesselFragments == 2;
                    break;
                case SplitName.Vessel1:
                    shouldSplit = gameManager.playerData.MPReserveMax == 33;
                    break;
                case SplitName.VesselFragment4:
                    shouldSplit = gameManager.playerData.vesselFragments == 4 || (gameManager.playerData.MPReserveMax == 33 && gameManager.playerData.vesselFragments == 1);
                    break;
                case SplitName.VesselFragment5:
                    shouldSplit = gameManager.playerData.vesselFragments == 5 || (gameManager.playerData.MPReserveMax == 33 && gameManager.playerData.vesselFragments == 2);
                    break;
                case SplitName.Vessel2:
                    shouldSplit = gameManager.playerData.MPReserveMax == 66;
                    break;
                case SplitName.VesselFragment7:
                    shouldSplit = gameManager.playerData.vesselFragments == 7 || (gameManager.playerData.MPReserveMax == 66 && gameManager.playerData.vesselFragments == 1);
                    break;
                case SplitName.VesselFragment8:
                    shouldSplit = gameManager.playerData.vesselFragments == 8 || (gameManager.playerData.MPReserveMax == 66 && gameManager.playerData.vesselFragments == 2);
                    break;
                case SplitName.Vessel3:
                    shouldSplit = gameManager.playerData.MPReserveMax == 99;
                    break;
                case SplitName.VoidHeart:
                    shouldSplit = gameManager.playerData.gotShadeCharm;
                    break;
                case SplitName.WatcherChandelier:
                    shouldSplit = gameManager.playerData.watcherChandelier;
                    break;
                case SplitName.WaywardCompass:
                    shouldSplit = gameManager.playerData.gotCharm_2;
                    break;
                case SplitName.Weaversong:
                    shouldSplit = gameManager.playerData.gotCharm_39;
                    break;
                case SplitName.WhiteDefender:
                    shouldSplit = gameManager.playerData.killedWhiteDefender;
                    break;
                case SplitName.WhitePalace:
                    shouldSplit = gameManager.playerData.visitedWhitePalace;
                    break;
                case SplitName.Xero:
                    shouldSplit = gameManager.playerData.killedGhostXero;
                    break;
                case SplitName.Zote1:
                    shouldSplit = gameManager.playerData.zoteRescuedBuzzer;
                    break;
                case SplitName.Zote2:
                    shouldSplit = gameManager.playerData.zoteRescuedDeepnest;
                    break;
                case SplitName.ZoteKilled:
                    shouldSplit = gameManager.playerData.killedZote;
                    break;

                case SplitName.Flame1:
                    shouldSplit = gameManager.playerData.flamesCollected == 1;
                    break;
                case SplitName.Flame2:
                    shouldSplit = gameManager.playerData.flamesCollected == 2;
                    break;
                case SplitName.Flame3:
                    shouldSplit = gameManager.playerData.flamesCollected == 3;
                    break;
#if v1432
                case SplitName.HiveKnight:
                    shouldSplit = gameManager.playerData.killedHiveKnight;
                    break;
#endif
                case SplitName.Ore1:
                case SplitName.Ore2:
                case SplitName.Ore3:
                case SplitName.Ore4:
                case SplitName.Ore5:
                case SplitName.Ore6:
                    int upgrades = gameManager.playerData.nailSmithUpgrades;
                    int oreFromUpgrades = (upgrades * (upgrades - 1)) / 2;
                    int ore = oreFromUpgrades + gameManager.playerData.ore;

                    switch (split) {
                        case SplitName.Ore1:
                            shouldSplit = ore == 1;
                            break;
                        case SplitName.Ore2:
                            shouldSplit = ore == 2;
                            break;
                        case SplitName.Ore3:
                            shouldSplit = ore == 3;
                            break;
                        case SplitName.Ore4:
                            shouldSplit = ore == 4;
                            break;
                        case SplitName.Ore5:
                            shouldSplit = ore == 5;
                            break;
                        case SplitName.Ore6:
                            shouldSplit = ore == 6;
                            break;
                    }

                    break;

                case SplitName.Grub1:
                    shouldSplit = gameManager.playerData.grubsCollected == 1;
                    break;
                case SplitName.Grub2:
                    shouldSplit = gameManager.playerData.grubsCollected == 2;
                    break;
                case SplitName.Grub3:
                    shouldSplit = gameManager.playerData.grubsCollected == 3;
                    break;
                case SplitName.Grub4:
                    shouldSplit = gameManager.playerData.grubsCollected == 4;
                    break;
                case SplitName.Grub5:
                    shouldSplit = gameManager.playerData.grubsCollected == 5;
                    break;
                case SplitName.Grub6:
                    shouldSplit = gameManager.playerData.grubsCollected == 6;
                    break;
                case SplitName.Grub7:
                    shouldSplit = gameManager.playerData.grubsCollected == 7;
                    break;
                case SplitName.Grub8:
                    shouldSplit = gameManager.playerData.grubsCollected == 8;
                    break;
                case SplitName.Grub9:
                    shouldSplit = gameManager.playerData.grubsCollected == 9;
                    break;
                case SplitName.Grub10:
                    shouldSplit = gameManager.playerData.grubsCollected == 10;
                    break;
                case SplitName.Grub11:
                    shouldSplit = gameManager.playerData.grubsCollected == 11;
                    break;
                case SplitName.Grub12:
                    shouldSplit = gameManager.playerData.grubsCollected == 12;
                    break;
                case SplitName.Grub13:
                    shouldSplit = gameManager.playerData.grubsCollected == 13;
                    break;
                case SplitName.Grub14:
                    shouldSplit = gameManager.playerData.grubsCollected == 14;
                    break;
                case SplitName.Grub15:
                    shouldSplit = gameManager.playerData.grubsCollected == 15;
                    break;
                case SplitName.Grub16:
                    shouldSplit = gameManager.playerData.grubsCollected == 16;
                    break;
                case SplitName.Grub17:
                    shouldSplit = gameManager.playerData.grubsCollected == 17;
                    break;
                case SplitName.Grub18:
                    shouldSplit = gameManager.playerData.grubsCollected == 18;
                    break;
                case SplitName.Grub19:
                    shouldSplit = gameManager.playerData.grubsCollected == 19;
                    break;
                case SplitName.Grub20:
                    shouldSplit = gameManager.playerData.grubsCollected == 20;
                    break;
                case SplitName.Grub21:
                    shouldSplit = gameManager.playerData.grubsCollected == 21;
                    break;
                case SplitName.Grub22:
                    shouldSplit = gameManager.playerData.grubsCollected == 22;
                    break;
                case SplitName.Grub23:
                    shouldSplit = gameManager.playerData.grubsCollected == 23;
                    break;
                case SplitName.Grub24:
                    shouldSplit = gameManager.playerData.grubsCollected == 24;
                    break;
                case SplitName.Grub25:
                    shouldSplit = gameManager.playerData.grubsCollected == 25;
                    break;
                case SplitName.Grub26:
                    shouldSplit = gameManager.playerData.grubsCollected == 26;
                    break;
                case SplitName.Grub27:
                    shouldSplit = gameManager.playerData.grubsCollected == 27;
                    break;
                case SplitName.Grub28:
                    shouldSplit = gameManager.playerData.grubsCollected == 28;
                    break;
                case SplitName.Grub29:
                    shouldSplit = gameManager.playerData.grubsCollected == 29;
                    break;
                case SplitName.Grub30:
                    shouldSplit = gameManager.playerData.grubsCollected == 30;
                    break;
                case SplitName.Grub31:
                    shouldSplit = gameManager.playerData.grubsCollected == 31;
                    break;
                case SplitName.Grub32:
                    shouldSplit = gameManager.playerData.grubsCollected == 32;
                    break;
                case SplitName.Grub33:
                    shouldSplit = gameManager.playerData.grubsCollected == 33;
                    break;
                case SplitName.Grub34:
                    shouldSplit = gameManager.playerData.grubsCollected == 34;
                    break;
                case SplitName.Grub35:
                    shouldSplit = gameManager.playerData.grubsCollected == 35;
                    break;
                case SplitName.Grub36:
                    shouldSplit = gameManager.playerData.grubsCollected == 36;
                    break;
                case SplitName.Grub37:
                    shouldSplit = gameManager.playerData.grubsCollected == 37;
                    break;
                case SplitName.Grub38:
                    shouldSplit = gameManager.playerData.grubsCollected == 38;
                    break;
                case SplitName.Grub39:
                    shouldSplit = gameManager.playerData.grubsCollected == 39;
                    break;
                case SplitName.Grub40:
                    shouldSplit = gameManager.playerData.grubsCollected == 40;
                    break;
                case SplitName.Grub41:
                    shouldSplit = gameManager.playerData.grubsCollected == 41;
                    break;
                case SplitName.Grub42:
                    shouldSplit = gameManager.playerData.grubsCollected == 42;
                    break;
                case SplitName.Grub43:
                    shouldSplit = gameManager.playerData.grubsCollected == 43;
                    break;
                case SplitName.Grub44:
                    shouldSplit = gameManager.playerData.grubsCollected == 44;
                    break;
                case SplitName.Grub45:
                    shouldSplit = gameManager.playerData.grubsCollected == 45;
                    break;
                case SplitName.Grub46:
                    shouldSplit = gameManager.playerData.grubsCollected == 46;
                    break;

                case SplitName.GrubBasinDive:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Abyss_17";
                    break;
                case SplitName.GrubBasinWings:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Abyss_19";
                    break;
                case SplitName.GrubCityBelowLoveTower:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Ruins2_07";
                    break;
                case SplitName.GrubCityBelowSanctum:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Ruins1_05";
                    break;
                case SplitName.GrubCityCollector:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Ruins2_11";
                    break;
                case SplitName.GrubCityCollectorAll:
                    shouldSplit = gameManager.playerData.scenesGrubRescued.Contains("Ruins2_11");
                    break;
                case SplitName.GrubCityGuardHouse:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Ruins_House_01";
                    break;
                case SplitName.GrubCitySanctum:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Ruins1_32";
                    break;
                case SplitName.GrubCitySpire:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Ruins2_03";
                    break;
                case SplitName.GrubCliffsBaldurShell:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus1_28";
                    break;
                case SplitName.GrubCrossroadsAcid:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Crossroads_35";
                    break;
                case SplitName.GrubCrossroadsGuarded:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Crossroads_48";
                    break;
                case SplitName.GrubCrossroadsSpikes:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Crossroads_31";
                    break;
                case SplitName.GrubCrossroadsVengefly:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Crossroads_05";
                    break;
                case SplitName.GrubCrossroadsWall:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Crossroads_03";
                    break;
                case SplitName.GrubCrystalPeaksBottomLever:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Mines_04";
                    break;
                case SplitName.GrubCrystalPeaksCrown:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Mines_24";
                    break;
                case SplitName.GrubCrystalPeaksCrushers:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Mines_19";
                    break;
                case SplitName.GrubCrystalPeaksCrystalHeart:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Mines_31";
                    break;
                case SplitName.GrubCrystalPeaksMimics:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Mines_16";
                    break;
                case SplitName.GrubCrystalPeaksMound:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Mines_35";
                    break;
                case SplitName.GrubCrystalPeaksSpikes:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Mines_03";
                    break;
                case SplitName.GrubDeepnestBeastsDen:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Deepnest_Spider_Town";
                    break;
                case SplitName.GrubDeepnestDark:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Deepnest_39";
                    break;
                case SplitName.GrubDeepnestMimics:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Deepnest_36";
                    break;
                case SplitName.GrubDeepnestNosk:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Deepnest_31";
                    break;
                case SplitName.GrubDeepnestSpikes:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Deepnest_03";
                    break;
                case SplitName.GrubFogCanyonArchives:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus3_47";
                    break;
                case SplitName.GrubFungalBouncy:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus2_18";
                    break;
                case SplitName.GrubFungalSporeShroom:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus2_20";
                    break;
                case SplitName.GrubGreenpathCornifer:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus1_06";
                    break;
                case SplitName.GrubGreenpathHunter:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus1_07";
                    break;
                case SplitName.GrubGreenpathMossKnight:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus1_21";
                    break;
                case SplitName.GrubGreenpathVesselFragment:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus1_13";
                    break;
                case SplitName.GrubHiveExternal:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Hive_03";
                    break;
                case SplitName.GrubHiveInternal:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Hive_04";
                    break;
                case SplitName.GrubKingdomsEdgeCenter:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Deepnest_East_11";
                    break;
                case SplitName.GrubKingdomsEdgeOro:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Deepnest_East_14";
                    break;
                case SplitName.GrubQueensGardensBelowStag:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus3_10";
                    break;
                case SplitName.GrubQueensGardensUpper:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus3_22";
                    break;
                case SplitName.GrubQueensGardensWhiteLady:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Fungus3_48";
                    break;
                case SplitName.GrubRestingGroundsCrypts:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "RestingGrounds_10";
                    break;
                case SplitName.GrubWaterwaysCenter:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Waterways_04";
                    break;
                case SplitName.GrubWaterwaysHwurmps:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Waterways_14";
                    break;
                case SplitName.GrubWaterwaysIsma:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected) && sceneName == "Waterways_13";
                    break;

                case SplitName.Mimic1:
                    shouldSplit = gameManager.playerData.killsGrubMimic == 4;
                    break;
                case SplitName.Mimic2:
                    shouldSplit = gameManager.playerData.killsGrubMimic == 3;
                    break;
                case SplitName.Mimic3:
                    shouldSplit = gameManager.playerData.killsGrubMimic == 2;
                    break;
                case SplitName.Mimic4:
                    shouldSplit = gameManager.playerData.killsGrubMimic == 1;
                    break;
                case SplitName.Mimic5:
                    shouldSplit = gameManager.playerData.killsGrubMimic == 0;
                    break;

                case SplitName.TreeCity:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Ruins1_17");
                    break;
                case SplitName.TreeCliffs:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Cliffs_01");
                    break;
                case SplitName.TreeCrossroads:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Crossroads_07");
                    break;
                case SplitName.TreeDeepnest:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Deepnest_39");
                    break;
                case SplitName.TreeGlade:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("RestingGrounds_08");
                    break;
                case SplitName.TreeGreenpath:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Fungus1_13");
                    break;
                case SplitName.TreeHive:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Hive_02");
                    break;
                case SplitName.TreeKingdomsEdge:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Deepnest_East_07");
                    break;
                case SplitName.TreeLegEater:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Fungus2_33");
                    break;
                case SplitName.TreeMantisVillage:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Fungus2_17");
                    break;
                case SplitName.TreeMound:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Crossroads_ShamanTemple");
                    break;
                case SplitName.TreePeak:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Mines_23");
                    break;
                case SplitName.TreeQueensGardens:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Fungus3_11");
                    break;
                case SplitName.TreeRestingGrounds:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("RestingGrounds_05");
                    break;
                case SplitName.TreeWaterways:
                    shouldSplit = gameManager.playerData.scenesEncounteredDreamPlantC.Contains("Abyss_01");
                    break;

                case SplitName.Essence100:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 100;
                    break;
                case SplitName.Essence200:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 200;
                    break;
                case SplitName.Essence300:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 300;
                    break;
                case SplitName.Essence400:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 400;
                    break;
                case SplitName.Essence500:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 500;
                    break;
                case SplitName.Essence600:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 600;
                    break;
                case SplitName.Essence700:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 700;
                    break;
                case SplitName.Essence800:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 800;
                    break;
                case SplitName.Essence900:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 900;
                    break;
                case SplitName.Essence1000:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1000;
                    break;
                case SplitName.Essence1100:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1100;
                    break;
                case SplitName.Essence1200:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1200;
                    break;
                case SplitName.Essence1300:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1300;
                    break;
                case SplitName.Essence1400:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1400;
                    break;
                case SplitName.Essence1500:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1500;
                    break;
                case SplitName.Essence1600:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1600;
                    break;
                case SplitName.Essence1700:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1700;
                    break;
                case SplitName.Essence1800:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1800;
                    break;
                case SplitName.Essence1900:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 1900;
                    break;
                case SplitName.Essence2000:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 2000;
                    break;
                case SplitName.Essence2100:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 2100;
                    break;
                case SplitName.Essence2200:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 2200;
                    break;
                case SplitName.Essence2300:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 2300;
                    break;
                case SplitName.Essence2400:
                    shouldSplit = gameManager.playerData.dreamOrbs >= 2400;
                    break;

                case SplitName.KingsPass:
                    shouldSplit = sceneName.StartsWith("Tutorial_01") && nextScene.StartsWith("Town");
                    break;
                case SplitName.KingsPassEnterFromTown:
                    shouldSplit = sceneName.StartsWith("Town") && nextScene.StartsWith("Tutorial_01");
                    break;
                case SplitName.BlueLake:
                    shouldSplit = !sceneName.StartsWith("Crossroads_50") && nextScene.StartsWith("Crossroads_50");
                    break;
                case SplitName.CatacombsEntry:
                    shouldSplit = !sceneName.StartsWith("RestingGrounds_10") && nextScene.StartsWith("RestingGrounds_10");
                    break;

                case SplitName.VengeflyKingP:
                    shouldSplit = sceneName.StartsWith("GG_Vengefly") && nextScene.StartsWith("GG_Gruz_Mother");
                    break;
                case SplitName.GruzMotherP:
                    shouldSplit = sceneName.StartsWith("GG_Gruz_Mother") && nextScene.StartsWith("GG_False_Knight");
                    break;
                case SplitName.FalseKnightP:
                    shouldSplit = sceneName.StartsWith("GG_False_Knight") && nextScene.StartsWith("GG_Mega_Moss_Charger");
                    break;
                case SplitName.MassiveMossChargerP:
                    shouldSplit = sceneName.StartsWith("GG_Mega_Moss_Charger") && nextScene.StartsWith("GG_Hornet_1");
                    break;
                case SplitName.Hornet1P:
                    shouldSplit = sceneName.StartsWith("GG_Hornet_1") && (nextScene == "GG_Spa" || nextScene == "GG_Engine");
                    break;
                case SplitName.GorbP:
                    shouldSplit = sceneName.StartsWith("GG_Ghost_Gorb") && nextScene.StartsWith("GG_Dung_Defender");
                    break;
                case SplitName.DungDefenderP:
                    shouldSplit = sceneName.StartsWith("GG_Dung_Defender") && nextScene.StartsWith("GG_Mage_Knight");
                    break;
                case SplitName.SoulWarriorP:
                    shouldSplit = sceneName.StartsWith("GG_Mage_Knight") && nextScene.StartsWith("GG_Brooding_Mawlek");
                    break;
                case SplitName.BroodingMawlekP:
                    shouldSplit = sceneName.StartsWith("GG_Brooding_Mawlek") && (nextScene == "GG_Engine" || nextScene.StartsWith("GG_Nailmasters"));
                    break;
                case SplitName.OroMatoNailBrosP:
                    shouldSplit = sceneName.StartsWith("GG_Nailmasters") && (nextScene == "GG_End_Sequence" || nextScene == "GG_Spa");
                    break;

                case SplitName.XeroP:
                    shouldSplit = sceneName.StartsWith("GG_Ghost_Xero") && nextScene.StartsWith("GG_Crystal_Guardian");
                    break;
                case SplitName.CrystalGuardianP:
                    shouldSplit = sceneName.StartsWith("GG_Crystal_Guardian") && nextScene.StartsWith("GG_Soul_Master");
                    break;
                case SplitName.SoulMasterP:
                    shouldSplit = sceneName.StartsWith("GG_Soul_Master") && nextScene.StartsWith("GG_Oblobbles");
                    break;
                case SplitName.OblobblesP:
                    shouldSplit = sceneName.StartsWith("GG_Oblobbles") && nextScene.StartsWith("GG_Mantis_Lords");
                    break;
                case SplitName.MantisLordsP:
                    shouldSplit = sceneName.StartsWith("GG_Mantis_Lords") && nextScene == "GG_Spa";
                    break;
                case SplitName.MarmuP:
                    shouldSplit = sceneName.StartsWith("GG_Ghost_Marmu") && (nextScene.StartsWith("GG_Nosk") || nextScene.StartsWith("GG_Flukemarm"));
                    break;
                case SplitName.NoskP:
                    shouldSplit = sceneName.StartsWith("GG_Nosk") && nextScene.StartsWith("GG_Flukemarm");
                    break;
                case SplitName.FlukemarmP:
                    shouldSplit = sceneName.StartsWith("GG_Flukemarm") && nextScene.StartsWith("GG_Broken_Vessel");
                    break;
                case SplitName.BrokenVesselP:
                    shouldSplit = sceneName.StartsWith("GG_Broken_Vessel") && (nextScene == "GG_Engine" || nextScene.StartsWith("GG_Ghost_Galien"));
                    break;
                case SplitName.SheoPaintmasterP:
                    shouldSplit = sceneName.StartsWith("GG_Painter") && (nextScene == "GG_End_Sequence" || nextScene == "GG_Spa");
                    break;

                case SplitName.HiveKnightP:
                    shouldSplit = sceneName.StartsWith("GG_Hive_Knight") && nextScene.StartsWith("GG_Ghost_Hu");
                    break;
                case SplitName.ElderHuP:
                    shouldSplit = sceneName.StartsWith("GG_Ghost_Hu") && nextScene.StartsWith("GG_Collector");
                    break;
                case SplitName.CollectorP:
                    shouldSplit = sceneName.StartsWith("GG_Collector") && nextScene.StartsWith("GG_God_Tamer");
                    break;
                case SplitName.GodTamerP:
                    shouldSplit = sceneName.StartsWith("GG_God_Tamer") && nextScene.StartsWith("GG_Grimm");
                    break;
                case SplitName.TroupeMasterGrimmP:
                    shouldSplit = sceneName.StartsWith("GG_Grimm") && nextScene == "GG_Spa";
                    break;
                case SplitName.GalienP:
                    shouldSplit = sceneName.StartsWith("GG_Ghost_Galien") && (nextScene.StartsWith("GG_Grey_Prince_Zote") || nextScene.StartsWith("GG_Painter") || nextScene.StartsWith("GG_Uumuu"));
                    break;
                case SplitName.GreyPrinceZoteP:
                    shouldSplit = sceneName.StartsWith("GG_Grey_Prince_Zote") && (nextScene.StartsWith("GG_Uumuu") || nextScene.StartsWith("GG_Failed_Champion"));
                    break;
                case SplitName.UumuuP:
                    shouldSplit = sceneName.StartsWith("GG_Uumuu") && (nextScene.StartsWith("GG_Hornet_2") || nextScene.StartsWith("GG_Nosk_Hornet"));
                    break;
                case SplitName.Hornet2P:
                    shouldSplit = sceneName.StartsWith("GG_Hornet_2") && (nextScene == "GG_Engine" || nextScene == "GG_Spa");
                    break;
                case SplitName.SlyP:
                    shouldSplit = sceneName.StartsWith("GG_Sly") && (nextScene == "GG_End_Sequence" || nextScene.StartsWith("GG_Hornet_2"));
                    break;

                case SplitName.EnragedGuardianP:
                    shouldSplit = sceneName.StartsWith("GG_Crystal_Guardian_2") && nextScene.StartsWith("GG_Lost_Kin");
                    break;
                case SplitName.LostKinP:
                    shouldSplit = sceneName.StartsWith("GG_Lost_Kin") && nextScene.StartsWith("GG_Ghost_No_Eyes");
                    break;
                case SplitName.NoEyesP:
                    shouldSplit = sceneName.StartsWith("GG_Ghost_No_Eyes") && nextScene.StartsWith("GG_Traitor_Lord");
                    break;
                case SplitName.TraitorLordP:
                    shouldSplit = sceneName.StartsWith("GG_Traitor_Lord") && nextScene.StartsWith("GG_White_Defender");
                    break;
                case SplitName.WhiteDefenderP:
                    shouldSplit = sceneName.StartsWith("GG_White_Defender") && nextScene == "GG_Spa";
                    break;
                case SplitName.FailedChampionP:
                    shouldSplit = sceneName.StartsWith("GG_Failed_Champion") && (nextScene.StartsWith("GG_Ghost_Markoth") || nextScene.StartsWith("GG_Grimm_Nightmare"));
                    break;
                case SplitName.MarkothP:
                    shouldSplit = sceneName.StartsWith("GG_Ghost_Markoth") && (nextScene.StartsWith("GG_Watcher_Knights") || nextScene.StartsWith("GG_Grey_Prince_Zote") || nextScene.StartsWith("GG_Failed_Champion"));
                    break;
                case SplitName.WatcherKnightsP:
                    shouldSplit = sceneName.StartsWith("GG_Watcher_Knights") && (nextScene.StartsWith("GG_Soul_Tyrant") || nextScene.StartsWith("GG_Uumuu"));
                    break;
                case SplitName.SoulTyrantP:
                    shouldSplit = sceneName.StartsWith("GG_Soul_Tyrant") && (nextScene == "GG_Engine_Prime" || nextScene.StartsWith("GG_Ghost_Markoth"));
                    break;
                case SplitName.PureVesselP:
                    shouldSplit = sceneName.StartsWith("GG_Hollow_Knight") && (nextScene == "GG_End_Sequence" || nextScene.StartsWith("GG_Radiance") || nextScene == "GG_Door_5_Finale");
                    break;

                case SplitName.NoskHornetP:
                    shouldSplit = sceneName.StartsWith("GG_Nosk_Hornet") && nextScene.StartsWith("GG_Sly");
                    break;
                case SplitName.NightmareKingGrimmP:
                    shouldSplit = sceneName.StartsWith("GG_Grimm_Nightmare") && nextScene == "GG_Spa";
                    break;

                case SplitName.WhitePalaceOrb1:
                    shouldSplit = gameManager.playerData.whitePalaceOrb_1;
                    break;
                case SplitName.WhitePalaceOrb2:
                    shouldSplit = gameManager.playerData.whitePalaceOrb_2;
                    break;
                case SplitName.WhitePalaceOrb3:
                    shouldSplit = gameManager.playerData.whitePalaceOrb_3;
                    break;
                case SplitName.WhitePalaceSecretRoom:
                    shouldSplit = gameManager.playerData.whitePalaceSecretRoomVisited;
                    break;

                case SplitName.WhitePalaceLeftEntry:
                    shouldSplit = nextScene.StartsWith("White_Palace_04") && nextScene != sceneName;
                    break;
                case SplitName.WhitePalaceLeftWingMid:
                    shouldSplit = sceneName.StartsWith("White_Palace_04") && nextScene.StartsWith("White_Palace_14");
                    break;
                case SplitName.WhitePalaceRightEntry:
                    shouldSplit = nextScene.StartsWith("White_Palace_15") && nextScene != sceneName;
                    break;
                case SplitName.WhitePalaceRightClimb:
                    shouldSplit = sceneName.StartsWith("White_Palace_05") && nextScene.StartsWith("White_Palace_16");
                    break;
                case SplitName.WhitePalaceRightSqueeze:
                    shouldSplit = sceneName.StartsWith("White_Palace_16") && nextScene.StartsWith("White_Palace_05");
                    break;
                case SplitName.WhitePalaceRightDone:
                    shouldSplit = sceneName.StartsWith("White_Palace_05") && nextScene.StartsWith("White_Palace_15");
                    break;
                case SplitName.WhitePalaceTopEntry:
                    shouldSplit = sceneName.StartsWith("White_Palace_03_hub") && nextScene.StartsWith("White_Palace_06");
                    break;
                case SplitName.WhitePalaceTopClimb:
                    shouldSplit = sceneName.StartsWith("White_Palace_06") && nextScene.StartsWith("White_Palace_07");
                    break;
                case SplitName.WhitePalaceTopLeverRoom:
                    shouldSplit = sceneName.StartsWith("White_Palace_07") && nextScene.StartsWith("White_Palace_12");
                    break;
                case SplitName.WhitePalaceTopLastPlats:
                    shouldSplit = sceneName.StartsWith("White_Palace_12") && nextScene.StartsWith("White_Palace_13");
                    break;
                case SplitName.WhitePalaceThroneRoom:
                    shouldSplit = sceneName.StartsWith("White_Palace_13") && nextScene.StartsWith("White_Palace_09");
                    break;
                case SplitName.WhitePalaceAtrium:
                    shouldSplit = nextScene.StartsWith("White_Palace_03_hub") && nextScene != sceneName;
                    break;

                case SplitName.PathOfPainEntry:
                    shouldSplit = nextScene.StartsWith("White_Palace_18") && sceneName.StartsWith("White_Palace_06");
                    break;
                case SplitName.PathOfPainTransition1:
                    shouldSplit = nextScene.StartsWith("White_Palace_17") && sceneName.StartsWith("White_Palace_18");
                    break;
                case SplitName.PathOfPainTransition2:
                    shouldSplit = nextScene.StartsWith("White_Palace_19") && sceneName.StartsWith("White_Palace_17");
                    break;
                case SplitName.PathOfPainTransition3:
                    shouldSplit = nextScene.StartsWith("White_Palace_20") && sceneName.StartsWith("White_Palace_19");
                    break;

                case SplitName.WhiteFragmentLeft:
                    shouldSplit = gameManager.playerData.gotQueenFragment;
                    break;
                case SplitName.WhiteFragmentRight:
                    shouldSplit = gameManager.playerData.gotKingFragment;
                    break;

                // sit at benches
                case SplitName.BenchAny:
                    shouldSplit = gameManager.playerData.atBench;
                    break;
                /*
                case SplitName.BenchDirtmouth : shouldSplit = gameManager.playerData.atBench && sceneName == "Town"; break;
                case SplitName.BenchMato : shouldSplit = gameManager.playerData.atBench && sceneName == "Room_Naimlaster"; break;
                case SplitName.BenchCrossroadsHotsprings : shouldSplit = gameManager.playerData.atBench && sceneName == "Crossroads_30"; break;*/
                case SplitName.BenchCrossroadsStag:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "Crossroads_47";
                    break;/*
                case SplitName.BenchSalubra : shouldSplit = gameManager.playerData.atBench && sceneName == "Crossroads_04"; break;
                case SplitName.BenchAncestralMound : shouldSplit = gameManager.playerData.atBench && sceneName == "Crossroads_ShamanTemple"; break;
                case SplitName.BenchBlackEgg : shouldSplit = gameManager.playerData.atBench && sceneName == "Room_Final_Boss_Atrium"; break;
                case SplitName.BenchWaterfall : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus1_01b"; break;
                case SplitName.BenchStoneSanctuary : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus1_37"; break;
                case SplitName.BenchGreenpathToll : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus1_31"; break;*/
                case SplitName.BenchGreenpathStag:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus1_16_alt";
                    break;/*
                case SplitName.BenchLakeOfUnn : shouldSplit = gameManager.playerData.atBench && sceneName == "Room_Slug_Shrine"; break;
                case SplitName.BenchSheo : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus1_16"; break;
                case SplitName.BenchArchives : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus3_Archive"; break;*/
                case SplitName.BenchQueensStation:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus2_02";
                    break;/*
                case SplitName.BenchLegEater : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus2_26"; break;
                case SplitName.BenchBretta : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus2_13"; break;
                case SplitName.BenchMantisVillage : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus2_31"; break;
                case SplitName.BenchQuirrel : shouldSplit = gameManager.playerData.atBench && sceneName == "Ruins1_02"; break;
                case SplitName.BenchCityToll : shouldSplit = gameManager.playerData.atBench && sceneName == "Ruins1_31"; break;*/
                case SplitName.BenchStorerooms:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "Ruins1_29";
                    break;
                case SplitName.BenchSpire:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "Ruins1_18";
                    break;
                case SplitName.BenchSpireGHS:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "Ruins1_18" && gameManager.playerData.killsGreatShieldZombie < 10;
                    break;
                case SplitName.BenchKingsStation:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "Ruins2_08";
                    break;/*
                case SplitName.BenchPleasureHouse : shouldSplit = gameManager.playerData.atBench && sceneName == "Ruins_Bathhouse"; break;
                case SplitName.BenchWaterways : shouldSplit = gameManager.playerData.atBench && sceneName == "Waterways"; break;
                case SplitName.BenchDeepnestHotsprings : shouldSplit = gameManager.playerData.atBench && sceneName == "Deepnest_30"; break;
                case SplitName.BenchFailedTramway : shouldSplit = gameManager.playerData.atBench && sceneName == "Deepnest_14"; break;
                case SplitName.BenchDeepnestSpiderTown : shouldSplit = gameManager.playerData.atBench && sceneName == "Deepnest_Spider_Town"; break;
                case SplitName.BenchBasinToll : shouldSplit = gameManager.playerData.atBench && sceneName == "Abyss_18"; break;*/
                case SplitName.BenchHiddenStation:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "Abyss_22";
                    break;/*
                case SplitName.BenchOro : shouldSplit = gameManager.playerData.atBench && sceneName == "Deepnest_East_06"; break;
                case SplitName.BenchCamp : shouldSplit = gameManager.playerData.atBench && sceneName == "Deepnest_East_13"; break;
                case SplitName.BenchColosseum : shouldSplit = gameManager.playerData.atBench && sceneName.StartsWith("Room_Colosseum"); break;
                case SplitName.BenchHive : shouldSplit = gameManager.playerData.atBench && sceneName.StartsWith("Hive"); break;
                case SplitName.BenchDarkRoom : shouldSplit = gameManager.playerData.atBench && sceneName == "Mines_29"; break;
                case SplitName.BenchCG1 : shouldSplit = gameManager.playerData.atBench && sceneName == "Mines_18"; break;*/
                case SplitName.BenchRGStag:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "RestingGrounds_09";
                    break;/*
                case SplitName.BenchFlowerQuest : shouldSplit = gameManager.playerData.atBench && sceneName == "RestingGrounds_12"; break;
                case SplitName.BenchQGCornifer : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus1_24"; break;
                case SplitName.BenchQGToll : shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus3_50"; break;*/
                case SplitName.BenchQGStag:
                    shouldSplit = gameManager.playerData.atBench && sceneName == "Fungus3_40";
                    break;/*
                case SplitName.BenchTram : shouldSplit = gameManager.playerData.atBench && (sceneName == "Room_Tram" || sceneName == "Room_Tram_RG"); break;
                case SplitName.BenchWhitePalaceEntrance : shouldSplit = gameManager.playerData.atBench && sceneName == "White_Palace_01"; break;
                case SplitName.BenchWhitePalaceAtrium : shouldSplit = gameManager.playerData.atBench && sceneName == "White_Palace_03"; break;
                case SplitName.BenchWhitePalaceBalcony : shouldSplit = gameManager.playerData.atBench && sceneName == "White_Palace_06"; break;
                case SplitName.BenchGodhomeAtrium : shouldSplit = gameManager.playerData.atBench && sceneName == "GG_Atrium"; break;
                case SplitName.BenchHallOfGods : shouldSplit = gameManager.playerData.atBench && sceneName == "GG_Workshop"; break;                
                */

                // unlock toll benches
                case SplitName.TollBenchQG:
                    shouldSplit = gameManager.playerData.tollBenchQueensGardens;
                    break;
                case SplitName.TollBenchCity:
                    shouldSplit = gameManager.playerData.tollBenchCity;
                    break;
                case SplitName.TollBenchBasin:
                    shouldSplit = gameManager.playerData.tollBenchAbyss;
                    break;

                case SplitName.CityGateOpen:
                    shouldSplit = gameManager.playerData.openedCityGate;
                    break;
                case SplitName.CityGateAndMantisLords:
                    shouldSplit = gameManager.playerData.openedCityGate && gameManager.playerData.defeatedMantisLords;
                    break;
                case SplitName.NailsmithKilled:
                    shouldSplit = gameManager.playerData.nailsmithKilled;
                    break;
                case SplitName.NailsmithChoice:
                    shouldSplit = gameManager.playerData.nailsmithKilled;
                    shouldSkip = gameManager.playerData.nailsmithSpared;
                    break;

                /*
                 case SplitName.NailsmithSpared: shouldSplit = gameManager.playerData.nailsmithSpared); break;
            case SplitName.MageDoor: shouldSplit = gameManager.playerData.openedMageDoor); break;
            case SplitName.MageWindow: shouldSplit = gameManager.playerData.brokenMageWindow); break;
            case SplitName.MageLordEncountered: shouldSplit = gameManager.playerData.mageLordEncountered); break;
            case SplitName.MageDoor2: shouldSplit = gameManager.playerData.openedMageDoor_v2); break;
            case SplitName.MageWindowGlass: shouldSplit = gameManager.playerData.brokenMageWindowGlass); break;
            case SplitName.MageLordEncountered2: shouldSplit = gameManager.playerData.mageLordEncountered_2); break;
                */
                case SplitName.TramDeepnest:
                    shouldSplit = gameManager.playerData.openedTramLower;
                    break;
                case SplitName.WaterwaysManhole:
                    shouldSplit = gameManager.playerData.openedWaterwaysManhole;
                    break;
                case SplitName.NotchGrimm:
                    shouldSplit = gameManager.playerData.gotGrimmNotch;
                    break;
                //case SplitName.NotchSly1: shouldSplit = gameManager.playerData.slyNotch1); break;
                //case SplitName.NotchSly2: shouldSplit = gameManager.playerData.slyNotch2); break;
                case SplitName.SlyRescued:
                    shouldSplit = gameManager.playerData.slyRescued;
                    break;

                case SplitName.FlowerQuest:
                    shouldSplit = gameManager.playerData.xunFlowerGiven;
                    break;
                case SplitName.CityKey:
                    shouldSplit = gameManager.playerData.hasCityKey;
                    break;
                //case SplitName.Al2ba: shouldSplit = gameManager.playerData.killsLazyFlyer == 2; break;
                //case SplitName.Revek: shouldSplit = gameManager.playerData.gladeGhostsKilled == 19; break;
                //case SplitName.EquippedFragileHealth: shouldSplit = gameManager.playerData.equippedCharm_23); break;
                case SplitName.CanOvercharm:
                    shouldSplit = gameManager.playerData.canOvercharm;
                    break;
                case SplitName.MetGreyMourner:
                    shouldSplit = gameManager.playerData.metXun;
                    break;
                case SplitName.GreyMournerSeerAscended:
                    shouldSplit = gameManager.playerData.metXun && gameManager.playerData.mothDeparted;
                    break;
                case SplitName.HasDelicateFlower:
                    shouldSplit = gameManager.playerData.hasXunFlower;
                    break;

                //case SplitName.AreaTestingSanctum: shouldSplit = gameManager.playerData.currentArea == (int)MapZone.SOUL_SOCIETY; break;
                //case SplitName.AreaTestingSanctumUpper: shouldSplit = gameManager.playerData.currentArea == (int)MapZone.MAGE_TOWER; break;

                case SplitName.killedSanctumWarrior:
                    shouldSplit = gameManager.playerData.killedMageKnight;
                    break;
                case SplitName.killedSoulTwister:
                    shouldSplit = gameManager.playerData.killedMage;
                    break;

                case SplitName.EnterNKG:
                    shouldSplit = sceneName.StartsWith("Grimm_Main_Tent") && nextScene.StartsWith("Grimm_Nightmare");
                    break;
                case SplitName.EnterGreenpath:
                    shouldSplit = !sceneName.StartsWith("Fungus1_01") && nextScene.StartsWith("Fungus1_01");
                    break;
                case SplitName.EnterGreenpathWithOvercharm:
                    shouldSplit = !sceneName.StartsWith("Fungus1_01")
                        && nextScene.StartsWith("Fungus1_01")
                        && gameManager.playerData.canOvercharm;
                    break;
                case SplitName.EnterSanctum:
                    shouldSplit = !sceneName.StartsWith("Ruins1_23") && nextScene.StartsWith("Ruins1_23");
                    break;
                case SplitName.EnterSanctumWithShadeSoul:
                    shouldSplit = !sceneName.StartsWith("Ruins1_23")
                        && nextScene.StartsWith("Ruins1_23")
                        && gameManager.playerData.fireballLevel == 2;
                    break;
                case SplitName.EnterAnyDream:
                    shouldSplit = nextScene.StartsWith("Dream_") && nextScene != sceneName;
                    break;
                case SplitName.EnterGodhome:
                    shouldSplit = nextScene.StartsWith("GG_Atrium") && nextScene != sceneName;
                    break;
                case SplitName.DgateKingdomsEdgeAcid:
                    shouldSplit =
                        gameManager.playerData.dreamGateScene.StartsWith("Deepnest_East_04") &&
                        (gameManager.playerData.dreamGateX > 27.0f && gameManager.playerData.dreamGateX < 29f) &&
                        (gameManager.playerData.dreamGateY > 7.0f && gameManager.playerData.dreamGateY < 9f);
                    break;
                case SplitName.EnterJunkPit:
                    shouldSplit = nextScene.Equals("GG_Waterways") && nextScene != sceneName;
                    break;
                case SplitName.EnterDeepnest:
                    shouldSplit =
                        (nextScene.Equals("Fungus2_25") ||
                        nextScene.Equals("Deepnest_42") ||
                        nextScene.Equals("Abyss_03b") ||
                        nextScene.Equals("Deepnest_01b")) &&
                        nextScene != sceneName;
                    break;
                case SplitName.EnterCrown:
                    shouldSplit = nextScene.Equals("Mines_23") && nextScene != sceneName;
                    break;
                case SplitName.EnterDirtmouth:
                    shouldSplit = nextScene.Equals("Town") && nextScene != sceneName;
                    break;
                case SplitName.EnterRafters:
                    shouldSplit = nextScene.Equals("Ruins1_03") && nextScene != sceneName;
                    break;

                case SplitName.FailedChampionEssence:
                    shouldSplit = gameManager.playerData.falseKnightOrbsCollected;
                    break;
                case SplitName.SoulTyrantEssence:
                    shouldSplit = gameManager.playerData.mageLordOrbsCollected;
                    break;
                case SplitName.LostKinEssence:
                    shouldSplit = gameManager.playerData.infectedKnightOrbsCollected;
                    break;
                case SplitName.WhiteDefenderEssence:
                    shouldSplit = gameManager.playerData.whiteDefenderOrbsCollected;
                    break;
                case SplitName.GreyPrinceEssence:
                    shouldSplit = gameManager.playerData.greyPrinceOrbsCollected;
                    break;

                case SplitName.PreGrimmShop:
                    shouldSplit = gameManager.playerData.hasLantern
                        && gameManager.playerData.maxHealthBase == 6
                        && (gameManager.playerData.vesselFragments == 4 || (gameManager.playerData.MPReserveMax == 33 && gameManager.playerData.vesselFragments == 2));
                    break;
                case SplitName.PreGrimmShopTrans:
                    shouldSplit = gameManager.playerData.hasLantern
                        && gameManager.playerData.maxHealthBase == 6
                        && (gameManager.playerData.vesselFragments == 4 || (gameManager.playerData.MPReserveMax == 33 && gameManager.playerData.vesselFragments == 2))
                        && !sceneName.StartsWith("Room_shop");
                    break;
                case SplitName.ElderHuEssence:
                    shouldSplit = gameManager.playerData.elderHuDefeated == 2;
                    break;
                case SplitName.GalienEssence:
                    shouldSplit = gameManager.playerData.galienDefeated == 2;
                    break;
                case SplitName.GorbEssence:
                    shouldSplit = gameManager.playerData.aladarSlugDefeated == 2;
                    break;
                case SplitName.MarmuEssence:
                    shouldSplit = gameManager.playerData.mumCaterpillarDefeated == 2;
                    break;
                case SplitName.NoEyesEssence:
                    shouldSplit = gameManager.playerData.noEyesDefeated == 2;
                    break;
                case SplitName.XeroEssence:
                    shouldSplit = gameManager.playerData.xeroDefeated == 2;
                    break;
                case SplitName.MarkothEssence:
                    shouldSplit = gameManager.playerData.markothDefeated == 2;
                    break;

                case SplitName.DungDefenderIdol:
                    shouldSplit = store.CheckIncreased(Offset.trinket3, gameManager.playerData.trinket3) && sceneName.StartsWith("Waterways_15");
                    break;
                case SplitName.WaterwaysEntry:
                    shouldSplit = nextScene.StartsWith("Waterways_01") && nextScene != sceneName;
                    break;
                case SplitName.FogCanyonEntry:
                    shouldSplit = nextScene.StartsWith("Fungus3_26") && nextScene != sceneName;
                    break;
                case SplitName.FungalWastesEntry:
                    shouldSplit = (nextScene.StartsWith("Fungus2_06") // Room outside Leg Eater
                        || nextScene.StartsWith("Fungus2_03") // From Queens' Station
                        || nextScene.StartsWith("Fungus2_23") // Bretta from Waterways
                        || nextScene.StartsWith("Fungus2_20") // Spore Shroom room, from QG (this one's unlikely to come up)
                        ) && nextScene != sceneName;
                    break;
                case SplitName.SoulMasterEncountered:
                    shouldSplit = gameManager.playerData.mageLordEncountered;
                    break;

                case SplitName.CrystalMoundExit:
                    shouldSplit = sceneName.StartsWith("Mines_35") && nextScene != sceneName;
                    break;
                case SplitName.CrystalPeakEntry:
                    shouldSplit = (nextScene.StartsWith("Mines_02") || nextScene.StartsWith("Mines_10")) && nextScene != sceneName;
                    break;
                case SplitName.QueensGardensEntry:
                    shouldSplit = (nextScene.StartsWith("Fungus3_34") || nextScene.StartsWith("Deepnest_43")) && nextScene != sceneName;
                    break;
                case SplitName.BasinEntry:
                    shouldSplit = nextScene.StartsWith("Abyss_04") && nextScene != sceneName;
                    break;
                case SplitName.HiveEntry:
                    shouldSplit = nextScene.StartsWith("Hive_01") && nextScene != sceneName;
                    break;
                case SplitName.KingdomsEdgeEntry:
                    shouldSplit = nextScene.StartsWith("Deepnest_East_03") && nextScene != sceneName;
                    break;
                case SplitName.KingdomsEdgeOvercharmedEntry:
                    shouldSplit =
                        nextScene.StartsWith("Deepnest_East_03") &&
                        nextScene != sceneName &&
                        gameManager.playerData.overcharmed;
                    break;
                case SplitName.AllCharmNotchesLemm2CP:
                    shouldSplit =
                        gameManager.playerData.soldTrinket1 == 1 &&
                        gameManager.playerData.soldTrinket2 == 6 &&
                        gameManager.playerData.soldTrinket3 == 4;
                    break;
                case SplitName.HappyCouplePlayerDataEvent:
                    shouldSplit = gameManager.playerData.nailsmithConvoArt;
                    break;
                case SplitName.GodhomeBench:
                    shouldSplit = sceneName.StartsWith("GG_Spa") && sceneName != nextScene;
                    break;
                case SplitName.GodhomeLoreRoom:
                    shouldSplit =
                        (sceneName.StartsWith("GG_Engine") || sceneName.StartsWith("GG_Unn") || sceneName.StartsWith("GG_Wyrm"))
                        && sceneName != nextScene;
                    break;
                case SplitName.Menu:
                    shouldSplit = sceneName == "Menu_Title";
                    break;
                case SplitName.MenuClaw:
                    shouldSplit = gameManager.playerData.hasWalljump;
                    break;
                case SplitName.MenuGorgeousHusk:
                    shouldSplit = gameManager.playerData.killedGorgeousHusk;
                    break;
                case SplitName.TransClaw:
                    shouldSplit = gameManager.playerData.hasWalljump && nextScene != sceneName;
                    break;
                case SplitName.TransGorgeousHusk:
                    shouldSplit = gameManager.playerData.killedGorgeousHusk && nextScene != sceneName;
                    break;
                case SplitName.TransDescendingDark:
                    shouldSplit = gameManager.playerData.quakeLevel == 2 && nextScene != sceneName;
                    break;
                case SplitName.TransTear:
                    shouldSplit = gameManager.playerData.hasAcidArmour && nextScene != sceneName;
                    break;
                case SplitName.TransTearWithGrub:
                    shouldSplit =
                        gameManager.playerData.hasAcidArmour &&
                        gameManager.playerData.scenesGrubRescued.Contains("Waterways_13") &&

                        nextScene != sceneName;
                    break;
                case SplitName.PlayerDeath:
                    shouldSplit = gameManager.playerData.health == 0;
                    break;
                case SplitName.ShadeKilled:
                    shouldSplit = store.CheckToggledFalse(Offset.soulLimited, gameManager.playerData.soulLimited);
                    break;
                case SplitName.SlyShopFinished:
                    shouldSplit =
                        gameManager.playerData.vesselFragments == 8 || (gameManager.playerData.MPReserveMax == 66
                        && gameManager.playerData.vesselFragments == 2)
                        && !sceneName.StartsWith("Room_shop")
                        && gameManager.playerData.gotCharm_37;
                    break;
                case SplitName.ElegantKeyShoptimised:
                    shouldSplit = gameManager.playerData.maxHealthBase == 5 && gameManager.playerData.heartPieces == 1
                        && gameManager.playerData.hasWhiteKey;
                    break;
                case SplitName.CorniferAtHome:
                    shouldSplit = gameManager.playerData.corniferAtHome && sceneName.StartsWith("Town") && nextScene.StartsWith("Room_mapper");
                    break;

                case SplitName.AllSeals:
                    shouldSplit = gameManager.playerData.trinket2 + gameManager.playerData.soldTrinket2 == 17;
                    break;
                case SplitName.AllEggs:
                    shouldSplit = gameManager.playerData.rancidEggs + gameManager.playerData.jinnEggsSold == 21;
                    break;
                case SplitName.SlySimpleKey:
                    shouldSplit = gameManager.playerData.slySimpleKey;
                    break;
                case SplitName.AllBreakables:
                    shouldSplit = gameManager.playerData.brokenCharm_23 &&
                        gameManager.playerData.brokenCharm_24 &&
                        gameManager.playerData.brokenCharm_25;
                    break;
                case SplitName.AllUnbreakables:
                    shouldSplit = gameManager.playerData.fragileGreed_unbreakable &&
                        gameManager.playerData.fragileHealth_unbreakable &&
                        gameManager.playerData.fragileStrength_unbreakable;
                    break;

                case SplitName.MetEmilitia:
                    shouldSplit = gameManager.playerData.metEmilitia;
                    break;
                case SplitName.SavedCloth:
                    shouldSplit = gameManager.playerData.savedCloth;
                    break;
                case SplitName.MineLiftOpened:
                    shouldSplit = gameManager.playerData.mineLiftOpened;
                    break;
                case SplitName.mapDirtmouth:
                    shouldSplit = gameManager.playerData.mapDirtmouth;
                    break;
                case SplitName.mapCrossroads:
                    shouldSplit = gameManager.playerData.mapCrossroads;
                    break;
                case SplitName.mapGreenpath:
                    shouldSplit = gameManager.playerData.mapGreenpath;
                    break;
                case SplitName.mapFogCanyon:
                    shouldSplit = gameManager.playerData.mapFogCanyon;
                    break;
                case SplitName.mapRoyalGardens:
                    shouldSplit = gameManager.playerData.mapRoyalGardens;
                    break;
                case SplitName.mapFungalWastes:
                    shouldSplit = gameManager.playerData.mapFungalWastes;
                    break;
                case SplitName.mapCity:
                    shouldSplit = gameManager.playerData.mapCity;
                    break;
                case SplitName.mapWaterways:
                    shouldSplit = gameManager.playerData.mapWaterways;
                    break;
                case SplitName.mapMines:
                    shouldSplit = gameManager.playerData.mapMines;
                    break;
                case SplitName.mapDeepnest:
                    shouldSplit = gameManager.playerData.mapDeepnest;
                    break;
                case SplitName.mapCliffs:
                    shouldSplit = gameManager.playerData.mapCliffs;
                    break;
                case SplitName.mapOutskirts:
                    shouldSplit = gameManager.playerData.mapOutskirts;
                    break;
                case SplitName.mapRestingGrounds:
                    shouldSplit = gameManager.playerData.mapRestingGrounds;
                    break;
                case SplitName.mapAbyss:
                    shouldSplit = gameManager.playerData.mapAbyss;
                    break;
#if V1432
                case SplitName.givenGodseekerFlower:
                    shouldSplit = gameManager.playerData.givenGodseekerFlower;
                    break;
                case SplitName.givenOroFlower:
                    shouldSplit = gameManager.playerData.givenOroFlower;
                    break;
                case SplitName.givenWhiteLadyFlower:
                    shouldSplit = gameManager.playerData.givenWhiteLadyFlower;
                    break;
                case SplitName.givenEmilitiaFlower:
                    shouldSplit = gameManager.playerData.givenEmilitiaFlower;
                    break;
#endif
                case SplitName.KilledOblobbles:
                    shouldSplit = gameManager.playerData.killsOblobble == 1;
                    break;
                case SplitName.WhitePalaceEntry:
                    shouldSplit = nextScene.StartsWith("White_Palace_11") && nextScene != sceneName;
                    break;
                case SplitName.AnyTransition:
                    shouldSplit = ShouldSplitTransition(nextScene, sceneName);
                    break;
                case SplitName.RandoWake:
                    shouldSplit = gameManager.playerData.disablePause && gameManager.gameState == GameState.PLAYING && !menuingSceneNames.Contains(sceneName);
                    break;
                case SplitName.RidingStag:
                    shouldSplit = gameManager.playerData.travelling;
                    break;
                case SplitName.WhitePalaceLowerEntry:
                    shouldSplit = nextScene.StartsWith("White_Palace_01") && nextScene != sceneName;
                    break;
                case SplitName.WhitePalaceLowerOrb:
                    shouldSplit = nextScene.StartsWith("White_Palace_02") && nextScene != sceneName;
                    break;
                case SplitName.QueensGardensPostArenaTransition:
                    shouldSplit = nextScene.StartsWith("Fungus3_13") && nextScene != sceneName;
                    break;
                case SplitName.QueensGardensFrogsTrans:
                    shouldSplit = nextScene.StartsWith("Fungus1_23") && nextScene != sceneName;
                    break;
                case SplitName.Pantheon1to4Entry:
                    shouldSplit = nextScene.StartsWith("GG_Boss_Door_Entrance") && nextScene != sceneName;
                    break;
                case SplitName.Pantheon5Entry:
                    shouldSplit = nextScene.StartsWith("GG_Vengefly_V") && nextScene != sceneName;
                    break;

                case SplitName.OnObtainGhostMarissa:
                    shouldSplit = store.CheckIncremented(Offset.dreamOrbs, gameManager.playerData.dreamOrbs) && sceneName == "Ruins_Bathhouse";
                    break;
                case SplitName.OnObtainGhostCaelifFera:
                    shouldSplit = store.CheckIncremented(Offset.dreamOrbs, gameManager.playerData.dreamOrbs) && sceneName == "Fungus1_24";
                    break;
                case SplitName.OnObtainGhostPoggy:
                    shouldSplit = store.CheckIncremented(Offset.dreamOrbs, gameManager.playerData.dreamOrbs) && sceneName == "Ruins_Elevator";
                    break;
                case SplitName.OnObtainGhostGravedigger:
                    shouldSplit = store.CheckIncremented(Offset.dreamOrbs, gameManager.playerData.dreamOrbs) && sceneName == "Town";
                    break;
                case SplitName.OnObtainGhostJoni:
                    shouldSplit = store.CheckIncremented(Offset.dreamOrbs, gameManager.playerData.dreamOrbs) && sceneName == "Cliffs_05";
                    break;
                case SplitName.OnObtainGhostCloth:
                    shouldSplit = store.CheckIncremented(Offset.dreamOrbs, gameManager.playerData.dreamOrbs) && sceneName == "Fungus3_23" && store.TraitorLordDeadOnEntry;
                    break;
                case SplitName.OnObtainGhostVespa:
                    shouldSplit = store.CheckIncremented(Offset.dreamOrbs, gameManager.playerData.dreamOrbs) && sceneName == "Hive_05" && gameManager.playerData.gotCharm_29;
                    break;
                case SplitName.OnObtainGhostRevek:
                    if (sceneName == "RestingGrounds_08") {
                        shouldSplit = store.GladeEssence == 19 || store.GladeEssence == 18 && store.CheckIncremented(Offset.dreamOrbs, gameManager.playerData.dreamOrbs);
                    }
                    break;

                case SplitName.MaskShardMawlek:
                    if (sceneName == "Crossroads_09") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardGrubfather:
                    if (sceneName == "Crossroads_38") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardBretta:
                    if (sceneName == "Room_Bretta") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardQueensStation:
                    if (sceneName == "Fungus2_01") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardEnragedGuardian:
                    if (sceneName == "Mines_32") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardSeer:
                    if (sceneName == "RestingGrounds_07") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardGoam:
                    if (sceneName == "Crossroads_13") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardStoneSanctuary:
                    if (sceneName == "Fungus1_36") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardWaterways:
                    if (sceneName == "Waterways_04b") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardFungalCore:
                    if (sceneName == "Fungus2_25") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardHive:
                    if (sceneName == "Hive_04") { goto case SplitName.OnObtainMaskShard; }
                    break;
                case SplitName.MaskShardFlower:
                    if (sceneName == "Room_Mansion") { goto case SplitName.OnObtainMaskShard; }
                    break;

                case SplitName.VesselFragGreenpath:
                    if (sceneName == "Fungus1_13") { goto case SplitName.OnObtainVesselFragment; }
                    break;
                case SplitName.VesselFragCrossroadsLift:
                    if (sceneName == "Crossroads_37") { goto case SplitName.OnObtainVesselFragment; }
                    break;
                case SplitName.VesselFragKingsStation:
                    if (sceneName == "Ruins2_09") { goto case SplitName.OnObtainVesselFragment; }
                    break;
                case SplitName.VesselFragGarpedes:
                    if (sceneName == "Deepnest_38") { goto case SplitName.OnObtainVesselFragment; }
                    break;
                case SplitName.VesselFragStagNest:
                    if (sceneName == "Cliffs_03") { goto case SplitName.OnObtainVesselFragment; }
                    break;
                case SplitName.VesselFragSeer:
                    if (sceneName == "RestingGrounds_07") { goto case SplitName.OnObtainVesselFragment; }
                    break;
                case SplitName.VesselFragFountain:
                    if (sceneName == "Abyss_04") { goto case SplitName.OnObtainVesselFragment; }
                    break;

                case SplitName.OnObtainWanderersJournal:
                    shouldSplit = store.CheckIncremented(Offset.trinket1, gameManager.playerData.trinket1);
                    break;
                case SplitName.OnObtainHallownestSeal:
                    shouldSplit = store.CheckIncremented(Offset.trinket2, gameManager.playerData.trinket2);
                    break;
                case SplitName.OnObtainKingsIdol:
                    shouldSplit = store.CheckIncremented(Offset.trinket3, gameManager.playerData.trinket3);
                    break;
                case SplitName.ArcaneEgg8:
                    shouldSplit = gameManager.playerData.trinket4 == 8;
                    break;
                case SplitName.OnObtainArcaneEgg:
                    shouldSplit = store.CheckIncremented(Offset.trinket4, gameManager.playerData.trinket4);
                    break;
                case SplitName.OnObtainRancidEgg:
                    shouldSplit = store.CheckIncremented(Offset.rancidEggs, gameManager.playerData.rancidEggs);
                    break;
                case SplitName.OnObtainMaskShard:
                    shouldSplit = store.CheckIncremented(Offset.maxHealthBase, gameManager.playerData.maxHealthBase) || (store.CheckIncremented(Offset.heartPieces, gameManager.playerData.heartPieces) && gameManager.playerData.heartPieces < 4);
                    break;
                case SplitName.OnObtainVesselFragment:
                    shouldSplit = store.CheckIncreasedBy(Offset.MPReserveMax, 33, gameManager.playerData.MPReserveMax) || (store.CheckIncremented(Offset.vesselFragments, gameManager.playerData.vesselFragments) && gameManager.playerData.vesselFragments < 3);
                    break;
                case SplitName.OnObtainSimpleKey:
                    shouldSplit = store.CheckIncremented(Offset.simpleKeys, gameManager.playerData.simpleKeys);
                    break;
                case SplitName.OnUseSimpleKey:
                    shouldSplit = store.CheckIncreasedBy(Offset.simpleKeys, -1, gameManager.playerData.simpleKeys);
                    break;
                case SplitName.OnObtainGrub:
                    shouldSplit = store.CheckIncremented(Offset.grubsCollected, gameManager.playerData.grubsCollected);
                    break;
                case SplitName.OnObtainPaleOre:
                    shouldSplit = store.CheckIncremented(Offset.ore, gameManager.playerData.ore);
                    break;
                case SplitName.OnObtainWhiteFragment:
                    shouldSplit = store.CheckIncreased(Offset.royalCharmState, gameManager.playerData.royalCharmState);
                    break;
                case SplitName.OnDefeatGPZ:
                    shouldSplit = store.CheckIncremented(Offset.greyPrinceDefeats, gameManager.playerData.greyPrinceDefeats);
                    break;
                case SplitName.OnDefeatWhiteDefender:
                    shouldSplit = store.CheckIncremented(Offset.whiteDefenderDefeats, gameManager.playerData.whiteDefenderDefeats);
                    break;

                case SplitName.FlowerRewardGiven:
                    shouldSplit = gameManager.playerData.xunRewardGiven;
                    break;
                case SplitName.ColosseumBronzeUnlocked:
                    shouldSplit = gameManager.playerData.colosseumBronzeOpened;
                    break;
                case SplitName.ColosseumSilverUnlocked:
                    shouldSplit = gameManager.playerData.colosseumSilverOpened;
                    break;
                case SplitName.ColosseumGoldUnlocked:
                    shouldSplit = gameManager.playerData.colosseumGoldOpened;
                    break;
                case SplitName.ColosseumBronzeEntry:
                    shouldSplit = sceneName == "Room_Colosseum_01" && nextScene == "Room_Colosseum_Bronze";
                    break;
                case SplitName.ColosseumSilverEntry:
                    shouldSplit = sceneName == "Room_Colosseum_01" && nextScene == "Room_Colosseum_Silver";
                    break;
                case SplitName.ColosseumGoldEntry:
                    shouldSplit = sceneName == "Room_Colosseum_01" && nextScene == "Room_Colosseum_Gold";
                    break;
                case SplitName.ColosseumBronzeExit:
                    shouldSplit = gameManager.playerData.colosseumBronzeCompleted && !nextScene.StartsWith("Room_Colosseum_Bronze") && nextScene != sceneName;
                    break;
                case SplitName.ColosseumSilverExit:
                    shouldSplit = gameManager.playerData.colosseumSilverCompleted && !nextScene.StartsWith("Room_Colosseum_Silver") && nextScene != sceneName;
                    break;
                case SplitName.ColosseumGoldExit:
                    shouldSplit = gameManager.playerData.colosseumGoldCompleted && !nextScene.StartsWith("Room_Colosseum_Gold") && nextScene != sceneName;
                    break;
                case SplitName.SoulTyrantEssenceWithSanctumGrub:
                    shouldSplit = gameManager.playerData.mageLordOrbsCollected && gameManager.playerData.scenesGrubRescued.Contains("Ruins1_32");
                    break;
                case SplitName.EndingSplit:
                    shouldSplit = nextScene.StartsWith("Cinematic_Ending", StringComparison.OrdinalIgnoreCase) || nextScene == "GG_End_Sequence";
                    break;

                case SplitName.EnterHornet1:
                    shouldSplit = nextScene.StartsWith("Fungus1_04") && nextScene != sceneName;
                    break;
                case SplitName.EnterSoulMaster:
                    shouldSplit = nextScene.StartsWith("Ruins1_24") && nextScene != sceneName;
                    break;
                case SplitName.EnterHiveKnight:
                    shouldSplit = nextScene.StartsWith("Hive_05") && nextScene != sceneName;
                    break;
                case SplitName.EnterHornet2:
                    shouldSplit = nextScene.StartsWith("Deepnest_East_Hornet") && nextScene != sceneName;
                    break;
                case SplitName.EnterBroodingMawlek:
                    shouldSplit = nextScene.StartsWith("Crossroads_09") && nextScene != sceneName;
                    break;
                case SplitName.EnterTMG:
                    shouldSplit = nextScene.StartsWith("Grimm_Main_Tent") && nextScene != sceneName
                    && gameManager.playerData.grimmChildLevel == 2
                    && gameManager.playerData.flamesCollected == 3;
                    break;
                case SplitName.EnterLoveTower:
                    shouldSplit = nextScene.StartsWith("Ruins2_11") && nextScene != sceneName;
                    break;

                case SplitName.VengeflyKingTrans:
                    shouldSplit = gameManager.playerData.zoteRescuedBuzzer && nextScene != sceneName;
                    break;
                case SplitName.MegaMossChargerTrans:
                    shouldSplit = gameManager.playerData.megaMossChargerDefeated && nextScene != sceneName;
                    break;
                case SplitName.ElderHuTrans:
                    shouldSplit = gameManager.playerData.killedGhostHu && nextScene != sceneName;
                    break;
                case SplitName.BlackKnightTrans:
                    shouldSplit = gameManager.playerData.killedBlackKnight && nextScene != sceneName;
                    break;

                case SplitName.GladeIdol:
                    shouldSplit = store.CheckIncreased(Offset.trinket3, gameManager.playerData.trinket3) && sceneName.StartsWith("RestingGrounds_08");
                    break;
                case SplitName.AbyssDoor:
                    shouldSplit = gameManager.playerData.abyssGateOpened;
                    break;
                case SplitName.AbyssLighthouse:
                    shouldSplit = gameManager.playerData.abyssLighthouse;
                    break;
                case SplitName.LumaflyLanternTransition:
                    shouldSplit = gameManager.playerData.hasLantern && !sceneName.StartsWith("Room_shop");
                    break;

                // Spore Shroom : 17, Shape of Unn : 28, Quick Focus : 7, Baldur Shell : 5
                case SplitName.PureSnail:
                    shouldSplit = store.CheckIncreasedBy(Offset.health, 1, gameManager.playerData.health) &&
                        gameManager.playerData.equippedCharm_5 &&
                        gameManager.playerData.equippedCharm_7 &&
                        gameManager.playerData.equippedCharm_17 &&
                        gameManager.playerData.equippedCharm_28;
                    break;


                #region Trial of the Warrior
                case SplitName.Bronze1a: // 1 × Shielded Fool
                    shouldSplit = store.killsColShieldStart - gameManager.playerData.killsColShield == 1;
                    shouldSkip = gameManager.playerData.killsColShield == 0 || store.killsColShieldStart - gameManager.playerData.killsColShield > 1;
                    break;
                case SplitName.Bronze1b: // 2 × Shielded Fool
                    shouldSplit = store.killsColShieldStart - gameManager.playerData.killsColShield == 3;
                    shouldSkip = gameManager.playerData.killsColShield == 0 || store.killsColShieldStart - gameManager.playerData.killsColShield > 3;
                    break;
                case SplitName.Bronze1c: // 2 × Baldur
                    shouldSplit = store.killsColRollerStart - gameManager.playerData.killsColRoller == 2;
                    shouldSkip = gameManager.playerData.killsColRoller == 0 || store.killsColRollerStart - gameManager.playerData.killsColRoller > 2;
                    break;
                case SplitName.Bronze2: // 5 × Baldur
                    shouldSplit = store.killsColRollerStart - gameManager.playerData.killsColRoller == 7;
                    shouldSkip = gameManager.playerData.killsColRoller == 0 || store.killsColRollerStart - gameManager.playerData.killsColRoller > 7;
                    break;
                case SplitName.Bronze3a: // 1 × Sturdy Fool
                    shouldSplit = store.killsColMinerStart - gameManager.playerData.killsColMiner == 1;
                    shouldSkip = gameManager.playerData.killsColMiner == 0 || store.killsColMinerStart - gameManager.playerData.killsColMiner > 1;
                    break;
                case SplitName.Bronze3b: // 2 × Sturdy Fool
                    shouldSplit = store.killsColMinerStart - gameManager.playerData.killsColMiner == 3;
                    shouldSkip = gameManager.playerData.killsColMiner == 0 || store.killsColMinerStart - gameManager.playerData.killsColMiner > 3;
                    break;
                case SplitName.Bronze4: // 2 × Aspid
                    shouldSplit = store.killsSpitterStart - gameManager.playerData.killsSpitter == 2;
                    shouldSkip = gameManager.playerData.killsSpitter == 0 || store.killsSpitterStart - gameManager.playerData.killsSpitter > 2;
                    break;
                case SplitName.Bronze5: // 2 × Aspid
                    shouldSplit = store.killsSpitterStart - gameManager.playerData.killsSpitter == 4;
                    shouldSkip = gameManager.playerData.killsSpitter == 0 || store.killsSpitterStart - gameManager.playerData.killsSpitter > 4;
                    break;
                case SplitName.Bronze6: // 3 × Sturdy Fool
                    shouldSplit = store.killsColMinerStart - gameManager.playerData.killsColMiner == 6;
                    shouldSkip = gameManager.playerData.killsColMiner == 0 || store.killsColMinerStart - gameManager.playerData.killsColMiner > 6;
                    break;
                case SplitName.Bronze7: // 2 × Aspid, 2 × Baldur
                    shouldSplit =
                        store.killsSpitterStart - gameManager.playerData.killsSpitter == 6 &&
                        store.killsColRollerStart - gameManager.playerData.killsColRoller == 9;
                    shouldSkip = gameManager.playerData.killsSpitter == 0 || gameManager.playerData.killsColRoller == 0 ||
                        store.killsSpitterStart - gameManager.playerData.killsSpitter > 6 ||
                        store.killsColRollerStart - gameManager.playerData.killsColRoller > 9;
                    break;
                case SplitName.Bronze8a: // 4 × Vengefly
                    shouldSplit = store.killsBuzzerStart - gameManager.playerData.killsBuzzer == 4;
                    shouldSkip = gameManager.playerData.killsBuzzer == 0 || store.killsBuzzerStart - gameManager.playerData.killsBuzzer > 4;
                    break;
                case SplitName.Bronze8b: // 1 × Vengefly King
                    shouldSplit = store.killsBigBuzzerStart - gameManager.playerData.killsBigBuzzer == 1;
                    shouldSkip = gameManager.playerData.killsBigBuzzer == 0 || store.killsBigBuzzerStart - gameManager.playerData.killsBigBuzzer > 1;
                    break;
                case SplitName.Bronze9: // 3 × Sturdy Fool, 2 × Shielded Fool, 2 × Aspid, 2 × Baldur
                    shouldSplit =
                        store.killsSpitterStart - gameManager.playerData.killsSpitter == 8 &&
                        store.killsColRollerStart - gameManager.playerData.killsColRoller == 10 &&
                        store.killsColMinerStart - gameManager.playerData.killsColMiner == 9 &&
                        store.killsColShieldStart - gameManager.playerData.killsColShield == 5;
                    shouldSkip = gameManager.playerData.killsSpitter == 0 ||
                        gameManager.playerData.killsColRoller == 0 ||
                        gameManager.playerData.killsColMiner == 0 ||
                        gameManager.playerData.killsColShield == 0 ||
                        store.killsSpitterStart - gameManager.playerData.killsSpitter > 8 ||
                        store.killsColRollerStart - gameManager.playerData.killsColRoller > 10 ||
                        store.killsColMinerStart - gameManager.playerData.killsColMiner > 9 ||
                        store.killsColShieldStart - gameManager.playerData.killsColShield > 5;
                    break;
                case SplitName.Bronze10: // 3 × Baldur
                    shouldSplit = store.killsColRollerStart - gameManager.playerData.killsColRoller == 13;
                    shouldSkip = gameManager.playerData.killsColRoller == 0 || store.killsColRollerStart - gameManager.playerData.killsColRoller > 13;
                    break;
                case SplitName.Bronze11a: // 2 × Infected Gruzzer
                    shouldSplit = store.killsBurstingBouncerStart - gameManager.playerData.killsBurstingBouncer == 2;
                    shouldSkip = gameManager.playerData.killsBurstingBouncer == 0 || store.killsBurstingBouncerStart - gameManager.playerData.killsBurstingBouncer > 2;
                    break;
                case SplitName.Bronze11b: // 3 × Infected Gruzzer
                    shouldSplit = store.killsBurstingBouncerStart - gameManager.playerData.killsBurstingBouncer == 5;
                    shouldSkip = gameManager.playerData.killsBurstingBouncer == 0 || store.killsBurstingBouncerStart - gameManager.playerData.killsBurstingBouncer > 5;
                    break;
                case SplitName.BronzeEnd: // 2 × Gruz Mom
                    shouldSplit = store.killsBigFlyStart - gameManager.playerData.killsBigFly == 2;
                    shouldSkip =
                        gameManager.playerData.killsBigFly == 0 &&
                        sceneName.StartsWith("Room_Colosseum_Bronze") &&
                        nextScene != sceneName;
                    break;
                #endregion

                #region Trial of the Conqueror
                case SplitName.Silver1: // 2 × Heavy Fool, 3 × Winged Fool
                    shouldSplit =
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 2 &&
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 3;
                    shouldSkip =
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 2 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 3;
                    break;
                case SplitName.Silver2: // 2 × Squit
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 2;
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 2;
                    break;
                case SplitName.Silver3: // 2 × Squit
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 4;
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 4;
                    break;
                case SplitName.Silver4: // 1 × Squit, 1 × Winged Fool
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 5 &&
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 4;
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 5 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 4;
                    break;
                case SplitName.Silver5: // 2 × Aspid, 2 × Squit, 5 × Infected Gruzzer, not checking for aspid kills here because i think something weird is going on with their journal data stuff
                    shouldSplit =
                        store.killsBurstingBouncerStart - gameManager.playerData.killsBurstingBouncer == 5 &&
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 7;
                    shouldSkip =
                        gameManager.playerData.killsBurstingBouncer == 0 ||
                        gameManager.playerData.killsColMosquito == 0 ||
                        store.killsBurstingBouncerStart - gameManager.playerData.killsBurstingBouncer > 5 &&
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 7;
                    break;
                case SplitName.Silver6: // 1 × Heavy Fool, 3 × Belfly
                    shouldSplit =
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 3 &&
                        store.killsCeilingDropperStart - gameManager.playerData.killsCeilingDropper == 3;
                    shouldSkip =
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsCeilingDropper == 0 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 3 ||
                        store.killsCeilingDropperStart - gameManager.playerData.killsCeilingDropper > 3;
                    break;
                case SplitName.Silver7: // 1 × Belfly
                    shouldSplit =
                        store.killsCeilingDropperStart - gameManager.playerData.killsCeilingDropper == 4;
                    shouldSkip =
                        gameManager.playerData.killsCeilingDropper == 0 ||
                        store.killsCeilingDropperStart - gameManager.playerData.killsCeilingDropper > 4;
                    break;
                case SplitName.Silver8: // 8 × Hopper, 1 × Great Hopper
                    shouldSplit =
                        store.killsGiantHopperStart - gameManager.playerData.killsGiantHopper == 1; // only checking great hopper
                    shouldSkip =
                        gameManager.playerData.killsGiantHopper == 0 ||
                        store.killsGiantHopperStart - gameManager.playerData.killsGiantHopper > 1;
                    break;
                case SplitName.Silver9: // 1 × Great Hopper
                    shouldSplit =
                        store.killsGiantHopperStart - gameManager.playerData.killsGiantHopper == 2;
                    shouldSkip =
                        gameManager.playerData.killsGiantHopper == 0 ||
                        store.killsGiantHopperStart - gameManager.playerData.killsGiantHopper > 2;
                    break;
                case SplitName.Silver10: // 1 × Mimic
                    shouldSplit =
                        store.killsGrubMimicStart - gameManager.playerData.killsGrubMimic == 1;
                    shouldSkip =
                        gameManager.playerData.killsGrubMimic == 0 ||
                        store.killsGrubMimicStart - gameManager.playerData.killsGrubMimic > 1;
                    break;
                case SplitName.Silver11: // 2 × Shielded fool, 2 × Winged Fool, 1 × Heavy Fool, 2 × Squit
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 9 &&
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 6 &&
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 4;
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 9 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 6 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 4;
                    break;
                case SplitName.Silver12: // 1 × Heavy Fool, 1 × Winged Fool
                    shouldSplit =
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 7 &&
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 5;
                    shouldSkip =
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 7 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 5;
                    break;
                case SplitName.Silver13: // 1 × Winged Fool, 3 × Squit
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 12 &&
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 8;
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 12 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 8;
                    break;
                case SplitName.Silver14: // 3 × Winged Fool, 2 × Squit
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 14 &&
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 11;
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 14 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 11;
                    break;
                case SplitName.Silver15: // 9 × Obbles
                    shouldSplit =
                        store.killsBlobbleStart - gameManager.playerData.killsBlobble == 9;
                    shouldSkip =
                        store.killsBlobbleStart - gameManager.playerData.killsBlobble > 9 ||
                        gameManager.playerData.killsBlobble == 0;
                    break;
                case SplitName.Silver16: // 4 × Obbles
                    shouldSplit =
                        store.killsBlobbleStart - gameManager.playerData.killsBlobble == 13;
                    shouldSkip =
                        store.killsBlobbleStart - gameManager.playerData.killsBlobble > 13 ||
                        gameManager.playerData.killsBlobble == 0;
                    break;
                case SplitName.SilverEnd: // 2 × Oblobbles
                    shouldSplit =
                        store.killsOblobbleStart - gameManager.playerData.killsOblobble == 2;
                    shouldSkip =
                        gameManager.playerData.killsOblobble == 0 &&
                        sceneName.StartsWith("Room_Colosseum_Silver") &&
                        nextScene != sceneName;
                    break;
                #endregion

                #region Trial of the Fool
                case SplitName.Gold1:
                    #region
                    shouldSplit =
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 1 &&  // 1 Heavy Fool
                        store.killsColMinerStart - gameManager.playerData.killsColMiner == 1 &&  // 1 Sturdy Fool
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 2 &&  // 2 Squit
                        store.killsColShieldStart - gameManager.playerData.killsColShield == 2 &&  // 2 Shielded Fool
                        store.killsSpitterStart - gameManager.playerData.killsSpitter == 1 &&  // 1 Aspid
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 2 &&  // 2 Winged Fool
                        store.killsColRollerStart - gameManager.playerData.killsColRoller == 2;    // 2 Baldurs
                    shouldSkip =
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsColMiner == 0 ||
                        gameManager.playerData.killsColMosquito == 0 ||
                        gameManager.playerData.killsColShield == 0 ||
                        gameManager.playerData.killsSpitter == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        gameManager.playerData.killsColRoller == 0 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 1 ||
                        store.killsColMinerStart - gameManager.playerData.killsColMiner > 1 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 2 ||
                        store.killsColShieldStart - gameManager.playerData.killsColShield > 2 ||
                        store.killsSpitterStart - gameManager.playerData.killsSpitter > 1 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 2 ||
                        store.killsColRollerStart - gameManager.playerData.killsColRoller > 2;
                    break;
                #endregion
                // Wave 2 splits inconsistently since the enemies are killed by the spikes on the floor automatically
                case SplitName.Gold3:
                    #region
                    shouldSplit =
                        store.killsBlobbleStart - gameManager.playerData.killsBlobble == 3 &&  // 3 Obble
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 3 &&  // 1 Winged Fool
                        store.killsAngryBuzzerStart - gameManager.playerData.killsAngryBuzzer == 2;    // 2 Infected Vengefly
                    shouldSkip =
                        gameManager.playerData.killsBlobble == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        gameManager.playerData.killsAngryBuzzer == 0 ||
                        store.killsBlobbleStart - gameManager.playerData.killsBlobble > 3 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 3 ||
                        store.killsAngryBuzzerStart - gameManager.playerData.killsAngryBuzzer > 2;
                    break;
                #endregion
                case SplitName.Gold4:
                    #region
                    shouldSplit =
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 3 &&  // 2 Heavy Fool
                        store.killsCeilingDropperStart - gameManager.playerData.killsCeilingDropper == 6;    // 6 Belflies
                    shouldSkip =
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsCeilingDropper == 0 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 3 ||
                        store.killsCeilingDropperStart - gameManager.playerData.killsCeilingDropper > 6;
                    break;
                #endregion
                case SplitName.Gold5:
                    #region
                    shouldSplit =
                        store.killsColHopperStart - gameManager.playerData.killsColHopper == 3;    // 3 Loodle
                    shouldSkip =
                        gameManager.playerData.killsColHopper == 0 ||
                        store.killsColHopperStart - gameManager.playerData.killsColHopper > 3;
                    break;
                #endregion
                case SplitName.Gold6:
                    #region
                    shouldSplit =
                        store.killsColHopperStart - gameManager.playerData.killsColHopper == 8;    // 5 Loodle
                    shouldSkip =
                        gameManager.playerData.killsColHopper == 0 ||
                        store.killsColHopperStart - gameManager.playerData.killsColHopper > 8;
                    break;
                #endregion
                case SplitName.Gold7:
                    #region
                    shouldSplit =
                        store.killsColHopperStart - gameManager.playerData.killsColHopper == 11;   // 3 Loodle
                    shouldSkip =
                        gameManager.playerData.killsColHopper == 0 ||
                        store.killsColHopperStart - gameManager.playerData.killsColHopper > 11;
                    break;
                #endregion
                case SplitName.Gold8a:
                    #region
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 4 &&  // 2 Squit
                        store.killsSpitterStart - gameManager.playerData.killsSpitter == 5 &&  // 3 Aspid
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 4;    // 1 Winged Fool
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        gameManager.playerData.killsSpitter == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 4 ||
                        store.killsSpitterStart - gameManager.playerData.killsSpitter > 5 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 4;
                    break;
                #endregion
                case SplitName.Gold8:
                    #region
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 6 &&  // 2 Squit
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 5;    // 1 Winged Fool
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 6 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 5;
                    break;
                #endregion
                case SplitName.Gold9a:
                    #region
                    shouldSplit =
                        store.killsColShieldStart - gameManager.playerData.killsColShield == 3 &&  // 1 Shielded Fool
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 5 &&  // 2 Heavy Fool
                        store.killsSpitterStart - gameManager.playerData.killsSpitter == 6 &&  // 1 Aspid
                        store.killsHeavyMantisStart - gameManager.playerData.killsHeavyMantis == 2 &&  // 2 Mantis Traitor
                        store.killsHeavyMantisFlyerStart - gameManager.playerData.killsMantisHeavyFlyer == 4;    // 4 Mantis Petra
                    shouldSkip =
                        gameManager.playerData.killsColShield == 0 ||
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsSpitter == 0 ||
                        gameManager.playerData.killsHeavyMantis == 0 ||
                        gameManager.playerData.killsMantisHeavyFlyer == 0 ||
                        store.killsColShieldStart - gameManager.playerData.killsColShield > 3 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 5 ||
                        store.killsSpitterStart - gameManager.playerData.killsSpitter > 6 ||
                        store.killsHeavyMantisStart - gameManager.playerData.killsHeavyMantis > 2 ||
                        store.killsHeavyMantisFlyerStart - gameManager.playerData.killsMantisHeavyFlyer > 4;
                    break;
                #endregion
                case SplitName.Gold9b:
                    #region
                    shouldSplit =
                        store.killsMageKnightStart - gameManager.playerData.killsMageKnight == 1;    // 1 Soul Warrior
                    shouldSkip =
                        gameManager.playerData.killsMageKnight == 0 ||
                        store.killsMageKnightStart - gameManager.playerData.killsMageKnight > 1;
                    break;
                #endregion
                case SplitName.Gold10:
                    #region
                    shouldSplit =
                        store.killsElectricMageStart - gameManager.playerData.killsElectricMage == 3 &&  // 3 Volt Twister
                        store.killsMageStart - gameManager.playerData.killsMage == 4;    // 2 Soul Twister
                    shouldSkip =
                        gameManager.playerData.killsElectricMage == 0 ||
                        gameManager.playerData.killsMage == 0 ||
                        store.killsElectricMageStart - gameManager.playerData.killsElectricMage > 3 ||
                        store.killsMageStart - gameManager.playerData.killsMage > 4;
                    break;
                #endregion
                case SplitName.Gold11:
                    #region
                    shouldSplit =
                        store.killsMageKnightStart - gameManager.playerData.killsMageKnight == 2 &&  // 1 Soul Warrior
                        store.killsMageStart - gameManager.playerData.killsMage == 5;    // 1 Soul Twister
                    shouldSkip =
                        gameManager.playerData.killsMageKnight == 0 ||
                        store.killsMageKnightStart - gameManager.playerData.killsMageKnight > 2 ||
                        store.killsMageStart - gameManager.playerData.killsMage > 5;
                    break;
                #endregion
                case SplitName.Gold12a:
                    #region
                    shouldSplit =
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 7 &&  // 2 Winged Fool
                        store.killsColMinerStart - gameManager.playerData.killsColMiner == 4 &&  // 1 Sturdy Fool
                        store.killsLesserMawlekStart - gameManager.playerData.killsLesserMawlek == 4;    // 4 Lesser Mawlek
                    shouldSkip =
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        gameManager.playerData.killsColMiner == 0 ||
                        gameManager.playerData.killsLesserMawlek == 0 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 7 ||
                        store.killsColMinerStart - gameManager.playerData.killsColMiner > 4 ||
                        store.killsLesserMawlekStart - gameManager.playerData.killsLesserMawlek > 4;
                    break;
                #endregion
                case SplitName.Gold12b:
                    #region
                    shouldSplit =
                        store.killsMawlekStart - gameManager.playerData.killsMawlek == 1;    // 1 Brooding Mawlek
                    shouldSkip =
                        gameManager.playerData.killsMawlek == 0 ||
                        store.killsMawlekStart - gameManager.playerData.killsMawlek > 1;
                    break;
                #endregion
                // Wave 13 doesn't really exist, it's just vertical Garpedes so there's nothing to Split on
                case SplitName.Gold14a:
                    #region
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 10 && // 1 Squit
                        store.killsSpitterStart - gameManager.playerData.killsSpitter == 7 &&  // 1 Aspid
                        store.killsHeavyMantisFlyerStart - gameManager.playerData.killsMantisHeavyFlyer == 5;    // 1 Mantis Petra
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        gameManager.playerData.killsMantisHeavyFlyer == 0 ||
                        gameManager.playerData.killsSpitter == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 10 ||
                        store.killsHeavyMantisFlyerStart - gameManager.playerData.killsMantisHeavyFlyer > 5 ||
                        store.killsSpitterStart - gameManager.playerData.killsSpitter - 1 > 7;
                    break;
                #endregion
                case SplitName.Gold14b:
                    #region
                    shouldSplit =
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 10 && // 2 Winged Fool
                        store.killsBlobbleStart - gameManager.playerData.killsBlobble == 7;    // 4 Obble
                    shouldSkip =
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        gameManager.playerData.killsBlobble == 0 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 10 ||
                        store.killsBlobbleStart - gameManager.playerData.killsBlobble > 7;
                    break;
                #endregion
                case SplitName.Gold15:
                    #region
                    shouldSplit =
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 12;    // 2 Squit
                    shouldSkip =
                        gameManager.playerData.killsColMosquito == 0 ||
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito > 12;
                    break;
                #endregion
                case SplitName.Gold16:
                    #region
                    shouldSplit =
                        store.killsColHopperStart - gameManager.playerData.killsColHopper == 25;    // 14 Loodle elderC
                    shouldSkip =
                        gameManager.playerData.killsColHopper == 0 ||
                        store.killsColHopperStart - gameManager.playerData.killsColHopper > 25;
                    break;
                #endregion
                case SplitName.Gold17a:
                    #region
                    shouldSplit =
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 6 &&  // 1 Heavy Fool
                        store.killsColMinerStart - gameManager.playerData.killsColMiner == 5 &&  // 1 Sturdy Fool
                        store.killsColShieldStart - gameManager.playerData.killsColShield == 4 &&  // 1 Shielded Fool
                        store.killsHeavyMantisFlyerStart - gameManager.playerData.killsMantisHeavyFlyer == 6 &&  // 1 Mantis Petra
                        store.killsHeavyMantisStart - gameManager.playerData.killsHeavyMantis == 3 &&  // 1 Mantis Traitor
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 11;   // 1 Winged Fool
                    shouldSkip =
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsColMiner == 0 ||
                        gameManager.playerData.killsColShield == 0 ||
                        gameManager.playerData.killsMantisHeavyFlyer == 0 ||
                        gameManager.playerData.killsHeavyMantis == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 6 ||
                        store.killsColMinerStart - gameManager.playerData.killsColMiner > 5 ||
                        store.killsColShieldStart - gameManager.playerData.killsColShield > 4 ||
                        store.killsHeavyMantisFlyerStart - gameManager.playerData.killsMantisHeavyFlyer - 1 > 6 ||
                        store.killsHeavyMantisStart - gameManager.playerData.killsHeavyMantis > 3 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry - 1 > 11;
                    break;
                #endregion
                case SplitName.Gold17b:
                    #region
                    shouldSplit =
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 7 &&  // 1 Heavy Fool
                        store.killsColShieldStart - gameManager.playerData.killsColShield == 5 &&  // 1 Shielded Fool
                        store.killsMageStart - gameManager.playerData.killsMage == 6 &&  // 1 Soul Twister
                        store.killsElectricMageStart - gameManager.playerData.killsElectricMage == 4;    // 1 Volt Twister
                    shouldSkip =
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsColShield == 0 ||
                        gameManager.playerData.killsMage == 0 ||
                        gameManager.playerData.killsElectricMage == 0 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 7 ||
                        store.killsColShieldStart - gameManager.playerData.killsColShield > 5 ||
                        store.killsMageStart - gameManager.playerData.killsMage > 6 ||
                        store.killsElectricMageStart - gameManager.playerData.killsElectricMage > 4;
                    break;
                #endregion
                case SplitName.Gold17c:
                    #region
                    shouldSplit =
                        store.killsColRollerStart - gameManager.playerData.killsColRoller == 4 &&  // 2 Baldur
                        store.killsColMosquitoStart - gameManager.playerData.killsColMosquito == 14 && // 2 Squit
                        store.killsColWormStart - gameManager.playerData.killsColWorm == 8 &&  // 1 Heavy Fool
                        store.killsColShieldStart - gameManager.playerData.killsColShield == 6 &&  // 1 Shielded Fool
                        store.killsColMinerStart - gameManager.playerData.killsColMiner == 6 &&  // 1 Sturdy Fool
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry == 12;   // 1 Winged Fool
                    shouldSkip =
                        gameManager.playerData.killsColWorm == 0 ||
                        gameManager.playerData.killsColShield == 0 ||
                        gameManager.playerData.killsColMiner == 0 ||
                        gameManager.playerData.killsColFlyingSentry == 0 ||
                        gameManager.playerData.killsColRoller == 0 ||
                        gameManager.playerData.killsColShield == 0 ||
                        store.killsColRollerStart - gameManager.playerData.killsColRoller > 4 ||
                        store.killsColShieldStart - gameManager.playerData.killsColShield > 14 ||
                        store.killsColWormStart - gameManager.playerData.killsColWorm > 8 ||
                        store.killsColShieldStart - gameManager.playerData.killsColShield > 6 ||
                        store.killsColMinerStart - gameManager.playerData.killsColMiner > 6 ||
                        store.killsColFlyingSentryStart - gameManager.playerData.killsColFlyingSentry > 12;
                    break;
                #endregion
                case SplitName.GoldEnd:
                    #region
                    shouldSplit =
                        store.killsLobsterLancerStart - gameManager.playerData.killsLobsterLancer == 1;    // God Tamer
                    shouldSkip =
                        gameManager.playerData.killsLobsterLancer == 0 &&
                        sceneName.StartsWith("Room_Colosseum_Gold") &&
                        nextScene != sceneName;
                    break;
                #endregion
                #endregion

                default:
                    break;
            }
            return shouldSplit;
        }

        public static void OnInit(GameManager gameManager) {
            store = new HollowKnightStoredData(gameManager);
        }

        public static void OnPreRender(GameManager gameManager, StringBuilder infoBuilder) {
            string currentScene = gameManager.sceneName;
            string nextScene = gameManager.nextSceneName;
            GameState gameState = gameManager.gameState;

            if (Input.GetKeyDown(KeyCode.P)) {
                isPaused = !isPaused;
            }

            if (!timeStart && (nextScene.Equals("Tutorial_01", StringComparison.OrdinalIgnoreCase) && gameState == GameState.ENTERING_LEVEL ||
                               nextScene is "GG_Vengefly_V" or "GG_Boss_Door_Entrance" or "GG_Entrance_Cutscene" ||
                               HeroController.instance != null)) {
                timeStart = true;
                ref Split refSplit = ref SplitReader.SplitList.ElementAt(currentSplitIndex).SplitRef;
                refSplit.StartSplitTimer(inGameTime);
            }

            if (timeStart && !timeEnd && (nextScene.StartsWith("Cinematic_Ending", StringComparison.OrdinalIgnoreCase) ||
                                          nextScene == "GG_End_Sequence") || SplitLastSplit) {
                timeEnd = true;
            }

            bool timePaused = false;

            // thanks ShootMe, in game time logic copy from https://github.com/ShootMe/LiveSplit.HollowKnight
            try {
                UIState uiState = gameManager.ui.uiState;
                bool loadingMenu = currentScene != "Menu_Title" && string.IsNullOrEmpty(nextScene) ||
                                   currentScene != "Menu_Title" && nextScene == "Menu_Title";
                if (gameState == GameState.PLAYING && lastGameState == GameState.MAIN_MENU) {
                    lookForTeleporting = true;
                }

                bool teleporting = (bool)TeleportingFieldInfo.GetValue(gameManager.cameraCtrl);
                if (lookForTeleporting && (teleporting || gameState != GameState.PLAYING && gameState != GameState.ENTERING_LEVEL)) {
                    lookForTeleporting = false;
                }

                timePaused =
                    gameState == GameState.PLAYING && teleporting && gameManager.hero_ctrl?.cState.hazardRespawning == false
                    || lookForTeleporting
                    || gameState is GameState.PLAYING or GameState.ENTERING_LEVEL && uiState != UIState.PLAYING
                    || gameState != GameState.PLAYING && !gameManager.inputHandler.acceptingInput
                    || gameState is GameState.EXITING_LEVEL or GameState.LOADING
                    || gameManager.hero_ctrl?.transitionState == HeroTransitionState.WAITING_TO_ENTER_LEVEL
                    || uiState != UIState.PLAYING &&
                    (loadingMenu || uiState != UIState.PAUSED && (!string.IsNullOrEmpty(nextScene) || currentScene == "_test_charms")) &&
                    nextScene != currentScene
                    || minorVersion < 3 && (bool)TilemapDirtyFieldInfo.GetValue(gameManager)
                    || ConfigManager.PauseTimer
                    || isPaused;
            } catch {
                // ignore
            }

            lastGameState = gameState;
            ref Split splitRef = ref SplitReader.SplitList.ElementAt(currentSplitIndex).SplitRef;

            if (timeStart && !timePaused && !timeEnd) {
                inGameTime += Time.unscaledDeltaTime;
                splitRef.IncreaseTimer(Time.unscaledDeltaTime);
            }
            if (!SplitLastSplit && CheckSplit(gameManager, splitRef.SplitTrigger, gameManager.nextSceneName, gameManager.sceneName)) {
                if (currentSplitIndex < SplitReader.SplitList.Count-1) {
                    currentSplitIndex++;
                    ref Split newSplitRef = ref SplitReader.SplitList.ElementAt(currentSplitIndex).SplitRef;
                    newSplitRef.StartSplitTimer(inGameTime);
                } else {
                    SplitLastSplit = true;
                }
            }
            if (inGameTime > 0 && ConfigManager.ShowSplits) {
                infoBuilder.AppendLine(FormattedTime);
            }
        }
    }
}
