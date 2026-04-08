using Game.CameraSystem;
using Game.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>
    /// 데모 편의: 집결(랠리) 지점으로 카메라 시야 이동 등 — 전투 중·브리핑 아닐 때만.
    /// </summary>
    public sealed class BattleAcesQolHotkeys : MonoBehaviour
    {
        private void Update()
        {
            if (BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking)
            {
                return;
            }

            if (!BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController match) || match.IsFinished)
            {
                return;
            }

            Keyboard kb = Keyboard.current;
            if (kb == null || !kb.commaKey.wasPressedThisFrame)
            {
                return;
            }

            BattleAcesCore core = match.PlayerCore;
            if (core == null || core.Team != UnitTeam.Player || core.Health == null || !core.Health.IsAlive)
            {
                return;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            RTSCameraController rts = cam.GetComponent<RTSCameraController>();
            if (rts == null)
            {
                return;
            }

            rts.CenterViewOnWorldPoint(core.RallyWorldPosition);
        }
    }
}
