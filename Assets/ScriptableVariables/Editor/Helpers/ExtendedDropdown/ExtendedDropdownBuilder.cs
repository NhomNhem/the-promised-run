#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace OpenUtility.Editor
{
    /// <summary>
    /// Can setup an extended dropdown display using the builder pattern. 
    /// </summary>
    public class ExtendedDropdownBuilder
    {
        /// <summary>
        /// The extended dropdown instance used for showing the dropdown.
        /// </summary>
        private readonly ExtendedDropdown _dropdown;

        /// <summary>
        /// The indent text path used for adding depth to the 
        /// menu items.
        /// </summary>
        private string _indentTextPath = string.Empty;
        
        /// <summary>
        /// Creates initial the dropdown state.
        /// </summary>
        /// <param name="name">The name of the dropdown.</param>
        /// <param name="position">The position of the dropdown.</param>
        public ExtendedDropdownBuilder(string name, Rect position)
            => _dropdown = new ExtendedDropdown(name, position, new AdvancedDropdownState());
        
        /// <summary>
        /// Creates initial the dropdown state.
        /// </summary>
        /// <param name="name">The name of the dropdown.</param>
        /// <param name="position">The position of the dropdown.</param>
        /// <param name="state">The dropdown state (This can be serialized).</param>
        public ExtendedDropdownBuilder(string name, Rect position, AdvancedDropdownState state)
            => _dropdown = new ExtendedDropdown(name, position, state);
        
        /// <summary>
        /// Starts a new indent using given path. If there alread is an indent, this one will be 
        /// added to the current.
        /// <para>The path should be in the format 'Parent/Child/' and should end with a '/'.</para>
        /// </summary>
        /// <param name="path">The path for the indent.</param>
        /// <returns>The builder.</returns>
        public ExtendedDropdownBuilder StartIndent(string path)
        {
            _indentTextPath += path;
            return this;
        }

        /// <summary>
        /// Ends the current indent.
        /// </summary>
        /// <returns>The builder.</returns>
        public ExtendedDropdownBuilder EndIndent()
        {
            _indentTextPath = string.Empty;
            return this;
        }

        /// <summary>
        /// Adds a new item to the dropdown.
        /// </summary>
        /// <param name="name">The name of the item. This can be written as a path (e.g. Fruit/Apple).</param>
        /// <param name="clicked">The method to call when this item is clicked.</param>
        /// <returns>The builder.</returns>
        public ExtendedDropdownBuilder AddItem(string name, Action<object> clicked = null) 
            => AddItem(name, false, null, null, clicked);

        /// <summary>
        /// Adds a new item to the dropdown.
        /// </summary>
        /// <param name="name">The name of the item. This can be written as a path (e.g. Fruit/Apple).</param>
        /// <param name="disabled">Whether this item is disabled or not.</param>
        /// <param name="clicked">The method to call when this item is clicked.</param>
        /// <returns>The builder.</returns>
        public ExtendedDropdownBuilder AddItem(string name, bool disabled, Action<object> clicked = null)
            => AddItem(name, disabled, null, null, clicked);
        
        /// <summary>
        /// Adds a new item to the dropdown.
        /// </summary>
        /// <param name="name">The name of the item. This can be written as a path (e.g. Fruit/Apple).</param>
        /// <param name="disabled">Whether this item is disabled or not.</param>
        /// <param name="data">Optional data to associate with this item.</param>
        /// <param name="clicked">The method to call when this item is clicked.</param>
        /// <returns>The builder.</returns>
        public ExtendedDropdownBuilder AddItem(string name, bool disabled, object data, Action<object> clicked = null)
            => AddItem(name, disabled, null, data, clicked);
        
        
        /// <summary>
        /// Adds a new item to the dropdown.
        /// </summary>
        /// <param name="name">The name of the item. This can be written as a path (e.g. Fruit/Apple).</param>
        /// <param name="disabled">Whether this item is disabled or not.</param>
        /// <param name="icon">An optional icon to use for the display.</param>
        /// <param name="data">Optional data to associate with this item.</param>
        /// <param name="clicked">The method to call when this item is clicked.</param>
        /// <returns>The builder.</returns>
        public ExtendedDropdownBuilder AddItem(string name, bool disabled, Texture2D icon, object data, Action<object> clicked = null)
        {
            string path = string.IsNullOrEmpty(_indentTextPath) ? name : (_indentTextPath + "/" + name);
            _dropdown.AddItem(new ExtendedDropdownItem
            {
                name = path,
                disabled = disabled,
                icon = icon,
                clicked = clicked,
                data = data
            });
            return this;
        }

        /// <summary>
        /// Adds given item to the dropdown.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The builder.</returns>
        public ExtendedDropdownBuilder AddItem(ExtendedDropdownItem item)
        {
            _dropdown.AddItem(item);
            return this;
        }

        /// <summary>
        /// Adds given items to the dropdown.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The builder.</returns>
        public ExtendedDropdownBuilder AddItems(IEnumerable<ExtendedDropdownItem> items)
        {
            foreach (ExtendedDropdownItem item in items)
                _dropdown.AddItem(item);

            return this;
        }

        /// <summary>
        /// Adds a minimum size to the dropdown.
        /// </summary>
        /// <param name="minSize">The minimum size to use.</param>
        /// <returns>The builder.</returns>
        public ExtendedDropdownBuilder AddMinimumSize(Vector2 minSize)
        {
            _dropdown.AddMinimumSize(minSize);
            return this;
        }

        /// <summary>
        /// Returns the result of the dropdown build.
        /// </summary>
        /// <returns>The extended dropdown instance.</returns>
        public ExtendedDropdown GetResult() => _dropdown;

        /// <summary>
        /// Converts the dropdown builder to the dropdown instance using the <see cref="GetResult"/> method.
        /// </summary>
        /// <param name="builder">The builder to convert.</param>
        public static implicit operator ExtendedDropdown(ExtendedDropdownBuilder builder) => builder.GetResult();
    }
}

#endif
