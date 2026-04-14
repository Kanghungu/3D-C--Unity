using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Resources 기준 유닛 비주얼 프리팹 경로 — 에셋 없으면 로드만 실패하고 프리미티브 폴백.
    /// </summary>
    public static class PrototypeUnitVisualResourceCatalog
    {
        /// <summary>정의에 경로가 비어 있을 때 보병 3종 기본 후보(앞에서부터 시도)</summary>
        public static bool TryGetPrefabResourceCandidates(UnitArchetype archetype, out string[] candidatesOrdered)
        {
            switch (archetype)
            {
                case UnitArchetype.Spearman:
                    candidatesOrdered = new[] { "Units/Spearman", "PrototypeUnits/test" };
                    return true;
                case UnitArchetype.ShieldInfantry:
                    candidatesOrdered = new[] { "Units/ShieldInfantry" };
                    return true;
                case UnitArchetype.Rifleman:
                    candidatesOrdered = new[] { "Units/Rifleman" };
                    return true;
                default:
                    candidatesOrdered = null;
                    return false;
            }
        }

        /// <summary>첫 성공 로드까지 시도 — 없으면 null</summary>
        public static GameObject LoadUnitVisualPrefab(UnitDefinition definition)
        {
            return TryLoadUnitVisualPrefab(definition, out GameObject prefab, out _) ? prefab : null;
        }

        /// <summary>로드 성공 시 사용된 Resources 경로(확장자 없음)를 반환 — 피벗 보정 등에 사용</summary>
        public static bool TryLoadUnitVisualPrefab(UnitDefinition definition, out GameObject prefab, out string resourcePathUsed)
        {
            prefab = null;
            resourcePathUsed = null;

            if (definition == null)
            {
                return false;
            }

            string explicitPath = definition.OptionalUnitVisualResourcePath;
            if (!string.IsNullOrWhiteSpace(explicitPath))
            {
                string trimmed = explicitPath.Trim();
                GameObject fromExplicit = Resources.Load<GameObject>(trimmed);
                if (fromExplicit != null)
                {
                    prefab = fromExplicit;
                    resourcePathUsed = trimmed;
                    return true;
                }
            }

            if (!TryGetPrefabResourceCandidates(definition.Archetype, out string[] candidates) || candidates == null)
            {
                return false;
            }

            for (int i = 0; i < candidates.Length; i++)
            {
                GameObject loaded = Resources.Load<GameObject>(candidates[i]);
                if (loaded != null)
                {
                    prefab = loaded;
                    resourcePathUsed = candidates[i];
                    return true;
                }
            }

            return false;
        }
    }
}
