#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// 승·패 스팅 전용 AudioMixer 생성 — Master 아래 ResultSting 그룹 추가 후 Volume 노출.
    /// Unity 에디터 API(내부)를 리플렉션으로 호출 — 버전에 따라 실패할 수 있음.
    /// </summary>
    public static class BattleAcesAudioMixerMenu
    {
        private const string MixerPath = "Assets/Audio/BattleAces_Main.mixer";

        [MenuItem("Game/Audio/Create BattleAces_Main.mixer (ResultSting)")]
        private static void CreateMixer()
        {
            if (!Directory.Exists("Assets/Audio"))
            {
                Directory.CreateDirectory("Assets/Audio");
                AssetDatabase.Refresh();
            }

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(MixerPath) != null)
            {
                EditorUtility.DisplayDialog(
                    "AudioMixer",
                    "이미 있습니다: " + MixerPath + "\n삭제 후 다시 실행하거나, 인스펙터에서 Battle Aces Root 에 AudioMixer 를 할당하세요.",
                    "OK");
                UnityEditor.Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(MixerPath);
                return;
            }

            Assembly editorAsm = typeof(UnityEditor.Editor).Assembly;
            Type controllerType = editorAsm.GetType("UnityEditor.Audio.AudioMixerController");
            if (controllerType == null)
            {
                Debug.LogError("[BattleAcesAudioMixer] UnityEditor.Audio.AudioMixerController 타입을 찾을 수 없습니다.");
                return;
            }

            MethodInfo create = controllerType.GetMethod(
                "CreateMixerControllerAtPath",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(string) },
                null);

            if (create == null)
            {
                Debug.LogError("[BattleAcesAudioMixer] CreateMixerControllerAtPath 메서드를 찾을 수 없습니다.");
                return;
            }

            object mixerController = create.Invoke(null, new object[] { MixerPath });
            if (mixerController == null)
            {
                Debug.LogError("[BattleAcesAudioMixer] 믹서 생성 실패.");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(MixerPath);
            UnityEditor.Selection.activeObject = asset;
            EditorUtility.DisplayDialog(
                "AudioMixer",
                "생성됨: " + MixerPath + "\n\n" +
                "Expose 이름(설정 O 패널과 동일해야 함):\n" +
                "· ResultSting 그룹 Volume → Expose 이름 ResultStingVol\n" +
                "· BattleAmbient 그룹 Volume → Expose 이름 BattleAmbientVol\n" +
                "NewSampleScene → Battle Aces Root → 믹서 할당.",
                "OK");
        }
    }
}
#endif
