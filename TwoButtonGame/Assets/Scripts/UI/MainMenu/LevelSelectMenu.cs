using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.MainMenus
{
    public class LevelSelectMenu : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Text m_levelName;
        [SerializeField] private Text m_levelDifficulty;
        [SerializeField] private Image m_levelPreview;
        [SerializeField] private Image m_levelHighlight;
        [SerializeField] private ControlPanel m_levelControls1;
        [SerializeField] private ControlPanel m_levelControls2;
        [SerializeField] private ControlPanel m_levelControls3;

        [Header("Options")]
        [SerializeField] [Range(1, 10)]
        private int m_maxLapCount = 5;
        [SerializeField] [Range(1, 10)]
        private int m_defaultLapCount = 3;

        private int m_selectedLevel;
        public LevelConfig SelectedLevel
        {
            get { return Main.Instance.LevelConfigs[m_selectedLevel]; }
        }

        private int m_lapCount;
        public int LapCount { get { return m_lapCount; } }

        public override void InitMenu(RaceParameters lastRace)
        {
            if (lastRace != null)
            {
                m_selectedLevel = Array.IndexOf(Main.Instance.LevelConfigs, lastRace.LevelConfig);
                m_lapCount = lastRace.Laps;
            }
            else
            {
                m_lapCount = m_defaultLapCount;
            }
        }

        protected override void OnResetMenu(bool fullReset)
        {
            m_levelHighlight.color = new Color(1, 1, 1, 0);
        }

        protected override void OnUpdate()
        {
            foreach (PlayerBaseInput input in MainMenu.ActiveInputs)
            {
                if (input.UI_Accept)
                {
                    MainMenu.LaunchRace();
                }
                else if (input.UI_Left || input.UI_Right)
                {
                    int configCount = Main.Instance.LevelConfigs.Length;
                    m_selectedLevel = (m_selectedLevel + configCount + (input.UI_Right ? 1 : -1)) % configCount;
                    m_levelHighlight.color = new Color(1, 1, 1, 0.5f);
                    MainMenu.PlaySelectSound();
                }
                else if (input.UI_Cancel)
                {
                    MainMenu.SetMenu(Menu.PlayerSelect, true);
                }
            }
        }

        protected override void OnUpdateGraphics()
        {
            LevelConfig config = SelectedLevel;
            m_levelName.text = config.Name;
            m_levelDifficulty.text = config.LevelDifficulty.ToString();
            m_levelPreview.sprite = config.Preview;

            m_levelHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_levelHighlight.color.a, 0, Time.unscaledDeltaTime / 0.05f));
            
            m_levelControls1.UpdateUI("Select", MainMenu.ActiveInputs.SelectMany(i => i.SpriteAccept).ToList());
            m_levelControls2.UpdateUI("Next",   MainMenu.ActiveInputs.SelectMany(i => i.SpriteLeftRight).ToList());
            m_levelControls3.UpdateUI("Back",   MainMenu.ActiveInputs.SelectMany(i => i.SpriteCancel).ToList());
        }
    }
}
