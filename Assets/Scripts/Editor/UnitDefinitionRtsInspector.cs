using Game.Units;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Shows a short RTS-focused summary under the default UnitDefinition inspector.
    /// </summary>
    [CustomEditor(typeof(UnitDefinition))]
    public sealed class UnitDefinitionRtsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UnitDefinition definition = (UnitDefinition)target;
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("RTS Summary", EditorStyles.boldLabel);

            MoveProfileData moveProfile = definition.MoveProfile;
            EditorGUILayout.LabelField("Nav Acceleration", $"{moveProfile.navAcceleration:0.#}");
            EditorGUILayout.LabelField("Nav Turn Speed", $"{moveProfile.navAngularSpeedDeg:0.#} deg");
            EditorGUILayout.LabelField("Windup Move Multiplier", $"{moveProfile.windupMoveSpeedMultiplier:0.##}");
            EditorGUILayout.LabelField("Flying Acceleration", $"{moveProfile.flyHorizontalAcceleration:0.#}");
            EditorGUILayout.LabelField("Crowd Separation", moveProfile.enableCrowdSeparation ? "Enabled" : "Disabled");

            AnimCombatProfileData animProfile = definition.AnimCombatProfile;
            bool usesAnimDrivenStrike = animProfile.useAnimDrivenStrike;
            bool hasAnimatorController = definition.OptionalAnimatorController != null;
            bool bridgeExpected = usesAnimDrivenStrike && hasAnimatorController;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Animation Combat", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Anim-Driven Strike", usesAnimDrivenStrike ? "Enabled" : "Disabled");
            EditorGUILayout.LabelField("Strike Fallback", $"{animProfile.strikeFallbackTimeoutUnscaled:0.##} s");
            EditorGUILayout.LabelField("Death Destroy Delay", $"{animProfile.deathDestroyDelayUnscaled:0.##} s");
            EditorGUILayout.LabelField("Animator Controller", hasAnimatorController ? definition.OptionalAnimatorController.name : "(None)");
            EditorGUILayout.LabelField("Runtime Strike Bridge", bridgeExpected ? "Added at spawn time" : "Not required");

            string visualPath = definition.OptionalUnitVisualResourcePath;
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Visual Override", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Resources Path", string.IsNullOrWhiteSpace(visualPath) ? "(Catalog default)" : visualPath);

            EditorGUILayout.HelpBox(
                "Animation parameter names come from AnimCombatProfile. If Anim-Driven Strike is enabled and an Animator Controller is assigned, the strike bridge is added automatically at spawn time.",
                MessageType.Info);
        }
    }
}
