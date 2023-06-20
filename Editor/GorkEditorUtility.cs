using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Graphs;

namespace Gork.Editor
{
    /// <summary>
    /// Static utility class for GorkGraph in the UnityEditor.
    /// </summary>
    public static class GorkEditorUtility
    {
        /// <summary>
        /// Creates a Tag Dropdown where every tag that is present in the given <paramref name="tagOptions"/> collection will be an option in the dropdown. <para/>
        /// Also features an additional customizable "No Tag" boolean that you can set to true for a no tag option in the dropdown.
        /// </summary>
        /// <param name="currentTag">The current tag that is selected.</param>
        /// <param name="tagOptions">An enumerable collection of all available tag options. Use <see cref="GorkGraph.Tags"/> if you don't want to do anything too fancy.</param>
        /// <param name="onSelectEntry">Callback for whenever an option in this list is selected.</param>
        /// <param name="haveNoTagsOption">If there should be an additional option for having no tag set at all. If this option is selected, then the given value on the <paramref name="onSelectEntry"/> event will be set as an empty string, not as a null value.</param>
        /// <param name="noTagsOptionText">What the No Tags option should display, if <paramref name="haveNoTagsOption"/> is true.</param>
        public static void TagDropdown(string currentTag, IEnumerable<string> tagOptions, Action<string> onSelectEntry, bool haveNoTagsOption, string noTagsOptionText = "<No Tag>")
        {
            // Do button
            if (!EditorGUILayout.DropdownButton(new GUIContent(string.IsNullOrEmpty(currentTag) && haveNoTagsOption ? noTagsOptionText : currentTag), FocusType.Keyboard))
            {
                // This is when the button is not pressed
                return;
            }

            // Button has been pressed

            // Create an empty generic menu which will be our dropdown menu
            GenericMenu menu = new GenericMenu();

            if (haveNoTagsOption)
            {
                // Add an option for no tags at all
                bool noTagsActive = string.IsNullOrEmpty(currentTag);

                menu.AddItem(new GUIContent(noTagsOptionText), noTagsActive, () =>
                {
                    // Do nothing if the tag is already set to this value
                    if (noTagsActive)
                    {
                        return;
                    }

                    onSelectEntry?.Invoke("");
                });
            }

            // Loop through all tags
            foreach (string tag in tagOptions)
            {
                bool on = currentTag == tag;

                // Add the menu item
                menu.AddItem(new GUIContent(tag), on, () =>
                {
                    // Do nothing if the tag is already set to this value
                    if (on)
                    {
                        return;
                    }

                    onSelectEntry?.Invoke(tag);
                });
            }

            // Display the menu as a dropdown menu at the correct position
            Rect rect = EditorGUILayout.GetControlRect();
            rect.y += 18;
            menu.DropDown(rect);
        }
        /// <summary>
        /// Creates an Event Dropdown where either every internal or external event that is present in the given <paramref name="eventOptions"/> collection will be an option in the dropdown. <para/>
        /// Set the <paramref name="eventType"/> to the event type that should be featured in the dropdown.
        /// </summary>
        /// <param name="currentEvent">The current event that is selected.</param>
        /// <param name="eventOptions">An enumerable collection of all available event options. Use <see cref="GorkGraph.Events"/> if you don't want to do anything too fancy.</param>
        /// <param name="onSelectEntry">Callback for whenever an option in this list is selected.</param>
        /// <param name="eventType">What type of event that should only be displayed in the dropdown.</param>
        public static void EventDropdown(string currentEvent, IEnumerable<GorkGraph.Event> eventOptions, Action<string> onSelectEntry, GorkGraph.Event.Type eventType)
        {
            // Do button
            if (!EditorGUILayout.DropdownButton(new GUIContent(currentEvent), FocusType.Keyboard))
            {
                // This is when the button is not pressed
                return;
            }

            // Button has been pressed

            // Create an empty generic menu which will be our dropdown menu
            GenericMenu menu = new GenericMenu();

            // Loop through all events
            foreach (GorkGraph.Event evt in eventOptions)
            {
                // Ignore events without the same type
                if (evt.EventType != eventType)
                {
                    continue;
                }

                string name = evt.Name;
                bool on = currentEvent == name;

                // Add the menu item
                menu.AddItem(new GUIContent(name), on, () =>
                {
                    // Do nothing if the event is already set to this value
                    if (on)
                    {
                        return;
                    }

                    onSelectEntry?.Invoke(name);
                });
            }

            // Display the menu as a dropdown menu at the correct position
            Rect rect = EditorGUILayout.GetControlRect();
            rect.y += 18;
            menu.DropDown(rect);
        }

        /// <summary>
        /// A button which will have a dropdown menu appear when pressed, with customizable options!
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="options"></param>
        /// <param name="onSelectEntry"></param>
        /// <param name="buttonContent"></param>
        public static void CustomDropdown(string currentValue, IEnumerable<string> options, Action<string> onSelectEntry, GUIContent buttonContent = null)
        {
            CustomDropdown(EditorGUILayout.GetControlRect(), currentValue, options, onSelectEntry, buttonContent);
        }

        public static void CustomDropdown(Rect rect, string currentValue, IEnumerable<string> options, Action<string> onSelectEntry, GUIContent buttonContent = null)
        {
            Rect dropdownRect = rect;
            dropdownRect.y += rect.height;
            CustomDropdown(rect, dropdownRect, currentValue, options, onSelectEntry, buttonContent);
        }

        public static void CustomDropdown(Rect rect, Rect dropdownRect, string currentValue, IEnumerable<string> options, Action<string> onSelectEntry, GUIContent buttonContent = null)
        {
            // Display the dropdown button
            if (!EditorGUI.DropdownButton(rect, buttonContent == null ? new GUIContent(currentValue) : buttonContent, FocusType.Keyboard))
            {
                // This is when the button is not pressed
                return;
            }

            // Button has been pressed

            // Create an empty generic menu which will be our dropdown menu
            GenericMenu menu = new GenericMenu();

            // Loop through options
            foreach (string option in options)
            {
                bool on = currentValue == option;

                // Add the menu item
                menu.AddItem(new GUIContent(option), on, () =>
                {
                    // Do nothing if the option is already selected
                    if (on)
                    {
                        return;
                    }

                    onSelectEntry?.Invoke(option);
                });
            }

            // Display the menu as a dropdown menu at the dropdown rect position
            menu.DropDown(dropdownRect);
        }
    }
}
