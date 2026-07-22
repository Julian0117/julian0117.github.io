using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AssignToggleIcon))]
public class ToggleIconAssignerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AssignToggleIcon assigner = (AssignToggleIcon)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Setze Icon Images"))
        {
            assigner.SetIcons();
        }
    }
}