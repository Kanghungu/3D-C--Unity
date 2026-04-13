using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// 애니메이션 이벤트에서 <c>AnimStrike()</c> 호출 → <see cref="UnitCombat"/> 타격 확정.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnitCombatStrikeBridge : MonoBehaviour
    {
        private UnitCombat combat;

        private void Awake()
        {
            combat = GetComponent<UnitCombat>();
        }

        /// <summary>Animator 이벤트에서 이 이름으로 호출</summary>
        public void AnimStrike()
        {
            combat?.NotifyAnimStrikeFromAnimation();
        }
    }
}
