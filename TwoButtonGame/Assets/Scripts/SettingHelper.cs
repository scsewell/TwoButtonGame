using UnityEngine;

using Framework;
using Framework.Settings;

namespace BoostBlasters
{
    public enum AntiAliasingQuality
    {
        Off,
        Low,
        High,
    }

    public enum ShadowQuality
    {
        Off,
        Low,
        Medium,
        High,
        Ultra,
    }

    /// <summary>
    /// Contains utilities for managing settings.
    /// </summary>
    public class SettingHelper : ComponentSingleton<SettingHelper>
    {
        [SerializeField]
        private ResolutionSetting m_resolution = null;
        [SerializeField]
        private EnumSetting m_screenMode = null;
        [SerializeField]
        private BoolSetting m_vsync = null;

        [SerializeField]
        private EnumSetting m_shadowQuality = null;


        protected override void Awake()
        {
            base.Awake();

            // apply the current settings
            Screen.SetResolution(m_resolution.Value.width, m_resolution.Value.height, m_screenMode.GetTypedValue<FullScreenMode>());
            QualitySettings.vSyncCount = m_vsync.Value ? 1 : 0;

            SetShadowQuality(m_shadowQuality.GetTypedValue<ShadowQuality>());
            
            // listen for changes to the settings
            m_resolution.OnValueChanged += (res) => Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
            m_screenMode.OnValueChanged += (mode) => Screen.fullScreenMode = m_screenMode.GetTypedValue<FullScreenMode>();
            m_vsync.OnValueChanged += (vsync) => QualitySettings.vSyncCount = vsync ? 1 : 0;

            m_shadowQuality.OnValueChanged += (quality) => SetShadowQuality(m_shadowQuality.GetTypedValue<ShadowQuality>());
        }

        private void SetShadowQuality(ShadowQuality quality)
        {
            QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
            QualitySettings.shadowProjection = ShadowProjection.StableFit;
            QualitySettings.shadowNearPlaneOffset = 3f;

            switch (quality)
            {
                case ShadowQuality.Off:
                {
                    QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
                    break;
                }
                case ShadowQuality.Low:
                {
                    QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    QualitySettings.shadowCascades = 0;
                    QualitySettings.shadowDistance = 100f;
                    break;
                }
                case ShadowQuality.Medium:
                {
                    QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.Medium;
                    QualitySettings.shadowCascades = 2;
                    QualitySettings.shadowDistance = 300f;
                    break;
                }
                case ShadowQuality.High:
                {
                    QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.High;
                    QualitySettings.shadowCascades = 4;
                    QualitySettings.shadowDistance = 450f;
                    break;
                }
                case ShadowQuality.Ultra:
                {
                    QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                    QualitySettings.shadowCascades = 4;
                    QualitySettings.shadowDistance = 500f;
                    break;
                }
            }
        }
    }
}
