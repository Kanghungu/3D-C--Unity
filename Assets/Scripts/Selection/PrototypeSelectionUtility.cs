using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Selection
{
    /// <summary>
    /// Shared selection math helpers for control-group summaries and formation placement.
    /// </summary>
    public static class PrototypeSelectionUtility
    {
        public static string BuildControlGroupSummary(Dictionary<int, List<SelectableUnit>> controlGroups)
        {
            if (controlGroups.Count == 0)
            {
                return "None";
            }

            List<string> parts = new();

            for (int index = 1; index <= 5; index++)
            {
                if (!controlGroups.TryGetValue(index, out List<SelectableUnit> units))
                {
                    continue;
                }

                int aliveCount = 0;
                foreach (SelectableUnit unit in units)
                {
                    if (unit != null)
                    {
                        aliveCount++;
                    }
                }

                if (aliveCount > 0)
                {
                    parts.Add($"F{index}:{aliveCount}");
                }
            }

            return parts.Count > 0 ? string.Join("  ", parts) : "None";
        }

        public static Vector3 GetSelectionCenter(IReadOnlyList<SelectableUnit> units)
        {
            Vector3 center = Vector3.zero;
            int count = 0;

            foreach (SelectableUnit unit in units)
            {
                if (unit == null)
                {
                    continue;
                }

                center += unit.transform.position;
                count++;
            }

            return count > 0 ? center / count : Vector3.zero;
        }

        public static List<Vector3> BuildFormationPoints(Vector3 center, int count, float spacing)
        {
            List<Vector3> points = new(count);
            if (count <= 0)
            {
                return points;
            }

            if (count == 1)
            {
                points.Add(center);
                return points;
            }

            const float goldenAngle = 2.39996323f;
            float baseRadius = Mathf.Max(spacing * 0.6f, 1.8f);
            float swirl = (Mathf.Abs(center.x) + Mathf.Abs(center.z)) * 0.0137f;

            for (int index = 0; index < count; index++)
            {
                float radius = baseRadius * Mathf.Sqrt(index + 0.35f);
                float angle = index * goldenAngle + swirl;
                float petal = Mathf.Sin((index + 1) * 1.618f + swirl) * spacing * 0.22f;
                float xOffset = Mathf.Cos(angle) * (radius + petal);
                float zOffset = Mathf.Sin(angle) * (radius - petal * 0.5f);
                Vector3 point = center + new Vector3(xOffset, 0f, zOffset);
                point.y = center.y;
                points.Add(point);
            }

            return points;
        }
    }
}
