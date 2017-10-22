using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{
    [SerializeField]
    private RectTransform m_buttonPrefab;
    [SerializeField]
    private Text m_description;

    private List<string> m_buttonNames = new List<string>();
    private List<RectTransform> m_buttons = new List<RectTransform>();

    private RectTransform m_key;

    private void Awake()
    {
        m_key = Instantiate(m_buttonPrefab, transform, false);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void UpdateUI(string descrption, string buttonName)
    {
        UpdateUI(descrption, new List<string>() { buttonName });
    }

    public void UpdateUI(string descrption, List<string> buttonNames)
    {
        m_description.text = descrption;

        bool needsRefresh = m_buttonNames.Count != buttonNames.Count;
        if (!needsRefresh)
        {
            for (int i = 0; i < buttonNames.Count; i++)
            {
                if (!m_buttonNames.Contains(buttonNames[i]))
                {
                    needsRefresh = true;
                }
            }
        }

        if (needsRefresh)
        {
            m_buttonNames = new List<string>(buttonNames);

            //*
            string str = "";
            for (int i = 0; i < m_buttonNames.Count; i++)
            {
                str += m_buttonNames[i];
                if (i != m_buttonNames.Count - 1)
                {
                    str += " + ";
                }
            }
            m_key.GetComponentInChildren<Text>().text = str;
            /*/
            for (int i = 0; i < Mathf.Max(m_buttonNames.Count, m_buttons.Count); i++)
            {
                RectTransform button;

                if (i < m_buttonNames.Count)
                {
                    if (i >= m_buttons.Count)
                    {
                        button = Instantiate(m_buttonPrefab, transform, false);
                        m_buttons.Add(button);
                    }
                    else
                    {
                        button = m_buttons[i];
                        button.gameObject.SetActive(true);
                    }
                    button.GetComponentInChildren<Text>().text = m_buttonNames[i];
                }
                else
                {
                    button = m_buttons[i];
                    button.gameObject.SetActive(false);
                }
            }
            //*/
        }
    }
}
