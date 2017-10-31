using UnityEngine;

public class OnWillRenderCapture : MonoBehaviour
{
    private OnWillRenderReceiver m_receiver;

    private void Awake()
    {
        m_receiver = GetComponentInParent<OnWillRenderReceiver>();
    }

    private void OnWillRenderObject()
    {
        if (m_receiver != null)
        {
            m_receiver.OnWillRender(Camera.current);
        }
    }
}
