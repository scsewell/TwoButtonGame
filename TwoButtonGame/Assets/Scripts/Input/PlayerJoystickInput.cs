using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJoystickInput : PlayerBaseInput
{
    private static readonly string PREFIX = "J";

    private int m_joystickNumber;

    private string m_LX;
    private string m_LY;
    private string m_RX;
    private string m_RY;
    private string m_DX;
    private string m_DY;
    private string m_TL;
    private string m_TR;
    
    private KeyCode m_boost;

    public override float H
    {
        get { return Input.GetAxis(m_LX); }
    }
    public override float V
    {
        get { return -Input.GetAxis(m_LY); }
    }
    public override bool Boost
    {
        get { return Input.GetKey(m_boost); }
    }

    private KeyCode m_accept;
    private KeyCode m_cancel;
    private KeyCode m_menu;

    public override float UI_H { get { return H + Input.GetAxis(m_DX); } }
    public override float UI_V { get { return V + Input.GetAxis(m_DY); } }

    public override bool UI_Accept
    {
        get { return Input.GetKeyDown(m_accept); }
    }
    public override bool UI_Cancel
    {
        get { return Input.GetKeyDown(m_cancel); }
    }
    public override bool UI_Menu
    {
        get { return Input.GetKeyDown(m_menu); }
    }

    public PlayerJoystickInput(int number) : base()
    {
        m_joystickNumber = number;

        m_LX = GetAxis("LX");
        m_LY = GetAxis("LY");
        m_RX = GetAxis("RX");
        m_RY = GetAxis("RY");
        m_DX = GetAxis("DX");
        m_DY = GetAxis("DY");
        m_TL = GetAxis("TL");
        m_TR = GetAxis("TR");

        m_boost = GetButton(0);
        m_accept = GetButton(0);
        m_cancel = GetButton(1);
        m_menu = GetButton(10);

        m_spriteLeft    = new List<Sprite>() { LoadSprite("LS") };
        m_spriteRight   = new List<Sprite>() { LoadSprite("LS") };
        m_spriteFore    = new List<Sprite>() { LoadSprite("LS") };
        m_spriteBrake   = new List<Sprite>() { LoadSprite("LS") };
        m_spriteBoost   = new List<Sprite>() { LoadSprite("A") };

        m_spriteLeftRight   = new List<Sprite>() { LoadSprite("Dpad_X") };
        m_spriteDownUp      = new List<Sprite>() { LoadSprite("Dpad_Y") };
        m_spriteAccept      = new List<Sprite>() { LoadSprite("A") };
        m_spriteCancel      = new List<Sprite>() { LoadSprite("B") };
        m_spriteMenu        = new List<Sprite>() { LoadSprite("Start") };
    }

    private string GetAxis(string axis)
    {
        return PREFIX + m_joystickNumber + axis;
    }

    private KeyCode GetButton(int number)
    {
        return (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + m_joystickNumber + "Button" + Mathf.Clamp(number, 0, 19));
    }
    
    private Sprite LoadSprite(string name)
    {
        return Resources.Load<Sprite>("Controller_" + name);
    }
}
