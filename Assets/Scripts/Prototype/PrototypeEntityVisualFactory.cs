using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared primitive-based silhouettes and faction visual helpers for runtime prototype entities.
    /// 아군 청록·적 앰버는 <c>BattleAcesArtDirection.PointTeal</c>(0.18,0.88,0.98)·<c>EnemyEmber</c>(0.92,0.38,0.22) 와 숫자 동기화(Prototype 어셈블리가 BattleAces 를 참조하지 않음).
    /// </summary>
    public static class PrototypeEntityVisualFactory
    {
        private const string SpearmanModelResourcePath = "PrototypeUnits/test";

        /// <summary>BattleAcesArtDirection.PointTeal 과 동기 — Prototype 은 BattleAces 어셈블리 미참조</summary>
        private static readonly Color SyncedAllyPointTeal = new Color(0.18f, 0.88f, 0.98f);

        /// <summary>BattleAcesArtDirection.EnemyEmber 과 동기</summary>
        private static readonly Color SyncedEnemyEmber = new Color(0.92f, 0.38f, 0.22f);

        public static void ApplyBaseVisuals(GameObject target, UnitTeam team)
        {
            Color primaryColor = team == UnitTeam.Player ? new Color(0.46f, 0.56f, 0.8f) : new Color(0.74f, 0.27f, 0.19f);
            Color secondaryColor = team == UnitTeam.Player ? new Color(0.84f, 0.8f, 0.64f) : new Color(0.36f, 0.1f, 0.07f);
            Color accentColor = team == UnitTeam.Player ? SyncedAllyPointTeal : SyncedEnemyEmber;
            Renderer rootRenderer = target.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                ReadablePrimitiveMaterialUtility.Apply(rootRenderer, primaryColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            }

            BuildBaseSilhouette(target.transform, secondaryColor, accentColor);
            BuildStructureFactionSignature(target.transform, team, accentColor, 2.42f, 1.08f);
        }

        public static void ApplyProductionVisuals(GameObject target, UnitTeam team, bool isSiegeStructure)
        {
            Color primaryColor = team == UnitTeam.Player ? new Color(0.72f, 0.66f, 0.52f) : new Color(0.82f, 0.34f, 0.2f);
            Color secondaryColor = team == UnitTeam.Player ? new Color(0.92f, 0.84f, 0.58f) : new Color(0.4f, 0.16f, 0.09f);
            Color accentColor = team == UnitTeam.Player ? SyncedAllyPointTeal : SyncedEnemyEmber;
            Renderer rootRenderer = target.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                ReadablePrimitiveMaterialUtility.Apply(rootRenderer, primaryColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            }

            BuildProductionSilhouette(target.transform, secondaryColor, accentColor, isSiegeStructure);
            BuildStructureFactionSignature(target.transform, team, accentColor, isSiegeStructure ? 2.24f : 1.76f, isSiegeStructure ? 0.92f : 0.74f);
        }

        public static void ApplyTurretVisuals(GameObject target, UnitTeam team)
        {
            Color primaryColor = team == UnitTeam.Player ? new Color(0.62f, 0.74f, 0.9f) : new Color(0.88f, 0.36f, 0.2f);
            Color accentColor = team == UnitTeam.Player ? SyncedAllyPointTeal : SyncedEnemyEmber;
            Renderer rootRenderer = target.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                ReadablePrimitiveMaterialUtility.Apply(rootRenderer, primaryColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            }

            BuildTurretSilhouette(target.transform, accentColor);
            BuildStructureFactionSignature(target.transform, team, accentColor, 1.14f, 0.58f);
        }

        public static void BuildUnitSilhouette(Transform root, UnitDefinition definition, UnitTeam team)
        {
            // 아군: 차가운 청·시안 대비 / 적: 따뜻한 적갈·주황 포인트 — 멀리서 실루엣 분리
            Color armor = team == UnitTeam.Player ? new Color(0.64f, 0.76f, 0.96f) : new Color(0.58f, 0.27f, 0.21f);
            Color cloth = team == UnitTeam.Player ? new Color(0.16f, 0.34f, 0.56f) : new Color(0.37f, 0.13f, 0.09f);
            Color skin = team == UnitTeam.Player ? new Color(0.82f, 0.72f, 0.6f) : new Color(0.72f, 0.55f, 0.42f);
            Color glow = team == UnitTeam.Player ? SyncedAllyPointTeal : SyncedEnemyEmber;
            Color weapon = team == UnitTeam.Player ? new Color(0.88f, 0.92f, 1f) : new Color(0.26f, 0.07f, 0.05f);

            switch (definition.Archetype)
            {
                case UnitArchetype.Spearman:
                    if (TryBuildImportedSpearman(root, team, armor, cloth, glow))
                    {
                        break;
                    }

                    BuildHumanoid(root, armor, cloth, skin, 1.05f, false);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Pike Shaft", new Vector3(0.22f, 1.06f, 0.28f), new Vector3(0.06f, 1.12f, 0.06f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Pike Head", new Vector3(0.22f, 1.62f, 0.28f), new Vector3(0.12f, 0.18f, 0.12f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Pike Counterweight", new Vector3(0.22f, 0.54f, 0.28f), new Vector3(0.12f, 0.14f, 0.12f), cloth, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Sphere, "Pike Lantern", new Vector3(-0.18f, 0.98f, 0.24f), new Vector3(0.14f, 0.14f, 0.14f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "PauldronBadgeL", new Vector3(-0.26f, 1.12f, 0.06f), new Vector3(0.1f, 0.08f, 0.12f), glow, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "PauldronBadgeR", new Vector3(0.26f, 1.12f, 0.06f), new Vector3(0.1f, 0.08f, 0.12f), glow, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    break;
                case UnitArchetype.ShieldInfantry:
                    BuildHumanoid(root, armor, cloth, skin, 1.12f, true);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Shield", new Vector3(-0.28f, 0.82f, 0.48f), new Vector3(0.44f, 0.62f, 0.14f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Short Blade", new Vector3(0.26f, 0.88f, 0.46f), new Vector3(0.06f, 0.14f, 0.34f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "ShieldBoss", new Vector3(-0.28f, 0.82f, 0.58f), new Vector3(0.12f, 0.12f, 0.04f), glow, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "ShieldRim", new Vector3(-0.28f, 0.82f, 0.62f), new Vector3(0.46f, 0.64f, 0.035f), weapon, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    break;
                case UnitArchetype.Rifleman:
                    BuildHumanoid(root, armor, cloth, skin, 0.98f, false);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Long Rifle", new Vector3(0f, 0.96f, 0.46f), new Vector3(0.08f, 0.08f, 0.88f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Back Pack", new Vector3(0f, 0.9f, -0.22f), new Vector3(0.24f, 0.34f, 0.18f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Scope", new Vector3(0f, 1.08f, 0.42f), new Vector3(0.08f, 0.04f, 0.18f), glow, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "MuzzleBrake", new Vector3(0f, 0.96f, 0.9f), new Vector3(0.12f, 0.06f, 0.06f), glow, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Foregrip", new Vector3(0f, 0.88f, 0.62f), new Vector3(0.06f, 0.1f, 0.14f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    break;
                case UnitArchetype.SpecialWarrior:
                    BuildSpecialWarriorSilhouette(root, armor, cloth, skin, glow, weapon);
                    break;
                case UnitArchetype.RoyalGuard:
                    BuildRoyalGuardSilhouette(root, armor, cloth, skin, glow, weapon);
                    break;
                case UnitArchetype.Artillery:
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Gun Carriage", new Vector3(0f, 0.22f, 0f), new Vector3(0.62f, 0.2f, 0.9f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Siege Barrel", new Vector3(0f, 0.44f, 0.86f), new Vector3(0.22f, 0.14f, 0.92f), weapon, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "Wheel Left", new Vector3(-0.46f, 0.2f, 0f), new Vector3(0.18f, 0.22f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "Wheel Right", new Vector3(0.46f, 0.2f, 0f), new Vector3(0.18f, 0.22f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "RecoilBraceLeft", new Vector3(-0.18f, 0.36f, 0.22f), new Vector3(0.08f, 0.1f, 0.64f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "RecoilBraceRight", new Vector3(0.18f, 0.36f, 0.22f), new Vector3(0.08f, 0.1f, 0.64f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "TargetGlass", new Vector3(0f, 0.58f, 0.42f), new Vector3(0.16f, 0.08f, 0.12f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "BarrelLug", new Vector3(0f, 0.44f, 1.18f), new Vector3(0.08f, 0.1f, 0.08f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    break;
                case UnitArchetype.Fighter:
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Hull", new Vector3(0f, 0.12f, 0f), new Vector3(0.3f, 0.12f, 0.86f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Cockpit", new Vector3(0f, 0.22f, 0.08f), new Vector3(0.18f, 0.12f, 0.28f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Wing Left", new Vector3(-0.62f, 0.06f, 0f), new Vector3(0.66f, 0.04f, 0.26f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Wing Right", new Vector3(0.62f, 0.06f, 0f), new Vector3(0.66f, 0.04f, 0.26f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Tail Fin", new Vector3(0f, 0.26f, -0.42f), new Vector3(0.08f, 0.22f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "WingStrakeL", new Vector3(-0.62f, 0.02f, 0.1f), new Vector3(0.52f, 0.03f, 0.12f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "WingStrakeR", new Vector3(0.62f, 0.02f, 0.1f), new Vector3(0.52f, 0.03f, 0.12f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Sphere, "EngineGlowLeft", new Vector3(-0.18f, 0.1f, -0.38f), new Vector3(0.12f, 0.12f, 0.12f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    CreateChildPrimitive(root, PrimitiveType.Sphere, "EngineGlowRight", new Vector3(0.18f, 0.1f, -0.38f), new Vector3(0.12f, 0.12f, 0.12f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    break;
                case UnitArchetype.MobileFortress:
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Deck", new Vector3(0f, 0.52f, 0f), new Vector3(0.82f, 0.12f, 0.82f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Citadel", new Vector3(0f, 0.92f, -0.08f), new Vector3(0.44f, 0.34f, 0.44f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Cannon", new Vector3(0f, 0.72f, 1.04f), new Vector3(0.16f, 0.16f, 0.72f), weapon, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "Track Left", new Vector3(-0.76f, 0.18f, 0f), new Vector3(0.18f, 0.18f, 0.88f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "Track Right", new Vector3(0.76f, 0.18f, 0f), new Vector3(0.18f, 0.18f, 0.88f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "SideTowerLeft", new Vector3(-0.46f, 0.86f, -0.12f), new Vector3(0.18f, 0.26f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "SideTowerRight", new Vector3(0.46f, 0.86f, -0.12f), new Vector3(0.18f, 0.26f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "ProwRam", new Vector3(0f, 0.46f, 1.22f), new Vector3(0.22f, 0.12f, 0.28f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    break;
                case UnitArchetype.AirborneCitadel:
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Flight Deck", new Vector3(0f, 0.82f, 0f), new Vector3(0.92f, 0.12f, 0.92f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Core Keep", new Vector3(0f, 1.16f, 0f), new Vector3(0.52f, 0.42f, 0.52f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Wing Left", new Vector3(-1.18f, 0.24f, 0f), new Vector3(0.88f, 0.06f, 0.42f), weapon, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Wing Right", new Vector3(1.18f, 0.24f, 0f), new Vector3(0.88f, 0.06f, 0.42f), weapon, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "EngineHalo", new Vector3(0f, 0.42f, -0.18f), new Vector3(0.28f, 0.03f, 0.28f), glow, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "DorsalFin", new Vector3(0f, 1.48f, -0.16f), new Vector3(0.16f, 0.34f, 0.2f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
                    break;
                case UnitArchetype.Outrider:
                    BuildOutriderSilhouette(root, armor, cloth, skin, glow, weapon);
                    break;
            }

            BuildUnitFactionSignature(root, definition.Archetype, team, cloth, glow);
        }

        private static bool TryBuildImportedSpearman(Transform root, UnitTeam team, Color armor, Color cloth, Color accent)
        {
            // Temporary fallback: the imported FBX currently comes in with an unusable
            // runtime scale/pivot, so keep Spearman on the primitive silhouette.
            bool useImportedSpearman = false;
            if (!useImportedSpearman)
            {
                return false;
            }

            GameObject source = Resources.Load<GameObject>(SpearmanModelResourcePath);
            if (source == null)
            {
                Debug.LogWarning($"[VisualFactory] FBX 로드 실패: Resources/{SpearmanModelResourcePath} — 프리미티브 폴백 사용");
                return false;
            }

            Debug.Log($"[VisualFactory] FBX 로드 성공: {source.name}, 렌더러 수={source.GetComponentsInChildren<Renderer>(true).Length}");

            GameObject visual = Object.Instantiate(source, root);
            visual.name = "SpearmanVisual";
            visual.transform.localPosition = new Vector3(0f, -0.46f, 0f);
            visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            visual.transform.localScale = Vector3.one * 0.78f;

            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                Material materialInstance = renderer.material;

                if (index == 0)
                {
                    materialInstance.color = armor;
                }
                else if (index % 3 == 0)
                {
                    materialInstance.color = accent;
                }
                else
                {
                    materialInstance.color = cloth;
                }
            }

            Transform visualTransform = visual.transform;
            visualTransform.SetSiblingIndex(0);
            return true;
        }

        public static void BuildControlNodeSilhouette(Transform root)
        {
            // 거점·목표: 중립 암석 + 시안/황금 대비, 세로 스파이어로 실루엣 고정
            Color glow = SyncedAllyPointTeal;
            Color stone = new(0.42f, 0.38f, 0.34f);
            Color accentGold = new(0.95f, 0.78f, 0.28f);

            Renderer rootR = root.GetComponent<Renderer>();
            if (rootR != null)
            {
                ReadablePrimitiveMaterialUtility.Apply(rootR, new Color(0.24f, 0.28f, 0.32f), ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            }

            CreateChildPrimitive(root, PrimitiveType.Cylinder, "Shrine Base", new Vector3(0f, 0.36f, 0f), new Vector3(1.05f, 0.1f, 1.05f), stone, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "Middle Ring", new Vector3(0f, 0.82f, 0f), new Vector3(0.72f, 0.06f, 0.72f), glow, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "Central Core", new Vector3(0f, 1.46f, 0f), new Vector3(0.52f, 0.52f, 0.52f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "Objective Spire", new Vector3(0f, 2.35f, 0f), new Vector3(0.1f, 0.95f, 0.1f), accentGold, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Objective Crown", new Vector3(0f, 3.15f, 0f), new Vector3(0.28f, 0.12f, 0.28f), accentGold, ReadablePrimitiveMaterialUtility.EmissionAccent);
        }

        public static void EnsureFogObject(GameObject target, BattlefieldFogRequirement requirement)
        {
            BattlefieldFogObject fogObject = target.GetComponent<BattlefieldFogObject>();
            if (fogObject == null)
            {
                fogObject = target.AddComponent<BattlefieldFogObject>();
            }

            fogObject.Configure(requirement);
        }

        private static void BuildHumanoid(Transform root, Color armor, Color cloth, Color skin, float torsoHeight, bool bulky)
        {
            float torsoWidth = bulky ? 0.42f : 0.34f;
            float torsoDepth = bulky ? 0.26f : 0.22f;
            float legGap = bulky ? 0.16f : 0.12f;
            float armOffset = bulky ? 0.34f : 0.26f;
            float headScale = bulky ? 0.22f : 0.2f;

            CreateChildPrimitive(root, PrimitiveType.Capsule, "Torso", new Vector3(0f, torsoHeight, 0f), new Vector3(torsoWidth, 0.42f, torsoDepth), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "Head", new Vector3(0f, torsoHeight + 0.54f, 0.04f), new Vector3(headScale, headScale, headScale), skin, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Visor", new Vector3(0f, torsoHeight + 0.56f, 0.18f), new Vector3(0.12f, 0.04f, 0.08f), cloth, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Breastplate", new Vector3(0f, torsoHeight + 0.02f, 0.18f), new Vector3(torsoWidth + 0.08f, 0.2f, 0.08f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "SpinePack", new Vector3(0f, torsoHeight + 0.04f, -0.18f), new Vector3(torsoWidth * 0.82f, 0.28f, 0.1f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "ShoulderLeft", new Vector3(-armOffset, torsoHeight + 0.28f, 0f), new Vector3(0.16f, 0.1f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "ShoulderRight", new Vector3(armOffset, torsoHeight + 0.28f, 0f), new Vector3(0.16f, 0.1f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Hip Guard", new Vector3(0f, torsoHeight - 0.28f, 0f), new Vector3(torsoWidth + 0.04f, 0.14f, torsoDepth), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Capsule, "Left Leg", new Vector3(-legGap, 0.42f, 0f), new Vector3(0.14f, 0.34f, 0.14f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Capsule, "Right Leg", new Vector3(legGap, 0.42f, 0f), new Vector3(0.14f, 0.34f, 0.14f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "KneeGuardLeft", new Vector3(-legGap, 0.38f, 0.1f), new Vector3(0.1f, 0.08f, 0.08f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "KneeGuardRight", new Vector3(legGap, 0.38f, 0.1f), new Vector3(0.1f, 0.08f, 0.08f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "FootGuardLeft", new Vector3(-legGap, 0.06f, 0.12f), new Vector3(0.12f, 0.06f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "FootGuardRight", new Vector3(legGap, 0.06f, 0.12f), new Vector3(0.12f, 0.06f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Capsule, "Left Arm", new Vector3(-armOffset, torsoHeight + 0.08f, 0f), new Vector3(0.12f, 0.28f, 0.12f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Capsule, "Right Arm", new Vector3(armOffset, torsoHeight + 0.08f, 0f), new Vector3(0.12f, 0.28f, 0.12f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "BackHalo", new Vector3(0f, torsoHeight + 0.28f, -0.14f), new Vector3(0.14f, 0.02f, 0.14f), cloth, ReadablePrimitiveMaterialUtility.EmissionAccent);
        }

        private static void BuildSpecialWarriorSilhouette(Transform root, Color armor, Color cloth, Color skin, Color glow, Color weapon)
        {
            BuildHumanoid(root, armor, cloth, skin, 1.12f, false);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Phase Blade Left", new Vector3(-0.28f, 1.02f, 0.42f), new Vector3(0.06f, 0.14f, 0.54f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Phase Blade Right", new Vector3(0.28f, 0.96f, 0.42f), new Vector3(0.06f, 0.14f, 0.46f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Phase Thruster Left", new Vector3(-0.16f, 1.08f, -0.22f), new Vector3(0.12f, 0.26f, 0.18f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Phase Thruster Right", new Vector3(0.16f, 1.08f, -0.22f), new Vector3(0.12f, 0.26f, 0.18f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Phase Shoulder Left", new Vector3(-0.32f, 1.28f, -0.02f), new Vector3(0.18f, 0.08f, 0.26f), weapon, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Phase Shoulder Right", new Vector3(0.32f, 1.28f, -0.02f), new Vector3(0.18f, 0.08f, 0.26f), weapon, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "Phase Halo", new Vector3(0f, 1.54f, 0.02f), new Vector3(0.18f, 0.18f, 0.18f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
        }

        private static void BuildRoyalGuardSilhouette(Transform root, Color armor, Color cloth, Color skin, Color glow, Color weapon)
        {
            BuildHumanoid(root, armor, cloth, skin, 1.22f, true);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Tower Shield", new Vector3(-0.38f, 0.9f, 0.5f), new Vector3(0.58f, 0.84f, 0.18f), weapon, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Guardian Pike", new Vector3(0.28f, 1.06f, 0.48f), new Vector3(0.06f, 0.16f, 0.62f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Guardian Pike Head", new Vector3(0.28f, 1.28f, 0.82f), new Vector3(0.12f, 0.18f, 0.16f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Guardian Shoulder Left", new Vector3(-0.42f, 1.34f, 0f), new Vector3(0.2f, 0.1f, 0.28f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Guardian Shoulder Right", new Vector3(0.42f, 1.34f, 0f), new Vector3(0.2f, 0.1f, 0.28f), armor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Guardian Banner", new Vector3(0f, 1.74f, -0.16f), new Vector3(0.12f, 0.48f, 0.06f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "Guardian Halo", new Vector3(0f, 1.6f, 0.02f), new Vector3(0.2f, 0.2f, 0.2f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
        }

        private static void BuildOutriderSilhouette(Transform root, Color armor, Color cloth, Color skin, Color glow, Color weapon)
        {
            CreateChildPrimitive(root, PrimitiveType.Cube, "Bike Hull", new Vector3(0f, 0.56f, 0f), new Vector3(0.34f, 0.18f, 0.98f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Bike Nose", new Vector3(0f, 0.6f, 0.72f), new Vector3(0.18f, 0.12f, 0.42f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Bike Engine Left", new Vector3(-0.34f, 0.52f, -0.08f), new Vector3(0.18f, 0.18f, 0.52f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Bike Engine Right", new Vector3(0.34f, 0.52f, -0.08f), new Vector3(0.18f, 0.18f, 0.52f), cloth, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Hover Fin Left", new Vector3(-0.54f, 0.42f, 0f), new Vector3(0.34f, 0.04f, 0.78f), weapon, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Hover Fin Right", new Vector3(0.54f, 0.42f, 0f), new Vector3(0.34f, 0.04f, 0.78f), weapon, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Capsule, "Rider Torso", new Vector3(0f, 1.06f, -0.04f), new Vector3(0.2f, 0.26f, 0.18f), armor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "Rider Head", new Vector3(0f, 1.42f, 0f), new Vector3(0.18f, 0.18f, 0.18f), skin, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Rider Visor", new Vector3(0f, 1.42f, 0.14f), new Vector3(0.12f, 0.04f, 0.08f), glow, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Lance", new Vector3(0.24f, 1.02f, 0.44f), new Vector3(0.06f, 0.08f, 0.68f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "Engine Core Left", new Vector3(-0.34f, 0.48f, -0.42f), new Vector3(0.12f, 0.12f, 0.12f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "Engine Core Right", new Vector3(0.34f, 0.48f, -0.42f), new Vector3(0.12f, 0.12f, 0.12f), glow, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            CreateChildPrimitive(root, PrimitiveType.Cube, "RearFinLeft", new Vector3(-0.22f, 0.72f, -0.56f), new Vector3(0.08f, 0.2f, 0.12f), weapon, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "RearFinRight", new Vector3(0.22f, 0.72f, -0.56f), new Vector3(0.08f, 0.2f, 0.12f), weapon, ReadablePrimitiveMaterialUtility.EmissionAccent);
        }

        private static void BuildBaseSilhouette(Transform root, Color secondaryColor, Color accentColor)
        {
            CreateChildPrimitive(root, PrimitiveType.Cube, "Foundation", new Vector3(0f, -1.25f, 0f), new Vector3(1.34f, 0.16f, 1.34f), secondaryColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "Core Beacon", new Vector3(0f, 2.02f, 0f), new Vector3(0.14f, 0.54f, 0.14f), accentColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "BastionNorth", new Vector3(0f, 0.9f, 1.08f), new Vector3(0.92f, 0.24f, 0.14f), secondaryColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "BastionSouth", new Vector3(0f, 0.9f, -1.08f), new Vector3(0.92f, 0.24f, 0.14f), secondaryColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "BastionEast", new Vector3(1.08f, 0.9f, 0f), new Vector3(0.14f, 0.24f, 0.92f), secondaryColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "BastionWest", new Vector3(-1.08f, 0.9f, 0f), new Vector3(0.14f, 0.24f, 0.92f), secondaryColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
        }

        private static void BuildProductionSilhouette(Transform root, Color secondaryColor, Color accentColor, bool isSiegeStructure)
        {
            CreateChildPrimitive(root, PrimitiveType.Cube, "Base Plinth", new Vector3(0f, -1.02f, 0f), new Vector3(1.34f, 0.12f, 1.34f), secondaryColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "Assembly Pad", new Vector3(0f, -0.9f, 0f), new Vector3(1.08f, 0.05f, 1.08f), accentColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cube, "AssemblyLintel", new Vector3(0f, 1.16f, 0f), new Vector3(1.4f, 0.12f, 0.24f), secondaryColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            if (isSiegeStructure)
            {
                CreateChildPrimitive(root, PrimitiveType.Cylinder, "Siege Tower", new Vector3(0f, 1.9f, 0f), new Vector3(0.14f, 0.76f, 0.14f), accentColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            }
            else
            {
                CreateChildPrimitive(root, PrimitiveType.Sphere, "AssemblyOrb", new Vector3(0f, 1.64f, 0f), new Vector3(0.28f, 0.28f, 0.28f), accentColor, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            }
        }

        private static void BuildTurretSilhouette(Transform root, Color accentColor)
        {
            CreateChildPrimitive(root, PrimitiveType.Cube, "Turret Barrel", new Vector3(0f, 0.46f, 0.94f), new Vector3(0.18f, 0.1f, 0.66f), accentColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "TurretRing", new Vector3(0f, 0.12f, 0f), new Vector3(0.64f, 0.04f, 0.64f), accentColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "TurretLens", new Vector3(0f, 0.54f, 0.56f), new Vector3(0.16f, 0.16f, 0.16f), accentColor, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
        }

        private static void BuildUnitFactionSignature(Transform root, UnitArchetype archetype, UnitTeam team, Color bannerColor, Color accentColor)
        {
            float markerBaseHeight = archetype switch
            {
                UnitArchetype.Artillery => 0.58f,
                UnitArchetype.Fighter => 0.42f,
                UnitArchetype.MobileFortress => 1.18f,
                UnitArchetype.AirborneCitadel => 1.48f,
                UnitArchetype.RoyalGuard => 1.78f,
                _ => 1.52f
            };

            float mastHeight = archetype switch
            {
                UnitArchetype.Artillery => 0.2f,
                UnitArchetype.Fighter => 0.16f,
                UnitArchetype.MobileFortress => 0.32f,
                UnitArchetype.AirborneCitadel => 0.36f,
                UnitArchetype.RoyalGuard => 0.3f,
                _ => 0.24f
            };

            float bannerWidth = archetype switch
            {
                UnitArchetype.Artillery => 0.26f,
                UnitArchetype.Fighter => 0.22f,
                UnitArchetype.MobileFortress => 0.42f,
                UnitArchetype.AirborneCitadel => 0.52f,
                UnitArchetype.RoyalGuard => 0.34f,
                _ => 0.28f
            };

            float markerDepth = archetype switch
            {
                UnitArchetype.Artillery => -0.12f,
                UnitArchetype.MobileFortress => -0.18f,
                UnitArchetype.AirborneCitadel => -0.16f,
                _ => -0.06f
            };

            if (team == UnitTeam.Player)
            {
                CreateChildPrimitive(root, PrimitiveType.Cylinder, "Signal Mast", new Vector3(0f, markerBaseHeight, markerDepth), new Vector3(0.05f, mastHeight, 0.05f), bannerColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                GameObject banner = CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Banner", new Vector3(bannerWidth * 0.42f, markerBaseHeight + mastHeight * 0.58f, markerDepth), new Vector3(bannerWidth, mastHeight * 0.34f, 0.04f), accentColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
                banner.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);
                CreateChildPrimitive(root, PrimitiveType.Sphere, "Signal Beacon", new Vector3(0f, markerBaseHeight + mastHeight * 1.22f, markerDepth), new Vector3(0.12f, 0.12f, 0.12f), accentColor, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Wing Left", new Vector3(-bannerWidth * 0.32f, markerBaseHeight + mastHeight * 0.28f, markerDepth - 0.02f), new Vector3(0.08f, 0.05f, bannerWidth * 0.78f), bannerColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Wing Right", new Vector3(bannerWidth * 0.32f, markerBaseHeight + mastHeight * 0.28f, markerDepth - 0.02f), new Vector3(0.08f, 0.05f, bannerWidth * 0.78f), bannerColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                return;
            }

            CreateChildPrimitive(root, PrimitiveType.Cylinder, "War Pike", new Vector3(0f, markerBaseHeight, markerDepth), new Vector3(0.05f, mastHeight, 0.05f), bannerColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "War Crest", new Vector3(0f, markerBaseHeight + mastHeight * 0.64f, markerDepth), new Vector3(bannerWidth * 0.9f, mastHeight * 0.28f, 0.04f), accentColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            GameObject hornLeft = CreateChildPrimitive(root, PrimitiveType.Cube, "War Horn Left", new Vector3(-bannerWidth * 0.34f, markerBaseHeight + mastHeight * 1.04f, markerDepth), new Vector3(0.08f, mastHeight * 0.62f, 0.08f), accentColor, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            hornLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 30f);
            GameObject hornRight = CreateChildPrimitive(root, PrimitiveType.Cube, "War Horn Right", new Vector3(bannerWidth * 0.34f, markerBaseHeight + mastHeight * 1.04f, markerDepth), new Vector3(0.08f, mastHeight * 0.62f, 0.08f), accentColor, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            hornRight.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
        }

        private static void BuildStructureFactionSignature(Transform root, UnitTeam team, Color accentColor, float topHeight, float span)
        {
            Color supportColor = team == UnitTeam.Player ? new Color(0.78f, 0.9f, 1f) : new Color(0.42f, 0.14f, 0.08f);

            if (team == UnitTeam.Player)
            {
                CreateChildPrimitive(root, PrimitiveType.Cylinder, "Signal Tower", new Vector3(0f, topHeight, 0f), new Vector3(0.08f, 0.42f, 0.08f), supportColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Bridge", new Vector3(0f, topHeight + 0.22f, 0f), new Vector3(span, 0.08f, 0.12f), accentColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Fin Left", new Vector3(-span * 0.46f, topHeight + 0.06f, 0f), new Vector3(0.12f, 0.22f, 0.08f), supportColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Fin Right", new Vector3(span * 0.46f, topHeight + 0.06f, 0f), new Vector3(0.12f, 0.22f, 0.08f), supportColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
                CreateChildPrimitive(root, PrimitiveType.Sphere, "Signal Core", new Vector3(0f, topHeight + 0.44f, 0f), new Vector3(0.18f, 0.18f, 0.18f), accentColor, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
                return;
            }

            CreateChildPrimitive(root, PrimitiveType.Cylinder, "War Spire", new Vector3(0f, topHeight, 0f), new Vector3(0.08f, 0.48f, 0.08f), supportColor, ReadablePrimitiveMaterialUtility.EmissionSubtleBody);
            CreateChildPrimitive(root, PrimitiveType.Cube, "War Crest", new Vector3(0f, topHeight + 0.2f, 0f), new Vector3(span * 0.84f, 0.08f, 0.1f), accentColor, ReadablePrimitiveMaterialUtility.EmissionAccent);
            GameObject spikeLeft = CreateChildPrimitive(root, PrimitiveType.Cube, "War Spike Left", new Vector3(-span * 0.34f, topHeight + 0.4f, 0f), new Vector3(0.1f, 0.34f, 0.1f), accentColor, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            spikeLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
            GameObject spikeRight = CreateChildPrimitive(root, PrimitiveType.Cube, "War Spike Right", new Vector3(span * 0.34f, topHeight + 0.4f, 0f), new Vector3(0.1f, 0.34f, 0.1f), accentColor, ReadablePrimitiveMaterialUtility.EmissionStrongGlow);
            spikeRight.transform.localRotation = Quaternion.Euler(0f, 0f, -22f);
        }

        private static GameObject CreateChildPrimitive(
            Transform parent,
            PrimitiveType primitiveType,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            float emissionIntensity = 0f)
        {
            GameObject child = GameObject.CreatePrimitive(primitiveType);
            child.name = objectName;
            child.transform.SetParent(parent);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = localScale;

            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Renderer renderer = child.GetComponent<Renderer>();
            ReadablePrimitiveMaterialUtility.Apply(renderer, color, emissionIntensity);
            return child;
        }
    }
}



