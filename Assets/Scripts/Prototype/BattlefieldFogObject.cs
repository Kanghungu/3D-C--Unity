using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    public enum BattlefieldFogRequirement
    {
        Explored,
        Visible
    }

    /// <summary>
    /// Lightweight visibility target for prototype fog-of-war style hiding.
    /// </summary>
    public class BattlefieldFogObject : MonoBehaviour
    {
        private static readonly HashSet<BattlefieldFogObject> registeredObjects = new();

        [SerializeField] private BattlefieldFogRequirement requirement = BattlefieldFogRequirement.Explored;

        private Renderer[] cachedRenderers;
        private bool isFogged;

        public BattlefieldFogRequirement Requirement => requirement;
        public Vector3 WorldPosition => transform.position;
        public bool IsFogged => isFogged;
        public static IReadOnlyCollection<BattlefieldFogObject> RegisteredObjects => registeredObjects;

        public void Configure(BattlefieldFogRequirement newRequirement)
        {
            requirement = newRequirement;
        }

        private void Awake()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
        }

        private void OnEnable()
        {
            if (cachedRenderers == null || cachedRenderers.Length == 0)
            {
                cachedRenderers = GetComponentsInChildren<Renderer>(true);
            }

            registeredObjects.Add(this);
        }

        private void OnDisable()
        {
            registeredObjects.Remove(this);
        }

        public void SetFogged(bool fogged)
        {
            if (isFogged == fogged)
            {
                return;
            }

            isFogged = fogged;

            foreach (Renderer rendererComponent in cachedRenderers)
            {
                if (rendererComponent != null)
                {
                    rendererComponent.enabled = !fogged;
                }
            }
        }
    }
}
