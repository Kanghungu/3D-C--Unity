using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared primitive-based silhouettes and faction visual helpers for runtime prototype entities.
    /// </summary>
    public static class PrototypeEntityVisualFactory
    {
        private const string SpearmanModelResourcePath = "PrototypeUnits/test";

        public static void ApplyBaseVisuals(GameObject target, UnitTeam team)
        {
            Color primaryColor = team == UnitTeam.Player ? new Color(0.56f, 0.64f, 0.78f) : new Color(0.66f, 0.29f, 0.2f);
            Color secondaryColor = team == UnitTeam.Player ? new Color(0.86f, 0.82f, 0.66f) : new Color(0.32f, 0.08f, 0.06f);
            Color accentColor = team == UnitTeam.Player ? new Color(0.34f, 0.9f, 1f) : new Color(1f, 0.52f, 0.22f);
            target.GetComponent<Renderer>().material.color = primaryColor;
            BuildBaseSilhouette(target.transform, secondaryColor, accentColor);
            BuildStructureFactionSignature(target.transform, team, accentColor, 2.42f, 1.08f);
        }

        public static void ApplyProductionVisuals(GameObject target, UnitTeam team, bool isSiegeStructure)
        {
            Color primaryColor = team == UnitTeam.Player ? new Color(0.78f, 0.72f, 0.58f) : new Color(0.8f, 0.38f, 0.22f);
            Color secondaryColor = team == UnitTeam.Player ? new Color(0.95f, 0.86f, 0.62f) : new Color(0.34f, 0.14f, 0.08f);
            Color accentColor = team == UnitTeam.Player ? new Color(0.22f, 0.85f, 0.95f) : new Color(1f, 0.58f, 0.22f);
            target.GetComponent<Renderer>().material.color = primaryColor;
            BuildProductionSilhouette(target.transform, secondaryColor, accentColor, isSiegeStructure);
            BuildStructureFactionSignature(target.transform, team, accentColor, isSiegeStructure ? 2.24f : 1.76f, isSiegeStructure ? 0.92f : 0.74f);
        }

        public static void ApplyTurretVisuals(GameObject target, UnitTeam team)
        {
            Color primaryColor = team == UnitTeam.Player ? new Color(0.74f, 0.79f, 0.88f) : new Color(0.92f, 0.42f, 0.22f);
            Color accentColor = team == UnitTeam.Player ? new Color(0.24f, 0.9f, 1f) : new Color(0.35f, 0.08f, 0.06f);
            target.GetComponent<Renderer>().material.color = primaryColor;
            BuildTurretSilhouette(target.transform, accentColor);
            BuildStructureFactionSignature(target.transform, team, accentColor, 1.14f, 0.58f);
        }

        public static void BuildUnitSilhouette(Transform root, UnitDefinition definition, UnitTeam team)
        {
            Color armor = team == UnitTeam.Player ? new Color(0.84f, 0.86f, 0.92f) : new Color(0.72f, 0.38f, 0.28f);
            Color cloth = team == UnitTeam.Player ? new Color(0.36f, 0.46f, 0.62f) : new Color(0.28f, 0.12f, 0.1f);
            Color skin = team == UnitTeam.Player ? new Color(0.85f, 0.75f, 0.64f) : new Color(0.78f, 0.63f, 0.5f);
            Color glow = team == UnitTeam.Player ? new Color(0.28f, 0.9f, 1f) : new Color(1f, 0.58f, 0.18f);
            Color weapon = team == UnitTeam.Player ? new Color(0.92f, 0.95f, 1f) : new Color(0.34f, 0.08f, 0.06f);

            switch (definition.Archetype)
            {
                case UnitArchetype.Spearman:
                    if (TryBuildImportedSpearman(root, team, armor, cloth, glow))
                    {
                        break;
                    }

                    BuildHumanoid(root, armor, cloth, skin, 1.05f, false);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Pike Shaft", new Vector3(0.22f, 1.06f, 0.28f), new Vector3(0.06f, 1.12f, 0.06f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Pike Head", new Vector3(0.22f, 1.62f, 0.28f), new Vector3(0.12f, 0.18f, 0.12f), glow);
                    break;
                case UnitArchetype.ShieldInfantry:
                    BuildHumanoid(root, armor, cloth, skin, 1.12f, true);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Shield", new Vector3(-0.28f, 0.82f, 0.48f), new Vector3(0.44f, 0.62f, 0.14f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Short Blade", new Vector3(0.26f, 0.88f, 0.46f), new Vector3(0.06f, 0.14f, 0.34f), glow);
                    break;
                case UnitArchetype.Rifleman:
                    BuildHumanoid(root, armor, cloth, skin, 0.98f, false);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Long Rifle", new Vector3(0f, 0.96f, 0.46f), new Vector3(0.08f, 0.08f, 0.88f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Back Pack", new Vector3(0f, 0.9f, -0.22f), new Vector3(0.24f, 0.34f, 0.18f), cloth);
                    break;
                case UnitArchetype.SpecialWarrior:
                    BuildHumanoid(root, armor, cloth, skin, 1.08f, false);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Blade", new Vector3(-0.26f, 0.98f, 0.42f), new Vector3(0.06f, 0.16f, 0.48f), glow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Pistol", new Vector3(0.24f, 0.92f, 0.42f), new Vector3(0.08f, 0.08f, 0.26f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Back Thruster", new Vector3(0f, 1.06f, -0.24f), new Vector3(0.22f, 0.32f, 0.18f), glow);
                    break;
                case UnitArchetype.RoyalGuard:
                    BuildHumanoid(root, armor, cloth, skin, 1.18f, true);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Tower Shield", new Vector3(-0.34f, 0.88f, 0.5f), new Vector3(0.52f, 0.76f, 0.16f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "Crest", new Vector3(0f, 1.56f, 0f), new Vector3(0.08f, 0.18f, 0.08f), glow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Ceremonial Blade", new Vector3(0.28f, 1f, 0.44f), new Vector3(0.06f, 0.14f, 0.44f), glow);
                    break;
                case UnitArchetype.Artillery:
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Gun Carriage", new Vector3(0f, 0.22f, 0f), new Vector3(0.62f, 0.2f, 0.9f), cloth);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Siege Barrel", new Vector3(0f, 0.44f, 0.86f), new Vector3(0.22f, 0.14f, 0.92f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "Wheel Left", new Vector3(-0.46f, 0.2f, 0f), new Vector3(0.18f, 0.22f, 0.18f), armor);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "Wheel Right", new Vector3(0.46f, 0.2f, 0f), new Vector3(0.18f, 0.22f, 0.18f), armor);
                    break;
                case UnitArchetype.Fighter:
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Hull", new Vector3(0f, 0.12f, 0f), new Vector3(0.3f, 0.12f, 0.86f), cloth);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Cockpit", new Vector3(0f, 0.22f, 0.08f), new Vector3(0.18f, 0.12f, 0.28f), glow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Wing Left", new Vector3(-0.62f, 0.06f, 0f), new Vector3(0.66f, 0.04f, 0.26f), armor);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Wing Right", new Vector3(0.62f, 0.06f, 0f), new Vector3(0.66f, 0.04f, 0.26f), armor);
                    break;
                case UnitArchetype.MobileFortress:
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Deck", new Vector3(0f, 0.52f, 0f), new Vector3(0.82f, 0.12f, 0.82f), glow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Citadel", new Vector3(0f, 0.92f, -0.08f), new Vector3(0.44f, 0.34f, 0.44f), armor);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Cannon", new Vector3(0f, 0.72f, 1.04f), new Vector3(0.16f, 0.16f, 0.72f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "Track Left", new Vector3(-0.76f, 0.18f, 0f), new Vector3(0.18f, 0.18f, 0.88f), cloth);
                    CreateChildPrimitive(root, PrimitiveType.Cylinder, "Track Right", new Vector3(0.76f, 0.18f, 0f), new Vector3(0.18f, 0.18f, 0.88f), cloth);
                    break;
                case UnitArchetype.AirborneCitadel:
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Flight Deck", new Vector3(0f, 0.82f, 0f), new Vector3(0.92f, 0.12f, 0.92f), glow);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Core Keep", new Vector3(0f, 1.16f, 0f), new Vector3(0.52f, 0.42f, 0.52f), armor);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Wing Left", new Vector3(-1.18f, 0.24f, 0f), new Vector3(0.88f, 0.06f, 0.42f), weapon);
                    CreateChildPrimitive(root, PrimitiveType.Cube, "Wing Right", new Vector3(1.18f, 0.24f, 0f), new Vector3(0.88f, 0.06f, 0.42f), weapon);
                    break;
            }

            BuildUnitFactionSignature(root, definition.Archetype, team, cloth, glow);
        }

        private static bool TryBuildImportedSpearman(Transform root, UnitTeam team, Color armor, Color cloth, Color accent)
        {
            GameObject source = Resources.Load<GameObject>(SpearmanModelResourcePath);
            if (source == null)
            {
                return false;
            }

            GameObject visual = Object.Instantiate(source, root);
            visual.name = "SpearmanVisual";
            visual.transform.localPosition = new Vector3(0f, -0.88f, 0f);
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
            Color glow = new(0.32f, 0.95f, 1f);
            Color stone = new(0.78f, 0.72f, 0.58f);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "Shrine Base", new Vector3(0f, 0.36f, 0f), new Vector3(1.05f, 0.1f, 1.05f), stone);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "Middle Ring", new Vector3(0f, 0.82f, 0f), new Vector3(0.72f, 0.06f, 0.72f), glow);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "Central Core", new Vector3(0f, 1.46f, 0f), new Vector3(0.52f, 0.52f, 0.52f), glow);
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

            CreateChildPrimitive(root, PrimitiveType.Capsule, "Torso", new Vector3(0f, torsoHeight, 0f), new Vector3(torsoWidth, 0.42f, torsoDepth), armor);
            CreateChildPrimitive(root, PrimitiveType.Sphere, "Head", new Vector3(0f, torsoHeight + 0.54f, 0.04f), new Vector3(headScale, headScale, headScale), skin);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Visor", new Vector3(0f, torsoHeight + 0.56f, 0.18f), new Vector3(0.12f, 0.04f, 0.08f), cloth);
            CreateChildPrimitive(root, PrimitiveType.Cube, "Hip Guard", new Vector3(0f, torsoHeight - 0.28f, 0f), new Vector3(torsoWidth + 0.04f, 0.14f, torsoDepth), cloth);
            CreateChildPrimitive(root, PrimitiveType.Capsule, "Left Leg", new Vector3(-legGap, 0.42f, 0f), new Vector3(0.14f, 0.34f, 0.14f), cloth);
            CreateChildPrimitive(root, PrimitiveType.Capsule, "Right Leg", new Vector3(legGap, 0.42f, 0f), new Vector3(0.14f, 0.34f, 0.14f), cloth);
            CreateChildPrimitive(root, PrimitiveType.Capsule, "Left Arm", new Vector3(-armOffset, torsoHeight + 0.08f, 0f), new Vector3(0.12f, 0.28f, 0.12f), armor);
            CreateChildPrimitive(root, PrimitiveType.Capsule, "Right Arm", new Vector3(armOffset, torsoHeight + 0.08f, 0f), new Vector3(0.12f, 0.28f, 0.12f), armor);
        }

        private static void BuildBaseSilhouette(Transform root, Color secondaryColor, Color accentColor)
        {
            CreateChildPrimitive(root, PrimitiveType.Cube, "Foundation", new Vector3(0f, -1.25f, 0f), new Vector3(1.34f, 0.16f, 1.34f), secondaryColor);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "Core Beacon", new Vector3(0f, 2.02f, 0f), new Vector3(0.14f, 0.54f, 0.14f), accentColor);
        }

        private static void BuildProductionSilhouette(Transform root, Color secondaryColor, Color accentColor, bool isSiegeStructure)
        {
            CreateChildPrimitive(root, PrimitiveType.Cube, "Base Plinth", new Vector3(0f, -1.02f, 0f), new Vector3(1.34f, 0.12f, 1.34f), secondaryColor);
            CreateChildPrimitive(root, PrimitiveType.Cylinder, "Assembly Pad", new Vector3(0f, -0.9f, 0f), new Vector3(1.08f, 0.05f, 1.08f), accentColor);
            if (isSiegeStructure)
            {
                CreateChildPrimitive(root, PrimitiveType.Cylinder, "Siege Tower", new Vector3(0f, 1.9f, 0f), new Vector3(0.14f, 0.76f, 0.14f), accentColor);
            }
        }

        private static void BuildTurretSilhouette(Transform root, Color accentColor)
        {
            CreateChildPrimitive(root, PrimitiveType.Cube, "Turret Barrel", new Vector3(0f, 0.46f, 0.94f), new Vector3(0.18f, 0.1f, 0.66f), accentColor);
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
                CreateChildPrimitive(root, PrimitiveType.Cylinder, "Signal Mast", new Vector3(0f, markerBaseHeight, markerDepth), new Vector3(0.05f, mastHeight, 0.05f), bannerColor);
                GameObject banner = CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Banner", new Vector3(bannerWidth * 0.42f, markerBaseHeight + mastHeight * 0.58f, markerDepth), new Vector3(bannerWidth, mastHeight * 0.34f, 0.04f), accentColor);
                banner.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);
                CreateChildPrimitive(root, PrimitiveType.Sphere, "Signal Beacon", new Vector3(0f, markerBaseHeight + mastHeight * 1.22f, markerDepth), new Vector3(0.12f, 0.12f, 0.12f), accentColor);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Wing Left", new Vector3(-bannerWidth * 0.32f, markerBaseHeight + mastHeight * 0.28f, markerDepth - 0.02f), new Vector3(0.08f, 0.05f, bannerWidth * 0.78f), bannerColor);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Wing Right", new Vector3(bannerWidth * 0.32f, markerBaseHeight + mastHeight * 0.28f, markerDepth - 0.02f), new Vector3(0.08f, 0.05f, bannerWidth * 0.78f), bannerColor);
                return;
            }

            CreateChildPrimitive(root, PrimitiveType.Cylinder, "War Pike", new Vector3(0f, markerBaseHeight, markerDepth), new Vector3(0.05f, mastHeight, 0.05f), bannerColor);
            CreateChildPrimitive(root, PrimitiveType.Cube, "War Crest", new Vector3(0f, markerBaseHeight + mastHeight * 0.64f, markerDepth), new Vector3(bannerWidth * 0.9f, mastHeight * 0.28f, 0.04f), accentColor);
            GameObject hornLeft = CreateChildPrimitive(root, PrimitiveType.Cube, "War Horn Left", new Vector3(-bannerWidth * 0.34f, markerBaseHeight + mastHeight * 1.04f, markerDepth), new Vector3(0.08f, mastHeight * 0.62f, 0.08f), accentColor);
            hornLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 30f);
            GameObject hornRight = CreateChildPrimitive(root, PrimitiveType.Cube, "War Horn Right", new Vector3(bannerWidth * 0.34f, markerBaseHeight + mastHeight * 1.04f, markerDepth), new Vector3(0.08f, mastHeight * 0.62f, 0.08f), accentColor);
            hornRight.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
        }

        private static void BuildStructureFactionSignature(Transform root, UnitTeam team, Color accentColor, float topHeight, float span)
        {
            Color supportColor = team == UnitTeam.Player ? new Color(0.78f, 0.9f, 1f) : new Color(0.42f, 0.14f, 0.08f);

            if (team == UnitTeam.Player)
            {
                CreateChildPrimitive(root, PrimitiveType.Cylinder, "Signal Tower", new Vector3(0f, topHeight, 0f), new Vector3(0.08f, 0.42f, 0.08f), supportColor);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Bridge", new Vector3(0f, topHeight + 0.22f, 0f), new Vector3(span, 0.08f, 0.12f), accentColor);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Fin Left", new Vector3(-span * 0.46f, topHeight + 0.06f, 0f), new Vector3(0.12f, 0.22f, 0.08f), supportColor);
                CreateChildPrimitive(root, PrimitiveType.Cube, "Signal Fin Right", new Vector3(span * 0.46f, topHeight + 0.06f, 0f), new Vector3(0.12f, 0.22f, 0.08f), supportColor);
                CreateChildPrimitive(root, PrimitiveType.Sphere, "Signal Core", new Vector3(0f, topHeight + 0.44f, 0f), new Vector3(0.18f, 0.18f, 0.18f), accentColor);
                return;
            }

            CreateChildPrimitive(root, PrimitiveType.Cylinder, "War Spire", new Vector3(0f, topHeight, 0f), new Vector3(0.08f, 0.48f, 0.08f), supportColor);
            CreateChildPrimitive(root, PrimitiveType.Cube, "War Crest", new Vector3(0f, topHeight + 0.2f, 0f), new Vector3(span * 0.84f, 0.08f, 0.1f), accentColor);
            GameObject spikeLeft = CreateChildPrimitive(root, PrimitiveType.Cube, "War Spike Left", new Vector3(-span * 0.34f, topHeight + 0.4f, 0f), new Vector3(0.1f, 0.34f, 0.1f), accentColor);
            spikeLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
            GameObject spikeRight = CreateChildPrimitive(root, PrimitiveType.Cube, "War Spike Right", new Vector3(span * 0.34f, topHeight + 0.4f, 0f), new Vector3(0.1f, 0.34f, 0.1f), accentColor);
            spikeRight.transform.localRotation = Quaternion.Euler(0f, 0f, -22f);
        }

        private static GameObject CreateChildPrimitive(Transform parent, PrimitiveType primitiveType, string objectName, Vector3 localPosition, Vector3 localScale, Color color)
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

            child.GetComponent<Renderer>().material.color = color;
            return child;
        }
    }
}
