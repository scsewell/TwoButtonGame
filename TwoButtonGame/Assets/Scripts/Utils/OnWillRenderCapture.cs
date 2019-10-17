using UnityEngine;

namespace BoostBlasters
{
    public class OnWillRenderCapture : MonoBehaviour
    {
        private IOnWillRenderReceiver m_receiver;

        private void Awake()
        {
            m_receiver = GetComponentInParent<IOnWillRenderReceiver>();
        }

        private void OnWillRenderObject()
        {
            if (m_receiver != null)
            {
                m_receiver.OnWillRender(Camera.current);
            }
        }
    }
}
