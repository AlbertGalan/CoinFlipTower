using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TutorialTrigger))]
public class TutorialTriggerEditor : Editor
{
    SerializedProperty stepIndexProp;
    SerializedProperty selectModeProp;
    SerializedProperty panelRefProp;
    SerializedProperty requiredTagProp;

    void OnEnable()
    {
        stepIndexProp = serializedObject.FindProperty("stepIndex");
        selectModeProp = serializedObject.FindProperty("selectMode");
        panelRefProp = serializedObject.FindProperty("panelReference");
        requiredTagProp = serializedObject.FindProperty("requiredTag");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(selectModeProp);

        var manager = FindObjectOfType<TutorialMessageManager>();
        if (manager != null && manager.messagePanels != null && manager.messagePanels.Length > 0)
        {
            string[] names = manager.GetPanelNames();
            int current = stepIndexProp.intValue;
            current = Mathf.Clamp(current, 0, names.Length - 1);
            int choice = EditorGUILayout.Popup("Panel (from Manager)", current, names);
            stepIndexProp.intValue = choice;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Or drag a panel manually:");
            EditorGUILayout.PropertyField(panelRefProp);
        }
        else
        {
            EditorGUILayout.HelpBox("No TutorialMessageManager with panels found in scene. You can still assign a panel manually below.", MessageType.Info);
            EditorGUILayout.PropertyField(stepIndexProp);
            EditorGUILayout.PropertyField(panelRefProp);
        }

        EditorGUILayout.PropertyField(requiredTagProp);

        // draw remaining default fields (mode, oneShot, etc.)
        DrawDefaultInspectorExcept(new string[] { "stepIndex", "selectMode", "panelReference", "requiredTag" });

        serializedObject.ApplyModifiedProperties();
    }

    void DrawDefaultInspectorExcept(string[] excluded)
    {
        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true); // skip script
        while (prop.NextVisible(false))
        {
            bool skip = false;
            foreach (var ex in excluded) if (prop.name == ex) { skip = true; break; }
            if (!skip) EditorGUILayout.PropertyField(prop, true);
        }
    }
}
