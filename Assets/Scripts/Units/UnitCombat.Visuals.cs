using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// 교전 빔·명령 앵커 등 프로토타입용 시각 피드백.
    /// </summary>
    public partial class UnitCombat
    {
        private void EnsureEngagementVisuals()
        {
            if (engagementAnchor != null)
            {
                return;
            }

            engagementAnchor = new GameObject("Engagement Anchor").transform;
            engagementAnchor.SetParent(transform);
            engagementAnchor.localPosition = new Vector3(0f, 0.82f, 0f);
            engagementAnchor.localRotation = Quaternion.identity;
            engagementAnchor.localScale = Vector3.one;

            engagementBeam = CreateEngagementPrimitive(
                engagementAnchor,
                PrimitiveType.Cube,
                "Engagement Beam",
                new Vector3(0f, 0f, 0.4f),
                new Vector3(0.05f, 0.05f, 0.8f),
                new Color(1f, 0.42f, 0.24f));
            engagementBeamRenderer = engagementBeam.GetComponent<Renderer>();

            engagementTip = CreateEngagementPrimitive(
                engagementAnchor,
                PrimitiveType.Sphere,
                "Engagement Tip",
                new Vector3(0f, 0f, 0.82f),
                new Vector3(0.12f, 0.12f, 0.12f),
                new Color(1f, 0.42f, 0.24f));
            engagementTipRenderer = engagementTip.GetComponent<Renderer>();
        }

        private void UpdateEngagementVisuals()
        {
            if (engagementAnchor == null)
            {
                return;
            }

            bool show = engagementBeamTimer > 0f
                && health != null && health.IsAlive
                && currentTarget != null && currentTarget.IsAlive;
            engagementAnchor.gameObject.SetActive(show);

            if (!show)
            {
                return;
            }

            Vector3 targetPosition = currentTarget.transform.position + Vector3.up * 0.72f;
            Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
            Vector3 planarTarget = new Vector3(localTarget.x, Mathf.Clamp(localTarget.y, -0.35f, 0.65f), localTarget.z);
            float distance = Mathf.Max(0.12f, planarTarget.magnitude);
            Vector3 direction = planarTarget / distance;
            float pulse = 0.88f + Mathf.PingPong(Time.time * 4.4f, 0.18f);
            Color linkColor = usesProjectile ? new Color(1f, 0.56f, 0.24f) : new Color(1f, 0.34f, 0.22f);

            if (engagementAnchor != null)
            {
                engagementAnchor.localPosition = new Vector3(0f, 0.82f + Mathf.PingPong(Time.time * 1.2f, 0.06f), 0f);
                engagementAnchor.localRotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            if (engagementBeam != null)
            {
                engagementBeam.localPosition = new Vector3(0f, 0f, distance * 0.5f);
                engagementBeam.localScale = new Vector3(0.045f, 0.045f, distance);
            }

            if (engagementBeamRenderer != null)
            {
                engagementBeamRenderer.material.color = linkColor * pulse;
            }

            if (engagementTip != null)
            {
                engagementTip.localPosition = new Vector3(0f, 0f, distance);
                engagementTip.localScale = Vector3.one * (0.1f + Mathf.PingPong(Time.time * 2.8f, 0.03f));
            }

            if (engagementTipRenderer != null)
            {
                engagementTipRenderer.material.color = Color.Lerp(linkColor, Color.white, 0.18f) * pulse;
            }
        }

        private static Transform CreateEngagementPrimitive(Transform parent, PrimitiveType primitiveType, string objectName, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject child = GameObject.CreatePrimitive(primitiveType);
            child.name = objectName;
            child.transform.SetParent(parent);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = localScale;

            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Renderer rendererComponent = child.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
            }

            return child.transform;
        }

        private void EnsureOrderAnchorVisuals()
        {
            if (orderAnchorVisual != null)
            {
                return;
            }

            orderAnchorVisual = new GameObject("Order Anchor Visual").transform;
            orderAnchorVisual.SetParent(transform);
            orderAnchorVisual.localPosition = new Vector3(0f, 0.42f, 0f);
            orderAnchorVisual.localRotation = Quaternion.identity;
            orderAnchorVisual.localScale = Vector3.one;

            orderAnchorBeam = CreateEngagementPrimitive(
                orderAnchorVisual,
                PrimitiveType.Cube,
                "Order Anchor Beam",
                new Vector3(0f, 0f, 0.5f),
                new Vector3(0.04f, 0.04f, 1f),
                new Color(0.34f, 0.95f, 1f));
            orderAnchorBeamRenderer = orderAnchorBeam.GetComponent<Renderer>();

            orderAnchorMarker = CreateEngagementPrimitive(
                orderAnchorVisual,
                PrimitiveType.Cylinder,
                "Order Anchor Marker",
                new Vector3(0f, -0.1f, 1f),
                new Vector3(0.12f, 0.04f, 0.12f),
                new Color(0.34f, 0.95f, 1f));
            orderAnchorMarkerRenderer = orderAnchorMarker.GetComponent<Renderer>();
        }

        private void UpdateOrderAnchorVisuals()
        {
            if (orderAnchorVisual == null)
            {
                return;
            }

            bool canShow = health != null
                && health.IsAlive
                && selectableUnit != null
                && selectableUnit.IsSelected
                && currentTarget == null;

            bool hasAnchor = canShow && (hasHoldPosition || hasGuardPoint || hasAttackMoveDestination);
            orderAnchorVisual.gameObject.SetActive(hasAnchor);

            if (!hasAnchor)
            {
                return;
            }

            Vector3 anchorWorld = hasGuardPoint
                ? guardPoint
                : hasHoldPosition
                    ? holdPosition
                    : attackMoveDestination;
            Vector3 targetPoint = new Vector3(anchorWorld.x, transform.position.y + 0.08f, anchorWorld.z);
            Vector3 localTarget = transform.InverseTransformPoint(targetPoint);
            Vector3 planarTarget = new Vector3(localTarget.x, Mathf.Clamp(localTarget.y, -0.25f, 0.35f), localTarget.z);
            float distance = Mathf.Max(0.2f, planarTarget.magnitude);
            Vector3 direction = planarTarget / distance;
            float pulse = 0.86f + Mathf.PingPong(Time.time * 3.2f, 0.16f);

            Color anchorColor = hasGuardPoint
                ? new Color(1f, 0.86f, 0.34f)
                : hasHoldPosition
                    ? new Color(0.52f, 0.76f, 1f)
                    : new Color(0.34f, 0.95f, 1f);

            orderAnchorVisual.localPosition = new Vector3(0f, 0.42f + Mathf.PingPong(Time.time * 1.2f, 0.04f), 0f);
            orderAnchorVisual.localRotation = Quaternion.LookRotation(direction, Vector3.up);

            if (orderAnchorBeam != null)
            {
                orderAnchorBeam.localPosition = new Vector3(0f, 0f, distance * 0.5f);
                orderAnchorBeam.localScale = new Vector3(0.035f, 0.035f, distance);
            }

            if (orderAnchorBeamRenderer != null)
            {
                orderAnchorBeamRenderer.material.color = anchorColor * pulse;
            }

            if (orderAnchorMarker != null)
            {
                orderAnchorMarker.localPosition = new Vector3(0f, -0.08f, distance);
                orderAnchorMarker.localScale = new Vector3(0.12f + Mathf.PingPong(Time.time * 1.5f, 0.03f), 0.04f, 0.12f + Mathf.PingPong(Time.time * 1.5f, 0.03f));
            }

            if (orderAnchorMarkerRenderer != null)
            {
                orderAnchorMarkerRenderer.material.color = Color.Lerp(anchorColor, Color.white, 0.14f) * pulse;
            }
        }
    }
}
