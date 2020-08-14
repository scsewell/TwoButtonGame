using System;
using System.IO;

using Framework.IO;

using UnityEngine;
using UnityEngine.InputSystem;

namespace BoostBlasters
{
    /// <summary>
    /// Manages screenshots captured from the application.
    /// </summary>
    public class Screenshot : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The action used to take a screenshot.")]
        private InputAction m_screenshotAction = null;

        private void Start()
        {
            var screenshotAction = m_screenshotAction;
            screenshotAction.performed += OnActionPerformed;
            screenshotAction.Enable();
        }

        private void OnActionPerformed(InputAction.CallbackContext ctx)
        {
            TakeScreenshot();
        }

        /// <summary>
        /// Captures a screenshot and saves it.
        /// </summary>
        public static void TakeScreenshot()
        {
            var dir = Path.Combine(FileIO.ConfigDirectory, "Screenshots");
            var path = Path.Combine(dir, $"screenshot_{DateTime.Now:MM-dd-yy_H-mm-ss}.png");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            ScreenCapture.CaptureScreenshot(path);

            Debug.Log($"Saved screenshot \"{path}\"");
        }
    }
}
