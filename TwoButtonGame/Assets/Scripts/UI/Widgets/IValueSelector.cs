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
        /// The currently selected optoin.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// The index of the selected option.
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Is the value able to be changed by the user.
        /// </summary>
        bool Modifiable { get; set; }

        /// <summary>
        /// An event triggered when the value is changed that provides the index of the selected option.
        /// </summary>
        event Action<int> ValueChanged;
    }
}
