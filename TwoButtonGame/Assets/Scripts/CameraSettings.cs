using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

using Framework.Settings;

namespace BoostBlasters
{
    /// <summary>
    /// Applies graphics settings to a camera.
    /// </summary>
    public class CameraSettings : MonoBehaviour
    {
        [SerializeField]
        private EnumSetting m_antiAliasing = null;

        private PostProcessLayer m_post = null;

        private void Awake()
        {
            m_post = GetComponentInChildren<PostProcessLayer>();

            // apply the current settings
            ConfigureAA(m_antiAliasing.GetTypedValue<AntiAliasingQuality>());

            // listen for changes to the settings
            m_antiAliasing.OnValueChanged += (aa) => ConfigureAA(m_antiAliasing.GetTypedValue<AntiAliasingQuality>());
        }

        private void ConfigureAA(AntiAliasingQuality quality)
        {
            if (m_post != null)
            {
                switch (quality)
                {
                    case AntiAliasingQuality.Off:
                        m_post.antialiasingMode = PostProcessLayer.Antialiasing.None;
                        break;
                    case AntiAliasingQuality.Low:
                        m_post.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                        break;
                    case AntiAliasingQuality.High:
                        m_post.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                        break;
                }
            }
        }
    }
}
