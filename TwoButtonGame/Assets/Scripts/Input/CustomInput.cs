using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomInput : BaseInput
{
    private List<PlayerBaseInput> m_inputs;

    protected override void Awake()
    {
        base.Awake();

        m_inputs = new List<PlayerBaseInput>();
    }

    public void SetInputs(List<PlayerBaseInput> inputs)
    {
        m_inputs = inputs;
    }

    public override float GetAxisRaw(string axisName)
    {
        if (m_inputs != null && m_inputs.Count > 0)
        {
            if (axisName == CustomInputModule.H_AXIS)
            {
                return m_inputs.Select(i => i.UI_H).OrderByDescending(v => Mathf.Abs(v)).FirstOrDefault();
            }
            if (axisName == CustomInputModule.V_AXIS)
            {
                return m_inputs.Select(i => i.UI_V).OrderByDescending(v => Mathf.Abs(v)).FirstOrDefault();
            }
        }
        return 0;
    }

    public override bool GetButtonDown(string buttonName)
    {
        if (m_inputs != null && m_inputs.Count > 0)
        {
            if (buttonName == CustomInputModule.ACCEPT_BUTTON)
            {
                return m_inputs.Any(i => i.UI_Accept) || Input.GetKeyDown(KeyCode.Return);
            }
            if (buttonName == CustomInputModule.CANCEL_BUTTON)
            {
                return m_inputs.Any(i => i.UI_Cancel) || Input.GetKeyDown(KeyCode.Escape);
            }
        }
        return false;
    }
}
