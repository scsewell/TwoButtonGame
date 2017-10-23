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

        if (path != null)
        {
            for (int i = 0; i < path.Length; i++)
            {
                Waypoint start = path[i];
                Waypoint end = path[(i + 1) % path.Length];

                if (start == null || end == null)
                {
                    continue;
                }
                
                Vector3 startPos = start.Position;
                Vector3 EndPos = end.Position;

                Handles.color = Color.yellow;
                Handles.DrawLine(startPos, EndPos);

                if ((EndPos - startPos).magnitude > 0.5f)
                {
                    Handles.ConeHandleCap(0, (startPos + EndPos) / 2, Quaternion.LookRotation(EndPos - startPos), 2f, EventType.Repaint);
                }
            }
        }
    }
}
