using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Adds simple battlefield ambience by animating banners, shrine beacons, and lighting.
    /// Keeps the prototype map feeling alive without requiring authored animation assets.
    /// </summary>
    public class BattlefieldAmbientAnimator : MonoBehaviour
    {
        private struct RouteConvoy
        {
            public Transform Transform;
            public Vector3 BaseLocalPosition;
            public Quaternion BaseLocalRotation;
            public Vector3 BaseScale;
            public Vector3 TravelDirection;
            public float TravelDistance;
            public float Phase;
        }

        private readonly List<Transform> bannerTargets = new();
        private readonly List<Quaternion> bannerBaseRotations = new();
        private readonly List<float> bannerPhases = new();
        private readonly List<Transform> routeBeacons = new();
        private readonly List<Vector3> routeBeaconBasePositions = new();
        private readonly List<float> routeBeaconPhases = new();
        private readonly List<RouteConvoy> routeConvoys = new();
        private readonly List<Renderer> convoyGlowRenderers = new();
        private readonly List<Color> convoyGlowBaseColors = new();
        private readonly List<Transform> smokeColumns = new();
        private readonly List<Vector3> smokeBaseScales = new();
        private readonly List<float> smokePhases = new();
        private readonly List<Renderer> emberRenderers = new();
        private readonly List<Color> emberBaseColors = new();
        private readonly List<Vector3> emberBasePositions = new();
        private readonly List<float> emberPhases = new();
        private readonly List<Transform> searchlightBeams = new();
        private readonly List<Quaternion> searchlightBaseRotations = new();
        private readonly List<Vector3> searchlightBaseScales = new();
        private readonly List<float> searchlightPhases = new();
        private readonly List<Renderer> searchlightRenderers = new();
        private readonly List<Color> searchlightBaseColors = new();
        private readonly List<Transform> altarRings = new();
        private readonly List<Vector3> altarRingBaseScales = new();
        private readonly List<float> altarRingPhases = new();
        private readonly List<Renderer> altarRingRenderers = new();
        private readonly List<Color> altarRingBaseColors = new();
        private readonly List<Transform> altarBeams = new();
        private readonly List<Vector3> altarBeamBaseScales = new();
        private readonly List<float> altarBeamPhases = new();
        private readonly List<Renderer> altarBeamRenderers = new();
        private readonly List<Color> altarBeamBaseColors = new();
        private readonly List<Renderer> pulseRenderers = new();
        private readonly List<Color> pulseBaseColors = new();
        private BattlefieldTheme theme;
        private Light keyLight;
        private Color baseFogColor;
        private Color baseLightColor;
        private float baseLightIntensity;

        public void Configure(BattlefieldTheme battlefieldTheme, Transform terrainRoot, Light sceneLight)
        {
            theme = battlefieldTheme;
            keyLight = sceneLight;
            bannerTargets.Clear();
            bannerBaseRotations.Clear();
            bannerPhases.Clear();
            routeBeacons.Clear();
            routeBeaconBasePositions.Clear();
            routeBeaconPhases.Clear();
            routeConvoys.Clear();
            convoyGlowRenderers.Clear();
            convoyGlowBaseColors.Clear();
            smokeColumns.Clear();
            smokeBaseScales.Clear();
            smokePhases.Clear();
            emberRenderers.Clear();
            emberBaseColors.Clear();
            emberBasePositions.Clear();
            emberPhases.Clear();
            searchlightBeams.Clear();
            searchlightBaseRotations.Clear();
            searchlightBaseScales.Clear();
            searchlightPhases.Clear();
            searchlightRenderers.Clear();
            searchlightBaseColors.Clear();
            altarRings.Clear();
            altarRingBaseScales.Clear();
            altarRingPhases.Clear();
            altarRingRenderers.Clear();
            altarRingBaseColors.Clear();
            altarBeams.Clear();
            altarBeamBaseScales.Clear();
            altarBeamPhases.Clear();
            altarBeamRenderers.Clear();
            altarBeamBaseColors.Clear();
            pulseRenderers.Clear();
            pulseBaseColors.Clear();

            if (terrainRoot != null)
            {
                foreach (Transform child in terrainRoot.GetComponentsInChildren<Transform>())
                {
                    if (child == null)
                    {
                        continue;
                    }

                    string objectName = child.name;
                    if (objectName.Contains("Banner"))
                    {
                        bannerTargets.Add(child);
                        bannerBaseRotations.Add(child.localRotation);
                        bannerPhases.Add(Random.value * Mathf.PI * 2f);
                    }

                    if (objectName.Contains("Route Beacon"))
                    {
                        routeBeacons.Add(child);
                        routeBeaconBasePositions.Add(child.localPosition);
                        routeBeaconPhases.Add(Random.value * Mathf.PI * 2f);
                    }

                    if (objectName.Contains("Route Convoy Group"))
                    {
                        routeConvoys.Add(new RouteConvoy
                        {
                            Transform = child,
                            BaseLocalPosition = child.localPosition,
                            BaseLocalRotation = child.localRotation,
                            BaseScale = child.localScale,
                            TravelDirection = child.localRotation * Vector3.forward,
                            TravelDistance = 12f + Random.Range(0f, 6f),
                            Phase = Random.value * Mathf.PI * 2f
                        });
                    }

                    if (objectName.Contains("Route Convoy Core") || objectName.Contains("Route Convoy Escort"))
                    {
                        Renderer rendererComponent = child.GetComponent<Renderer>();
                        if (rendererComponent != null)
                        {
                            convoyGlowRenderers.Add(rendererComponent);
                            convoyGlowBaseColors.Add(rendererComponent.material.color);
                        }
                    }

                    if (objectName.Contains("Smoke Column"))
                    {
                        smokeColumns.Add(child);
                        smokeBaseScales.Add(child.localScale);
                        smokePhases.Add(Random.value * Mathf.PI * 2f);
                    }

                    if (objectName.Contains("Ember"))
                    {
                        Renderer rendererComponent = child.GetComponent<Renderer>();
                        if (rendererComponent != null)
                        {
                            emberRenderers.Add(rendererComponent);
                            emberBaseColors.Add(rendererComponent.material.color);
                            emberBasePositions.Add(child.localPosition);
                            emberPhases.Add(Random.value * Mathf.PI * 2f);
                        }
                    }

                    if (objectName.Contains("Searchlight Beam"))
                    {
                        searchlightBeams.Add(child);
                        searchlightBaseRotations.Add(child.localRotation);
                        searchlightBaseScales.Add(child.localScale);
                        searchlightPhases.Add(Random.value * Mathf.PI * 2f);

                        Renderer rendererComponent = child.GetComponent<Renderer>();
                        if (rendererComponent != null)
                        {
                            searchlightRenderers.Add(rendererComponent);
                            searchlightBaseColors.Add(rendererComponent.material.color);
                        }
                    }

                    if (objectName.Contains("Altar Ring"))
                    {
                        altarRings.Add(child);
                        altarRingBaseScales.Add(child.localScale);
                        altarRingPhases.Add(Random.value * Mathf.PI * 2f);
                        Renderer rendererComponent = child.GetComponent<Renderer>();
                        if (rendererComponent != null)
                        {
                            altarRingRenderers.Add(rendererComponent);
                            altarRingBaseColors.Add(rendererComponent.material.color);
                        }
                    }

                    if (objectName.Contains("Altar Descent Beam"))
                    {
                        altarBeams.Add(child);
                        altarBeamBaseScales.Add(child.localScale);
                        altarBeamPhases.Add(Random.value * Mathf.PI * 2f);
                        Renderer rendererComponent = child.GetComponent<Renderer>();
                        if (rendererComponent != null)
                        {
                            altarBeamRenderers.Add(rendererComponent);
                            altarBeamBaseColors.Add(rendererComponent.material.color);
                        }
                    }

                    if (objectName.Contains("Beam")
                        || objectName.Contains("Beacon")
                        || objectName.Contains("Pyre")
                        || objectName.Contains("Halo")
                        || objectName.Contains("Crown"))
                    {
                        Renderer rendererComponent = child.GetComponent<Renderer>();
                        if (rendererComponent != null)
                        {
                            pulseRenderers.Add(rendererComponent);
                            pulseBaseColors.Add(rendererComponent.material.color);
                        }
                    }
                }
            }

            baseFogColor = RenderSettings.fogColor;
            if (keyLight != null)
            {
                baseLightColor = keyLight.color;
                baseLightIntensity = keyLight.intensity;
            }
        }

        private void Update()
        {
            AnimateBanners();
            AnimateRouteBeacons();
            AnimateRouteConvoys();
            AnimateSmokeColumns();
            AnimateEmbers();
            AnimateSearchlights();
            AnimateAltarSignals();
            AnimatePulseRenderers();
            AnimateLighting();
        }

        private void AnimateBanners()
        {
            float windSpeed = theme == BattlefieldTheme.CrimsonBasin ? 1.9f : 1.45f;
            float windTilt = theme == BattlefieldTheme.PaleSaltFlats ? 9f : 13f;

            for (int index = 0; index < bannerTargets.Count; index++)
            {
                Transform banner = bannerTargets[index];
                if (banner == null)
                {
                    continue;
                }

                float phase = bannerPhases[index];
                float wave = Mathf.Sin(Time.time * windSpeed + phase);
                float ripple = Mathf.Cos(Time.time * (windSpeed * 1.8f) + phase * 0.7f);
                banner.localRotation = bannerBaseRotations[index] * Quaternion.Euler(wave * windTilt, ripple * 3.4f, wave * 2.6f);
            }
        }

        private void AnimateRouteBeacons()
        {
            for (int index = 0; index < routeBeacons.Count; index++)
            {
                Transform beacon = routeBeacons[index];
                if (beacon == null)
                {
                    continue;
                }

                Vector3 basePosition = routeBeaconBasePositions[index];
                float phase = routeBeaconPhases[index];
                float bob = Mathf.Sin(Time.time * 2.1f + phase) * 0.14f;
                float sway = Mathf.Cos(Time.time * 1.3f + phase) * 6f;
                beacon.localPosition = basePosition + new Vector3(0f, bob, 0f);
                beacon.localRotation = Quaternion.Euler(0f, sway, 0f);
            }
        }

        private void AnimateRouteConvoys()
        {
            float convoySpeed = theme == BattlefieldTheme.CrimsonBasin ? 0.12f : 0.1f;

            for (int index = 0; index < routeConvoys.Count; index++)
            {
                RouteConvoy convoy = routeConvoys[index];
                if (convoy.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * convoySpeed + convoy.Phase, 1f);
                float forwardOffset = (cycle - 0.5f) * convoy.TravelDistance * 2f;
                float lateralOffset = Mathf.Sin(Time.time * 0.9f + convoy.Phase) * 0.75f;
                Vector3 lateralDirection = convoy.BaseLocalRotation * Vector3.right;
                convoy.Transform.localPosition = convoy.BaseLocalPosition
                    + convoy.TravelDirection * forwardOffset
                    + lateralDirection * lateralOffset;
                convoy.Transform.localRotation = convoy.BaseLocalRotation * Quaternion.Euler(
                    Mathf.Sin(Time.time * 1.2f + convoy.Phase) * 3f,
                    0f,
                    Mathf.Cos(Time.time * 1.4f + convoy.Phase) * 2.2f);
                convoy.Transform.localScale = convoy.BaseScale * (0.96f + Mathf.PingPong(Time.time * 0.38f + convoy.Phase, 0.08f));
                routeConvoys[index] = convoy;
            }

            for (int index = 0; index < convoyGlowRenderers.Count; index++)
            {
                Renderer rendererComponent = convoyGlowRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulse = 0.86f + Mathf.PingPong(Time.time * 1.6f + index * 0.31f, 0.24f);
                rendererComponent.material.color = convoyGlowBaseColors[index] * pulse;
            }
        }

        private void AnimateSmokeColumns()
        {
            for (int index = 0; index < smokeColumns.Count; index++)
            {
                Transform smoke = smokeColumns[index];
                if (smoke == null)
                {
                    continue;
                }

                Vector3 baseScale = smokeBaseScales[index];
                float phase = smokePhases[index];
                float pulse = 0.9f + Mathf.PingPong(Time.time * 0.22f + phase, 0.24f);
                float sway = Mathf.Sin(Time.time * 0.4f + phase) * 8f;
                smoke.localScale = new Vector3(baseScale.x * pulse, baseScale.y * (0.96f + pulse * 0.1f), baseScale.z * pulse);
                smoke.localRotation = Quaternion.Euler(0f, sway, Mathf.Cos(Time.time * 0.36f + phase) * 3.6f);
            }
        }

        private void AnimateEmbers()
        {
            for (int index = 0; index < emberRenderers.Count; index++)
            {
                Renderer ember = emberRenderers[index];
                if (ember == null)
                {
                    continue;
                }

                Transform emberTransform = ember.transform;
                float phase = emberPhases[index];
                Vector3 basePosition = emberBasePositions[index];
                float pulse = 0.78f + Mathf.PingPong(Time.time * 2.4f + phase, 0.28f);
                ember.material.color = emberBaseColors[index] * pulse;
                emberTransform.localPosition = basePosition + new Vector3(
                    Mathf.Sin(Time.time * 0.8f + phase) * 0.08f,
                    Mathf.PingPong(Time.time * 0.4f + phase, 0.28f),
                    Mathf.Cos(Time.time * 0.7f + phase) * 0.08f);
                emberTransform.localScale = Vector3.one * (0.4f + pulse * 0.22f);
            }
        }

        private void AnimateSearchlights()
        {
            for (int index = 0; index < searchlightBeams.Count; index++)
            {
                Transform beam = searchlightBeams[index];
                if (beam == null)
                {
                    continue;
                }

                float phase = searchlightPhases[index];
                float yaw = Mathf.Sin(Time.time * 0.42f + phase) * 38f;
                float pitch = -22f + Mathf.Cos(Time.time * 0.36f + phase * 0.8f) * 8f;
                float pulse = 0.84f + Mathf.PingPong(Time.time * 0.26f + phase, 0.12f);

                beam.localRotation = searchlightBaseRotations[index] * Quaternion.Euler(pitch, yaw, 0f);
                beam.localScale = new Vector3(
                    searchlightBaseScales[index].x * pulse,
                    searchlightBaseScales[index].y,
                    searchlightBaseScales[index].z * (0.92f + pulse * 0.12f));

                if (index < searchlightRenderers.Count && searchlightRenderers[index] != null)
                {
                    searchlightRenderers[index].material.color = searchlightBaseColors[index] * (0.82f + pulse * 0.18f);
                }
            }
        }

        private void AnimateAltarSignals()
        {
            for (int index = 0; index < altarRings.Count; index++)
            {
                Transform ring = altarRings[index];
                if (ring == null)
                {
                    continue;
                }

                float phase = altarRingPhases[index];
                float pulse = 0.88f + Mathf.PingPong(Time.time * 0.42f + phase, 0.18f);
                ring.localScale = altarRingBaseScales[index] * pulse;
                ring.localRotation *= Quaternion.Euler(0f, 12f * Time.deltaTime, 0f);

                if (index < altarRingRenderers.Count && altarRingRenderers[index] != null)
                {
                    altarRingRenderers[index].material.color = altarRingBaseColors[index] * pulse;
                }
            }

            for (int index = 0; index < altarBeams.Count; index++)
            {
                Transform beam = altarBeams[index];
                if (beam == null)
                {
                    continue;
                }

                float phase = altarBeamPhases[index];
                float pulse = 0.82f + Mathf.PingPong(Time.time * 0.56f + phase, 0.22f);
                beam.localScale = new Vector3(
                    altarBeamBaseScales[index].x * pulse,
                    altarBeamBaseScales[index].y * (0.94f + pulse * 0.12f),
                    altarBeamBaseScales[index].z * pulse);
                beam.localPosition = new Vector3(
                    beam.localPosition.x,
                    beam.localPosition.y + Mathf.Sin(Time.time * 0.44f + phase) * 0.01f,
                    beam.localPosition.z);

                if (index < altarBeamRenderers.Count && altarBeamRenderers[index] != null)
                {
                    altarBeamRenderers[index].material.color = altarBeamBaseColors[index] * pulse;
                }
            }
        }

        private void AnimatePulseRenderers()
        {
            for (int index = 0; index < pulseRenderers.Count; index++)
            {
                Renderer rendererComponent = pulseRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulse = 0.82f + Mathf.PingPong(Time.time * 1.9f + index * 0.37f, 0.26f);
                rendererComponent.material.color = pulseBaseColors[index] * pulse;
            }
        }

        private void AnimateLighting()
        {
            if (keyLight != null)
            {
                float intensityPulse = 0.94f + Mathf.PingPong(Time.time * 0.18f, 0.08f);
                keyLight.intensity = baseLightIntensity * intensityPulse;
                keyLight.color = Color.Lerp(baseLightColor, Color.white, 0.08f + Mathf.PingPong(Time.time * 0.12f, 0.06f));
            }

            RenderSettings.fogColor = Color.Lerp(baseFogColor, Color.white, 0.02f + Mathf.PingPong(Time.time * 0.1f, 0.03f));
        }
    }
}
