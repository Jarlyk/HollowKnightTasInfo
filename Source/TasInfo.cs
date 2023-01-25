using System;
using System.Text;
using UnityEngine;

namespace Assembly_CSharp.TasInfo.mm.Source {
    // ReSharper disable once UnusedType.Global
    public static class TasInfo {
        private static bool init;

        // 用于测试
        // ReSharper disable once MemberCanBePrivate.Global
        public static string AdditionalInfo = string.Empty;

        // ReSharper disable once UnusedMember.Global
        // CameraController.OnPreRender
        public static void OnPreRender() {
            if (GameManager.instance is not { } gameManager) {
                return;
            }

            AdditionalInfo = string.Empty;

            StringBuilder infoBuilder = new();

            try {
                DesyncChecker.BeforeUpdate();
                if (!init) {
                    init = true;
                    OnInit(gameManager);
                }

                OnPreRender(gameManager, infoBuilder);

                DesyncChecker.AfterUpdate(infoBuilder);
            } catch (Exception e) {
                Debug.LogException(e);
            }

            patch_GameManager.TasInfo = infoBuilder.AppendLine(AdditionalInfo).ToString();
        }

        // ReSharper disable once UnusedMember.Global
        // CameraController.OnPostRender
        public static void OnPostRender() {
            if (GameManager.instance is not { } gameManager) {
                return;
            }

            CameraManager.OnPostRender(gameManager);
        }

        // ReSharper disable once UnusedMember.Global
        // PlayMakerUnity2DProxy.start()
        public static void OnColliderCreate(GameObject gameObject) {
            HitboxInfo.TryAddHitbox(gameObject);
            EnemyInfo.TryAddEnemy(gameObject);
        }

        // 重叠房间加载后重新采集数据
#if V1028 || V1028_KRYTHOM || V1037
        public static void AfterManualLevelStart() {
            EnemyInfo.RefreshInfo(false);
            HitboxInfo.RefreshInfo(false);
        }
#endif

        private static void OnInit(GameManager gameManager) {
            ConfigManager.OnPreRender();

            EnemyInfo.OnInit();
            CustomInfo.OnInit();
            HitboxInfo.OnInit();
            RngInfo.OnInit();
            DiagnosticsLogger.OnInit();
            RandomInjection.Init();
            SplitReader.OnInit();
            AutoSplit.OnInit(gameManager);
        }

        private static void OnPreRender(GameManager gameManager, StringBuilder infoBuilder) {
            // 放第一位，先更新 settings
            ConfigManager.OnPreRender();

            // 放第二位，先处理镜头之后 camera.WorldToScreenPoint 才能获得正确数据
            CameraManager.OnPreRender(gameManager);
            HeroInfo.OnPreRender(gameManager, infoBuilder);
            CustomInfo.OnPreRender(gameManager, infoBuilder);
            AutoSplit.OnPreRender(gameManager, infoBuilder);
            TimeInfo.OnPreRender(gameManager, infoBuilder);
            EnemyInfo.OnPreRender(gameManager, infoBuilder);
            HitboxInfo.OnPreRender(gameManager, infoBuilder);
            RngInfo.OnPreRender(infoBuilder);
            DiagnosticsLogger.OnPreRender();
        }
    }
}