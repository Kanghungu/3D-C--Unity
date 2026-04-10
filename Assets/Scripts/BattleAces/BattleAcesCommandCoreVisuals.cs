using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Battle Aces command core silhouette pass.
    /// Uses an unscaled child root so the core body can stay gameplay-sized while the
    /// decorative architecture remains readable and intentional.
    /// </summary>
    public static class BattleAcesCommandCoreVisuals
    {
        public static void ApplyToCore(GameObject coreRoot, UnitTeam team, Color hullColor)
        {
            if (coreRoot == null)
            {
                return;
            }

            Renderer rootRenderer = coreRoot.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                // 과발광 방지 — ReadablePrimitiveMaterialUtility 전역 클램프와 맞물리게 보수적으로
                // 몸통 실루엣만 살짝 — 링/비콘은 아래에서 EmissionAccent·StrongGlow 로 구분
                // 몸통 실루엣 살짝 강화 — 과발광은 ReadablePrimitive 전역 클램프가 제한
                float hullEmission = team == UnitTeam.Player ? 0.162f : 0.118f;
                ReadablePrimitiveMaterialUtility.Apply(rootRenderer, hullColor, hullEmission);
            }

            Color beacon = team == UnitTeam.Player
                ? BattleAcesArtDirection.PointTeal
                : BattleAcesArtDirection.EnemyEmber;
            Color stone = Color.Lerp(hullColor, BattleAcesArtDirection.GunmetalDark, 0.35f);
            Color trim = Color.Lerp(hullColor, Color.white, 0.18f);
            Color accent = Color.Lerp(beacon, Color.white, 0.08f);

            Transform visualRoot = GetOrCreateUnscaledVisualRoot(coreRoot.transform);

            AttachChild(
                visualRoot,
                PrimitiveType.Cylinder,
                "CommandCoreDais",
                new Vector3(0f, -1.58f, 0f),
                new Vector3(8.6f, 0.26f, 8.6f),
                stone,
                ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            const float ringEmissionMul = 1.09f; // 전역 클램프 안 — 의식 링만 살짝 강조
            AttachChild(
                visualRoot,
                PrimitiveType.Cylinder,
                "CommandCoreInnerRing",
                new Vector3(0f, -1.28f, 0f),
                new Vector3(7.2f, 0.05f, 7.2f),
                accent,
                ReadablePrimitiveMaterialUtility.EmissionAccent * ringEmissionMul);
            AttachChild(
                visualRoot,
                PrimitiveType.Cylinder,
                "CommandCoreHaloRing",
                new Vector3(0f, 1.72f, 0f),
                new Vector3(5.15f, 0.06f, 5.15f),
                accent,
                ReadablePrimitiveMaterialUtility.EmissionAccent * ringEmissionMul);
            AttachChild(
                visualRoot,
                PrimitiveType.Cylinder,
                "CommandCoreBeacon",
                new Vector3(0f, 2.58f, 0f),
                new Vector3(0.42f, 1.26f, 0.42f),
                beacon,
                ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            AttachChild(
                visualRoot,
                PrimitiveType.Sphere,
                "CommandCoreCrown",
                new Vector3(0f, 4.1f, 0f),
                new Vector3(0.95f, 0.95f, 0.95f),
                accent,
                ReadablePrimitiveMaterialUtility.EmissionStrongGlow);

            float frameOffset = 3.45f;
            float frameHeight = 0.32f;
            AttachChild(visualRoot, PrimitiveType.Cube, "FrameNorth", new Vector3(0f, frameHeight, frameOffset), new Vector3(5.3f, 0.28f, 0.44f), trim, ReadablePrimitiveMaterialUtility.EmissionAccent);
            AttachChild(visualRoot, PrimitiveType.Cube, "FrameSouth", new Vector3(0f, frameHeight, -frameOffset), new Vector3(5.3f, 0.28f, 0.44f), trim, ReadablePrimitiveMaterialUtility.EmissionAccent);
            AttachChild(visualRoot, PrimitiveType.Cube, "FrameEast", new Vector3(frameOffset, frameHeight, 0f), new Vector3(0.44f, 0.28f, 5.3f), trim, ReadablePrimitiveMaterialUtility.EmissionAccent);
            AttachChild(visualRoot, PrimitiveType.Cube, "FrameWest", new Vector3(-frameOffset, frameHeight, 0f), new Vector3(0.44f, 0.28f, 5.3f), trim, ReadablePrimitiveMaterialUtility.EmissionAccent);

            AddPylon(visualRoot, "PylonNW", new Vector3(-2.62f, 1.18f, 2.62f), trim, accent);
            AddPylon(visualRoot, "PylonNE", new Vector3(2.62f, 1.18f, 2.62f), trim, accent);
            AddPylon(visualRoot, "PylonSW", new Vector3(-2.62f, 1.18f, -2.62f), trim, accent);
            AddPylon(visualRoot, "PylonSE", new Vector3(2.62f, 1.18f, -2.62f), trim, accent);

            if (team == UnitTeam.Player)
            {
                AttachChild(visualRoot, PrimitiveType.Cube, "SignalBridge", new Vector3(0f, 2.2f, 0f), new Vector3(6.8f, 0.16f, 0.58f), accent, ReadablePrimitiveMaterialUtility.EmissionAccent);
                AttachChild(visualRoot, PrimitiveType.Cube, "SignalFinLeft", new Vector3(-3.22f, 1.8f, 0f), new Vector3(0.46f, 1.4f, 0.6f), trim, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                AttachChild(visualRoot, PrimitiveType.Cube, "SignalFinRight", new Vector3(3.22f, 1.8f, 0f), new Vector3(0.46f, 1.4f, 0.6f), trim, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            }
            else
            {
                Transform crestLeft = AttachChild(visualRoot, PrimitiveType.Cube, "WarCrestLeft", new Vector3(-2.1f, 2.14f, 0f), new Vector3(0.38f, 1.8f, 0.38f), accent, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                crestLeft.localRotation = Quaternion.Euler(0f, 0f, 28f);
                Transform crestRight = AttachChild(visualRoot, PrimitiveType.Cube, "WarCrestRight", new Vector3(2.1f, 2.14f, 0f), new Vector3(0.38f, 1.8f, 0.38f), accent, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                crestRight.localRotation = Quaternion.Euler(0f, 0f, -28f);
                AttachChild(visualRoot, PrimitiveType.Cube, "WarLintel", new Vector3(0f, 1.98f, 0f), new Vector3(5.6f, 0.16f, 0.42f), trim, ReadablePrimitiveMaterialUtility.EmissionAccent);
            }
        }

        private static Transform GetOrCreateUnscaledVisualRoot(Transform parent)
        {
            Transform existing = parent.Find("CommandCoreVisualRoot");
            if (existing != null)
            {
                return existing;
            }

            GameObject root = new GameObject("CommandCoreVisualRoot");
            root.transform.SetParent(parent, false);
            Vector3 scale = parent.lossyScale;
            root.transform.localScale = new Vector3(
                SafeInverse(scale.x),
                SafeInverse(scale.y),
                SafeInverse(scale.z));
            return root.transform;
        }

        private static float SafeInverse(float value)
        {
            return Mathf.Abs(value) <= 0.0001f ? 1f : 1f / value;
        }

        private static void AddPylon(Transform parent, string name, Vector3 position, Color body, Color accent)
        {
            AttachChild(parent, PrimitiveType.Cube, name + "_Body", position, new Vector3(0.78f, 3.1f, 0.78f), body, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            AttachChild(parent, PrimitiveType.Cylinder, name + "_Halo", position + new Vector3(0f, 1.88f, 0f), new Vector3(0.46f, 0.04f, 0.46f), accent, ReadablePrimitiveMaterialUtility.EmissionAccent);
            AttachChild(parent, PrimitiveType.Sphere, name + "_Orb", position + new Vector3(0f, 2.26f, 0f), new Vector3(0.42f, 0.42f, 0.42f), accent, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
        }

        private static Transform AttachChild(
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
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = localScale;

            if (child.TryGetComponent(out Collider collider))
            {
                collider.enabled = false;
            }

            Renderer renderer = child.GetComponent<Renderer>();
            ReadablePrimitiveMaterialUtility.Apply(renderer, color, emissionIntensity);
            return child.transform;
        }
    }
}
