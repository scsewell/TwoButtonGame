using UnityEngine;

namespace BoostBlasters.Profiles
{
    /// <summary>
    /// A class containing utility methods used by profiles.
    /// </summary>
    public static class ProfileUtils
    {
        /// <summary>
        /// Gets a random profile color.
        /// </summary>
        /// <returns>The generated profile color.</returns>
        public static Color GetRandomColor()
        {
            // pick any hue
            var h = Mathf.Lerp(0f, 1f, Random.value);

            // weight towards higher saturation
            var satVal = Random.value;
            var s = Mathf.Lerp(0f, 0.9f, 1f - (satVal * satVal * satVal));

            // prevent the color from being too dark
            var v = Mathf.Lerp(ProfileConsts.MIN_COLOR_VALUE, 1f, Random.value);

            return Color.HSVToRGB(h, s, v);
        }
    }
}
