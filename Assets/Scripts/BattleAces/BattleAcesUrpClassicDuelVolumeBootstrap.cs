using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.BattleAces
{
    /// <summary>
    /// URP 도입 시 <see cref="BattleAcesDemoStageScreenTone"/> 대신 Global Volume(비네팅·색보정)으로
    /// Built-in 쉐이더 <c>Hidden/BattleAcesDemoStageTone</c>와 비슷한 톤을 맞춤.
    /// Core/Universal 어셈블리가 없으면 조용히 실패(현재 프로젝트는 Built-in 전제).
    /// </summary>
    public static class BattleAcesUrpClassicDuelVolumeBootstrap
    {
        public const string VolumeChildName = "BA_DemoGlobalVolume_ClassicDuel";

        private static bool warnedMissingAssemblies;

        private static bool warnedReflectionFailed;

        /// <summary>메인 카메라 자식으로 글로벌 볼륨 생성 — SRP 일 때만</summary>
        public static bool TryAttachUnderCamera(Camera main)
        {
            RemoveUnderCamera(main);

            if (main == null || BattleAcesRenderPipelineUtility.IsBuiltInRenderPipeline())
            {
                return false;
            }

            try
            {
                return TryAttachViaReflection(main);
            }
            catch (Exception e)
            {
                if (!warnedReflectionFailed)
                {
                    warnedReflectionFailed = true;
                    Debug.LogWarning(
                        "[BattleAces] URP Global Volume 자동 설정 실패 — 수동 Volume Profile 을 권장합니다: " + e.Message);
                }

                return false;
            }
        }

        /// <summary>씬·메뉴 복구 시 카메라 아래 데모 볼륨만 제거</summary>
        public static void RemoveUnderCamera(Camera main)
        {
            if (main == null)
            {
                return;
            }

            Transform child = main.transform.Find(VolumeChildName);
            if (child != null)
            {
                TryDestroyAttachedProfile(child);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        private static bool TryAttachViaReflection(Camera main)
        {
            const string core = "Unity.RenderPipelines.Core.Runtime";
            const string universal = "Unity.RenderPipelines.Universal.Runtime";

            Type volumeType = ResolveType(
                "UnityEngine.Rendering.Volume, " + core);
            Type profileType = ResolveType(
                "UnityEngine.Rendering.VolumeProfile, " + core);
            Type vignetteType = ResolveType(
                "UnityEngine.Rendering.Universal.Vignette, " + universal);
            Type colorAdjType = ResolveType(
                "UnityEngine.Rendering.Universal.ColorAdjustments, " + universal,
                "UnityEngine.Rendering.ColorAdjustments, " + core);

            if (volumeType == null || profileType == null || vignetteType == null || colorAdjType == null)
            {
                if (!warnedMissingAssemblies)
                {
                    warnedMissingAssemblies = true;
                    Debug.Log(
                        "[BattleAces] SRP 가 켜져 있으나 Core/Universal 타입을 찾지 못했습니다. " +
                        "URP 패키지 추가 후 플레이하거나, Global Volume 을 씬에 직접 배치하세요.");
                }

                return false;
            }

            ScriptableObject profile = ScriptableObject.CreateInstance(profileType) as ScriptableObject;
            if (profile == null)
            {
                return false;
            }

            object vignette = InvokeProfileAdd(profile, profileType, vignetteType);
            object colorAdj = InvokeProfileAdd(profile, profileType, colorAdjType);
            if (vignette == null || colorAdj == null)
            {
                UnityEngine.Object.Destroy(profile);
                return false;
            }

            // BattleAcesDemoStageTone.shader 와 근접: 비네팅 + 살짝 대비/노출 — 비네팅 색은 순정 검정 대신 쿨 건메탈(팔레트 일치)
            TrySetVolumeParameter(vignette, "active", true);
            Color vignetteColor = Color.Lerp(Color.black, BattleAcesArtDirection.GunmetalDark, 0.42f);
            TrySetNestedColor(vignette, "color", vignetteColor);
            TrySetNestedFloat(vignette, "intensity", 0.34f);
            TrySetNestedFloat(vignette, "smoothness", 0.44f);
            TrySetNestedBool(vignette, "rounded", false);

            TrySetVolumeParameter(colorAdj, "active", true);
            TrySetNestedFloat(colorAdj, "postExposure", 0.125f);
            TrySetNestedFloat(colorAdj, "contrast", 6.5f);

            GameObject root = new GameObject(VolumeChildName);
            root.transform.SetParent(main.transform, false);
            root.hideFlags = HideFlags.DontSaveInEditor;

            // Volume 은 카메라 자식에만 붙임(카메라 컴포넌트와 분리·씬 전환 시 같이 정리)
            Component volume = root.AddComponent(volumeType);

            PropertyInfo sharedProfile = volumeType.GetProperty(
                "sharedProfile",
                BindingFlags.Public | BindingFlags.Instance);
            if (sharedProfile != null)
            {
                sharedProfile.SetValue(volume, profile);
            }
            else
            {
                PropertyInfo legacyProfile = volumeType.GetProperty(
                    "profile",
                    BindingFlags.Public | BindingFlags.Instance);
                legacyProfile?.SetValue(volume, profile);
            }

            PropertyInfo isGlobal = volumeType.GetProperty("isGlobal", BindingFlags.Public | BindingFlags.Instance);
            isGlobal?.SetValue(volume, true);

            PropertyInfo priority = volumeType.GetProperty("priority", BindingFlags.Public | BindingFlags.Instance);
            priority?.SetValue(volume, 0f);

            PropertyInfo weight = volumeType.GetProperty("weight", BindingFlags.Public | BindingFlags.Instance);
            weight?.SetValue(volume, 1f);

            return true;
        }

        private static Type ResolveType(params string[] assemblyQualifiedNames)
        {
            if (assemblyQualifiedNames == null)
            {
                return null;
            }

            for (int i = 0; i < assemblyQualifiedNames.Length; i++)
            {
                string name = assemblyQualifiedNames[i];
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                Type resolved = Type.GetType(name);
                if (resolved != null)
                {
                    return resolved;
                }
            }

            return null;
        }

        private static void TryDestroyAttachedProfile(Transform volumeRoot)
        {
            if (volumeRoot == null)
            {
                return;
            }

            Component volume = volumeRoot.GetComponent("Volume");
            if (volume == null)
            {
                return;
            }

            Type volumeType = volume.GetType();
            PropertyInfo sharedProfile = volumeType.GetProperty(
                "sharedProfile",
                BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo profile = volumeType.GetProperty(
                "profile",
                BindingFlags.Public | BindingFlags.Instance);

            ScriptableObject attachedProfile =
                sharedProfile?.GetValue(volume) as ScriptableObject ??
                profile?.GetValue(volume) as ScriptableObject;

            if (attachedProfile != null)
            {
                UnityEngine.Object.Destroy(attachedProfile);
            }
        }

        private static object InvokeProfileAdd(ScriptableObject profile, Type profileType, Type componentType)
        {
            MethodInfo[] methods = profileType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo addGeneric = null;
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (m.Name != "Add" || !m.IsGenericMethodDefinition)
                {
                    continue;
                }

                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length != 1 || ps[0].ParameterType != typeof(bool))
                {
                    continue;
                }

                addGeneric = m;
                break;
            }

            if (addGeneric == null)
            {
                return null;
            }

            MethodInfo constructed = addGeneric.MakeGenericMethod(componentType);
            return constructed.Invoke(profile, new object[] { true });
        }

        private static void TrySetVolumeParameter(object volumeComponent, string boolFieldName, bool value)
        {
            if (volumeComponent == null)
            {
                return;
            }

            FieldInfo f = volumeComponent.GetType().GetField(boolFieldName, BindingFlags.Public | BindingFlags.Instance);
            object param = f?.GetValue(volumeComponent);
            if (param == null)
            {
                return;
            }

            PropertyInfo val = param.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
            val?.SetValue(param, value);
        }

        private static void TrySetNestedColor(object volumeComponent, string fieldName, Color value)
        {
            if (volumeComponent == null)
            {
                return;
            }

            FieldInfo f = volumeComponent.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            object param = f?.GetValue(volumeComponent);
            if (param == null)
            {
                return;
            }

            PropertyInfo val = param.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
            val?.SetValue(param, value);
        }

        private static void TrySetNestedFloat(object volumeComponent, string fieldName, float value)
        {
            if (volumeComponent == null)
            {
                return;
            }

            FieldInfo f = volumeComponent.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            object param = f?.GetValue(volumeComponent);
            if (param == null)
            {
                return;
            }

            PropertyInfo val = param.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
            val?.SetValue(param, value);
        }

        private static void TrySetNestedBool(object volumeComponent, string fieldName, bool value)
        {
            if (volumeComponent == null)
            {
                return;
            }

            FieldInfo f = volumeComponent.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            object param = f?.GetValue(volumeComponent);
            if (param == null)
            {
                return;
            }

            PropertyInfo val = param.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
            val?.SetValue(param, value);
        }
    }
}
