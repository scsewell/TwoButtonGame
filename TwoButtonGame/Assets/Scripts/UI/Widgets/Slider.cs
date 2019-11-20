using UnityEngine;

using Framework;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A UI widget which allows for choosing a numeric value on a range.
    /// </summary>
    public class Slider : Spinner
    {
        [SerializeField] private RectTransform m_fill = null;

        /// <summary>
        /// The range of values shown by this slider.
        /// </summary>
        public MinMaxRange Range { get; set; }

        protected override void OnValueChanged()
        {
            // fill the bar to match the value in the range
            Vector2 anchor = m_fill.anchorMax;
            anchor.x = Range.InverseLerp(float.Parse(Value));
            m_fill.anchorMax = anchor;
        }
    }
}
