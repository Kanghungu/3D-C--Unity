using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Strategic battlefield objective that can change production tempo, local sustain,
    /// and overall frontline pressure depending on its tier.
    /// </summary>
    public class ControlNode : MonoBehaviour
    {
        private static float nextProductionRefreshTime;

        [SerializeField] private float controlRadius = 18f;
        [SerializeField] private float captureDuration = 5.5f;
        [SerializeField] private float productionBonusMultiplier = 1.12f;
        [SerializeField] private float healingRadius = 18f;
        [SerializeField] private float healingPerSecond = 4.5f;
        [SerializeField] private string nodeLabel = "Holy Node";
        [SerializeField] private ControlNodeTier nodeTier = ControlNodeTier.Minor;

        private Renderer cachedRenderer;
        private UnitTeam? ownerTeam;
        private UnitTeam? capturingTeam;
        private float captureProgress;
        private float captureCheckTimer;
        private float healingTickTimer;

        public string OwnerLabel => ownerTeam.HasValue ? ownerTeam.Value.ToString() : "Neutral";
        public float BonusMultiplier => productionBonusMultiplier;
        public float HealingPerSecond => healingPerSecond;
        public string NodeLabel => string.IsNullOrWhiteSpace(nodeLabel) ? name : nodeLabel;
        public UnitTeam? OwnerTeam => ownerTeam;
        public float ControlRadius => controlRadius;
        public ControlNodeTier Tier => nodeTier;
        public bool IsGrand => nodeTier == ControlNodeTier.Grand;
        public bool IsMajor => nodeTier == ControlNodeTier.Major;
        public int StrategicWeight => nodeTier switch
        {
            ControlNodeTier.Grand => 4,
            ControlNodeTier.Major => 2,
            _ => 1
        };
        public int DesiredGarrison => nodeTier switch
        {
            ControlNodeTier.Grand => 26,
            ControlNodeTier.Major => 16,
            _ => 10
        };
        public int DesiredStrikeForce => nodeTier switch
        {
            ControlNodeTier.Grand => 36,
            ControlNodeTier.Major => 24,
            _ => 14
        };
        public string TierLabel => nodeTier switch
        {
            ControlNodeTier.Grand => "Grand",
            ControlNodeTier.Major => "Major",
            _ => "Minor"
        };

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();

            if (string.IsNullOrWhiteSpace(nodeLabel))
            {
                nodeLabel = name;
            }

            ApplyVisuals();
        }

        private void OnEnable()
        {
            PrototypeRuntimeRegistry.Register(this);
        }

        private void OnDisable()
        {
            PrototypeRuntimeRegistry.Unregister(this);
        }

        private void Update()
        {
            captureCheckTimer -= Time.deltaTime;
            healingTickTimer -= Time.deltaTime;

            if (captureCheckTimer <= 0f)
            {
                captureCheckTimer = 0.2f;
                UpdateCaptureState();
            }

            if (healingTickTimer <= 0f)
            {
                healingTickTimer = 0.25f;
                ApplyHealingAura(0.25f);
            }

            if (Time.time >= nextProductionRefreshTime)
            {
                nextProductionRefreshTime = Time.time + 0.4f;
                ApplyProductionBonus();
            }
        }

        public void ConfigureNode(string label, float newProductionBonusMultiplier, float newHealingPerSecond, ControlNodeTier tier)
        {
            nodeLabel = label;
            productionBonusMultiplier = newProductionBonusMultiplier;
            healingPerSecond = newHealingPerSecond;
            nodeTier = tier;
            ApplyVisuals();
        }

        private void UpdateCaptureState()
        {
            int playerCount = 0;
            int enemyCount = 0;

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, unit.transform.position) > controlRadius)
                {
                    continue;
                }

                if (unit.Team == UnitTeam.Player)
                {
                    playerCount++;
                }
                else if (unit.Team == UnitTeam.Enemy)
                {
                    enemyCount++;
                }
            }

            UnitTeam? nextCapturingTeam = null;

            if (playerCount > 0 && enemyCount == 0)
            {
                nextCapturingTeam = UnitTeam.Player;
            }
            else if (enemyCount > 0 && playerCount == 0)
            {
                nextCapturingTeam = UnitTeam.Enemy;
            }

            if (!nextCapturingTeam.HasValue)
            {
                captureProgress = Mathf.Max(0f, captureProgress - 0.1f);
                return;
            }

            if (capturingTeam != nextCapturingTeam)
            {
                capturingTeam = nextCapturingTeam;
                captureProgress = 0f;
            }

            if (ownerTeam == capturingTeam)
            {
                captureProgress = GetCaptureThreshold();
                return;
            }

            captureProgress += 0.2f;

            if (captureProgress >= GetCaptureThreshold())
            {
                ownerTeam = capturingTeam;
                captureProgress = GetCaptureThreshold();
                ApplyVisuals();
                ApplyProductionBonus();
                nextProductionRefreshTime = Time.time + 0.4f;
            }
        }

        private float GetCaptureThreshold()
        {
            return captureDuration * (nodeTier switch
            {
                ControlNodeTier.Grand => 1.45f,
                ControlNodeTier.Major => 1.18f,
                _ => 1f
            });
        }

        private void ApplyProductionBonus()
        {
            float playerMultiplier = CalculateTeamProductionMultiplier(UnitTeam.Player);
            float enemyMultiplier = CalculateTeamProductionMultiplier(UnitTeam.Enemy);

            foreach (ProductionStructure structure in PrototypeRuntimeRegistry.GetProductionStructures())
            {
                if (structure == null)
                {
                    continue;
                }

                if (structure.Team == UnitTeam.Player)
                {
                    structure.SetProductionSpeedMultiplier(playerMultiplier);
                }
                else if (structure.Team == UnitTeam.Enemy)
                {
                    structure.SetProductionSpeedMultiplier(enemyMultiplier);
                }
            }

            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);

            if (playerBase != null)
            {
                playerBase.SetProductionSpeedMultiplier(playerMultiplier);
            }

            if (enemyBase != null)
            {
                enemyBase.SetProductionSpeedMultiplier(enemyMultiplier);
            }
        }

        private float CalculateTeamProductionMultiplier(UnitTeam team)
        {
            float multiplier = 1f;

            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node != null && node.OwnerTeam == team)
                {
                    multiplier *= node.BonusMultiplier;
                }
            }

            return multiplier;
        }

        private void ApplyHealingAura(float tickDuration)
        {
            if (!ownerTeam.HasValue)
            {
                return;
            }

            float healAmount = healingPerSecond * tickDuration;

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null || unit.Team != ownerTeam.Value)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, unit.transform.position) > healingRadius)
                {
                    continue;
                }

                UnitHealth health = unit.GetComponent<UnitHealth>();
                health?.Heal(healAmount);
            }
        }

        private void ApplyVisuals()
        {
            if (cachedRenderer == null)
            {
                return;
            }

            Color baseColor = ownerTeam switch
            {
                UnitTeam.Player => new Color(0.28f, 0.9f, 1f),
                UnitTeam.Enemy => new Color(1f, 0.42f, 0.22f),
                _ => new Color(0.75f, 0.75f, 0.78f)
            };

            Color tierAccent = nodeTier switch
            {
                ControlNodeTier.Grand => new Color(1f, 0.92f, 0.52f),
                ControlNodeTier.Major => new Color(0.92f, 0.84f, 0.64f),
                _ => baseColor
            };

            cachedRenderer.material.color = Color.Lerp(baseColor, tierAccent, nodeTier == ControlNodeTier.Minor ? 0f : 0.25f);

            float scale = nodeTier switch
            {
                ControlNodeTier.Grand => 1.35f,
                ControlNodeTier.Major => 1.16f,
                _ => 1f
            };
            transform.localScale = new Vector3(scale, transform.localScale.y, scale);
        }
    }
}
