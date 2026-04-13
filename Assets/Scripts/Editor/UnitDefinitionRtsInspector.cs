using Game.Units;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// UnitDefinition 하단에 RTS 무게감 프로필 요약 표시 (튜닝 가이드).
    /// </summary>
    [CustomEditor(typeof(UnitDefinition))]
    public sealed class UnitDefinitionRtsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var def = (UnitDefinition)target;
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("RTS 무게감 요약", EditorStyles.boldLabel);
            MoveProfileData mp = def.MoveProfile;
            EditorGUILayout.LabelField("Nav 가속", $"{mp.navAcceleration:0.#}");
            EditorGUILayout.LabelField("Nav 각속도(deg)", $"{mp.navAngularSpeedDeg:0.#}");
            EditorGUILayout.LabelField("선딜 이동 배율", $"{mp.windupMoveSpeedMultiplier:0.##}");
            EditorGUILayout.LabelField("비행 수평 가속", $"{mp.flyHorizontalAcceleration:0.#}");
            EditorGUILayout.LabelField("군중 분리", mp.enableCrowdSeparation ? "켜짐" : "끔");

            AnimCombatProfileData ap = def.AnimCombatProfile;
            bool bridgeOk = ap.useAnimDrivenStrike && def.OptionalAnimatorController != null;
            EditorGUILayout.LabelField("애니 타격 동기", ap.useAnimDrivenStrike ? "켜짐 (AnimStrike 이벤트 필요)" : "끔 (레거시 타이머)");
            EditorGUILayout.LabelField("타격 타임아웃(uns)", $"{ap.strikeFallbackTimeoutUnscaled:0.##}");
            EditorGUILayout.LabelField("사망 후 파괴 지연(uns)", $"{ap.deathDestroyDelayUnscaled:0.##} (0=즉시)");
            string acName = def.OptionalAnimatorController != null ? def.OptionalAnimatorController.name : "(없음)";
            EditorGUILayout.LabelField("Animator 컨트롤러", acName);
            EditorGUILayout.LabelField("런타임 StrikeBridge", bridgeOk ? "스폰 시 부착됨" : "컨트롤러 없으면 미부착");
            EditorGUILayout.HelpBox(
                "Animator 파라미터 이름은 AnimCombatProfile 에서 수정. 계약은 Assets/Docs/BATTLE_ACES_ANIMATOR_CONTRACT.md 참고.",
                MessageType.Info);
        }
    }
}
