using System;
using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Characters;
using BoostBlasters.Profiles;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenu
{
    public class PlayerCharacterSelectMenu : MenuScreen
    {
        [Header("Prefabs")]

        [SerializeField] private GameObject m_characterPreviewPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_playerName = null;
        [SerializeField] private Spinner m_characterSpinner = null;
        [SerializeField] private RawImage m_previewImage = null;
        [SerializeField] private Image m_highlight = null;
        [SerializeField] private TextMeshProUGUI m_description = null;
        [SerializeField] private TextMeshProUGUI m_readyText = null;
        [SerializeField] private Button m_continue = null;
        [SerializeField] private Control[] m_hideWhenReady = null;

        [Header("Options")]

        [SerializeField]
        private Color m_bgColor = new Color(0.05f, 0.05f, 0.05f);
        [SerializeField]
        [Range(0f, 1f)]
        private float m_bgColorFac = 0.1f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_rotateWait = 2.0f;
        [SerializeField]
        [Range(0f, 180f)]
        private float m_rotateSpeed = 30.0f;


        private PlayerSelectPanel m_panel;
        private Camera m_camera = null;
        private CameraSettings m_cameraSettings = null;
        private Dictionary<Character, GameObject> m_configToPreview = null;
        private List<GameObject> m_previewObjects = null;
        private GameObject m_currentPreview = null;
        private float m_selectTime;
        private bool m_ready;

        /// <summary>
        /// An event invoked once the user has selected a character.
        /// </summary>
        public event Action<Character> CharacterSelected;

        /// <summary>
        /// An event invoked once the user changes their readyness.
        /// </summary>
        public event Action<bool> ReadyChanged;

        /// <summary>
        /// An event invoked once the user want to continue to the next menu screen.
        /// </summary>
        public event Action Continue;


        protected override void OnInitialize()
        {
            m_panel = GetComponentInParent<PlayerSelectPanel>();

            // create a gameobject to store preview characters under
            var index = m_panel.transform.GetSiblingIndex();
            var previewLayer = index + 8;

            var previewRoot = new GameObject($"CharacterPreview{index}")
            {
                layer = previewLayer,
            };

            // create a camera for rendering the character preview
            var previewRig = Instantiate(m_characterPreviewPrefab, previewRoot.transform);
            previewRig.layer = previewLayer;

            m_camera = previewRig.GetComponentInChildren<Camera>();
            m_camera.cullingMask = (1 << previewLayer);

            m_cameraSettings = previewRig.GetComponentInChildren<CameraSettings>();
            m_cameraSettings.RenderTextureChanged += OnRenderTextureChanged;

            // load all the character models
            m_configToPreview = new Dictionary<Character, GameObject>();
            m_previewObjects = new List<GameObject>();

            foreach (var config in CharacterManager.Characters)
            {
                var pivot = new GameObject(config.Meta.Name)
                {
                    layer = previewLayer,
                };
                pivot.transform.SetParent(previewRoot.transform);

                var character = Instantiate(config.Graphics.Rig, pivot.transform);
                character.transform.localPosition = config.Graphics.Offset + Vector3.up;
                character.GetComponentsInChildren<Transform>(true).ToList().ForEach(r => r.gameObject.layer = previewLayer);

                m_configToPreview.Add(config, pivot);
                m_previewObjects.Add(pivot);
            }

            // configure the character spinner
            m_characterSpinner.Options = CharacterManager.Characters.Select(c => c.Meta.Name).ToArray();
            m_characterSpinner.ValueChanged += PreviewCharacter;

            m_characterSpinner.GetComponent<Button>().onClick.AddListener(SelectCharacter);

            // configure the continue button
            m_continue.onClick.AddListener(() => Continue?.Invoke());
        }

        public void Set(IProfile profile, Character character, bool ready)
        {
            m_playerName.text = profile.Name;
            m_playerName.color = Color.Lerp(Color.white, profile.Color, 0.65f);
            m_camera.backgroundColor = Color.Lerp(m_bgColor, profile.Color, m_bgColorFac);

            m_characterSpinner.Index = character != null ? Array.IndexOf(CharacterManager.Characters, character) : 0;
            SetReady(ready);
        }

        protected override void OnShow()
        {
            PreviewCharacter(m_characterSpinner.Index);

            m_camera.enabled = true;
            m_highlight.color = new Color(1f, 1f, 1f, 0f);
            m_selectTime = float.MinValue;
        }

        protected override void OnHide()
        {
            if (m_camera != null)
            {
                m_camera.enabled = false;
            }
        }

        public override void Back()
        {
            if (m_ready)
            {
                SetReady(false);
            }
            else
            {
                m_panel.BackToProfile();
            }
        }

        protected override void OnUpdateVisuals()
        {
            // configure the preview camera
            m_cameraSettings.Resolution = Vector2Int.RoundToInt(m_previewImage.rectTransform.rect.size);

            // rotate the character preview
            var timeSinceSelect = Time.unscaledTime - m_selectTime;
            var rotSpeed = Mathf.Lerp(0, m_rotateSpeed, Mathf.SmoothStep(0f, 1f, (timeSinceSelect - m_rotateWait) / 4.0f));
            m_currentPreview.transform.Rotate(0, rotSpeed * Time.unscaledDeltaTime, 0, Space.Self);

            // fade out the highlight
            m_highlight.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.035f, 0f, timeSinceSelect / 0.125f));
        }

        private void PreviewCharacter(int index)
        {
            var character = CharacterManager.Characters[index];

            m_selectTime = Time.unscaledTime;
            m_description.text = character.Meta.Description;

            if (m_configToPreview.TryGetValue(character, out m_currentPreview))
            {
                m_currentPreview.transform.rotation = Quaternion.identity;
            }
            foreach (var go in m_previewObjects)
            {
                go.SetActive(go == m_currentPreview);
            }
        }

        private void SelectCharacter()
        {
            var character = CharacterManager.Characters[m_characterSpinner.Index];
            CharacterSelected?.Invoke(character);

            SetReady(true);
        }

        private void SetReady(bool ready)
        {
            PrimarySelection.Current = ready ? m_continue.gameObject : m_characterSpinner.gameObject;

            m_description.enabled = !ready;
            foreach (var control in m_hideWhenReady)
            {
                control.SetActive(!ready);
            }

            m_readyText.enabled = ready;

            if (m_ready != ready)
            {
                m_ready = ready;
                ReadyChanged?.Invoke(ready);
            }
        }

        private void OnRenderTextureChanged(RenderTexture texture)
        {
            if (m_previewImage != null)
            {
                m_previewImage.texture = texture;
            }
        }
    }
}
