using Game.BattleAces;
using Game.Prototype;
using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Lightweight projectile for ranged and artillery units.
    /// </summary>
    public class UnitProjectile : MonoBehaviour
    {
        private CombatTarget target;
        private UnitTeam ownerTeam;
        private UnitArchetype attackerArchetype;
        private float damage;
        private float travelDuration;
        private float travelTimer;
        private float arcHeight;
        private float splashRadius;
        private float impactEffectScale;
        private Vector3 startPosition;
        private Vector3 lastKnownTargetPosition;
        private Vector3 aimOffset;
        private Color impactColor;
        private Renderer coreRenderer;
        private Transform trailBody;
        private Renderer trailBodyRenderer;
        private Transform trailTip;
        private Renderer trailTipRenderer;
        private Transform ribbonTrail;
        private Renderer ribbonTrailRenderer;

        public void Initialize(CombatTarget assignedTarget, UnitTeam assignedOwnerTeam, float assignedDamage, float projectileSpeed, float assignedArcHeight, float assignedSplashRadius, float assignedImpactEffectScale, Color assignedImpactColor)
        {
            Initialize(assignedTarget, assignedOwnerTeam, UnitArchetype.Rifleman, assignedDamage, projectileSpeed, assignedArcHeight, assignedSplashRadius, assignedImpactEffectScale, assignedImpactColor, Vector3.zero);
        }

        public void Initialize(CombatTarget assignedTarget, UnitTeam assignedOwnerTeam, UnitArchetype assignedAttackerArchetype, float assignedDamage, float projectileSpeed, float assignedArcHeight, float assignedSplashRadius, float assignedImpactEffectScale, Color assignedImpactColor, Vector3 assignedAimOffset)
        {
            target = assignedTarget;
            ownerTeam = assignedOwnerTeam;
            attackerArchetype = assignedAttackerArchetype;
            damage = assignedDamage;
            arcHeight = assignedArcHeight;
            splashRadius = assignedSplashRadius;
            impactEffectScale = assignedImpactEffectScale;
            impactColor = assignedImpactColor;
            aimOffset = assignedAimOffset;
            startPosition = transform.position;
            lastKnownTargetPosition = GetTargetPosition();
            float distance = Vector3.Distance(startPosition, lastKnownTargetPosition);
            travelDuration = projectileSpeed <= 0.01f ? 0.05f : Mathf.Max(0.08f, distance / projectileSpeed);
            EnsureProjectileVisuals();
            UpdateProjectileVisuals(startPosition, lastKnownTargetPosition, 0f);
        }

        private void Update()
        {
            travelTimer += Time.deltaTime;
            Vector3 previousPosition = transform.position;
            lastKnownTargetPosition = GetTargetPosition();

            float normalized = travelDuration <= 0.001f ? 1f : Mathf.Clamp01(travelTimer / travelDuration);
            Vector3 flatPosition = Vector3.Lerp(startPosition, lastKnownTargetPosition, normalized);
            flatPosition.y += Mathf.Sin(normalized * Mathf.PI) * arcHeight;
            transform.position = flatPosition;
            UpdateProjectileVisuals(previousPosition, flatPosition, normalized);

            if (normalized >= 1f)
            {
                Impact();
            }
        }

        private Vector3 GetTargetPosition()
        {
            if (target != null)
            {
                return target.transform.position + Vector3.up * 0.6f + aimOffset;
            }

            return lastKnownTargetPosition;
        }

        private void Impact()
        {
            Vector3 impactPoint = lastKnownTargetPosition;

            if (splashRadius > 0.01f)
            {
                foreach (CombatTarget candidate in PrototypeRuntimeRegistry.GetCombatTargets())
                {
                    if (candidate == null || !candidate.IsAlive || candidate.Team == ownerTeam)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(candidate.transform.position, impactPoint);

                    if (distance > splashRadius)
                    {
                        continue;
                    }

                    float splashMultiplier = Mathf.Lerp(1f, 0.35f, Mathf.Clamp01(distance / splashRadius));
                    float resolvedDamage = CombatTriangleRules.ResolveDamage(attackerArchetype, candidate, damage * splashMultiplier, true);
                    candidate.Health.ApplyDamage(resolvedDamage, startPosition, fromProjectile: true);
                }
            }
            else if (target != null && target.IsAlive && target.Team != ownerTeam)
            {
                float resolvedDamage = CombatTriangleRules.ResolveDamage(attackerArchetype, target, damage, true);
                target.Health.ApplyDamage(resolvedDamage, startPosition, fromProjectile: true);
            }

            bool battleAcesActive = BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController ba) && !ba.IsFinished;
            float splashMul = splashRadius > 0.01f ? 1.18f : 1f;
            UnitCombat.SpawnImpactEffect(
                impactPoint,
                impactEffectScale,
                impactColor,
                battleAcesActive ? 1.15f * splashMul : 1f,
                battleAcesActive ? 1.12f : 1f,
                directMelee: false);
            if (battleAcesActive)
            {
                float ringBoost = splashRadius > 0.01f ? 1.32f : 1f;
                BattleAcesCombatJuice.NotifyImpactAccentRing(impactPoint, ownerTeam, ringBoost);
            }

            Destroy(gameObject);
        }

        private void EnsureProjectileVisuals()
        {
            if (coreRenderer == null)
            {
                coreRenderer = GetComponent<Renderer>();
            }

            if (coreRenderer != null)
            {
                coreRenderer.material.color = Color.Lerp(impactColor, Color.white, 0.16f);
            }

            if (trailBody == null)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Trail Body";
                body.transform.SetParent(transform);
                body.transform.localRotation = Quaternion.identity;
                body.GetComponent<Collider>().enabled = false;
                trailBody = body.transform;
                trailBodyRenderer = body.GetComponent<Renderer>();
            }

            if (trailTip == null)
            {
                GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tip.name = "Trail Tip";
                tip.transform.SetParent(transform);
                tip.transform.localRotation = Quaternion.identity;
                tip.GetComponent<Collider>().enabled = false;
                trailTip = tip.transform;
                trailTipRenderer = tip.GetComponent<Renderer>();
            }

            if (ribbonTrail == null)
            {
                GameObject ribbon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ribbon.name = "Trail Ribbon";
                ribbon.transform.SetParent(transform);
                ribbon.transform.localRotation = Quaternion.identity;
                ribbon.GetComponent<Collider>().enabled = false;
                ribbonTrail = ribbon.transform;
                ribbonTrailRenderer = ribbon.GetComponent<Renderer>();
                if (ribbonTrailRenderer != null)
                {
                    ribbonTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        private void UpdateProjectileVisuals(Vector3 previousPosition, Vector3 currentPosition, float normalized)
        {
            Vector3 direction = currentPosition - previousPosition;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = lastKnownTargetPosition - currentPosition;
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }

            float pulse = 0.88f + Mathf.PingPong(Time.time * 8f, 0.18f);
            bool battleAcesJuice = BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController baVis) && !baVis.IsFinished;
            float trailBoost = battleAcesJuice ? 1.48f : 1f;
            float trailLength = Mathf.Lerp(0.22f, 0.82f, Mathf.Clamp01(direction.magnitude * 6f + splashRadius * 0.18f)) * trailBoost;
            float trailWidth = Mathf.Clamp(0.08f + splashRadius * 0.05f + impactEffectScale * 0.02f, 0.08f, 0.22f);
            float lift = 0.02f + Mathf.Sin(normalized * Mathf.PI) * 0.03f;

            transform.localScale = Vector3.one * Mathf.Lerp(0.16f, 0.28f + splashRadius * 0.06f, pulse);

            if (coreRenderer != null)
            {
                coreRenderer.material.color = Color.Lerp(impactColor, Color.white, 0.22f) * pulse;
            }

            if (trailBody != null)
            {
                trailBody.localPosition = new Vector3(0f, lift, -trailLength * 0.5f);
                trailBody.localScale = new Vector3(trailWidth, trailWidth * 0.72f, trailLength);
            }

            if (trailBodyRenderer != null)
            {
                trailBodyRenderer.material.color = new Color(impactColor.r, impactColor.g, impactColor.b, 0.82f) * (0.82f + pulse * 0.18f);
            }

            if (trailTip != null)
            {
                trailTip.localPosition = new Vector3(0f, lift, -trailLength);
                trailTip.localScale = Vector3.one * Mathf.Max(0.06f, trailWidth * 0.9f);
            }

            if (trailTipRenderer != null)
            {
                trailTipRenderer.material.color = Color.Lerp(impactColor, Color.white, 0.1f) * 0.72f;
            }

            if (ribbonTrail != null)
            {
                ribbonTrail.gameObject.SetActive(battleAcesJuice);
                if (battleAcesJuice)
                {
                    ribbonTrail.localPosition = new Vector3(0f, lift * 0.88f, -trailLength * 0.58f);
                    ribbonTrail.localScale = new Vector3(trailWidth * 0.38f, trailWidth * 0.18f, trailLength * 1.05f);
                    if (ribbonTrailRenderer != null)
                    {
                        Color rc = new Color(impactColor.r, impactColor.g, impactColor.b, 0.42f);
                        ribbonTrailRenderer.material.color = rc * (0.75f + pulse * 0.25f);
                    }
                }
            }
        }
    }
}
