﻿using UnityEngine;
using System;

namespace BoostBlasters.Levels.Paths
{
    /// <summary>
    /// A path made of bezier curve segments.
    /// </summary>
    public class BezierSpline : MonoBehaviour
    {
        [SerializeField]
        private Vector3[] m_points = new Vector3[4];
        [SerializeField]
        private BezierHandleMode[] m_modes = new BezierHandleMode[2];
        [SerializeField]
        private float[] m_waitTimes = new float[2];
        [SerializeField]
        private float[] m_edgeTimes = new float[1];

        [SerializeField]
        private bool m_loop = false;
        public bool Loop
        {
            get => m_loop;
            set
            {
                m_loop = value;
                if (value == true)
                {
                    m_modes[m_modes.Length - 1] = m_modes[0];
                    SetControlPoint(0, m_points[0]);
                }
            }
        }

        public int ControlPointCount => m_points.Length;

        public int CurveCount => (m_points.Length - 1) / 3;

        public int EdgeCount => m_edgeTimes.Length;

        public Vector3 GetControlPoint(int index)
        {
            return m_points[index];
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                Vector3 delta = point - m_points[index];
                if (m_loop)
                {
                    if (index == 0)
                    {
                        m_points[1] += delta;
                        m_points[m_points.Length - 2] += delta;
                        m_points[m_points.Length - 1] = point;
                    }
                    else if (index == m_points.Length - 1)
                    {
                        m_points[0] = point;
                        m_points[1] += delta;
                        m_points[index - 1] += delta;
                    }
                    else
                    {
                        m_points[index - 1] += delta;
                        m_points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        m_points[index - 1] += delta;
                    }
                    if (index + 1 < m_points.Length)
                    {
                        m_points[index + 1] += delta;
                    }
                }
            }
            m_points[index] = point;
            EnforceMode(index);
        }

        public float GetWaitTime(int index)
        {
            return m_waitTimes[(index + 1) / 3];
        }

        public void SetWaitTime(int index, float wait)
        {
            int modeIndex = (index + 1) / 3;
            m_waitTimes[modeIndex] = wait;

            if (m_loop)
            {
                if (modeIndex == 0)
                {
                    m_waitTimes[m_waitTimes.Length - 1] = wait;
                }
                else if (modeIndex == m_waitTimes.Length - 1)
                {
                    m_waitTimes[0] = wait;
                }
            }
        }

        public float GetEdgeTime(int index)
        {
            return m_edgeTimes[index];
        }

        public void SetEdgeTime(int index, float time)
        {
            m_edgeTimes[index] = time;
        }

        public BezierHandleMode GetControlPointMode(int index)
        {
            return m_modes[(index + 1) / 3];
        }

        public void SetControlPointMode(int index, BezierHandleMode mode)
        {
            int modeIndex = (index + 1) / 3;
            m_modes[modeIndex] = mode;

            if (m_loop)
            {
                if (modeIndex == 0)
                {
                    m_modes[m_modes.Length - 1] = mode;
                }
                else if (modeIndex == m_modes.Length - 1)
                {
                    m_modes[0] = mode;
                }
            }
            EnforceMode(index);
        }

