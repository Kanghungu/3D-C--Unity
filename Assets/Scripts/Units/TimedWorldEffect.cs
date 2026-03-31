using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Lightweight primitive effect that fades, scales, drifts, and destroys itself.
    /// Keeps battlefield feedback simple and runtime-friendly for the prototype.
    /// </summary>
    public class TimedWorldEffect : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.8f;
        [SerializeField] private Vector3 endScale = Vector3.one;
        [SerializeField] private Vector3 driftVelocity = Vector3.zero;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private Renderer[] cachedRenderers = System.Array.Empty<Renderer>();
        private Color[] baseColors = System.Array.Empty<Color>();
        private Vector3 startScale;
        private float timer;

        public void Configure(float newLifetime, Vector3 newEndScale, Vector3 newDriftVelocity)
        {
            lifetime = Mathf.Max(0.05f, newLifetime);
            endScale = newEndScale;
            driftVelocity = newDriftVelocity;
        }

        private void Awake()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>();
            baseColors = new Color[cachedRenderers.Length];
            for (int index = 0; index < cachedRenderers.Length; index++)
            {
                baseColors[index] = cachedRenderers[index] != null ? cachedRenderers[index].material.color : Color.white;
            }

            startScale = transform.localScale;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float normalized = lifetime <= 0.001f ? 1f : Mathf.Clamp01(timer / lifetime);
            float fade = fadeCurve.Evaluate(normalized);

            transform.position += driftVelocity * Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, normalized);

            for (int index = 0; index < cachedRenderers.Length; index++)
            {
                Renderer rendererComponent = cachedRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                rendererComponent.material.color = baseColors[index] * fade;
            }

            if (normalized >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
