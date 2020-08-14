using UnityEngine.InputSystem;

namespace BoostBlasters.Input
{
    /// <summary>
    /// The input for actions which can be triggered by any player.
    /// </summary>
    public class GlobalInput : BaseInput
    {
        protected override void OnEnable()
        {
            m_primaryInput.actionsAsset = CloneActions(m_primaryInput.actionsAsset);
            m_secondaryInput.actionsAsset = CloneActions(m_secondaryInput.actionsAsset);

            base.OnEnable();
        }

        private InputActionAsset CloneActions(InputActionAsset actions)
        {
            var newActions = Instantiate(actions);

            for (var actionMap = 0; actionMap < actions.actionMaps.Count; actionMap++)
            {
                for (var binding = 0; binding < actions.actionMaps[actionMap].bindings.Count; binding++)
                {
                    var inputBinding = actions.actionMaps[actionMap].bindings[binding];
                    newActions.actionMaps[actionMap].ApplyBindingOverride(binding, inputBinding);
                }
            }

            return newActions;
        }
    }
}
