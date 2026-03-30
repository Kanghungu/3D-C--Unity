using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared primitive-based silhouettes and faction visual helpers for runtime prototype entities.
    /// </summary>
    public static class PrototypeEntityVisualFactory
    {
        public static void ApplyBaseVisuals(GameObject target, UnitTeam team)
        {
            Color primaryColor = team == UnitTeam.Player ? new Color(0.56f, 0.64f, 0.78f) : new Color(0.66f, 0.29f, 0.2f);
            Color secondaryColor = team == UnitTeam.Player ? new Color(0.86f, 0.82f, 0.66f) : new Color(0.32f, 0.08f, 0.06f);
            Color accentColor = team == UnitTeam.Player ? new Color(0.34f, 0.9f, 1f) : new Color(1f, 0.52f, 0.22f);
            target.GetComponent<Renderer>().material.color = primaryColor;
            BuildBaseSilhouette(target.transform, secondaryColor, accentColor);
        }

        public static void ApplyProductionVisuals(GameObject target, UnitTeam team, bool isSiegeStructure)
        {
            Color primaryColor = team == UnitTeam.Player ? new Color(0.78f, 0.72f, 0.58f) : new Color(0.8f, 0.38f, 0.22f);
            Color secondaryColor = team == UnitTeam.Player ? new Color(0.95f, 0.86f, 0.62f) : new Color(0.34f, 0.14f, 0.08f);
            Color accentColor = team == UnitTeam.Player ? new Color(0.22f, 0.85f, 0.95f) : new Color(1f, 0.58f, 0.22f);
            target.GetComponent<Renderer>().material.color = primaryColor;
            BuildProductionSilhouette(target.transform, secondaryColor, accentColor, isSiegeStructure);
        }

        public static void ApplyTurretVisuals(GameObject target, UnitTeam team)
        {
            Color primaryColor = team == UnitTeam.Player ? new Color(0.74f, 0.79f, 0.88f) : new Color(0.92f, 0.42f, 0.22f);
            Color accentColor = team == UnitTeam.Player ? new Color(0.24f, 0.9f, 1f) : new Color(0.35f, 0.08f, 0.06f);
            target.GetComponent<Renderer>().material.color = primaryColor;
            BuildTurretSilhouette(target.transform, accentColor);
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
