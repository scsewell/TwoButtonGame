using System;
using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Characters;

using Framework;

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
        [SerializeField] private Image m_background = null;
        [SerializeField] private RawImage m_previewImage = null;
        [SerializeField] private Image m_highlight = null;
        [SerializeField] private Image m_speed = null;
        [SerializeField] private Image m_agility = null;
        [SerializeField] private TextMeshProUGUI m_description = null;
        [SerializeField] private TextMeshProUGUI m_readyText = null;

        [Header("Options")]

        [SerializeField]
        [Range(0.25f, 1f)]
        private float m_resolutionScale = 1.0f;
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
        [SerializeField]
        private Gradient m_ratingGradient = null;


        private PlayerSelectPanel m_panel;
        private Camera m_camera = null;
        private RenderTexture m_texture = null;
        private Dictionary<Character, GameObject> m_configToPreview = null;
        private List<GameObject> m_previewObjects = null;
        private GameObject m_currentPreview = null;
        private float m_selectTime = 0f;

        /// <summary>
        /// An event invoked once the user has selected a character.
        /// </summary>
        public event Action<Character> CharacterSelected;


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
        }

        /// <summary>
        /// Sets the selected character.
        /// </summary>
        /// <param name="character">The character to select, or null to select the first character.</param>
        public void SetCharacter(Character character)
        {
            m_characterSpinner.Index = character != null ? Array.IndexOf(CharacterManager.Characters, character) : 0;
        }

        protected override void OnShow()
        {
            m_camera.enabled = true;
            m_highlight.color = new Color(1f, 1f, 1f, 0f);

            PreviewCharacter(m_characterSpinner.Index);
        }

        protected override void OnHide()
        {
            if (m_camera != null)
            {
                m_camera.enabled = false;
            }
            FreeTexture();
        }

        public override void Back()
        {
            m_panel.BackToProfile();
        }

        protected override void OnUpdateVisuals()
        {
            var playerColor = Color.Lerp(Consts.GetRacerColor(m_panel.transform.GetSiblingIndex()), Color.white, 0.35f);

            // configure the preview camera
            CreateTexture();
            m_camera.backgroundColor = Color.Lerp(m_bgColor, playerColor, m_bgColorFac);

            // rotate the character preview
            var timeSinceSelect = Time.unscaledTime - m_selectTime;
            var rotSpeed = Mathf.Lerp(0, m_rotateSpeed, Mathf.SmoothStep(0f, 1f, (timeSinceSelect - m_rotateWait) / 4.0f));
            m_currentPreview.transform.Rotate(0, rotSpeed * Time.unscaledDeltaTime, 0, Space.Self);

            // fade out the highlight
            m_highlight.color = new Color(1f, 1f, 1f, MathUtils.Damp(m_highlight.color.a, 0f, 0.00000001, Time.unscaledDeltaTime));
        }

        private void PreviewCharacter(int index)
        {
            var character = CharacterManager.Characters[index];

            m_selectTime = Time.unscaledTime;
            m_highlight.color = new Color(1f, 1f, 1f, 0.15f);

            var meta = character.Meta;
            //SetRating(m_speed, meta.SpeedRating);
            //SetRating(m_agility, meta.AgilityRating);
            m_description.text = meta.Description;

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
        }

        private void CreateTexture()
        {
            var rect = m_previewImage.GetComponent<RectTransform>();

            var width = Mathf.RoundToInt(m_resolutionScale * rect.rect.width);
            var height = Mathf.RoundToInt(m_resolutionScale * rect.rect.height);

            if (m_texture == null || width != m_texture.width || height != m_texture.height)
            {
                FreeTexture();

                m_texture = new RenderTexture(width, height, 24);
                m_camera.targetTexture = m_texture;
                m_previewImage.texture = m_texture;
            }
        }

        private void FreeTexture()
        {
            if (m_texture != null)
            {
                m_texture.Release();
                m_texture = null;
            }
            if (m_camera != null)
            {
                m_camera.targetTexture = null;
            }
            m_previewImage.texture = null;
        }
    }
}
