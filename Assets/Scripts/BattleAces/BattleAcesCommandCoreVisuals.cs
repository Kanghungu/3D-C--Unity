using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Battle Aces 지휘 코어 — 본체 톤·상단 비콘·가장자리 핀으로 아군/적 실루엣 분리(프리미티브만).
    /// </summary>
    public static class BattleAcesCommandCoreVisuals
    {
        /// <summary>런타임 생성 직후 호출 — 자식 콜라이더는 피킹 방해 없이 끔</summary>
        public static void ApplyToCore(GameObject coreRoot, UnitTeam team, Color hullColor)
        {
            if (coreRoot == null)
            {
                return;
            }

            Renderer rootRenderer = coreRoot.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                float hullEmission = team == UnitTeam.Player ? 0.15f : 0.11f;
                ReadablePrimitiveMaterialUtility.Apply(rootRenderer, hullColor, hullEmission);
            }

            Color beacon = team == UnitTeam.Player
                ? BattleAcesArtDirection.PointTeal
                : BattleAcesArtDirection.EnemyEmber;

            AttachChild(
                coreRoot.transform,
                PrimitiveType.Cylinder,
                "CommandCoreBeacon",
                new Vector3(0f, 2.08f, 0f),
                new Vector3(0.26f, 0.72f, 0.26f),
                beacon,
                ReadablePrimitiveMaterialUtility.EmissionStrongGlow);

            Color pinTint = Color.Lerp(hullColor, beacon, 0.5f);
            float pinEm = ReadablePrimitiveMaterialUtility.EmissionAccent;
            float half = 2.72f;
            float y = 0.95f;

            AttachChild(coreRoot.transform, PrimitiveType.Capsule, "CoreSilhouettePinN", new Vector3(0f, y, half), new Vector3(0.16f, 0.52f, 0.16f), pinTint, pinEm);
            AttachChild(coreRoot.transform, PrimitiveType.Capsule, "CoreSilhouettePinS", new Vector3(0f, y, -half), new Vector3(0.16f, 0.52f, 0.16f), pinTint, pinEm);
            AttachChild(coreRoot.transform, PrimitiveType.Capsule, "CoreSilhouettePinE", new Vector3(half, y, 0f), new Vector3(0.16f, 0.52f, 0.16f), pinTint, pinEm);
            AttachChild(coreRoot.transform, PrimitiveType.Capsule, "CoreSilhouettePinW", new Vector3(-half, y, 0f), new Vector3(0.16f, 0.52f, 0.16f), pinTint, pinEm);
        }

        private static void AttachChild(
            Transform parent,
            PrimitiveType primitiveType,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            float emissionIntensity)
        {
            GameObject child = GameObject.CreatePrimitive(primitiveType);
            child.name = objectName;
            child.transform.SetParent(parent);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = localScale;

            if (child.TryGetComponent(out Collider collider))
            {
                collider.enabled = false;
            }

            Renderer renderer = child.GetComponent<Renderer>();
            ReadablePrimitiveMaterialUtility.Apply(renderer, color, emissionIntensity);
        }
    }
}
