using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Prevents a RectTransform from scaling to a wider aspect ration than the
    /// canvas reference resolution.
    /// </summary>
    /// <remarks>
    /// This is useful to get more consistent UI layouts on ultrawide aspect ratios.
    /// </remarks>
    [ExecuteInEditMode]
    public class LimitAspectRatio : MonoBehaviour
    {
        private RectTransform m_rect;
        private CanvasScaler m_scaler;
        private RectTransform m_canvasRect;

        private void Start()
        {
            m_rect = GetComponent<RectTransform>();
            m_scaler = GetComponentInParent<CanvasScaler>();

            if (m_scaler != null)
            {
                m_canvasRect = m_scaler.GetComponent<RectTransform>();
            }
        }

        private void Update()
        {
            if (m_canvasRect == null)
            {
                return;
            }

            var referenceAspect = m_scaler.referenceResolution.x / m_scaler.referenceResolution.y;
            var actualAspect = m_canvasRect.rect.width / m_canvasRect.rect.height;

            if (actualAspect > referenceAspect)
            {
                m_rect.sizeDelta = new Vector2(
                    m_canvasRect.rect.height * referenceAspect,
                    m_canvasRect.rect.height
                );
            }
            else
            {
                m_rect.sizeDelta = m_canvasRect.rect.size;
            }
        }
    }
}
