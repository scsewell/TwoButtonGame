﻿using System;
using System.Collections.Generic;

using Framework.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A class that manages a list of items in the UI.
    /// </summary>
    /// <typeparam name="T">The type of value the items in the list correspond to.</typeparam>
    public class ItemList<T>
    {
        private readonly NavigationBuilder m_parent;
        private readonly Button m_itemPrefab;
        private readonly Func<T, string> m_getText;
        private readonly List<Button> m_items = new List<Button>();
        private readonly Dictionary<T, Button> m_valueToItem = new Dictionary<T, Button>();

        /// <summary>
        /// An event invoked just prior to rebuilding the layout for the items.
        /// </summary>
        public event Action PreLayoutUpdate;

        /// <summary>
        /// An event invoked when submit is pressed on a selected item, providing the value for the item.
        /// </summary>
        public event Action<T> Submit;


        /// <summary>
        /// Creates a new <see cref="ItemList{T}"/> instance.
        /// </summary>
        /// <param name="parent">The layout to parent the list items under.</param>
        /// <param name="itemPrefab">The prefab to instantiate for each item.</param>
        public ItemList(NavigationBuilder parent, Button itemPrefab) : this(parent, itemPrefab, (v) => v.ToString())
        {
        }

        /// <summary>
        /// Creates a new <see cref="ItemList{T}"/> instance.
        /// </summary>
        /// <param name="parent">The layout to parent the list items under.</param>
        /// <param name="itemPrefab">The prefab to instantiate for each item.</param>
        /// <param name="getText">The function that gets the display text for a value.</param>
        public ItemList(NavigationBuilder parent, Button itemPrefab, Func<T, string> getText)
        {
            m_parent = parent;
            m_itemPrefab = itemPrefab;
            m_getText = getText;
        }

        /// <summary>
        /// Gets the item panel for a value.
        /// </summary>
        /// <param name="value">The value to get the panel for.</param>
        /// <param name="item">The last item panel in the list that corresponds to the given value,
        /// or null if none was found.</param>
        /// <returns>True if an item was returned; false otherwise.</returns>
        public bool TryGetItem(T value, out Button item)
        {
            return m_valueToItem.TryGetValue(value, out item);
        }

        /// <summary>
        /// Creates and updates the item panels to match the provided list of values.
        /// </summary>
        /// <param name="values">The values to show in the list.</param>
        public void Refresh(IReadOnlyList<T> values)
        {
            for (var i = m_items.Count; i < values.Count; i++)
            {
                var button = GameObject.Instantiate(m_itemPrefab, m_parent.transform, false);
                button.gameObject.AddComponent<AutoScrollViewElement>();
                m_items.Add(button);
            }
            for (var i = 0; i < m_items.Count; i++)
            {
                var item = m_items[i];

                item.gameObject.SetActive(i < values.Count);

                if (item.gameObject.activeSelf)
                {
                    var value = values[i];
                    m_valueToItem[value] = item;

                    item.onClick.RemoveAllListeners();
                    item.onClick.AddListener(() => Submit?.Invoke(value));
                    item.GetComponentInChildren<TMP_Text>().text = m_getText(value);
                }
            }

            PreLayoutUpdate?.Invoke();
            m_parent.UpdateNavigation();
        }
    }
}
