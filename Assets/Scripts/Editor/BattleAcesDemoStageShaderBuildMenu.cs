#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Keeps the built-in demo stage tone shader in Graphics Settings for build-time stripping safety.
    /// Uses the serialized GraphicsSettings asset so it works on newer Unity versions too.
    /// </summary>
    public static class BattleAcesDemoStageShaderBuildMenu
    {
        private const string ShaderName = "Hidden/BattleAcesDemoStageTone";
        private const string GraphicsSettingsAssetPath = "ProjectSettings/GraphicsSettings.asset";
        private const string AlwaysIncludedShadersPropertyName = "m_AlwaysIncludedShaders";

        [MenuItem("Tools/Battle Aces/Graphics/Always Include Demo Stage Tone Shader", priority = 200)]
        public static void RegisterDemoStageToneShader()
        {
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError("[BattleAces] Demo stage tone shader was not found: " + ShaderName);
                return;
            }

            if (!TryGetAlwaysIncludedShadersProperty(out SerializedObject graphicsSettingsObject, out SerializedProperty shadersProperty))
            {
                return;
            }

            if (ContainsShader(shadersProperty, shader))
            {
                Debug.Log("[BattleAces] Shader is already registered in Always Included Shaders: " + ShaderName);
                return;
            }

            int newIndex = shadersProperty.arraySize;
            shadersProperty.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newElement = shadersProperty.GetArrayElementAtIndex(newIndex);
            newElement.objectReferenceValue = shader;
            graphicsSettingsObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            Debug.Log("[BattleAces] Added shader to Always Included Shaders: " + ShaderName);
        }

        [MenuItem("Tools/Battle Aces/Graphics/Log Demo Stage Tone Shader Included?", priority = 201)]
        public static void LogDemoStageToneShaderIncluded()
        {
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError("[BattleAces] Shader.Find failed: " + ShaderName);
                return;
            }

            if (!TryGetAlwaysIncludedShadersProperty(out _, out SerializedProperty shadersProperty))
            {
                return;
            }

            if (ContainsShader(shadersProperty, shader))
            {
                Debug.Log("[BattleAces] Shader is registered in Always Included Shaders: " + ShaderName);
                return;
            }

            Debug.LogWarning(
                "[BattleAces] Shader is not in Always Included Shaders. " +
                "If the build strips it, run Tools/Battle Aces/Graphics/Always Include Demo Stage Tone Shader.");
        }

        private static bool TryGetAlwaysIncludedShadersProperty(out SerializedObject graphicsSettingsObject, out SerializedProperty shadersProperty)
        {
            graphicsSettingsObject = null;
            shadersProperty = null;

            Object[] graphicsSettingsAssets = AssetDatabase.LoadAllAssetsAtPath(GraphicsSettingsAssetPath);
            if (graphicsSettingsAssets == null || graphicsSettingsAssets.Length == 0)
            {
                Debug.LogError("[BattleAces] Could not load ProjectSettings/GraphicsSettings.asset.");
                return false;
            }

            graphicsSettingsObject = new SerializedObject(graphicsSettingsAssets[0]);
            shadersProperty = graphicsSettingsObject.FindProperty(AlwaysIncludedShadersPropertyName);
            if (shadersProperty == null || !shadersProperty.isArray)
            {
                Debug.LogError("[BattleAces] Could not find Always Included Shaders on GraphicsSettings.asset.");
                return false;
            }

            return true;
        }

        private static bool ContainsShader(SerializedProperty shadersProperty, Shader shader)
        {
            for (int i = 0; i < shadersProperty.arraySize; i++)
            {
                SerializedProperty element = shadersProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == shader)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif
