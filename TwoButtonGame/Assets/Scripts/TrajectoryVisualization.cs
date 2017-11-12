using System.Collections;
using UnityEngine;
using Framework.Interpolation;

public class TrajectoryVisualization
{
    private static readonly int SEGMENTS = 10;
    private static readonly int FRAMES = 10;

    private GameObject m_crosshairBox;
    private GameObject m_accelerationCrosshairBox;
    private GameObject m_trajectoryBox;
    private GameObject[] m_accelerationBoxes;
    private Vector3 m_lastVelocity;

    public TrajectoryVisualization(bool interpolated)
    {
        m_crosshairBox = MakeBox(Color.green);
        m_crosshairBox.transform.localScale = 1.0f * Vector3.one;

        m_accelerationCrosshairBox = MakeBox(Color.yellow);
        m_accelerationCrosshairBox.transform.localScale = 1.0f * Vector3.one;
        
        m_trajectoryBox = MakeBox(Color.blue);

        m_accelerationBoxes = new GameObject[SEGMENTS];
        for (int i = 0; i < SEGMENTS; i++)
        {
            m_accelerationBoxes[i] = MakeBox(Color.Lerp(Color.red, Color.magenta, i / (float)SEGMENTS));
        }

        m_lastVelocity = Vector3.zero;
    }

    public GameObject MakeBox(Color color)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(box.GetComponent<Collider>());
        box.AddComponent<TransformInterpolator>();
        box.GetComponent<MeshRenderer>().material.color = color;
        return box;
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
            for (int j = 0; j < FRAMES; j++)
            {
                //estimatedVelocity += Time.fixedDeltaTime * acceleration;
                estimatedVelocity += acceleration;
                newEstimatedPosition += Time.fixedDeltaTime * estimatedVelocity;
            }

            PositionBox(m_accelerationBoxes[i], estimatedPosition, newEstimatedPosition);
            estimatedPosition = newEstimatedPosition;
        }
        m_accelerationCrosshairBox.transform.position = newEstimatedPosition;

        m_lastVelocity = velocity;
    }

    public void PositionBox(GameObject box, Vector3 start, Vector3 end)
    {
        box.transform.localScale = new Vector3(0.5f, 0.5f, (end - start).magnitude);
        box.transform.position = (start + end) / 2;
        box.transform.LookAt(end);
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

        if (m_accelerationCrosshairBox)
        {
            Object.Destroy(m_accelerationCrosshairBox);
            m_accelerationCrosshairBox = null;
        }
        for (int i = 0; i < SEGMENTS; i++)
        {
            if (m_accelerationBoxes[i])
            {
                Object.Destroy(m_accelerationBoxes[i]);
                m_accelerationBoxes[i] = null;
            }
        }
    }
}
