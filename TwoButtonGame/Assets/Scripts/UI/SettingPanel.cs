using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Framework.SettingManagement;

public class SettingPanel : MonoBehaviour, IMoveHandler
{
    [SerializeField] private Text m_label;
    [SerializeField] private Text m_valueText;

    private Menu m_menu;
    private Func<ISetting> m_getSetting;
    private string[] m_options;

    private void Awake()
    {
        m_menu = GetComponentInParent<Menu>();
    }

    public SettingPanel Init(Func<ISetting> getSetting)
    {
        m_getSetting = getSetting;
        ISetting setting = getSetting();

        m_label.text = setting.Name;
        m_options = setting.DisplayOptions.Values;

        GetValue();

        return this;
    }

    public void GetValue()
    {
        m_valueText.text = m_getSetting().Serialize();
    }

    public void Apply()
    {
        m_getSetting().Deserialize(m_valueText.text);
    }

    public void OnMove(AxisEventData eventData)
    {
        switch (eventData.moveDir)
        {
            case MoveDirection.Left: OnPreviousValue(); break;
            case MoveDirection.Right: OnNextValue(); break;
        }
    }

    public void OnPreviousValue()
    {
        m_valueText.text = m_options[(GetCurrentIndex() + m_options.Length - 1) % m_options.Length];
        m_menu.PlaySelectSound();
    }

    public void OnNextValue()
    {
        m_valueText.text = m_options[(GetCurrentIndex() + 1) % m_options.Length];
        m_menu.PlaySelectSound();
    }

    private int GetCurrentIndex()
    {
        for (int i = 0; i < m_options.Length; i++)
        {
            if (m_options[i] == m_valueText.text)
            {
                return i;
            }
        }
        return 0;
    }
}
