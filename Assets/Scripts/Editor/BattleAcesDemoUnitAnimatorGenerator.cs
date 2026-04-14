#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Game.Prototype;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Battle Aces 데모용 근접 Mecanim — Resources 로드 경로와 규약(BATTLE_ACES_ANIMATOR_CONTRACT)에 맞춤.
    /// 공격 클립은 자식 BA_AnimScaleRoot 의 스케일만 펄스(루트 스케일·이동 회전과 충돌 방지).
    /// </summary>
    public static class BattleAcesDemoUnitAnimatorGenerator
    {
        private const string MenuPath = "Game/Battle Aces/데모 유닛 애니 에셋 생성";

        private static readonly string ResourceRoot = Path.Combine("Assets", "Resources", "BattleAcesDemo").Replace('\\', '/');

        private static readonly string ControllerPath = $"{ResourceRoot}/Unit_GenericMelee.controller";

        private static readonly string IdleClipPath = $"{ResourceRoot}/GenericMelee_Idle.anim";

        private static readonly string AttackClipPath = $"{ResourceRoot}/GenericMelee_Attack.anim";

        private static readonly string DieClipPath = $"{ResourceRoot}/GenericMelee_Die.anim";

        private static readonly string ChildPath = BattleAcesDemoUnitAnimatorRuntime.AnimScaleRootChildName;

        [InitializeOnLoad]
        private static class AutoCreateIfMissing
        {
            static AutoCreateIfMissing()
            {
                EditorApplication.delayCall += TryCreateOnce;
            }

            private static void TryCreateOnce()
            {
                if (Application.isBatchMode)
                {
                    return;
                }

                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
                {
                    return;
                }

                GenerateInternal(deleteExisting: false);
            }
        }

        [MenuItem(MenuPath)]
        private static void MenuGenerate()
        {
            GenerateInternal(deleteExisting: true);
        }

        private static void GenerateInternal(bool deleteExisting)
        {
            try
            {
                EnsureFolderExists("Assets/Resources");
                EnsureFolderExists(ResourceRoot);

                if (deleteExisting)
                {
                    SafeDelete(ControllerPath);
                    SafeDelete(IdleClipPath);
                    SafeDelete(AttackClipPath);
                    SafeDelete(DieClipPath);
                    AssetDatabase.Refresh();
                }

                AnimationClip idleClip = BuildIdleClip();
                AssetDatabase.CreateAsset(idleClip, IdleClipPath);

                AnimationClip attackClip = BuildAttackClip();
                AssetDatabase.CreateAsset(attackClip, AttackClipPath);

                AnimationClip dieClip = BuildDieClip();
                AssetDatabase.CreateAsset(dieClip, DieClipPath);

                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
                if (controller == null)
                {
                    throw new System.InvalidOperationException("AnimatorController 생성 실패: " + ControllerPath);
                }

                controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
                controller.AddParameter("InCombat", AnimatorControllerParameterType.Bool);
                controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

                AnimatorStateMachine sm = controller.layers[0].stateMachine;
                RemoveAllUserStates(sm);

                AnimatorState idleState = sm.AddState("Idle");
                idleState.motion = idleClip;

                AnimatorState attackState = sm.AddState("Attack");
                attackState.motion = attackClip;

                AnimatorState dieState = sm.AddState("Die");
                dieState.motion = dieClip;

                sm.defaultState = idleState;

                AnimatorStateTransition anyToDie = sm.AddAnyStateTransition(dieState);
                anyToDie.hasExitTime = false;
                anyToDie.duration = 0.04f;
                anyToDie.AddCondition(AnimatorConditionMode.If, 0f, "Die");

                AnimatorStateTransition anyToAttack = sm.AddAnyStateTransition(attackState);
                anyToAttack.hasExitTime = false;
                anyToAttack.duration = 0.06f;
                anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

                AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
                attackToIdle.hasExitTime = true;
                attackToIdle.exitTime = 0.92f;
                attackToIdle.duration = 0.1f;

                AnimatorStateTransition dieToIdle = dieState.AddTransition(idleState);
                dieToIdle.hasExitTime = true;
                dieToIdle.exitTime = 0.88f;
                dieToIdle.duration = 0.08f;

                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[BattleAcesDemo] Mecanim 생성 완료: {ControllerPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[BattleAcesDemo] 애니 에셋 생성 실패: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private static void SafeDelete(string assetPath)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        private static void EnsureFolderExists(string assetPathFolder)
        {
            if (AssetDatabase.IsValidFolder(assetPathFolder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(assetPathFolder)?.Replace('\\', '/');
            string folderName = Path.GetFileName(assetPathFolder);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolderExists(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void RemoveAllUserStates(AnimatorStateMachine sm)
        {
            var buffer = new List<AnimatorState>();
            foreach (ChildAnimatorState child in sm.states)
            {
                buffer.Add(child.state);
            }

            for (int i = 0; i < buffer.Count; i++)
            {
                sm.RemoveState(buffer[i]);
            }
        }

        private static AnimationClip BuildIdleClip()
        {
            var clip = new AnimationClip { name = "GenericMelee_Idle" };
            float end = 1f;
            AnimationCurve flat = AnimationCurve.Constant(0f, end, 1f);
            SetScaleCurves(clip, flat, flat, flat);
            AnimationClipSettings st = AnimationUtility.GetAnimationClipSettings(clip);
            st.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, st);
            return clip;
        }

        private static AnimationClip BuildAttackClip()
        {
            var clip = new AnimationClip { name = "GenericMelee_Attack" };
            float peak = 0.11f;
            float strikeTime = 0.17f;
            float end = 0.42f;
            float maxScale = 1.16f;

            AnimationCurve cx = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(peak, maxScale),
                new Keyframe(strikeTime, maxScale * 0.98f),
                new Keyframe(end, 1f));

            SetScaleCurves(clip, cx, cx, cx);

            var animEvent = new AnimationEvent
            {
                time = strikeTime,
                functionName = "AnimStrike",
            };
            AnimationUtility.SetAnimationEvents(clip, new[] { animEvent });

            AnimationClipSettings st = AnimationUtility.GetAnimationClipSettings(clip);
            st.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, st);
            return clip;
        }

        private static AnimationClip BuildDieClip()
        {
            var clip = new AnimationClip { name = "GenericMelee_Die" };
            float end = 0.2f;
            AnimationCurve down = AnimationCurve.Linear(0f, 1f, end, 0.88f);
            SetScaleCurves(clip, down, down, down);
            AnimationClipSettings st = AnimationUtility.GetAnimationClipSettings(clip);
            st.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, st);
            return clip;
        }

        private static void SetScaleCurves(AnimationClip clip, AnimationCurve x, AnimationCurve y, AnimationCurve z)
        {
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(ChildPath, typeof(Transform), "m_LocalScale.x"), x);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(ChildPath, typeof(Transform), "m_LocalScale.y"), y);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(ChildPath, typeof(Transform), "m_LocalScale.z"), z);
        }
    }
}
#endif
