using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 시야 격자용 — 관측 높이에서 목표 XZ까지 수평 레이로 VisionObstacle 레이어만 검사.
    /// </summary>
    public static class FogOfWarLineOfSight
    {
        /// <summary>
        /// 관측자 위치 기준 눈 높이에서, 같은 눈 높이의 목표 XZ까지 장애물이 끼면 false.
        /// </summary>
        public static bool HasClearSight(
            Vector3 observerWorld,
            float targetWorldX,
            float targetWorldZ,
            float eyeHeightOffset,
            LayerMask visionBlockMask,
            float rayStartInset,
            float rayEndInset)
        {
            if (visionBlockMask.value == 0)
            {
                return true;
            }

            float eyeY = observerWorld.y + eyeHeightOffset;
            Vector3 rayOrigin = new Vector3(observerWorld.x, eyeY, observerWorld.z);
            Vector3 rayEnd = new Vector3(targetWorldX, eyeY, targetWorldZ);
            Vector3 delta = rayEnd - rayOrigin;
            float distance = delta.magnitude;
            if (distance < rayStartInset + rayEndInset + 0.02f)
            {
                return true;
            }

            Vector3 direction = delta / distance;
            // 부동소수·인셋 조합으로 음수 나오면 Raycast 예외/오동작 방지
            float castDistance = Mathf.Max(0f, distance - rayStartInset - rayEndInset);
            Vector3 castStart = rayOrigin + direction * rayStartInset;

            return !Physics.Raycast(
                castStart,
                direction,
                castDistance,
                visionBlockMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}
