using Game.Prototype;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 비클래식 전장 전용 — 얕은 사구·능선으로 평면 플레인 가독성 보강 (콜라이더로 NavMesh 반영).
    /// ClassicDuel 은 <see cref="BattleAcesDemoStagePresentation"/> 가 담당하므로 여기서는 스킵.
    /// </summary>
    public static class BattleAcesArenaTerrainReadability
    {
        private const string RootName = "BA_ArenaLightDunes";

        /// <param name="arenaScale">부트스트랩 Plane 스케일 — 맵이 커지면 코너 사구를 조금 더 밖으로</param>
        public static void SpawnLightDunes(Transform structuresParent, Vector3 arenaScale, BattleArenaLayoutKind kind, bool mirrorX)
        {
            float planeSpan = Mathf.Max(arenaScale.x, arenaScale.z);
            float cornerDist = Mathf.Lerp(48f, 58f, Mathf.InverseLerp(30f, 40f, planeSpan));

            if (structuresParent == null || kind == BattleArenaLayoutKind.ClassicDuel)
            {
                return;
            }

            if (structuresParent.Find(RootName) != null)
            {
                return;
            }

            GameObject rootGo = new GameObject(RootName);
            rootGo.transform.SetParent(structuresParent, false);
            Transform root = rootGo.transform;

            Color earth = BattleAcesArtDirection.TerrainBerm;

            if (kind == BattleArenaLayoutKind.CrossroadsSpirit)
            {
                // 모서리 사구 — 중앙 십자·벽 블록과 떨어진 코너
                float c = cornerDist;
                AddLightBerm(root, "LightDune_NW", M(new Vector3(-c, 0.38f, c), mirrorX), Quaternion.Euler(0f, mirrorX ? 38f : -38f, 0f), new Vector3(18f, 0.76f, 9f), earth);
                AddLightBerm(root, "LightDune_NE", M(new Vector3(c, 0.38f, c), mirrorX), Quaternion.Euler(0f, mirrorX ? -32f : 32f, 0f), new Vector3(17f, 0.72f, 8.5f), earth);
                AddLightBerm(root, "LightDune_SW", M(new Vector3(-c, 0.32f, -c), mirrorX), Quaternion.Euler(0f, mirrorX ? -28f : 28f, 0f), new Vector3(20f, 0.64f, 7.5f), earth);
                AddLightBerm(root, "LightDune_SE", M(new Vector3(c, 0.32f, -c), mirrorX), Quaternion.Euler(0f, mirrorX ? 22f : -22f, 0f), new Vector3(19f, 0.64f, 8f), earth);
            }
            else if (kind == BattleArenaLayoutKind.NarrowMidChoke)
            {
                // 초크 벽(x≈±7) 밖 측면·남북 끝
                AddLightBerm(root, "LightDune_W", M(new Vector3(-26f, 0.3f, -22f), mirrorX), Quaternion.Euler(0f, 18f, 0f), new Vector3(12f, 0.6f, 20f), earth);
                AddLightBerm(root, "LightDune_E", M(new Vector3(26f, 0.3f, 22f), mirrorX), Quaternion.Euler(0f, -18f, 0f), new Vector3(12f, 0.6f, 20f), earth);
                AddLightBerm(root, "LightDune_N", M(new Vector3(-18f, 0.28f, 46f), mirrorX), Quaternion.identity, new Vector3(22f, 0.56f, 6f), earth);
                AddLightBerm(root, "LightDune_S", M(new Vector3(18f, 0.28f, -46f), mirrorX), Quaternion.identity, new Vector3(22f, 0.56f, 6f), earth);
            }
        }

        private static Vector3 M(Vector3 world, bool mirrorX)
        {
            if (mirrorX)
            {
                return new Vector3(-world.x, world.y, world.z);
            }

            return world;
        }

        private static void AddLightBerm(Transform parent, string objectName, Vector3 position, Quaternion rotation, Vector3 scale, Color color)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.SetPositionAndRotation(position, rotation);
            cube.transform.localScale = scale;

            if (cube.TryGetComponent(out Collider col))
            {
                col.enabled = true;
            }

            if (cube.TryGetComponent(out Renderer renderer))
            {
                ReadablePrimitiveMaterialUtility.Apply(renderer, color, ReadablePrimitiveMaterialUtility.EmissionSubtleBody * 0.6f);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(cube, BattlefieldFogRequirement.Explored);
        }
    }
}
