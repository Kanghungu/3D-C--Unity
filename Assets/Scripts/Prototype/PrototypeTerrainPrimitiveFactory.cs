using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared primitive-based terrain and battlefield prop creation helpers.
    /// Keeps simple visual assembly readable without bloating the bootstrapper.
    /// </summary>
    public static class PrototypeTerrainPrimitiveFactory
    {
        public static void CreateTerrainBlock(string objectName, Vector3 position, Vector3 scale, Transform parent, Color color)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = objectName;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.transform.SetParent(parent);
            block.GetComponent<Renderer>().material.color = color;
            EnsureFogObject(block, BattlefieldFogRequirement.Explored);
        }

        public static void CreateTerrainPillar(string objectName, Vector3 position, Transform parent, Color color)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = objectName;
            pillar.transform.position = position;
            pillar.transform.localScale = new Vector3(2.6f, 6.2f, 2.6f);
            pillar.transform.SetParent(parent);
            pillar.GetComponent<Renderer>().material.color = color;
            EnsureFogObject(pillar, BattlefieldFogRequirement.Explored);
        }

        public static void CreateBanner(string objectName, Vector3 position, Color bannerColor, Transform parent)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = objectName + " Pole";
            pole.transform.position = position;
            pole.transform.localScale = new Vector3(0.28f, 7.4f, 0.28f);
            pole.transform.SetParent(parent);
            pole.GetComponent<Renderer>().material.color = new Color(0.34f, 0.28f, 0.2f);

            GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = objectName;
            cloth.transform.position = position + new Vector3(3.8f, 3.6f, 0f);
            cloth.transform.localScale = new Vector3(7.6f, 4.2f, 0.16f);
            cloth.transform.SetParent(parent);
            cloth.GetComponent<Renderer>().material.color = bannerColor;

            EnsureFogObject(pole, BattlefieldFogRequirement.Explored);
            EnsureFogObject(cloth, BattlefieldFogRequirement.Explored);
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
    }
}
