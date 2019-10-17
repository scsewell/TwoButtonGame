using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseInput
{
    abstract public float H             { get; }
    abstract public float V             { get; }
    abstract public bool BoostPress     { get; }
    abstract public bool BoostRelease   { get; }

    protected List<Sprite> m_spriteLeft;
    protected List<Sprite> m_spriteRight;
    protected List<Sprite> m_spriteFore;
    protected List<Sprite> m_spriteBrake;
    protected List<Sprite> m_spriteBoost;
    
    public List<Sprite> SpriteLeft  { get { return m_spriteLeft; } }
    public List<Sprite> SpriteRight { get { return m_spriteRight; } }
    public List<Sprite> SpriteFore  { get { return m_spriteFore; } }
    public List<Sprite> SpriteBrake { get { return m_spriteBrake; } }
    public List<Sprite> SpriteBoost { get { return m_spriteBoost; } }


    abstract public float UI_H      { get; }
    abstract public float UI_V      { get; }
    abstract public bool UI_Accept  { get; }
    abstract public bool UI_Cancel  { get; }
    abstract public bool UI_Menu    { get; }

    protected List<Sprite> m_spriteNavigate;
    protected List<Sprite> m_spriteLeftRight;
    protected List<Sprite> m_spriteDownUp;
    protected List<Sprite> m_spriteAccept;
    protected List<Sprite> m_spriteCancel;
    protected List<Sprite> m_spriteMenu;

    public List<Sprite> SpriteNavigate  { get { return m_spriteNavigate; } }
    public List<Sprite> SpriteLeftRight { get { return m_spriteLeftRight; } }
    public List<Sprite> SpriteDownUp    { get { return m_spriteDownUp; } }
    public List<Sprite> SpriteAccept    { get { return m_spriteAccept; } }
    public List<Sprite> SpriteCancel    { get { return m_spriteCancel; } }
    public List<Sprite> SpriteMenu      { get { return m_spriteMenu; } }


    private UIAxisInput m_left;
    private UIAxisInput m_right;
    private UIAxisInput m_down;
    private UIAxisInput m_up;
    
    public bool UI_Left     { get { return m_left.Pressed; } }
    public bool UI_Right    { get { return m_right.Pressed; } }
    public bool UI_Down     { get { return m_down.Pressed; } }
    public bool UI_Up       { get { return m_up.Pressed; } }

    protected InputMuting m_muting;

    public abstract bool IsController { get; }

    protected PlayerBaseInput()
    {
        m_left  = new UIAxisInput(true);
        m_right = new UIAxisInput(false);
        m_down  = new UIAxisInput(true);
        m_up    = new UIAxisInput(false);

        m_muting = InputMuting.None;
    }

    public void Update()
    {
        m_left.Update(UI_H);
        m_right.Update(UI_H);
        m_down.Update(UI_V);
        m_up.Update(UI_V);
    }

    public void SetMuting(InputMuting muting)
    {
        m_muting = muting;
    }

    protected float KeyAsAxis(KeyCode key)
    {
        return GetKey(key) ? 1 : 0;
    }

    protected bool GetKey(KeyCode key)
    {
        return CanPress(key) && Input.GetKey(key);
    }

    protected bool GetKeyDown(KeyCode key)
    {
        return CanPress(key) && Input.GetKeyDown(key);
    }

    protected bool GetKeyUp(KeyCode key)
    {
        return CanPress(key) && Input.GetKeyUp(key);
    }

    private bool CanPress(KeyCode key)
    {
        switch (m_muting)
        {
            case InputMuting.All:           return false;
            //case InputMuting.TypingKeys:    return !(UIUtils.IsAlphaNumeric(key) || key == KeyCode.Backspace || key == KeyCode.Delete);
            case InputMuting.TypingKeys: return !(key == KeyCode.Backspace || key == KeyCode.Delete);
            default: return true;
        }
    }
}
