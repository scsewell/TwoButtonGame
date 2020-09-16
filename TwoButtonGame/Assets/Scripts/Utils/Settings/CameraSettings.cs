using System;

using Framework.Settings;

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BoostBlasters
{
    /// <summary>
    /// Applies graphics settings to a camera.
    /// </summary>
    public class CameraSettings : MonoBehaviour
    {
        [Header("Settings")]

        [SerializeField]
        private EnumSetting m_antiAliasing = null;
        [SerializeField]
        private EnumSetting m_msaaSamples = null;

        [Header("Options")]

        [SerializeField]
        [Tooltip("The scaling factor applied to the camera's rendering resolution.")]
        [Range(0.25f, 1f)]
        private float m_resolutionScale = 1f;

        private Camera m_camera;
        private UniversalAdditionalCameraData m_universalCamera;
        private RenderTexture m_texture = null;
        private Vector2Int m_resolution = Vector2Int.zero;

        /// <summary>
        /// The base resolution of the render texture the camera draws to.
        /// </summary>
        /// <remarks>
        /// If set to <see cref="Vector2Int.zero"/>, the game view resolution will
        /// be used. Otherwise, the camera will render at the specified resolution.
        /// </remarks>
        public Vector2Int Resolution
        {
            get => m_resolution;
            set
            {
                var resolution = Vector2Int.Max(value, Vector2Int.zero);

                if (m_resolution != resolution)
                {
                    m_resolution = resolution;
                    CreateTextureIfNeeded();
                }
            }
        }

        /// <summary>
        /// The scaling factor applied to the camera's rendering resolution.
        /// </summary>
        /// <remarks>This value is clamped to the range [0,1].</remarks>
        public float ResolutionScale
        {
            get => m_resolutionScale;
            set
            {
                var scale = Mathf.Clamp01(value);

                if (m_resolutionScale != scale)
                {
                    m_resolutionScale = scale;
                    CreateTextureIfNeeded();
                }
            }
        }

        /// <summary>
        /// An event invoked when the render texture assigned to the camera has been set.
        /// </summary>
        public event Action<RenderTexture> RenderTextureChanged;


        private void Awake()
        {
            m_camera = GetComponent<Camera>();
            m_universalCamera = GetComponent<UniversalAdditionalCameraData>();

            // listen for changes to the settings
            m_antiAliasing.RegisterChangeHandler<AntiAliasingMode>(ConfigureAA);
            m_msaaSamples.RegisterChangeHandler<MsaaSamples>(ConfigureMSAA);
        }

        private void OnDestroy()
        {
            FreeTexture();
        }

        private void LateUpdate()
        {
            m_antiAliasing.Value = Time.time % 1f > 0.5f ? AntiAliasingMode.SMAA : AntiAliasingMode.FXAA;

            if (m_camera.enabled)
            {
                CreateTextureIfNeeded();
            }
            else
            {
                FreeTexture();
            }
        }

        private void ConfigureAA(AntiAliasingMode mode)
        {
            switch (mode)
            {
                case AntiAliasingMode.Off:
                case AntiAliasingMode.MSAA:
                    m_universalCamera.antialiasing = AntialiasingMode.None;
                    break;
                case AntiAliasingMode.SMAA:
                    m_universalCamera.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    m_universalCamera.antialiasingQuality = AntialiasingQuality.High;
                    break;
                case AntiAliasingMode.FXAA:
                    m_universalCamera.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    break;
            }

            CreateTextureIfNeeded();
        }

        private void ConfigureMSAA(MsaaSamples msaa)
        {
            CreateTextureIfNeeded();
        }

        private void CreateTextureIfNeeded()
        {
            var baseRes = m_resolution != Vector2Int.zero ? m_resolution : new Vector2(Screen.width, Screen.height);
            var res = Vector2Int.RoundToInt(m_resolutionScale * baseRes);

            var msaa = m_antiAliasing.GetTypedValue<AntiAliasingMode>() == AntiAliasingMode.MSAA ?
                (int)m_msaaSamples.GetTypedValue<MsaaSamples>():
                1;

            if (m_texture == null || res.x != m_texture.width || res.y != m_texture.height || msaa != m_texture.antiAliasing)
            {
                FreeTexture();

                var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                var format = pipeline.supportsHDR && m_camera.allowHDR ? DefaultFormat.HDR : DefaultFormat.LDR;

                var descriptor = new RenderTextureDescriptor(res.x, res.y)
                {
                    graphicsFormat = SystemInfo.GetGraphicsFormat(format),
                    depthBufferBits = 24,
                    msaaSamples = msaa,
                    useMipMap = false,
                };

                m_texture = new RenderTexture(descriptor);
                m_camera.targetTexture = m_texture;

                RenderTextureChanged?.Invoke(m_texture);
            }
        }

        private void FreeTexture()
        {
            if (m_camera != null)
            {
                m_camera.targetTexture = null;
            }
            if (m_texture != null)
            {
                m_texture.Release();
                m_texture = null;

                RenderTextureChanged?.Invoke(null);
            }
        }
    }
}
