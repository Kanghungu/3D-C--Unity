using System.Collections;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 승리 직후 짧게 카메라를 적 방향으로 살짝 당겨 스크린샷·연출감 강화(timeScale 0에서도 unscaled).
    /// </summary>
    public sealed class BattleAcesVictoryPresentation : MonoBehaviour
    {
        private BattleAcesMatchController bound;

        private void OnEnable()
        {
            if (!BattleAcesMatchController.TryGetInstance(out bound))
            {
                return;
            }

            bound.MatchEnded += OnMatchEnded;
        }

        private void OnDisable()
        {
            if (bound != null)
            {
                bound.MatchEnded -= OnMatchEnded;
                bound = null;
            }
        }

        private void OnMatchEnded(BattleAcesMatchController.MatchState state)
        {
            if (state != BattleAcesMatchController.MatchState.Victory)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(NudgeCameraRoutine());
        }

        private IEnumerator NudgeCameraRoutine()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                yield break;
            }

            Vector3 start = cam.transform.position;
            Vector3 focus = start;
            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController m) &&
                m.EnemyCore != null)
            {
                Vector3 e = m.EnemyCore.transform.position;
                focus = Vector3.Lerp(start, new Vector3(e.x, start.y, e.z), 0.14f);
            }

            const float duration = 0.42f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                cam.transform.position = Vector3.Lerp(start, focus, k);
                yield return null;
            }
        }
    }
}
