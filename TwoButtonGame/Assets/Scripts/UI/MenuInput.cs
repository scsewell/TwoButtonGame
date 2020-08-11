using UnityEngine.InputSystem.UI;

namespace BoostBlasters.UI
{
    public class MenuInput
    {
        public Input Primary { get; }
        public Input Secondary { get; }

        public MenuInput(Input primary, Input secondary)
        {
            Primary = primary;
            Secondary = secondary;
        }
    }

    public class Input
    {
        private InputSystemUIInputModule m_inputModule;

        public MultiplayerEventSystem EventSystem { get; }

        public Input(InputSystemUIInputModule inputModule)
        {
            m_inputModule = inputModule;
            EventSystem = inputModule.GetComponent<MultiplayerEventSystem>();
        }
    }
}
