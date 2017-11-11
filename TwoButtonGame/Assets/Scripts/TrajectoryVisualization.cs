using UnityEngine;
using System.Collections;
using Framework.Interpolation;

public class TrajectoryVisualization
{
    private GameObject trajectoryBox;
    private GameObject crosshairBox;

    public TrajectoryVisualization(bool interpolated)
    {
        trajectoryBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(trajectoryBox.GetComponent<Collider>());
        trajectoryBox.AddComponent<TransformInterpolator>();
        trajectoryBox.GetComponent<MeshRenderer>().material.color = Color.blue;

        crosshairBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(crosshairBox.GetComponent<Collider>());
        crosshairBox.AddComponent<TransformInterpolator>();
        crosshairBox.transform.localScale = new Vector3(1, 1, 1);
        crosshairBox.GetComponent<MeshRenderer>().material.color = Color.green;
    }

    public void FixedUpdateTrajectory(Vector3 position, Vector3 velocity)
    {
        Vector3 endpoint = position + velocity;

        trajectoryBox.transform.localScale = new Vector3(0.5f, 0.5f, velocity.magnitude);
        trajectoryBox.transform.position = (position + endpoint) / 2;
        trajectoryBox.transform.LookAt(endpoint);

        crosshairBox.transform.position = endpoint;
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
    }
    
}
