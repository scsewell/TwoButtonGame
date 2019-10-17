using UnityEngine;

namespace BoostBlasters
{
    public interface IOnWillRenderReceiver
    {
        void OnWillRender(Camera cam);
    }
}
