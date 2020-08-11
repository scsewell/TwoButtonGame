using UnityEngine;

namespace BoostBlasters.Levels.Elements
{
    public interface IOnWillRenderReceiver
    {
        void OnWillRender(Camera cam);
    }
}
