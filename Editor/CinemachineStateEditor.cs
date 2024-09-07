using UnityEditor;
using UnityEngine.UIElements;
using Arbelos.CameraUtility.Runtime;
using UnityEditor.UIElements;
using UnityEngine;

namespace Arbelos.CameraUtility.Editor
{
    [CustomEditor(typeof(CinemachineState))]
    public class CinemachineStateEditor: UnityEditor.Editor
    {
        public VisualTreeAsset m_InspectorXML;

        //State Type
        private PropertyField stateTypeField;
        private SerializedProperty stateTypeProperty;

        //Camera Speed
        private FloatField cameraSpeedField;
        private SerializedProperty cameraSpeedProperty;

        //State Duration
        private FloatField durationField;
        private SerializedProperty durationProperty;

        //Focus Object
        private PropertyField focusObjectField;
        private SerializedProperty focusObjectProperty;

        //Shake Variables
        private FloatField shakeAmplitudeField;
        private SerializedProperty shakeAmplitudeProperty;

        //Clash Zoom
        private FloatField zoomTargetFOVField;
        private SerializedProperty zoomTargetFOVProperty;

        private FloatField shakeFrequencyField;
        private SerializedProperty shakeFrequencyProperty;

        private PropertyField shakeSettingsField;
        private SerializedProperty shakeSettingsProperty;

        private SerializedProperty zoomDistanceFromTargetProperty;
        private FloatField zoomDistanceFromTargetField;

        private SerializedProperty zoomInDirectionProperty;
        private EnumField zoomInDirectionField;

        private SerializedProperty zoomCurveProperty;
        private PropertyField zoomCurveField;

        //Number of Cycles (Auto Pan)
        private IntegerField numOfCyclesField;
        private SerializedProperty numOfCyclesProperty;

        //Enter/Exit Events
        private SerializedProperty stateEnterEventProperty;
        private SerializedProperty stateExitEventProperty;
        private PropertyField stateEnterEventField;
        private PropertyField stateExitEventField;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            VisualElement myInspector = new VisualElement();

            // Load from default reference.
            m_InspectorXML.CloneTree(myInspector);

            AssignVariables(myInspector);

            OnlyDisplayRelevantTypeFields();

            stateTypeField.RegisterValueChangeCallback(evt =>
            {
                Debug.Log($"State Type Changed.");
                OnlyDisplayRelevantTypeFields();
            });

            // Return the finished Inspector UI.
            return myInspector;
        }

        private void AssignVariables(VisualElement _element)
        {
            //CinemachineState 
            stateTypeProperty = serializedObject.FindProperty("type");
            stateTypeField = _element.Q<PropertyField>("StateTypeField");

            //Camera Speed
            cameraSpeedProperty = serializedObject.FindProperty("cameraSpeed");
            cameraSpeedField = _element.Q<FloatField>("CameraSpeedField");

            //Auto Pan Duration
            durationProperty = serializedObject.FindProperty("duration");
            durationField = _element.Q<FloatField>("DurationField");

            //Auto Pan Focus Object
            focusObjectProperty = serializedObject.FindProperty("focusObject");
            focusObjectField = _element.Q<PropertyField>("FocusObjectField");

            //Shake Amplitude
            shakeAmplitudeProperty = serializedObject.FindProperty("shakeAmplitude");
            shakeAmplitudeField = _element.Q<FloatField>("ShakeAmplitudeField");

            //Shake Frequency
            shakeFrequencyProperty = serializedObject.FindProperty("shakeFrequency");
            shakeFrequencyField = _element.Q<FloatField>("ShakeFrequencyField");

            //Shake Settings
            shakeSettingsProperty = serializedObject.FindProperty("shakeSettings");
            shakeSettingsField = _element.Q<PropertyField>("ShakeSettingsField");

            //Clash Zoom State
            zoomTargetFOVProperty = serializedObject.FindProperty("zoomTargetFOV");
            zoomTargetFOVField = _element.Q<FloatField>("ZoomTargetFOVField");

            zoomInDirectionField = _element.Q<EnumField>("ZoomInDirectionField");
            zoomInDirectionProperty = serializedObject.FindProperty("zoomInDirection");

            zoomDistanceFromTargetField = _element.Q<FloatField>("ZoomDistanceFromTargetField");
            zoomDistanceFromTargetProperty = serializedObject.FindProperty("zoomDistanceFromTarget");

            zoomCurveField = _element.Q<PropertyField>("ZoomCurveField");
            zoomCurveProperty = serializedObject.FindProperty("zoomCurve");

            //Number of Cycles
            numOfCyclesProperty = serializedObject.FindProperty("numOfCycles");
            numOfCyclesField = _element.Q<IntegerField>("NumOfCyclesField");

            //Enter/Exit Events
            stateEnterEventProperty = serializedObject.FindProperty("stateEnterEvent");
            stateEnterEventField = _element.Q<PropertyField>("StateEnterEventField");
            stateExitEventProperty = serializedObject.FindProperty("stateExitEvent");
            stateExitEventField = _element.Q<PropertyField>("StateExitEventField");
        }

        private void HideAllFields()
        {
            //Auto Pan Fields
            cameraSpeedField.visible = false;
            durationField.visible = false;
            focusObjectField.visible = false;
            shakeAmplitudeField.visible = false;
            shakeFrequencyField.visible = false;
            numOfCyclesField.visible = false;
            shakeSettingsField.visible = false;
            zoomTargetFOVField.visible = false;
            zoomInDirectionField.visible = false;
            zoomDistanceFromTargetField.visible = false;
            zoomCurveField.visible = false;
        }

        private void OnlyDisplayRelevantTypeFields()
        {
            HideAllFields();
            switch(stateTypeProperty.enumValueIndex)
            {
                case (int)CinemachineStateType.Original:
                    cameraSpeedField.visible = true;
                    Debug.Log($"Current State Type is Original!");
                     break;

                case (int)CinemachineStateType.DollyPath:
                    cameraSpeedField.visible = true;
                    focusObjectField.visible = true;
                    Debug.Log($"Current State Type is Dolly Path!");
                    break;

                case (int)CinemachineStateType.ClashZoom:
                    focusObjectField.visible = true;
                    zoomTargetFOVField.visible = true;
                    durationField.visible = true;
                    zoomInDirectionField.visible = true;
                    zoomDistanceFromTargetField.visible = true;
                    zoomCurveField.visible = true;
                    Debug.Log($"Current State Type is Clash Zoom!");
                    break;

                case (int)CinemachineStateType.Shake:
                    Debug.Log($"Current State Type is Shake!");
                    durationField.visible = true;
                    shakeAmplitudeField.visible = true;
                    shakeFrequencyField.visible = true;
                    shakeSettingsField.visible = true;
                    break;

                case (int)CinemachineStateType.AutoPan:
                    cameraSpeedField.visible = true;
                    focusObjectField.visible = true;
                    numOfCyclesField.visible = true;
                    Debug.Log($"Current State Type is AutoPan!");
                    break;

            }
        }
    }
}

