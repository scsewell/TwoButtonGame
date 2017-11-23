using System.Collections.Generic;
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
    }
    public override bool Boost
    {
        get { return GetKey(m_boost); }
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

        m_spriteLeft    = new List<Sprite>() { LoadSprite(hneg) };
        m_spriteRight   = new List<Sprite>() { LoadSprite(hpos) };
        m_spriteFore    = new List<Sprite>() { m_spriteLeft[0], m_spriteRight[0] };
        m_spriteBrake   = new List<Sprite>() { LoadSprite(vneg) };
        m_spriteBoost   = new List<Sprite>() { LoadSprite(boost) };

        m_spriteLeftRight   = new List<Sprite>() { m_spriteLeft[0], m_spriteRight[0] };
        m_spriteDownUp      = new List<Sprite>() { LoadSprite(vneg), LoadSprite(vpos) };
        m_spriteAccept  = new List<Sprite>() { LoadSprite(accept) };
        m_spriteCancel  = new List<Sprite>() { LoadSprite(cancel) };
        m_spriteMenu    = new List<Sprite>() { LoadSprite(menu) };
    }

    private Sprite LoadSprite(KeyCode key)
    {
        return Resources.Load<Sprite>("Keyboard_" + key.ToString());
    }
}
