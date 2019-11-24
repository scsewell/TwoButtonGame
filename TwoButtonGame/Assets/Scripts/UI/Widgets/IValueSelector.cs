using System;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A widget which allows selecting an value from a set of options.
    /// </summary>
    public interface IValueSelector
    {
        /// <summary>
        /// The label text.
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// All the displayable values.
        /// </summary>
        string[] Options { get; set; }

        /// <summary>
        /// The currently selected value.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// An event triggered when the user changes the value.
        /// </summary>
        event Action<string> ValueChanged;
    }
}
