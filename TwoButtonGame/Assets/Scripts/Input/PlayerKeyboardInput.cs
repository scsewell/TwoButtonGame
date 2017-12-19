using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerKeyboardInput : PlayerBaseInput
{
    private KeyCode m_hneg;
    private KeyCode m_hpos;
    private KeyCode m_vneg;
    private KeyCode m_vpos;
    private KeyCode m_boost;

    public override float H
    {
        get { return KeyAsAxis(m_hpos) - KeyAsAxis(m_hneg); }
    }
    public override float V
    {
        get { return (GetKey(m_hneg) && GetKey(m_hpos) ? 1 : 0) - KeyAsAxis(m_vneg); }
    }
    public override bool Boost
    {
        get { return GetKeyDown(m_boost); }
    }

    private KeyCode m_accept;
    private KeyCode m_cancel;
    private KeyCode m_menu;

    public override float UI_H
    {
        get { return KeyAsAxis(m_hpos) - KeyAsAxis(m_hneg); }
    }
    public override float UI_V
    {
        get { return KeyAsAxis(m_vneg) - KeyAsAxis(m_vpos); }
    }
    public override bool UI_Accept
    {
        get { return GetKeyDown(m_accept); }
    }
    public override bool UI_Cancel
    {
        get { return GetKeyDown(m_cancel); }
    }
    public override bool UI_Menu
    {
        get { return GetKeyDown(m_menu); }
    }

    public override bool IsController { get { return false; } }

    public PlayerKeyboardInput(
        KeyCode hneg, KeyCode hpos,
        KeyCode vneg, KeyCode vpos,
        KeyCode boost,
        KeyCode accept,
        KeyCode cancel,
        KeyCode menu
        ) : base()
    {
        m_hneg = hneg;
        m_hpos = hpos;
        m_vneg = vneg;
        m_vpos = vpos;
        m_boost = boost;
        m_accept = accept;
        m_cancel = cancel;
        m_menu = menu;

        m_spriteLeft    = GetSprite(hneg);
        m_spriteRight   = GetSprite(hpos);
        m_spriteFore    = GetSprites(hneg, hpos);
        m_spriteBrake   = GetSprite(vneg);
        m_spriteBoost   = GetSprite(boost);
        
        m_spriteNavigate    = GetSprites(hneg, hpos, vneg, vpos);
        m_spriteLeftRight   = GetSprites(hneg, hpos);
        m_spriteDownUp      = GetSprites(vneg, vpos);
        m_spriteAccept      = GetSprite(accept);
        m_spriteCancel      = GetSprite(cancel);
        m_spriteMenu        = GetSprite(menu);
    }

    private static readonly List<KeyCode> WASD = new List<KeyCode>
    {
        KeyCode.W,
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
    };

    private static readonly List<KeyCode> ARROWS = new List<KeyCode>
    {
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.DownArrow,
        KeyCode.UpArrow,
    };

    private List<Sprite> GetSprite(KeyCode key)
    {
        return new List<Sprite>() { LoadSprite(key) };
    }

    private List<Sprite> GetSprites(params KeyCode[] keys)
    {
        List<Sprite> sprites = new List<Sprite>();

        if (ARROWS.All(k => keys.Contains(k)))
        {
            sprites.Add(LoadSprite("Arrows"));
        }
        else if (WASD.All(k => keys.Contains(k)))
        {
            sprites.Add(LoadSprite("WASD"));
        }
        else
        {
            foreach (KeyCode key in keys)
            {
                sprites.Add(LoadSprite(key));
            }
        }
        return sprites;
    }

    private Sprite LoadSprite(KeyCode key)
    {
        return LoadSprite(key.ToString());
    }

    private Sprite LoadSprite(string str)
    {
        return Resources.Load<Sprite>("Keyboard_" + str);
    }
}
