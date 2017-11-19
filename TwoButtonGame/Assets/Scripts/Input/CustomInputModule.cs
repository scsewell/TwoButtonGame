using UnityEngine;
using UnityEngine.EventSystems;

public class CustomInputModule : StandaloneInputModule
{
    public static readonly string H_AXIS = "H";
    public static readonly string V_AXIS = "V";
    public static readonly string ACCEPT_BUTTON = "Accept";
    public static readonly string CANCEL_BUTTON = "Cancel";

    public static readonly float REPEAT_WAIT = 0.25f;
    public static readonly float REPEAT_DELAY = 0.065f;

    protected override void Awake()
    {
        base.Awake();

        horizontalAxis  = H_AXIS;
        verticalAxis    = V_AXIS;
        submitButton    = ACCEPT_BUTTON;
        cancelButton    = CANCEL_BUTTON;
        repeatDelay     = REPEAT_WAIT;
        inputActionsPerSecond = (1.0f / REPEAT_DELAY);

        m_InputOverride = gameObject.AddComponent<CustomInput>();
    }
}
