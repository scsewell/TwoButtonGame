using UnityEngine;

namespace BoostBlasters
{
    public static class PhysicsUtils
    {
        public static void GetTensor(Rigidbody rb, out Vector3 tensor, out Quaternion tensorRotation)
        {
            var constraints = rb.constraints;
            rb.constraints = RigidbodyConstraints.None;
            rb.ResetInertiaTensor();

            tensor = rb.inertiaTensor;
            tensorRotation = rb.inertiaTensorRotation;

            rb.constraints = constraints;
        }

        public static Vector3 GetAngularAcceleration(Quaternion rotation, Vector3 tensor, Quaternion tensorRotation, Vector3 torque)
        {
            var q = rotation * tensorRotation;

            var acceleration = Quaternion.Inverse(q) * torque;
            if (tensor.x > 0)
            {
                acceleration.x /= tensor.x;
            }
            if (tensor.y > 0)
            {
                acceleration.y /= tensor.y;
            }
            if (tensor.z > 0)
            {
                acceleration.z /= tensor.z;
            }

            return q * acceleration;
        }

        public static float ApplyTorque(float angularVelocity, float torque, float tensor, float deltaTime)
        {
            return angularVelocity + ((torque / tensor) * deltaTime);
        }

        public static float ApplyAngularDamping(float angularVelocity, float damping, float deltaTime)
        {
            return angularVelocity * Mathf.Clamp01(1 - (damping * deltaTime));
        }

        public static Vector3 ApplyForce(Vector3 velocity, Vector3 force, float mass, float deltaTime)
        {
            return velocity + ((force / mass) * deltaTime);
        }

        public static Vector3 ApplyDamping(Vector3 velocity, float damping, float deltaTime)
        {
            return velocity * Mathf.Clamp01(1 - (damping * deltaTime));
        }
    }
}
