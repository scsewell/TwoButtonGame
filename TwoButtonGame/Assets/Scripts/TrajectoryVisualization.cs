using System.Collections;
using UnityEngine;
using Framework.Interpolation;
using System.Collections.Generic;

public class TrajectoryVisualization
{
    private const int SEGMENTS = 10;
    private const int FRAMES_PER_SEGMENT = 10;

    private GameObject m_crosshairBox;
    private GameObject m_trajectoryBox;
    private Dictionary<int, GameObject> m_accelerationCrosshairBox;
    private Dictionary<int, GameObject[]> m_accelerationBoxes;
    private Vector3 m_lastVelocity;
    private List<Vector3> m_positions;
    private List<float> m_rotations;
    private bool m_cubical;

    public TrajectoryVisualization(bool interpolated, bool cubical)
    {
        m_cubical = cubical;

        m_crosshairBox = MakeBox(Color.green, interpolated, cubical);
        m_crosshairBox.transform.localScale = 1.0f * Vector3.one;

        m_trajectoryBox = MakeBox(Color.blue, interpolated, cubical);

        m_accelerationCrosshairBox = new Dictionary<int, GameObject>();
        m_accelerationBoxes = new Dictionary<int, GameObject[]>();
        for (int input = 0; input < 5; input++)
        {
            m_accelerationCrosshairBox[input] = MakeBox(Color.yellow, interpolated, cubical);
            m_accelerationCrosshairBox[input].transform.localScale = 1.0f * Vector3.one;

            m_accelerationBoxes[input] = new GameObject[SEGMENTS];
            for (int i = 0; i < SEGMENTS; i++)
            {
                m_accelerationBoxes[input][i] = MakeBox(Color.Lerp(Color.red, Color.magenta, i / (float)SEGMENTS), interpolated, cubical);
            }
        }

        m_positions = new List<Vector3>(SEGMENTS * FRAMES_PER_SEGMENT);
        m_rotations = new List<float>(SEGMENTS * FRAMES_PER_SEGMENT);

        m_lastVelocity = Vector3.zero;
    }

    public GameObject MakeBox(Color color, bool interpolated, bool cubical)
    {
        GameObject box = GameObject.CreatePrimitive(m_cubical ? PrimitiveType.Cube : PrimitiveType.Cylinder);
        Object.DestroyImmediate(box.GetComponent<Collider>());
        box.GetComponent<MeshRenderer>().material.color = color;
        if (interpolated)
        {
            box.AddComponent<TransformInterpolator>();
        }
        return box;
    }

    public void FixedMemeUpdateTrajectory(MemeBoots memeBoots, Vector3 position, Vector3 velocity, float rotation, float angularVelocity, bool leftEngine, bool rightEngine, bool boost)
    {
        //Vector3 acceleration = velocity - m_lastVelocity;
        Vector3 endpoint = position + velocity;

        PositionBox(m_trajectoryBox, position, endpoint);
        m_crosshairBox.transform.position = endpoint;

        for (int input = 0; input < 5; input++)
        {
            m_positions.Clear();
            m_rotations.Clear();
            m_positions.Add(position);
            m_rotations.Add(rotation);
            memeBoots.PredictStep(SEGMENTS * FRAMES_PER_SEGMENT, m_positions, velocity, m_rotations, angularVelocity, input%2==0, input>1, input>3, Time.fixedDeltaTime);

            for (int i = 0; i < SEGMENTS; i++)
            {
                PositionBox(m_accelerationBoxes[input][i], m_positions[i * FRAMES_PER_SEGMENT], m_positions[(i + 1) * FRAMES_PER_SEGMENT]);
                m_accelerationBoxes[input][i].GetComponent<MeshRenderer>().material.color =
                    ((input % 2 == 0) == leftEngine) && ((input > 1) == rightEngine) && ((input > 3) == boost) ?
                    Color.Lerp(Color.green, Color.white, i / (float)SEGMENTS) : Color.Lerp(Color.red, Color.magenta, i / (float)SEGMENTS);
            }
            m_accelerationCrosshairBox[input].transform.position = m_positions[SEGMENTS * FRAMES_PER_SEGMENT];
        }

        m_lastVelocity = velocity;
    }

    public void FixedUpdateTrajectory(Vector3 position, Vector3 velocity)
    {
        Vector3 acceleration = velocity - m_lastVelocity;
        Vector3 endpoint = position + velocity;

        PositionBox(m_trajectoryBox, position, endpoint);
        m_crosshairBox.transform.position = endpoint;

        Vector3 estimatedPosition = position;
        Vector3 newEstimatedPosition = position;
        Vector3 estimatedVelocity = velocity;
        for (int i = 0; i < SEGMENTS; i++)
        {
            for (int j = 0; j < FRAMES_PER_SEGMENT; j++)
            {
                estimatedVelocity += acceleration;
                newEstimatedPosition += Time.fixedDeltaTime * estimatedVelocity;
            }

            PositionBox(m_accelerationBoxes[0][i], estimatedPosition, newEstimatedPosition);
            estimatedPosition = newEstimatedPosition;
        }
        m_accelerationCrosshairBox[0].transform.position = newEstimatedPosition;

        m_lastVelocity = velocity;
    }

    public void PositionBox(GameObject box, Vector3 start, Vector3 end)
    {
        box.transform.position = (start + end) / 2;
        if (m_cubical)
        {
            box.transform.localScale = new Vector3(0.5f, 0.5f, (end - start).magnitude);
            box.transform.LookAt(end);
        }
        else
        {
            box.transform.localScale = new Vector3(0.3f, (end - start).magnitude / 2, 0.3f);
            box.transform.rotation = Quaternion.LookRotation(end - start) * Quaternion.Euler(90, 0, 0);
        }
    }

    public void EndVisualization()
    {
        if (m_trajectoryBox)
        {
            Object.Destroy(m_trajectoryBox);
            m_trajectoryBox = null;
        }
        if (m_crosshairBox)
        {
            Object.Destroy(m_crosshairBox);
            m_crosshairBox = null;
        }

        for (int input = 0; input < 5; input++)
        {
            if (m_accelerationCrosshairBox != null)
            {
                if (m_accelerationCrosshairBox[input] != null)
                {
                    Object.Destroy(m_accelerationCrosshairBox[input]);
                    m_accelerationCrosshairBox[input] = null;
                }
            }
            if (m_accelerationBoxes != null)
            {
                if (m_accelerationBoxes[input] != null)
                {
                    for (int i = 0; i < SEGMENTS; i++)
                    {
                        if (m_accelerationBoxes[input][i] != null)
                        {
                            Object.Destroy(m_accelerationBoxes[input][i]);
                            m_accelerationBoxes[input][i] = null;
                        }
                    }
                    m_accelerationBoxes[input] = null;
                }
            }
        }
        if (m_accelerationCrosshairBox != null)
        {
            m_accelerationCrosshairBox = null;
        }
        if (m_accelerationBoxes != null)
        {
            m_accelerationBoxes = null;
        }
    }
}
