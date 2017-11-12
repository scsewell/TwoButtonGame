using UnityEngine;
using System.Collections;
using Framework.Interpolation;

public class TrajectoryVisualization
{
    private GameObject crosshairBox;
    private GameObject accelerationCrosshairBox;
    private GameObject trajectoryBox;
    private GameObject[] accelerationBoxes;
    private Vector3 lastVelocity;

    private static readonly int segments = 10;
    private static readonly int frames = 4;

    public TrajectoryVisualization(bool interpolated)
    {
        crosshairBox = makeBox(Color.green);
        crosshairBox.transform.localScale = new Vector3(1, 1, 1);

        accelerationCrosshairBox = makeBox(Color.yellow);
        accelerationCrosshairBox.transform.localScale = new Vector3(1, 1, 1);
        
        trajectoryBox = makeBox(Color.blue);

        accelerationBoxes = new GameObject[segments];
        for (int i = 0; i < segments; i++)
        {
            accelerationBoxes[i] = makeBox(Color.Lerp(Color.red, Color.magenta, i / (float) segments));
        }

        lastVelocity = Vector3.zero;
    }

    public GameObject makeBox(Color color)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(box.GetComponent<Collider>());
        box.AddComponent<TransformInterpolator>();
        box.GetComponent<MeshRenderer>().material.color = color;
        return box;
    }

    public void FixedUpdateTrajectory(Vector3 position, Vector3 velocity)
    {
        Vector3 acceleration = velocity - lastVelocity;
        Vector3 endpoint = position + velocity;

        positionBox(trajectoryBox, position, endpoint);
        crosshairBox.transform.position = endpoint;

        Vector3 estimatedPosition = position;
        Vector3 newEstimatedPosition = position;
        Vector3 estimatedVelocity = velocity;
        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < frames; j++)
            {
                //estimatedVelocity += Time.fixedDeltaTime * acceleration;
                estimatedVelocity += acceleration;
                newEstimatedPosition += Time.fixedDeltaTime * estimatedVelocity;
            }

            positionBox(accelerationBoxes[i], estimatedPosition, newEstimatedPosition);
            estimatedPosition = newEstimatedPosition;
        }
        accelerationCrosshairBox.transform.position = newEstimatedPosition;

        lastVelocity = velocity;
    }

    public void positionBox(GameObject box, Vector3 start, Vector3 end)
    {
        box.transform.localScale = new Vector3(0.5f, 0.5f, (end - start).magnitude);
        box.transform.position = (start + end) / 2;
        box.transform.LookAt(end);
    }

    public void EndVisualization()
    {
        if (trajectoryBox)
        {
            Object.Destroy(trajectoryBox);
        }
        if (crosshairBox)
        {
            Object.Destroy(crosshairBox);
        }
        for (int i = 0; i < segments; i++)
        {
            if (accelerationBoxes[i])
            {
                Object.Destroy(accelerationBoxes[i]);
            }
        }
    }
    
}
