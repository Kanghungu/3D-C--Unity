using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 지형 위 XZ 쿼드에 FoW 마스크 텍스처를 얹어 어둡게 함(유닛 메시는 가리지 않음).
    /// </summary>
    public sealed class BattleAcesFogWorldOverlay : MonoBehaviour
    {
        public static BattleAcesFogWorldOverlay Instance { get; private set; }

        private MeshRenderer meshRenderer;
        private Material fogMaterial;
        private bool setupOk;

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>전장 XZ 범위·지표 높이에 맞춰 메시·머티리얼 생성</summary>
        public void Initialize(Vector2 worldMin, Vector2 worldMax, float surfaceY)
        {
            Shader shader = Shader.Find("BattleAces/FogOverlay");
            if (shader == null)
            {
                Debug.LogWarning("[FoW] Shader BattleAces/FogOverlay 를 찾을 수 없습니다. 지형 오버레이를 건너뜁니다.");
                return;
            }

            fogMaterial = new Material(shader)
            {
                hideFlags = HideFlags.DontSave
            };

            Mesh mesh = BuildXzQuadMesh(worldMin, worldMax, surfaceY);

            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = gameObject.AddComponent<MeshFilter>();
            }

            mf.sharedMesh = mesh;

            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            meshRenderer.sharedMaterial = fogMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            gameObject.name = "FogOfWar_WorldOverlay";
            setupOk = true;
        }

        public void SetFogTexture(Texture2D tex)
        {
            if (!setupOk || fogMaterial == null)
            {
                return;
            }

            fogMaterial.SetTexture("_FogTex", tex);
        }

        private static Mesh BuildXzQuadMesh(Vector2 worldMin, Vector2 worldMax, float y)
        {
            var mesh = new Mesh { name = "FogOverlayQuad" };

            // worldMin.y / worldMax.y 는 월드 Z (FogOfWarGrid 와 동일)
            var v = new Vector3[4];
            var uv = new Vector2[4];
            v[0] = new Vector3(worldMin.x, y, worldMin.y);
            uv[0] = new Vector2(0f, 0f);
            v[1] = new Vector3(worldMax.x, y, worldMin.y);
            uv[1] = new Vector2(1f, 0f);
            v[2] = new Vector3(worldMax.x, y, worldMax.y);
            uv[2] = new Vector2(1f, 1f);
            v[3] = new Vector3(worldMin.x, y, worldMax.y);
            uv[3] = new Vector2(0f, 1f);

            mesh.vertices = v;
            mesh.uv = uv;
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void OnDestroy()
        {
            if (fogMaterial != null)
            {
                Destroy(fogMaterial);
                fogMaterial = null;
            }
        }
    }
}
