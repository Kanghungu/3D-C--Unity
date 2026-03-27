using Game.CameraSystem;
using UnityEngine;

namespace Game.Fleet
{
    /// <summary>
    /// Small reusable bootstrapper for the separate fleet experiment scene.
    /// </summary>
    public class FleetExperimentBootstrapper : MonoBehaviour
    {
        [SerializeField] private Vector3 groundScale = new(14f, 1f, 14f);
        [SerializeField] private Vector3 groundPosition = Vector3.zero;
        [SerializeField] private Vector3 cameraPosition = new(0f, 34f, -28f);
        [SerializeField] private Vector3 cameraRotation = new(52f, 0f, 0f);

        private void Awake()
        {
            SetupArena();
            SetupCamera();
            SetupShips();
            SetupControllers();
        }

        private void SetupArena()
        {
            GameObject arena = GameObject.Find("Space Arena");

            if (arena == null)
            {
                arena = GameObject.CreatePrimitive(PrimitiveType.Plane);
                arena.name = "Space Arena";
            }

            arena.transform.position = groundPosition;
            arena.transform.localScale = groundScale;
            arena.GetComponent<Renderer>().material.color = new Color(0.07f, 0.08f, 0.14f);
            RenderSettings.fog = false;
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                GameObject cameraObject = new("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            mainCamera.transform.position = cameraPosition;
            mainCamera.transform.rotation = Quaternion.Euler(cameraRotation);
            mainCamera.backgroundColor = new Color(0.03f, 0.04f, 0.09f);

            if (mainCamera.GetComponent<RTSCameraController>() == null)
            {
                mainCamera.gameObject.AddComponent<RTSCameraController>();
            }
        }

        private void SetupShips()
        {
            Transform playerRoot = GetOrCreateRoot("Player Fleet");
            Transform enemyRoot = GetOrCreateRoot("Enemy Fleet");

            if (playerRoot.childCount > 0 || enemyRoot.childCount > 0)
            {
                return;
            }

            CreateShip("Pilgrim Frigate", new Vector3(-8f, 1f, -4f), true, new Color(0.65f, 0.8f, 1f), playerRoot, PrimitiveType.Capsule, 52f, 9f, 8f, 8f);
            CreateShip("Pilgrim Frigate", new Vector3(-4f, 1f, 0f), true, new Color(0.65f, 0.8f, 1f), playerRoot, PrimitiveType.Capsule, 52f, 9f, 8f, 8f);
            CreateShip("War Spear", new Vector3(8f, 1f, 4f), false, new Color(1f, 0.5f, 0.28f), enemyRoot, PrimitiveType.Cylinder, 42f, 10f, 8.5f, 10f);
            CreateShip("War Spear", new Vector3(4f, 1f, 0f), false, new Color(1f, 0.5f, 0.28f), enemyRoot, PrimitiveType.Cylinder, 42f, 10f, 8.5f, 10f);
        }

        private void SetupControllers()
        {
            if (FindAnyObjectByType<FleetSelectionController>() == null)
            {
                gameObject.AddComponent<FleetSelectionController>();
            }
        }

        private static FleetShip CreateShip(string name, Vector3 position, bool isPlayer, Color color, Transform parent, PrimitiveType primitiveType, float health, float moveSpeed, float attackRange, float damage)
        {
            GameObject shipObject = GameObject.CreatePrimitive(primitiveType);
            shipObject.name = name;
            shipObject.transform.position = position;
            shipObject.transform.localScale = primitiveType == PrimitiveType.Cylinder ? new Vector3(1.4f, 0.5f, 1.4f) : new Vector3(1.1f, 1.1f, 1.8f);
            shipObject.transform.SetParent(parent);

            FleetShip ship = shipObject.AddComponent<FleetShip>();
            ship.Configure(isPlayer, color, health, moveSpeed, attackRange, damage);
            return ship;
        }

        private static Transform GetOrCreateRoot(string name)
        {
            GameObject root = GameObject.Find(name);
            return root != null ? root.transform : new GameObject(name).transform;
        }
    }
}
