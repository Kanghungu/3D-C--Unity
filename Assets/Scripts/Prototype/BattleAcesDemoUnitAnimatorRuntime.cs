using System.Collections.Generic;
using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Battle Aces 런타임 DB 유닛에 데모용 근접 Mecanim을 붙인다. 에셋은 Resources/BattleAcesDemo 에 두고 에디터에서 생성.
    /// </summary>
    public static class BattleAcesDemoUnitAnimatorRuntime
    {
        private const string ResourcesControllerPath = "BattleAcesDemo/Unit_GenericMelee";

        /// <summary>Mecanim 이 로컬 스케일을 건드릴 때 루트 유닛 스케일(아키타입별)을 덮어쓰지 않도록 하는 자식 이름 — 팩토리·에디터 클립 경로와 동일해야 함</summary>
        public const string AnimScaleRootChildName = "BA_AnimScaleRoot";

        /// <summary>플랜 기준 근접 위주(발사체 유닛도 AnimStrike 시 발사 경로로 동기 가능)</summary>
        private static readonly UnitArchetype[] MeleeArchetypes =
        {
            UnitArchetype.Spearman,
            UnitArchetype.ShieldInfantry,
            UnitArchetype.Outrider,
            UnitArchetype.RoyalGuard,
            UnitArchetype.SpecialWarrior,
        };

        /// <summary>
        /// 컨트롤러가 있으면 목록에서 해당 아키타입에 컨트롤러 + 애니 주도 타격을 켠다.
        /// </summary>
        public static void BindMeleeDemoAnimatorsIfAvailable(List<UnitDefinition> definitions)
        {
            if (definitions == null || definitions.Count == 0)
            {
                return;
            }

            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(ResourcesControllerPath);
            if (controller == null)
            {
                Debug.LogWarning(
                    $"[BattleAcesDemo] Resources 에 Mecanim 이 없습니다: '{ResourcesControllerPath}'. " +
                    "Unity 메뉴 Game/Battle Aces/데모 유닛 애니 에셋 생성 을 실행하세요.");
                return;
            }

            AnimCombatProfileData profile = AnimCombatProfileData.CreateDefault();
            profile.useAnimDrivenStrike = true;

            for (int i = 0; i < definitions.Count; i++)
            {
                UnitDefinition def = definitions[i];
                if (def == null)
                {
                    continue;
                }

                if (!IsMeleeDemoArchetype(def.Archetype))
                {
                    continue;
                }

                def.SetOptionalAnimatorControllerRuntime(controller);
                def.SetAnimCombatProfileRuntime(profile);
            }
        }

        private static bool IsMeleeDemoArchetype(UnitArchetype archetype)
        {
            for (int i = 0; i < MeleeArchetypes.Length; i++)
            {
                if (MeleeArchetypes[i] == archetype)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
