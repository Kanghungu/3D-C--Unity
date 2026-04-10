using Game.Campaign.Data;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 미션의 <see cref="MissionDefinition.ArenaLayoutKind"/> 에 따라 지면 스케일·코어·집결·장애물을 적용.
    /// </summary>
    public static class BattleArenaLayoutBootstrap
    {
        public static BattleArenaLayoutKind ResolveLayoutKind(MissionDefinition mission)
        {
            if (mission == null)
            {
                return BattleArenaLayoutKind.ClassicDuel;
            }

            if (DemoChapter6SingleMatchBundle.Matches(mission))
            {
                return BattleArenaLayoutKind.ClassicDuel;
            }

            return mission.ArenaLayoutKind;
        }

        /// <summary>부트스트랩 인스펙터 기본값을 Classic 기준으로 두고, 다른 레이아웃만 덮어씀.</summary>
        public static void ApplySpawnAndScale(
            MissionDefinition mission,
            Vector3 defaultScale,
            Vector3 defaultPlayer,
            Vector3 defaultEnemy,
            Vector3 defaultRally,
            out Vector3 arenaScale,
            out Vector3 playerCore,
            out Vector3 enemyCore,
            out Vector3 rally)
        {
            switch (ResolveLayoutKind(mission))
            {
                case BattleArenaLayoutKind.CrossroadsSpirit:
                    // 투혼 느낌: 조금 더 큰 판, 모서리 쪽 본진, 중앙은 십자 복도로 열림
                    arenaScale = new Vector3(38f, 1f, 38f);
                    playerCore = new Vector3(-42f, 1.6f, -36f);
                    enemyCore = new Vector3(42f, 1.6f, 36f);
                    rally = new Vector3(-30f, 1f, -26f);
                    break;
                case BattleArenaLayoutKind.NarrowMidChoke:
                    // 중앙 폭 좁힘 — 돌파/방어 리듬이 달라짐
                    arenaScale = new Vector3(32f, 1f, 34f);
                    playerCore = new Vector3(-34f, 1.6f, -29f);
                    enemyCore = new Vector3(34f, 1.6f, 29f);
                    rally = new Vector3(-22f, 1f, -19f);
                    break;
                default:
                    arenaScale = defaultScale;
                    playerCore = defaultPlayer;
                    enemyCore = defaultEnemy;
                    rally = defaultRally;
                    break;
            }
        }

        /// <summary>내비·시야(FoW) 겸용 장애물 — VisionObstacle 레이어가 없으면 Default 유지.</summary>
        public static void SpawnLayoutObstacles(Transform structuresParent, BattleArenaLayoutKind kind, bool mirrorX)
        {
            if (kind == BattleArenaLayoutKind.ClassicDuel)
            {
                return;
            }

            GameObject root = new GameObject("BA_ArenaLayoutObstacles");
            root.transform.SetParent(structuresParent, false);

            void AddWall(string objectName, Vector3 worldPos, Vector3 scale, Color color)
            {
                if (mirrorX)
                {
                    worldPos.x *= -1f;
                }

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = objectName;
                cube.transform.SetParent(root.transform, false);
                cube.transform.position = worldPos;
                cube.transform.localScale = scale;
                Renderer renderer = cube.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // 팔레트·발광 상한은 데모 무대와 동일 계열(ReadablePrimitiveMaterialUtility)
                    Color wall = Color.Lerp(color, BattleAcesArtDirection.AshStone, 0.35f);
                    ReadablePrimitiveMaterialUtility.Apply(
                        renderer,
                        wall,
                        ReadablePrimitiveMaterialUtility.EmissionSubtleBody * 0.5f);
                }

                int vis = LayerMask.NameToLayer("VisionObstacle");
                if (vis >= 0)
                {
                    cube.layer = vis;
                }

                // 상단 얇은 림 — 실루엣만 살림(티얼 외 새 색 없음)
                Color rimAlbedo = Color.Lerp(color, BattleAcesArtDirection.GunmetalLift, 0.45f);
                float topY = worldPos.y + scale.y * 0.5f + 0.09f;
                GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rim.name = objectName + "_Rim";
                rim.transform.SetParent(root.transform, false);
                rim.transform.position = new Vector3(worldPos.x, topY, worldPos.z);
                rim.transform.localScale = new Vector3(scale.x * 0.96f, 0.14f, scale.z * 0.96f);
                if (rim.TryGetComponent(out Collider rimCol))
                {
                    rimCol.enabled = false;
                }

                if (rim.TryGetComponent(out Renderer rimR))
                {
                    ReadablePrimitiveMaterialUtility.Apply(
                        rimR,
                        rimAlbedo,
                        ReadablePrimitiveMaterialUtility.EmissionSubtleBody * 0.62f);
                }

                if (vis >= 0)
                {
                    rim.layer = vis;
                }
            }

            // 비클래식 레이아웃도 티얼 외 새 악센트 없이 잿빛 석재 축만
            Color rock = Color.Lerp(BattleAcesArtDirection.ObstacleSlab, BattleAcesArtDirection.TerrainBerm, 0.32f);
            const float y = 1.45f;

            if (kind == BattleArenaLayoutKind.CrossroadsSpirit)
            {
                // 남·북·동·서 벽 — 중앙 십자 교차로는 비움 (모서리에서 중앙으로 합류)
                AddWall("Cross_N", new Vector3(0f, y, 24f), new Vector3(20f, 2.9f, 5.5f), rock);
                AddWall("Cross_S", new Vector3(0f, y, -24f), new Vector3(20f, 2.9f, 5.5f), rock);
                AddWall("Cross_E", new Vector3(24f, y, 0f), new Vector3(5.5f, 2.9f, 20f), rock);
                AddWall("Cross_W", new Vector3(-24f, y, 0f), new Vector3(5.5f, 2.9f, 20f), rock);
                // 측면 압박 — 짧은 우회 구간
                AddWall("Flank_W", new Vector3(-32f, y, -6f), new Vector3(5f, 2.6f, 14f), rock);
                AddWall("Flank_E", new Vector3(32f, y, 6f), new Vector3(5f, 2.6f, 14f), rock);
            }
            else if (kind == BattleArenaLayoutKind.NarrowMidChoke)
            {
                AddWall("Choke_Left", new Vector3(-7f, y, 0f), new Vector3(5f, 3f, 56f), rock);
                AddWall("Choke_Right", new Vector3(7f, y, 0f), new Vector3(5f, 3f, 56f), rock);
            }
        }
    }
}
