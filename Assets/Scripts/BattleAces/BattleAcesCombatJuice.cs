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
        private static float nextHitSparkAllowedPlayerOrSelectedUnscaled = -999f;
        private static float nextHitSparkAllowedEnemyBulkUnscaled = -999f;

        /// <summary>적 대상 공격 명령</summary>
        public static void NotifyAttackOrder(Vector3 worldPoint)
        {
            if (!IsBattleAcesMatchActive())
            {
                return;
            }

            ProceduralAudioUtility.PlayCombatAttackOrder();
            ImpulseCamera(0.11f, 1.15f);
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
            ImpulseCamera(0.075f, 1f);
            Color moveRing = Color.Lerp(BattleAcesArtDirection.EnemyEmber, new Color(1f, 0.72f, 0.28f, 1f), 0.35f);
            moveRing.a = 0.9f;
            SpawnRingBurst(worldPoint, moveRing, 0.72f, 0.32f);
        }

        /// <summary>일반 이동 — 공격 이동보다 약한 링·쉐이크·전용 확인음</summary>
        public static void NotifyMoveOrder(Vector3 worldPoint)
        {
            if (!IsBattleAcesMatchActive())
            {
                return;
            }

            ProceduralAudioUtility.PlayCombatMoveOrder();
            ImpulseCamera(0.042f, 0.85f);
            Color ring = Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.55f, 0.82f, 0.95f, 1f), 0.35f);
            ring.a = 0.82f;
            SpawnRingBurst(worldPoint, ring, 0.58f, 0.28f);
        }

        /// <summary>유닛 피격(히트음 쿨다운이 있는 호출에서만 옴) — 출처 없음</summary>
        public static void NotifyUnitHit(
            Vector3 worldPoint,
            float damageNormalized01,
            UnitTeam damagedTeam,
            bool damagedUnitIsSelected = false)
        {
            NotifyUnitHit(worldPoint, damageNormalized01, damagedTeam, damagedUnitIsSelected, null);
        }

        /// <param name="damageFromWorld">피해 방향(공격자 쪽 월드 좌표) — null 이면 방사형 스파크</param>
        public static void NotifyUnitHit(
            Vector3 worldPoint,
            float damageNormalized01,
            UnitTeam damagedTeam,
            bool damagedUnitIsSelected,
            Vector3? damageFromWorld)
        {
            if (!IsBattleAcesMatchActive())
            {
                return;
            }

            // 아군·현재 선택 유닛은 스파크 쿨다운을 짧게 해 밀집 전투에서도 피격이 읽히게
            bool prioritySparks = damagedTeam == UnitTeam.Player || damagedUnitIsSelected;
            float sparkCooldown = prioritySparks ? 0.032f : 0.058f;
            float nextAllowed = prioritySparks
                ? nextHitSparkAllowedPlayerOrSelectedUnscaled
                : nextHitSparkAllowedEnemyBulkUnscaled;

            if (Time.unscaledTime < nextAllowed)
            {
                ImpulseCamera(0.035f + damageNormalized01 * 0.045f, 1.05f + damageNormalized01 * 0.4f);
                return;
            }

            float gateTime = Time.unscaledTime + sparkCooldown;
            if (prioritySparks)
            {
                nextHitSparkAllowedPlayerOrSelectedUnscaled = gateTime;
            }
            else
            {
                nextHitSparkAllowedEnemyBulkUnscaled = gateTime;
            }

            float shakeSharp = 1f + damageNormalized01 * 0.85f + (prioritySparks ? 0.25f : 0f);
            ImpulseCamera(0.05f + damageNormalized01 * 0.09f, shakeSharp);
            Color spark = damagedTeam == UnitTeam.Player
                ? Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.18f)
                : Color.Lerp(BattleAcesArtDirection.EnemyEmber, Color.white, 0.12f);
            float sparkBoost = prioritySparks ? 1.12f : 1f;
            float intensity = Mathf.Lerp(0.65f, 1.15f, damageNormalized01) * sparkBoost;
            Vector3 sparkCenter = worldPoint + Vector3.up * 0.35f;

            Vector3? outward = null;
            if (damageFromWorld.HasValue)
            {
                Vector3 flat = worldPoint - damageFromWorld.Value;
                flat.y = 0f;
                if (flat.sqrMagnitude > 0.0004f)
                {
                    outward = flat.normalized;
                }
            }

            if (outward.HasValue)
            {
                SpawnDirectionalHitSparks(sparkCenter, outward.Value, spark, intensity);
            }
            else
            {
                SpawnHitSparks(sparkCenter, spark, intensity);
            }

            // 강한 한 방 — 이중 링 + 아주 짧은 히트스톱(카메라 쪽에서 쿨다운)
            if (damageNormalized01 >= 0.14f)
            {
                Color ringC = Color.Lerp(spark, Color.white, 0.12f);
                ringC.a = 0.82f;
                SpawnRingBurst(worldPoint, ringC, Mathf.Lerp(0.52f, 0.78f, damageNormalized01), 0.2f);
            }

            if (damageNormalized01 >= 0.26f && prioritySparks)
            {
                RequestMicroHitstop(damageNormalized01);
            }
        }

        /// <summary>유닛 사망 — 오디오는 BattleAcesCombatAudio 가 담당</summary>
        public static void NotifyUnitDeath(Vector3 worldPoint, UnitTeam team)
        {
            if (!IsBattleAcesMatchActive())
            {
                return;
            }

            ImpulseCamera(team == UnitTeam.Player ? 0.22f : 0.16f, team == UnitTeam.Player ? 1.35f : 1.2f);
            Color c = team == UnitTeam.Player
                ? Color.Lerp(BattleAcesArtDirection.PointTeal, new Color(0.5f, 0.55f, 0.62f, 1f), 0.22f)
                : Color.Lerp(BattleAcesArtDirection.EnemyEmber, BattleAcesArtDirection.GunmetalMid, 0.25f);
            SpawnDeathPop(worldPoint, c);
            SpawnDeathDebrisBurst(worldPoint, c, team == UnitTeam.Player ? 9 : 8);
        }

        /// <summary>명중 지점에 짧은 링 — 임팩트 프리미티브와 겹쳐도 판독용으로 한 겹 추가</summary>
        public static void NotifyImpactAccentRing(Vector3 worldPoint, UnitTeam attackerTeam)
        {
            NotifyImpactAccentRing(worldPoint, attackerTeam, 1f);
        }

        /// <param name="impactIntensity01">1 초과 시 스플래시 등 강한 명중</param>
        public static void NotifyImpactAccentRing(Vector3 worldPoint, UnitTeam attackerTeam, float impactIntensity01)
        {
            if (!IsBattleAcesMatchActive())
            {
                return;
            }

            Color c = attackerTeam == UnitTeam.Player
                ? Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.15f)
                : Color.Lerp(BattleAcesArtDirection.EnemyEmber, Color.white, 0.1f);
            c.a = 0.88f;
            float scale = Mathf.Lerp(0.42f, 0.58f, Mathf.Clamp01(impactIntensity01 - 1f));
            float life = Mathf.Lerp(0.22f, 0.3f, Mathf.Clamp01(impactIntensity01 - 1f));
            SpawnRingBurst(worldPoint, c, scale, life);
            if (impactIntensity01 >= 1.12f)
            {
                Color c2 = Color.Lerp(c, Color.white, 0.22f);
                c2.a = 0.55f;
                SpawnRingBurst(worldPoint + Vector3.up * 0.04f, c2, scale * 1.45f, life * 1.1f);
                ImpulseCamera(0.055f * Mathf.Clamp01(impactIntensity01 - 1f), 1.25f);
            }
        }

        /// <summary>승패 스팅과 동시 짧은 쉐이크 — 승리 카메라 당김(<c>BattleAcesVictoryPresentation</c>)과 겹쳐도 과하지 않게 절제</summary>
        public static void NotifyMatchResult(bool victory)
        {
            ImpulseCamera(victory ? 0.42f : 0.38f, 1.4f);
        }

        private static bool IsBattleAcesMatchActive()
        {
            return BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController m) && !m.IsFinished;
        }

        private static void ImpulseCamera(float impulse01, float shakeSharpnessMul = 1f)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            RTSCameraController rts = cam.GetComponent<RTSCameraController>();
            rts?.AddCombatShake(impulse01, shakeSharpnessMul);
        }

        private static void RequestMicroHitstop(float severity01)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            RTSCameraController rts = cam.GetComponent<RTSCameraController>();
            rts?.TryCombatMicroHitstop(severity01);
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
                float ang = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
                Vector3 offset = new Vector3(Mathf.Cos(ang) * 0.15f, 0f, Mathf.Sin(ang) * 0.15f);
                GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
                s.name = "Juice_HitSpark";
                s.transform.position = center + offset;
                s.transform.localScale = Vector3.one * Random.Range(0.06f, 0.11f) * intensity;
                s.transform.rotation = Quaternion.Euler(Random.Range(0f, 90f), ang * Mathf.Rad2Deg, Random.Range(0f, 90f));
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

                Vector3 drift = new Vector3(Mathf.Cos(ang) * 2.4f, Random.Range(1.2f, 2.2f), Mathf.Sin(ang) * 2.4f);
                TimedWorldEffect fx = s.AddComponent<TimedWorldEffect>();
                fx.Configure(Random.Range(0.14f, 0.2f), Vector3.zero, drift);
            }
        }

        /// <summary>피해 방향으로 튀는 스파크 — 공격 출처 가독성</summary>
        private static void SpawnDirectionalHitSparks(Vector3 center, Vector3 outwardDirXZ, Color color, float intensity)
        {
            Vector3 dir = outwardDirXZ;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f)
            {
                SpawnHitSparks(center, color, intensity);
                return;
            }

            dir.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
            int count = Mathf.Clamp(Mathf.RoundToInt(intensity * 5f), 4, 8);
            for (int i = 0; i < count; i++)
            {
                float spread = (i / (float)count - 0.5f) * 1.1f;
                Vector3 offset = dir * 0.12f + side * (spread * 0.14f) + Vector3.up * Random.Range(0f, 0.08f);
                GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
                s.name = "Juice_HitSparkDir";
                s.transform.position = center + offset;
                s.transform.localScale = Vector3.one * Random.Range(0.055f, 0.1f) * intensity;
                Quaternion look = Quaternion.LookRotation(dir + Vector3.up * Random.Range(0.15f, 0.45f), Vector3.up);
                s.transform.rotation = look * Quaternion.Euler(Random.Range(0f, 80f), Random.Range(0f, 80f), 0f);
                Collider c = s.GetComponent<Collider>();
                if (c != null)
                {
                    c.enabled = false;
                }

                Renderer ren = s.GetComponent<Renderer>();
                if (ren != null)
                {
                    ren.material.color = Color.Lerp(color, Color.white, 0.2f);
                    ren.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                Vector3 drift = dir * Random.Range(2.8f, 4.2f) + Vector3.up * Random.Range(1.4f, 2.6f) + side * Random.Range(-0.6f, 0.6f);
                TimedWorldEffect fx = s.AddComponent<TimedWorldEffect>();
                fx.Configure(Random.Range(0.13f, 0.19f), Vector3.zero, drift);
            }
        }

        private static void SpawnDeathPop(Vector3 basePos, Color teamColor)
        {
            GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            burst.name = "Juice_DeathPop";
            burst.transform.position = basePos + Vector3.up * 0.45f;
            burst.transform.localScale = Vector3.one * 0.34f;
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
            fx.Configure(0.38f, Vector3.one * 0.92f, Vector3.up * 0.65f);

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Juice_DeathRing";
            ring.transform.position = new Vector3(basePos.x, 0.1f, basePos.z);
            ring.transform.localScale = new Vector3(0.42f, 0.02f, 0.42f);
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
            rfx.Configure(0.48f, new Vector3(2.55f, 0.015f, 2.55f), Vector3.zero);
        }

        /// <summary>사망 순간 금속 파편 느낌의 큐브 분출</summary>
        private static void SpawnDeathDebrisBurst(Vector3 basePos, Color teamColor, int pieceCount)
        {
            pieceCount = Mathf.Clamp(pieceCount, 5, 14);
            for (int i = 0; i < pieceCount; i++)
            {
                float ang = (i / (float)pieceCount) * Mathf.PI * 2f + Random.Range(-0.35f, 0.35f);
                Vector3 radial = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
                GameObject d = GameObject.CreatePrimitive(PrimitiveType.Cube);
                d.name = "Juice_DeathDebris";
                d.transform.position = basePos + Vector3.up * Random.Range(0.25f, 0.62f) + radial * Random.Range(0.05f, 0.14f);
                d.transform.localScale = Vector3.one * Random.Range(0.05f, 0.11f);
                d.transform.rotation = Quaternion.Euler(Random.Range(0f, 90f), ang * Mathf.Rad2Deg, Random.Range(0f, 90f));
                Collider dc = d.GetComponent<Collider>();
                if (dc != null)
                {
                    dc.enabled = false;
                }

                Renderer dr = d.GetComponent<Renderer>();
                if (dr != null)
                {
                    dr.material.color = Color.Lerp(teamColor, BattleAcesArtDirection.GunmetalMid, Random.Range(0.35f, 0.62f));
                    dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                Vector3 drift = radial * Random.Range(1.8f, 3.2f) + Vector3.up * Random.Range(1.1f, 2.4f);
                TimedWorldEffect dfx = d.AddComponent<TimedWorldEffect>();
                dfx.Configure(Random.Range(0.35f, 0.55f), Vector3.one * 0.03f, drift);
            }
        }
    }
}
