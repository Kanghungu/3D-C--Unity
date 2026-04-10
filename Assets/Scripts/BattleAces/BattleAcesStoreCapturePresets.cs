using Game.CameraSystem;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 스토어 스크린샷·트레일러 캡처용 RTS 카메라 프리셋(에디터/개발에서 F8·F9).
    /// </summary>
    public static class BattleAcesStoreCapturePresets
    {
        public enum PresetKind
        {
            /// <summary>전장 중앙·줌 아웃</summary>
            BattlefieldOverview = 0,

            /// <summary>아군 코어 근접·전투 실루엣</summary>
            PlayerCoreClose = 1,
        }

        /// <summary>
        /// 현재 매치 기준으로 프리셋 적용. RTS 가 없거나 씬이 전투가 아니면 false.
        /// </summary>
        public static bool TryApply(PresetKind kind)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return false;
            }

            RTSCameraController rts = cam.GetComponent<RTSCameraController>();
            if (rts == null)
            {
                return false;
            }

            Vector3 focus = Vector3.zero;
            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController match))
            {
                if (match.PlayerCore != null && match.EnemyCore != null)
                {
                    focus = (match.PlayerCore.transform.position + match.EnemyCore.transform.position) * 0.5f;
                }
                else if (match.PlayerCore != null)
                {
                    focus = match.PlayerCore.transform.position;
                }
            }

            switch (kind)
            {
                case PresetKind.BattlefieldOverview:
                    rts.ApplyPresentationView(focus, 88f);
                    return true;
                case PresetKind.PlayerCoreClose:
                    if (BattleAcesCore.TryFindAliveCore(UnitTeam.Player, out BattleAcesCore core))
                    {
                        Vector3 p = core.transform.position;
                        // 살짝 비스듬히 — 정면 큐브만 보이지 않게
                        Vector3 lookAt = p + new Vector3(14f, 0f, 11f);
                        rts.ApplyPresentationView(lookAt, 26f);
                    }
                    else
                    {
                        rts.ApplyPresentationView(focus, 32f);
                    }

                    return true;
                default:
                    return false;
            }
        }
    }
}
