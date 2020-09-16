using Framework.Settings;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BoostBlasters
{
    public enum AntiAliasingMode
    {
        Off,
        FXAA,
        SMAA,
        MSAA,
    }

    public enum MsaaSamples
    {
        Sample2 = 2,
        Sample4 = 4,
        Sample8 = 8,
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
    public class SettingHelper : Framework.ComponentSingleton<SettingHelper>
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

            // listen for changes to the settings
            m_resolution.RegisterChangeHandler((res) => Screen.SetResolution(res.width, res.height, Screen.fullScreenMode));
            m_screenMode.RegisterChangeHandler((mode) => Screen.fullScreenMode = m_screenMode.GetTypedValue<FullScreenMode>());
            m_vsync.RegisterChangeHandler((vsync) => QualitySettings.vSyncCount = vsync ? 1 : 0);

            m_shadowQuality.RegisterChangeHandler<ShadowQuality>(SetShadowQuality);
        }

        private void SetShadowQuality(ShadowQuality quality)
        {
            //QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
            //QualitySettings.shadowProjection = ShadowProjection.StableFit;
            //QualitySettings.shadowNearPlaneOffset = 3f;

            //switch (quality)
            //{
            //    case ShadowQuality.Off:
            //    {
            //        QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
            //        break;
            //    }
            //    case ShadowQuality.Low:
            //    {
            //        QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            //        QualitySettings.shadowResolution = ShadowResolution.Low;
            //        QualitySettings.shadowCascades = 0;
            //        QualitySettings.shadowDistance = 150f;
            //        break;
            //    }
            //    case ShadowQuality.Medium:
            //    {
            //        QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            //        QualitySettings.shadowResolution = ShadowResolution.Medium;
            //        QualitySettings.shadowCascades = 2;
            //        QualitySettings.shadowDistance = 300f;
            //        break;
            //    }
            //    case ShadowQuality.High:
            //    {
            //        QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            //        QualitySettings.shadowResolution = ShadowResolution.High;
            //        QualitySettings.shadowCascades = 4;
            //        QualitySettings.shadowDistance = 600f;
            //        break;
            //    }
            //    case ShadowQuality.Ultra:
            //    {
            //        QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            //        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            //        QualitySettings.shadowCascades = 4;
            //        QualitySettings.shadowDistance = 1000f;
            //        break;
            //    }
            //}
        }
    }
}
