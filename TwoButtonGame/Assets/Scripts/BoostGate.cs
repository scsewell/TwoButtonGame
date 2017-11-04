using UnityEngine;

public class BoostGate : MonoBehaviour
{
    [SerializeField]
    private float m_energy = 50.0f;
    public float Energy { get { return m_energy; } }
}
