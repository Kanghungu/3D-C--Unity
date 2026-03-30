using Game.Units;
using System;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared army deployment helpers for prototype battlefield setup.
    /// Keeps repeated formation spawning loops out of the bootstrapper.
    /// </summary>
    public static class PrototypeArmyDeploymentUtility
    {
        public static void CreateGridFormation(PrototypeGameDatabase database, Vector3 start, Vector2Int grid, float spacing, UnitTeam team, Transform parent, Func<int, UnitArchetype> rowSelector)
        {
            for (int row = 0; row < grid.y; row++)
            {
                for (int column = 0; column < grid.x; column++)
                {
                    Vector3 spawnPosition = start + new Vector3(column * spacing, 0f, row * spacing);
                    CreateSingleUnit(database, spawnPosition, team, parent, rowSelector(row));
                }
            }
        }

        public static void CreateLine(PrototypeGameDatabase database, Vector3 start, UnitTeam team, Transform parent, int count, float spacing, UnitArchetype archetype)
        {
            for (int index = 0; index < count; index++)
            {
                Vector3 spawnPosition = start + new Vector3(index * spacing, 0f, 0f);
                CreateSingleUnit(database, spawnPosition, team, parent, archetype);
            }
        }

        public static void CreateWing(PrototypeGameDatabase database, Vector3 start, UnitTeam team, Transform parent, int count, float spacing, UnitArchetype archetype)
        {
            for (int index = 0; index < count; index++)
            {
                Vector3 spawnPosition = start + new Vector3(index * spacing, 0f, (index % 2 == 0 ? 0f : 8f));
                CreateSingleUnit(database, spawnPosition, team, parent, archetype);
            }
        }

        public static void CreateSingleUnit(PrototypeGameDatabase database, Vector3 position, UnitTeam team, Transform parent, UnitArchetype archetype)
        {
            UnitDefinition definition = database.GetDefinition(archetype);
            if (definition != null)
            {
                PrototypeEntityFactory.CreateUnit(team, definition, position, parent);
            }
        }
    }
}
