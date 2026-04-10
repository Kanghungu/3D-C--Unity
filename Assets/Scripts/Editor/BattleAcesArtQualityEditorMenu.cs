#if UNITY_EDITOR
using System;
using System.IO;
using Game.BattleAces;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// 레퍼 스크린샷 폴더 열기·품질 검수 텍스트 클립보드 복사 — 1인 작업에서 기대치 고정용.
    /// </summary>
    public static class BattleAcesArtQualityEditorMenu
    {
        private const string MenuRoot = "Game/Battle Aces/아트/";

        private const string MenuOpenRefs = MenuRoot + "레퍼런스 스크린샷 폴더 열기";

        private const string MenuCopyChecklist = MenuRoot + "품질 검수 체크리스트(한글) 클립보드 복사";

        private const string MenuCopySprintFocus = MenuRoot + "스프린트 포커스(한 화면만) 클립보드 복사";

        private const string MenuCopyScopeLock = MenuRoot + "스프린트 스코프 한 줄(템플릿) 클립보드 복사";

        private const string MenuCopySprintGuardrails = MenuRoot + "스프린트 시작 가드레일(고정·튜닝 축) 클립보드 복사";

        private static string ReferenceScreenshotsAbsolutePath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "Docs", "ReferenceScreenshots"));

        [MenuItem(MenuOpenRefs)]
        private static void OpenReferenceScreenshotsFolder()
        {
            string path = ReferenceScreenshotsAbsolutePath;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                EditorUtility.RevealInFinder(path);
            }
            catch (Exception ex)
            {
                Debug.LogError("[BattleAces] 레퍼런스 폴더 열기 실패: " + ex.Message);
            }
        }

        [MenuItem(MenuCopyChecklist)]
        private static void CopyQualityChecklistToClipboard()
        {
            string text = BattleAcesArtQualityScope.BuildClipboardChecklistKorean();
            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log("[BattleAces] 클립보드에 품질 검수 체크리스트를 복사했습니다.");
        }

        [MenuItem(MenuCopySprintFocus)]
        private static void CopySprintFocusToClipboard()
        {
            string text = BattleAcesDemoQualitySprintFocus.BuildSprintFocusClipboardKorean();
            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log("[BattleAces] 클립보드에 스프린트 포커스 문구를 복사했습니다.");
        }

        [MenuItem(MenuCopyScopeLock)]
        private static void CopyScopeLockTemplateToClipboard()
        {
            string text = BattleAcesArtQualityScope.BuildScopeLockClipboardKorean();
            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log("[BattleAces] 클립보드에 스프린트 스코프(데모 공통 + 이번 화면) 템플릿을 복사했습니다.");
        }

        [MenuItem(MenuCopySprintGuardrails)]
        private static void CopySprintGuardrailsToClipboard()
        {
            string text = BattleAcesArtQualityScope.BuildSprintStartGuardrailsClipboardKorean();
            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log("[BattleAces] 클립보드에 스프린트 시작 가드레일(고정/튜닝 축·Phase8 게이트)을 복사했습니다.");
        }
    }
}
#endif