        private void EnforceMode(int index)
        {
            int modeIndex = (index + 1) / 3;
            BezierHandleMode mode = m_modes[modeIndex];
        
            if (mode == BezierHandleMode.None)
            {
                Collapse(index);
                return;
            }

            if (mode == BezierHandleMode.Free || !m_loop && (modeIndex == 0 || modeIndex == m_modes.Length - 1))
            {
                return;
            }

            int middleIndex = modeIndex * 3;
            int fixedIndex;
            int enforcedIndex;

            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                if (fixedIndex < 0)
                {
                    fixedIndex = m_points.Length - 2;
                }
                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= m_points.Length)
                {
                    enforcedIndex = 1;
                }
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= m_points.Length)
                {
                    fixedIndex = 1;
                }
                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = m_points.Length - 2;
                }
            }

            Vector3 middle = m_points[middleIndex];
            Vector3 enforcedTangent = middle - m_points[fixedIndex];

            if (mode == BezierHandleMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, m_points[enforcedIndex]);
            }

            m_points[enforcedIndex] = middle + enforcedTangent;
        }

        private void Collapse(int index)
        {
            int pathIndex = ((index + 1) / 3) * 3;

            if (m_loop || pathIndex > 0)
            {
                m_points[(pathIndex - (m_loop && pathIndex == 0 ? 2 : 1) + m_points.Length) % m_points.Length] = m_points[pathIndex];
            }
            if (m_loop || pathIndex < m_points.Length - 1)
            {
                m_points[(pathIndex + (m_loop && pathIndex == m_points.Length - 1 ? 2 : 1)) % m_points.Length] = m_points[pathIndex];
            }
        }

        public bool IsUnusedHandle(int index)
        {
            return GetControlPointMode(index) == BezierHandleMode.None && index % 3 != 0;
        }

        public bool IsUnusedPoint(int index)
        {
            return IsUnusedHandle(index) || (m_loop && index == m_points.Length - 1);
        }

        public Vector3 GetPointWithWaits(float t, PathWrapMode warpMode)
        {
            int i;
            float time = GetCurveTimeWithWaits(t, warpMode, out i);
            return GetPoint(time, i);
        }

        public Vector3 GetVelocityWithWaits(float t, PathWrapMode warpMode)
        {
            int i;
            float time = GetCurveTimeWithWaits(t, warpMode, out i);
            return GetVelocity(time, i);
        }

        public Vector3 GetAccelerationWithWaits(float t, PathWrapMode warpMode)
        {
            int i;
            float time = GetCurveTimeWithWaits(t, warpMode, out i);
            return GetAcceleration(time, i);
        }

        public Vector3 GetPoint(float t)
        {
            int i;
            float time = GetCurveTime(t, out i);
            return GetPoint(time, i);
        }

        public Vector3 GetVelocity(float t)
        {
            int i;
            float time = GetCurveTime(t, out i);
            return GetVelocity(time, i);
        }

        public Vector3 GetAcceleration(float t)
        {
            int i;
            float time = GetCurveTime(t, out i);
            return GetAcceleration(time, i);
        }

        private float GetCurveTimeWithWaits(float t, PathWrapMode wrapMode, out int i)
        {
            if (!m_loop)
            {
                wrapMode = PathWrapMode.PingPong;
            }

            t = (wrapMode == PathWrapMode.PingPong) ? Mathf.PingPong(t, 1) : Mod(t, 1);

            bool loop = m_loop && wrapMode == PathWrapMode.Loop;

            float totalTime = 0;
            int lastIndex = m_waitTimes.Length - (loop ? 2 : 1);

            for (int j = 0; j <= lastIndex; j++)
            {
                if (wrapMode == PathWrapMode.PingPong && (j == 0 || j == lastIndex))
                {
                    totalTime += m_waitTimes[0] / 2;
                }
                else
                {
                    totalTime += m_waitTimes[j];
                }
                if (j < m_edgeTimes.Length)
                {
                    totalTime += m_edgeTimes[j];
                }
            }

            float evaluationTime = Mathf.Clamp01(t) * totalTime;
            
            float segmentStartTime = 0;
            float segmentEndTime = (wrapMode == PathWrapMode.PingPong) ? m_waitTimes[0] / 2 : m_waitTimes[0];
            i = 0;

            while (segmentEndTime < evaluationTime)
            {
                float segmentDuration = m_edgeTimes[i];
                segmentStartTime = segmentEndTime;
                segmentEndTime += segmentDuration;

                if (segmentEndTime >= evaluationTime)
                {
                    i *= 3;
                    return (evaluationTime - segmentStartTime) / segmentDuration;
                }

                i++;

                if (!(loop && i == m_waitTimes.Length - 1))
                {
                    segmentStartTime = segmentEndTime;
                    segmentEndTime += (wrapMode == PathWrapMode.PingPong && (i == m_waitTimes.Length - 1)) ? m_waitTimes[i] / 2 : m_waitTimes[i];
                    
                    if (segmentEndTime >= evaluationTime)
                    {
                        if (i == m_waitTimes.Length - 1)
                        {
                            i = m_points.Length - 4;
                            return 1;
                        }
                        else
                        {
                            i *= 3;
                            return 0;
                        }
                    }
                }
            }

            return 0;
        }

        private float GetCurveTime(float t, out int i)
        {
            if (t >= 1)
            {
                t = 1;
                i = m_points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }
            return t;
        }

        private float Mod(float x, float m)
        {
            float r = x % m;
            return r < 0 ? r + m : r;
        }

        private Vector3 GetPoint(float t, int i)
        {
            return transform.TransformPoint(Bezier.GetPoint(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], t));
        }

        private Vector3 GetVelocity(float t, int i)
        {
            return transform.TransformPoint(Bezier.GetFirstDerivative(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], t)) - transform.position;
        }

        private Vector3 GetAcceleration(float t, int i)
        {
            return transform.TransformPoint(Bezier.GetSecondDerivative(m_points[i], m_points[i + 1], m_points[i + 2], m_points[i + 3], t)) - transform.position;
        }

        public void AddCurve()
        {
            Vector3 point = m_points[m_points.Length - 1];

            Array.Resize(ref m_points, m_points.Length + 3);
            point.x += 1f;
            m_points[m_points.Length - 3] = point;
            point.x += 1f;
            m_points[m_points.Length - 2] = point;
            point.x += 1f;
            m_points[m_points.Length - 1] = point;

            Array.Resize(ref m_modes, m_modes.Length + 1);
            m_modes[m_modes.Length - 1] = m_modes[m_modes.Length - 2];
            EnforceMode(m_points.Length - 4);
        
            Array.Resize(ref m_waitTimes, m_waitTimes.Length + 1);
            m_waitTimes[m_modes.Length - 1] = 0;

            Array.Resize(ref m_edgeTimes, m_edgeTimes.Length + 1);
            m_edgeTimes[m_edgeTimes.Length - 1] = 1;

            if (m_loop)
            {
                m_points[m_points.Length - 1] = m_points[0];
                m_modes[m_modes.Length - 1] = m_modes[0];
                EnforceMode(0);
            }
        }

        public void RemoveCurve(int index)
        {
            if (m_points.Length > 1)
            {
                int pathIndex = ((index + 1) / 3) * 3;
                if (pathIndex < m_points.Length - 1)
                {
                    for (int i = pathIndex - 1; i < m_points.Length - 3; i += 3)
                    {
                        if (i > 0)
                        {
                            m_points[i + 0] = m_points[i + 3];
                        }

                        if (i + 4 < m_points.Length)
                        {
                            m_points[i + 1] = m_points[i + 4];
                        }

                        if (i + 5 < m_points.Length)
                        {
                            m_points[i + 2] = m_points[i + 5];
                        }
                    }
                }
                Array.Resize(ref m_points, m_points.Length - 3);

                int modeIndex = (index + 1) / 3;
                for (int i = modeIndex; i < m_modes.Length - 1; i++)
                {
                    m_modes[i] = m_modes[i + 1];
                    m_waitTimes[i] = m_waitTimes[i + 1];

                    if (i < m_edgeTimes.Length - 1)
                    {
                        m_edgeTimes[i] = m_edgeTimes[i + 1];
                    }
                }
                Array.Resize(ref m_modes, m_modes.Length - 1);
                Array.Resize(ref m_waitTimes, m_waitTimes.Length - 1);
                Array.Resize(ref m_edgeTimes, m_edgeTimes.Length - 1);

                for (int i = 0; i < m_points.Length; i++)
                {
                    EnforceMode(i);
                }
            }
        }

        public void CollapseHandles(int index)
        {
            Collapse(index);
            EnforceMode(index);
        }

        public void Reset()
        {
            m_points = new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 1f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
            };
            m_modes = new BezierHandleMode[]
            {
                BezierHandleMode.Free,
                BezierHandleMode.Free,
            };
            m_waitTimes = new float[2];
            m_edgeTimes = new float[] { 1f };
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < m_points.Length; i += 3)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(transform.TransformPoint(m_points[i]), 0.1f);
            }

            Gizmos.color = Color.gray;

            int steps = 20 * CurveCount;
            for (int i = 0; i < steps; i++)
            {
                Vector3 point = GetPoint(i / (float)steps);
                Gizmos.DrawLine(point, GetPoint((i + 1) / (float)steps));
            }
        }
    }
}
