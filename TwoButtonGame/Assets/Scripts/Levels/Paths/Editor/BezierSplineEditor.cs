using UnityEditor;
using UnityEngine;

namespace BoostBlasters.Levels.Paths
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineEditor : Editor
    {
        private const int STEPS_PER_SEGMENT = 10;
        private const float DIRECTION_SCALE = 0.5f;
        private const float HANDLE_SIZE = 0.055f;
        private const float PICK_SIZE = 0.085f;

        private static Color[] MODE_COLORS =
        {
            Color.red,
            Color.cyan,
            Color.yellow,
            Color.magenta,
        };

        private BezierSpline m_spline = null;
        private Transform m_transform = null;
        private int m_selectedPoint = -1;
        private int m_selectedEdge = -1;

        private void OnEnable()
        {
            m_spline = target as BezierSpline;
            m_transform = m_spline.transform;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            bool loop = EditorGUILayout.Toggle("Loop", m_spline.Loop);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_spline, "Toggle Loop");
                m_spline.Loop = loop;
                EditorUtility.SetDirty(m_spline);
                SceneView.RepaintAll();
            }
            EditorGUILayout.Space();

            if (m_selectedPoint >= 0 && m_selectedPoint < m_spline.ControlPointCount)
            {
                EditorGUILayout.Space();
                DrawSelectedPointInspector();
                EditorGUILayout.Space();
            }

            if (m_selectedEdge >= 0 && m_selectedEdge < m_spline.EdgeCount)
            {
                EditorGUILayout.Space();
                DrawSelectedEdgeInspector();
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add New Segment"))
            {
                Undo.RecordObject(m_spline, "Add New Curve Segment");
                m_spline.AddCurve();
                EditorUtility.SetDirty(m_spline);
            }
        }

        private void DrawSelectedPointInspector()
        {
            if (m_spline.IsUnusedHandle(m_selectedPoint))
            {
                m_selectedPoint = ((m_selectedPoint + 1) / 3) * 3;
                SceneView.RepaintAll();
            }
            else if (m_spline.IsUnusedPoint(m_selectedPoint))
            {
                m_selectedPoint = 0;
                SceneView.RepaintAll();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev", EditorStyles.miniButton, GUILayout.MaxWidth(55)))
            {
                SelectPrevPoint();
                while (m_spline.IsUnusedPoint(m_selectedPoint)) { SelectPrevPoint(); }
                SceneView.RepaintAll();
            }

            Color col = EditorStyles.centeredGreyMiniLabel.normal.textColor;
            EditorStyles.centeredGreyMiniLabel.normal.textColor = Color.black;
            GUILayout.Label("Selected Point: " + (m_selectedPoint + 1) + "/" + m_spline.ControlPointCount, EditorStyles.centeredGreyMiniLabel);
            EditorStyles.centeredGreyMiniLabel.normal.textColor = col;

            if (GUILayout.Button("Next", EditorStyles.miniButton, GUILayout.MaxWidth(55)))
            {
                SelectNextPoint();
                while (m_spline.IsUnusedPoint(m_selectedPoint)) { SelectNextPoint(); }
                SceneView.RepaintAll();
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            Vector3 point = EditorGUILayout.Vector3Field("Position", m_spline.GetControlPoint(m_selectedPoint));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_spline, "Move Point");
                m_spline.SetControlPoint(m_selectedPoint, point);
                EditorUtility.SetDirty(m_spline);
            }

            EditorGUI.BeginChangeCheck();
            float wait = EditorGUILayout.Slider("Wait Time", m_spline.GetWaitTime(m_selectedPoint), 0, 10);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_spline, "Change Point Wait Time");
                m_spline.SetWaitTime(m_selectedPoint, wait);
                EditorUtility.SetDirty(m_spline);
            }

            EditorGUI.BeginChangeCheck();
            BezierHandleMode mode = (BezierHandleMode)EditorGUILayout.EnumPopup("Handle Mode", m_spline.GetControlPointMode(m_selectedPoint));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_spline, "Change Point Mode");
                m_spline.SetControlPointMode(m_selectedPoint, mode);
                EditorUtility.SetDirty(m_spline);
            }

            if (GUILayout.Button("Collapse Handles"))
            {
                Undo.RecordObject(m_spline, "Collapse Curve Handles");
                m_spline.CollapseHandles(m_selectedPoint);
                EditorUtility.SetDirty(m_spline);
            }

            if (GUILayout.Button("Remove Point"))
            {
                Undo.RecordObject(m_spline, "Remove Curve Segment");
                m_spline.RemoveCurve(m_selectedPoint);
                EditorUtility.SetDirty(m_spline);
            }
        }

        private void SelectPrevPoint()
        {
            m_selectedPoint = ((m_selectedPoint - 1) + m_spline.ControlPointCount) % m_spline.ControlPointCount;
        }

        private void SelectNextPoint()
        {
            m_selectedPoint = (m_selectedPoint + 1) % m_spline.ControlPointCount;
        }

        private void DrawSelectedEdgeInspector()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev", EditorStyles.miniButton, GUILayout.MaxWidth(55)))
            {
                m_selectedEdge = ((m_selectedEdge - 1) + m_spline.EdgeCount) % m_spline.EdgeCount;
                SceneView.RepaintAll();
            }

            Color col = EditorStyles.centeredGreyMiniLabel.normal.textColor;
            EditorStyles.centeredGreyMiniLabel.normal.textColor = Color.black;
            GUILayout.Label("Selected Edge: " + (m_selectedEdge + 1) + "/" + m_spline.EdgeCount, EditorStyles.centeredGreyMiniLabel);
            EditorStyles.centeredGreyMiniLabel.normal.textColor = col;

            if (GUILayout.Button("Next", EditorStyles.miniButton, GUILayout.MaxWidth(55)))
            {
                m_selectedEdge = (m_selectedEdge + 1) % m_spline.EdgeCount;
                SceneView.RepaintAll();
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            float duration = EditorGUILayout.Slider("Edge Duration", m_spline.GetEdgeTime(m_selectedEdge), 0, 10);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_spline, "Change Edge Duration");
                m_spline.SetEdgeTime(m_selectedEdge, duration);
                EditorUtility.SetDirty(m_spline);
            }
        }

        private void OnSceneGUI()
        {
            Quaternion handleRot = Tools.pivotRotation == PivotRotation.Local ? m_transform.rotation : Quaternion.identity;

            Vector3 p0, p1, p2, p3;
            ShowPoint(0, handleRot, out p0);

            for (int i = 1; i < m_spline.ControlPointCount; i += 3)
            {
                ShowPoint(i + 2, handleRot, out p3);

                if (ShowPoint(i, handleRot, out p1))
                {
                    Handles.color = Color.gray;
                    Handles.DrawLine(p0, p1);
                }
                if (ShowPoint(i + 1, handleRot, out p2))
                {
                    Handles.color = Color.gray;
                    Handles.DrawLine(p2, p3);
                }

                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
                p0 = p3;
            }

            for (int i = 0; i < m_spline.EdgeCount; i++)
            {
                ShowEdge(i, handleRot);
            }
        }

        private void ShowDirections()
        {
            Handles.color = Color.green;
            Vector3 point = m_spline.GetPoint(0);
            Handles.DrawLine(point, point + m_spline.GetVelocity(0).normalized * DIRECTION_SCALE);

            int steps = STEPS_PER_SEGMENT * m_spline.CurveCount;
            for (int i = 1; i <= steps; i++)
            {
                point = m_spline.GetPoint(i / (float)steps);
                Handles.DrawLine(point, point + m_spline.GetVelocity(i / (float)steps).normalized * DIRECTION_SCALE);
            }
        }

        private bool ShowPoint(int index, Quaternion handleRot, out Vector3 point)
        {
            point = m_transform.TransformPoint(m_spline.GetControlPoint(index));

            if (!m_spline.IsUnusedHandle(index))
            {
                float size = HandleUtility.GetHandleSize(point);
                if (index == 0)
                {
                    size *= 2f;
                }

                Handles.color = MODE_COLORS[(int)m_spline.GetControlPointMode(index)];

                if (Handles.Button(point, handleRot, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotHandleCap))
                {
                    m_selectedPoint = index;
                    m_selectedEdge = -1;
                    Repaint();
                }

                if (m_selectedPoint == index)
                {
                    EditorGUI.BeginChangeCheck();
                    point = Handles.DoPositionHandle(point, handleRot);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_spline, "Move Point");
                        EditorUtility.SetDirty(m_spline);
                        m_spline.SetControlPoint(index, m_transform.InverseTransformPoint(point));
                    }
                }
                return true;
            }
            return false;
        }

        private void ShowEdge(int index, Quaternion handleRot)
        {
            Vector3 point = m_spline.GetPoint((index + 0.5f) / m_spline.EdgeCount);

            float size = HandleUtility.GetHandleSize(point);

            Handles.color = (m_selectedEdge == index) ? Color.white : Color.gray;

            if (Handles.Button(point, handleRot, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotHandleCap))
            {
                m_selectedPoint = -1;
                m_selectedEdge = index;
                Repaint();
            }
        }
    }
}
