#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Editor
{
    /// <summary>
    /// 빌드 시 Shader Stripping 으로 <c>Hidden/BattleAcesDemoStageTone</c> 이 빠지면
    /// Built-in 에서 데모 비네팅 머티리얼이 핑크가 될 수 있음 — Always Included 에 등록.
    /// </summary>
    public static class BattleAcesDemoStageShaderBuildMenu
    {
        private const string ShaderName = "Hidden/BattleAcesDemoStageTone";

        [MenuItem("Tools/Battle Aces/Graphics/Always Include Demo Stage Tone Shader", priority = 200)]
        public static void RegisterDemoStageToneShader()
        {
            Shader s = Shader.Find(ShaderName);
            if (s == null)
            {
                Debug.LogError("[BattleAces] 쉐이더를 찾을 수 없습니다. 에셋 경로: Assets/Shaders/BattleAcesDemoStageTone.shader — " + ShaderName);
                return;
            }

            Shader[] existing = GraphicsSettings.alwaysIncludedShaders;
            var list = new List<Shader>();
            if (existing != null)
            {
                for (int i = 0; i < existing.Length; i++)
                {
                    if (existing[i] != null)
                    {
                        list.Add(existing[i]);
                    }
                }
            }

            if (list.Contains(s))
            {
                Debug.Log("[BattleAces] 이미 Always Included Shaders 에 포함됨: " + ShaderName);
                return;
            }

            list.Add(s);
            GraphicsSettings.alwaysIncludedShaders = list.ToArray();
            Debug.Log("[BattleAces] Always Included Shaders 에 추가함(저장됨): " + ShaderName);
        }

        [MenuItem("Tools/Battle Aces/Graphics/Log Demo Stage Tone Shader — Included?", priority = 201)]
        public static void LogDemoStageToneShaderIncluded()
        {
            Shader s = Shader.Find(ShaderName);
            if (s == null)
            {
                Debug.LogError("[BattleAces] Shader.Find 실패: " + ShaderName);
                return;
            }

            Shader[] arr = GraphicsSettings.alwaysIncludedShaders;
            if (arr == null)
            {
                Debug.LogWarning("[BattleAces] alwaysIncludedShaders 가 null 입니다.");
                return;
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == s)
                {
                    Debug.Log("[BattleAces] OK — Always Included 에 등록되어 있습니다: " + ShaderName);
                    return;
                }
            }

            Debug.LogWarning(
                "[BattleAces] Always Included 에 없습니다. 빌드 스트리핑 시 연출이 빠질 수 있음 — " +
                "메뉴 「Always Include Demo Stage Tone Shader」 실행을 권장합니다.");
        }
    }
}
#endif
