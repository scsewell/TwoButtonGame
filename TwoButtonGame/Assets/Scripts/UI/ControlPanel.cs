using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{
    [SerializeField]
    private Text m_description;

    private List<Sprite> m_buttonSprites = new List<Sprite>();
    private List<Image> m_buttons = new List<Image>();

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void UpdateUI(string descrption, Sprite sprite)
    {
        UpdateUI(descrption, new List<Sprite>() { sprite });
    }

    public void UpdateUI(string descrption, List<Sprite> sprites)
    {
        m_description.text = descrption;

        sprites = sprites.Distinct().ToList();

        bool needsRefresh = m_buttonSprites.Count != sprites.Count;
        if (!needsRefresh)
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                if (!m_buttonSprites.Contains(sprites[i]))
                {
                    needsRefresh = true;
                }
            }
        }

        if (needsRefresh)
        {
            m_buttonSprites = new List<Sprite>(sprites);
            
            for (int i = 0; i < Mathf.Max(m_buttonSprites.Count, m_buttons.Count); i++)
            {
                Image image;

                if (i < m_buttonSprites.Count)
                {
                    LayoutElement layout;

                    if (i >= m_buttons.Count)
                    {
                        GameObject go = CreateRT(transform);
                        go.AddComponent<CanvasRenderer>();

                        image = go.AddComponent<Image>();
                        image.preserveAspect = true;

                        layout = go.AddComponent<LayoutElement>();

                        //LayoutElement spacer = CreateRT(transform).AddComponent<LayoutElement>();
                        //spacer.minWidth = 5;

                        m_buttons.Add(image);
                    }
                    else
                    {
                        image = m_buttons[i];
                        image.gameObject.SetActive(true);
                        layout = image.GetComponent<LayoutElement>();
                    }

                    Sprite sprite = m_buttonSprites[i];
                    image.sprite = sprite;
                    layout.preferredHeight = 50;
                    layout.preferredWidth = sprite.rect.width * (layout.preferredHeight / sprite.rect.height);
                }
                else
                {
                    image = m_buttons[i];
                    image.gameObject.SetActive(false);
                }
            }
        }
    }

    private GameObject CreateRT(Transform parent)
    {
        GameObject go = new GameObject();
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent);
        rt.localScale = Vector3.one;
        rt.pivot = 0.5f * Vector3.one;
        return go;
    }
}
