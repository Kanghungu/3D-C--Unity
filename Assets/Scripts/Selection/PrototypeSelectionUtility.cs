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

        public static List<Vector3> BuildFormationPoints(Vector3 center, int count, float spacing, Vector3 direction = default)
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

            // 이동 방향이 있으면 방향 기준 가로줄 대열
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                direction = direction.normalized;
                Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;

                int cols = Mathf.CeilToInt(Mathf.Sqrt(count * 1.6f)); // 가로를 더 넓게
                cols = Mathf.Max(cols, 2);
                int rows = Mathf.CeilToInt((float)count / cols);

                int unitIndex = 0;
                for (int row = 0; row < rows && unitIndex < count; row++)
                {
                    int unitsInRow = Mathf.Min(cols, count - unitIndex);
                    float rowOffset = row * spacing;
                    for (int col = 0; col < unitsInRow; col++)
                    {
                        float xOff = (col - (unitsInRow - 1) * 0.5f) * spacing;
                        Vector3 point = center - direction * rowOffset + right * xOff;
                        point.y = center.y;
                        points.Add(point);
                        unitIndex++;
                    }
                }

                return points;
            }

            // 방향 없으면 단순 가로줄 (원형 fallback)
            int fallbackCols = Mathf.CeilToInt(Mathf.Sqrt(count * 1.6f));
            fallbackCols = Mathf.Max(fallbackCols, 2);
            for (int index = 0; index < count; index++)
            {
                int row = index / fallbackCols;
                int col = index % fallbackCols;
                int unitsInRow = Mathf.Min(fallbackCols, count - row * fallbackCols);
                float xOff = (col - (unitsInRow - 1) * 0.5f) * spacing;
                Vector3 point = center + new Vector3(xOff, 0f, row * spacing);
                point.y = center.y;
                points.Add(point);
            }

            return points;
        }
    }
}
