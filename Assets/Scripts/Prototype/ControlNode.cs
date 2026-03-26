using Game.Units;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Central battlefield objective that speeds up production for the controlling team.
    /// </summary>
    public class ControlNode : MonoBehaviour
    {
        [SerializeField] private float controlRadius = 7.5f;
        [SerializeField] private float captureDuration = 4f;
        [SerializeField] private float productionBonusMultiplier = 1.35f;

        private Renderer cachedRenderer;
        private UnitTeam? ownerTeam;
        private UnitTeam? capturingTeam;
        private float captureProgress;

        public string OwnerLabel => ownerTeam.HasValue ? ownerTeam.Value.ToString() : "Neutral";
        public float BonusMultiplier => productionBonusMultiplier;

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            ApplyVisuals();
        }

        private void Update()
        {
            UpdateCaptureState();
            ApplyProductionBonus();
        }

        private void UpdateCaptureState()
        {
            int playerCount = 0;
            int enemyCount = 0;

            foreach (SelectableUnit unit in FindObjectsByType<SelectableUnit>())
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
                captureProgress = Mathf.Max(0f, captureProgress - Time.deltaTime * 0.5f);
                return;
            }

            if (capturingTeam != nextCapturingTeam)
            {
                capturingTeam = nextCapturingTeam;
                captureProgress = 0f;
            }

            if (ownerTeam == capturingTeam)
            {
                captureProgress = captureDuration;
                return;
            }

            captureProgress += Time.deltaTime;

            if (captureProgress >= captureDuration)
            {
                ownerTeam = capturingTeam;
                captureProgress = captureDuration;
                ApplyVisuals();
            }
        }

        private void ApplyProductionBonus()
        {
            ProductionStructure playerProduction = PrototypeRuntimeQuery.FindProductionStructure(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);

            if (playerProduction != null)
            {
                playerProduction.SetProductionSpeedMultiplier(ownerTeam == UnitTeam.Player ? productionBonusMultiplier : 1f);
            }

            if (enemyBase != null)
            {
                enemyBase.SetProductionSpeedMultiplier(ownerTeam == UnitTeam.Enemy ? productionBonusMultiplier : 1f);
            }
        }

        private void ApplyVisuals()
        {
            if (cachedRenderer == null)
            {
                return;
            }

            cachedRenderer.material.color = ownerTeam switch
            {
                UnitTeam.Player => new Color(0.28f, 0.9f, 1f),
                UnitTeam.Enemy => new Color(1f, 0.42f, 0.22f),
                _ => new Color(0.75f, 0.75f, 0.78f)
            };
        }
    }
}
