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
        private float damage;
        private float travelDuration;
        private float travelTimer;
        private float arcHeight;
        private float splashRadius;
        private float impactEffectScale;
        private Vector3 startPosition;
        private Vector3 lastKnownTargetPosition;
        private Color impactColor;

        public void Initialize(
            CombatTarget assignedTarget,
            UnitTeam assignedOwnerTeam,
            float assignedDamage,
            float projectileSpeed,
            float assignedArcHeight,
            float assignedSplashRadius,
            float assignedImpactEffectScale,
            Color assignedImpactColor)
        {
            target = assignedTarget;
            ownerTeam = assignedOwnerTeam;
            damage = assignedDamage;
            arcHeight = assignedArcHeight;
            splashRadius = assignedSplashRadius;
            impactEffectScale = assignedImpactEffectScale;
            impactColor = assignedImpactColor;
            startPosition = transform.position;
            lastKnownTargetPosition = GetTargetPosition();
            float distance = Vector3.Distance(startPosition, lastKnownTargetPosition);
            travelDuration = projectileSpeed <= 0.01f ? 0.05f : Mathf.Max(0.08f, distance / projectileSpeed);
        }

        private void Update()
        {
            travelTimer += Time.deltaTime;
            lastKnownTargetPosition = GetTargetPosition();

            float normalized = travelDuration <= 0.001f ? 1f : Mathf.Clamp01(travelTimer / travelDuration);
            Vector3 flatPosition = Vector3.Lerp(startPosition, lastKnownTargetPosition, normalized);
            flatPosition.y += Mathf.Sin(normalized * Mathf.PI) * arcHeight;
            transform.position = flatPosition;

            if (normalized >= 1f)
            {
                Impact();
            }
        }

        private Vector3 GetTargetPosition()
        {
            if (target != null)
            {
                return target.transform.position + Vector3.up * 0.6f;
            }

            return lastKnownTargetPosition;
        }

        private void Impact()
        {
            Vector3 impactPoint = lastKnownTargetPosition;

            if (splashRadius > 0.01f)
            {
                foreach (CombatTarget candidate in FindObjectsByType<CombatTarget>())
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

                    float damageMultiplier = Mathf.Lerp(1f, 0.35f, Mathf.Clamp01(distance / splashRadius));
                    candidate.Health.ApplyDamage(damage * damageMultiplier);
                }
            }
            else if (target != null && target.IsAlive && target.Team != ownerTeam)
            {
                target.Health.ApplyDamage(damage);
            }

            UnitCombat.SpawnImpactEffect(impactPoint, impactEffectScale, impactColor);
            Destroy(gameObject);
        }
    }
}
