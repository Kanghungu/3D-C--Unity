using Game.Units;
using UnityEngine;

namespace Game.Campaign.Scene
{
    /// <summary>
    /// 점령 미션 — 플레이어 유닛이 트리거 안에 머무른 시간을 누적한다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class MissionCaptureZone : MonoBehaviour
    {
        [SerializeField] private float holdSecondsRequired = 14f;

        private float holdTimer;
        private bool completed;

        public bool IsCompleted => completed;
        public float HoldProgress01 => completed ? 1f : Mathf.Clamp01(holdTimer / Mathf.Max(0.01f, holdSecondsRequired));

        private void Reset()
        {
            Collider c = GetComponent<Collider>();
            if (c != null)
            {
                c.isTrigger = true;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (completed)
            {
                return;
            }

            SelectableUnit su = other.GetComponent<SelectableUnit>();
            if (su == null || su.Team != UnitTeam.Player)
            {
                return;
            }

            holdTimer += Time.deltaTime;
            if (holdTimer >= holdSecondsRequired)
            {
                completed = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            SelectableUnit su = other.GetComponent<SelectableUnit>();
            if (su == null || su.Team != UnitTeam.Player)
            {
                return;
            }

            // 플레이어가 나가면 진행 리셋(단순 점령 규칙)
            if (!completed)
            {
                holdTimer = 0f;
            }
        }

        public void ConfigureHoldSeconds(float seconds)
        {
            holdSecondsRequired = Mathf.Max(1f, seconds);
        }
    }
}
