using Game.Audio;
using Game.CameraSystem;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 공격 명령·명중·사망·승패를 한 톤으로 묶은 전투 주스(카메라 쉐이크 + 짧은 월드 이펙트 + 프로시저럴 음).
    /// </summary>
    public static class BattleAcesCombatJuice
    {
        private static float nextHitSparkAllowedUnscaled = -999f;

        /// <summary>적 대상 공격 명령</summary>
        public static void NotifyAttackOrder(Vector3 worldPoint)
        {
            if (!IsBattleAcesMatchActive())
            {
                return;
            }

            ProceduralAudioUtility.PlayCombatAttackOrder();
            ImpulseCamera(0.11f);
            Color attackRing = BattleAcesArtDirection.EnemyEmber;
            attackRing.a = 0.95f;
            SpawnRingBurst(worldPoint, attackRing, 0.85f, 0.38f);
        }

        /// <summary>A+이동 등 공격 이동</summary>
        public static void NotifyAttackMoveOrder(Vector3 worldPoint)
        {
            if (!IsBattleAcesMatchActive())
            {
                return;
            }

            ProceduralAudioUtility.PlayCombatAttackMoveOrder();
            ImpulseCamera(0.075f);
            Color moveRing = Color.Lerp(BattleAcesArtDirection.EnemyEmber, new Color(1f, 0.72f, 0.28f, 1f), 0.35f);
            moveRing.a = 0.9f;
            SpawnRingBurst(worldPoint, moveRing, 0.72f, 0.32f);
        }

        /// <summary>유닛 피격(히트음 쿨다운이 있는 호출에서만 옴)</summary>
        public static void NotifyUnitHit(Vector3 worldPoint, float damageNormalized01, UnitTeam damagedTeam)
        {
            if (!IsBattleAcesMatchActive())
            {
                return;
            }

            if (Time.unscaledTime < nextHitSparkAllowedUnscaled)
            {
                ImpulseCamera(0.035f + damageNormalized01 * 0.045f);
                return;
            }

            nextHitSparkAllowedUnscaled = Time.unscaledTime + 0.055f;
            ImpulseCamera(0.05f + damageNormalized01 * 0.09f);
            Color spark = damagedTeam == UnitTeam.Player
                ? Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.18f)
                : Color.Lerp(BattleAcesArtDirection.EnemyEmber, Color.white, 0.12f);
            SpawnHitSparks(worldPoint + Vector3.up * 0.35f, spark, Mathf.Lerp(0.65f, 1.15f, damageNormalized01));
        }

        /// <summary>유닛 사망 — 오디오는 BattleAcesCombatAudio 가 담당</summary>
        public static void NotifyUnitDeath(Vector3 worldPoint, UnitTeam team)
        {
            if (!IsBattleAcesMatchActive())
            {
                return;
            }

            ImpulseCamera(team == UnitTeam.Player ? 0.22f : 0.16f);
            Color c = team == UnitTeam.Player
                ? Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.5f, 0.55f, 0.62f, 1f), 0.22f)
                : Color.Lerp(BattleAcesArtDirection.EnemyEmber, BattleAcesArtDirection.GunmetalMid, 0.25f);
            SpawnDeathPop(worldPoint, c);
        }

        /// <summary>승패 스팅과 동시에 강한 쉐이크(오디오는 기존 경로)</summary>
        public static void NotifyMatchResult(bool victory)
        {
            ImpulseCamera(victory ? 0.58f : 0.48f);
        }

        private static bool IsBattleAcesMatchActive()
        {
            return BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController m) && !m.IsFinished;
        }

        private static void ImpulseCamera(float impulse01)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            RTSCameraController rts = cam.GetComponent<RTSCameraController>();
            rts?.AddCombatShake(impulse01);
        }

        private static void SpawnRingBurst(Vector3 groundPoint, Color color, float ringScale, float lifetime)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Juice_CommandRing";
            ring.transform.position = new Vector3(groundPoint.x, 0.11f, groundPoint.z);
            ring.transform.localScale = new Vector3(ringScale, 0.04f, ringScale);
            Collider col = ring.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            Renderer r = ring.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = color;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            TimedWorldEffect fx = ring.AddComponent<TimedWorldEffect>();
            fx.Configure(lifetime, new Vector3(ringScale * 1.65f, 0.02f, ringScale * 1.65f), Vector3.zero);
        }

        private static void SpawnHitSparks(Vector3 center, Color color, float intensity)
        {
            int count = Mathf.Clamp(Mathf.RoundToInt(intensity * 4f), 3, 6);
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.2f, 0.2f);
                Vector3 offset = new Vector3(Mathf.Cos(ang) * 0.15f, 0f, Mathf.Sin(ang) * 0.15f);
                GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
                s.name = "Juice_HitSpark";
                s.transform.position = center + offset;
                s.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.06f, 0.11f) * intensity;
                s.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0f, 90f), ang * Mathf.Rad2Deg, UnityEngine.Random.Range(0f, 90f));
                Collider c = s.GetComponent<Collider>();
                if (c != null)
                {
                    c.enabled = false;
                }

                Renderer ren = s.GetComponent<Renderer>();
                if (ren != null)
                {
                    ren.material.color = Color.Lerp(color, Color.white, 0.25f);
                    ren.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                Vector3 drift = new Vector3(Mathf.Cos(ang) * 2.4f, UnityEngine.Random.Range(1.2f, 2.2f), Mathf.Sin(ang) * 2.4f);
                TimedWorldEffect fx = s.AddComponent<TimedWorldEffect>();
                fx.Configure(UnityEngine.Random.Range(0.14f, 0.2f), Vector3.zero, drift);
            }
        }

        private static void SpawnDeathPop(Vector3 basePos, Color teamColor)
        {
            GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            burst.name = "Juice_DeathPop";
            burst.transform.position = basePos + Vector3.up * 0.45f;
            burst.transform.localScale = Vector3.one * 0.28f;
            Collider col = burst.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            Renderer r = burst.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = teamColor;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            TimedWorldEffect fx = burst.AddComponent<TimedWorldEffect>();
            fx.Configure(0.35f, Vector3.one * 0.85f, Vector3.up * 0.6f);

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Juice_DeathRing";
            ring.transform.position = new Vector3(basePos.x, 0.1f, basePos.z);
            ring.transform.localScale = new Vector3(0.35f, 0.02f, 0.35f);
            Collider rc = ring.GetComponent<Collider>();
            if (rc != null)
            {
                rc.enabled = false;
            }

            Renderer rr = ring.GetComponent<Renderer>();
            if (rr != null)
            {
                rr.material.color = new Color(teamColor.r, teamColor.g, teamColor.b, 0.75f);
                rr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            TimedWorldEffect rfx = ring.AddComponent<TimedWorldEffect>();
            rfx.Configure(0.45f, new Vector3(2.2f, 0.015f, 2.2f), Vector3.zero);
        }
    }
}
