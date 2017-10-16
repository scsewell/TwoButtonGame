using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.EditorTools;

[CustomEditor(typeof(RacePath))]
public class RacePathEditor : Editor
{
    private RacePath m_racePath;
    private SerializedProperty m_path;

    private void OnEnable()
    {
        m_racePath = (RacePath)target;
        m_path = serializedObject.FindProperty("m_path");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorExtentions.DrawList(m_path);
        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        Waypoint[] path = m_racePath.Path;

        for (int i = 0; i < path.Length; i++)
        {
            Handles.color = Color.yellow;

            Vector3 start = path[i].Position;
            Vector3 end = path[(i + 1) % path.Length].Position;
            Handles.DrawLine(start, end);

            if ((end - start).magnitude > 0.5f)
            {
                Handles.ConeHandleCap(0, (start + end) / 2, Quaternion.LookRotation(end - start), 2f, EventType.Repaint);
            }
        }
    }
}
