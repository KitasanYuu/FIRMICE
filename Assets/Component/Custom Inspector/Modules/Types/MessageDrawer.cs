using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// Only valid for MessageDrawer! Used to fix overriding of other attributes
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class MessageDrawerAttribute : PropertyAttribute { }

    /// <summary>
    /// Can be accessed by script to draw a message in the inspector runtime.
    /// <para>Use MessageBoxAttribute for non-runtime messages</para>
    /// </summary>
    [System.Serializable]
    public class MessageDrawer
    {

#if UNITY_EDITOR
        [MessageBox("You are overriding the default PropertyDrawer of MessageDrawer", MessageBoxType.Error)]
        [SerializeField, HideField] string info;

        List<(string, MessageBoxType)> messages = new();
#endif

        /// <summary>
        /// Disposes all current messages
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public void HideMessages(bool value)
        {
#if UNITY_EDITOR
            messages = null;
#endif
        }

        /// <summary>
        /// Draws a message in the inspector
        /// </summary>
        /// <param name="message">The content shown in the inspector</param>
        /// <param name="type">Determines the icon in front of the message</param>
        /// <param name="replaceCurrents">If all currently shown messages on this object should be deleted</param>
        [Conditional("UNITY_EDITOR")]
        public void DrawMessage(string message, MessageBoxType type = MessageBoxType.None, bool replaceCurrents = false)
        {
#if UNITY_EDITOR
            if (replaceCurrents)
                messages.Clear();
            
            messages.Add((message, type));
#endif
        }
        /// <summary>
        /// Draws a message in the inspector with an info icon
        /// </summary>
        /// <param name="message">The content shown in the inspector</param>
        /// <param name="replaceCurrents">If all currently shown messages on this object should be deleted</param>
        [Conditional("UNITY_EDITOR")]
        public void DrawInfo(string message, bool replaceCurrents = false)
            => DrawMessage(message, MessageBoxType.Info, replaceCurrents);
        /// <summary>
        /// Draws a message in the inspector with an warning icon
        /// </summary>
        /// <param name="message">The content shown in the inspector</param>
        /// <param name="replaceCurrents">If all currently shown messages on this object should be deleted</param>
        [Conditional("UNITY_EDITOR")]
        public void DrawWarning(string message, bool replaceCurrents = false)
            => DrawMessage(message, MessageBoxType.Warning, replaceCurrents);
        /// <summary>
        /// Draws a message in the inspector with an error icon
        /// </summary>
        /// <param name="message">The content shown in the inspector</param>
        /// <param name="replaceCurrents">If all currently shown messages on this object should be deleted</param>
        [Conditional("UNITY_EDITOR")]
        public void DrawError(string message, bool replaceCurrents = false)
            => DrawMessage(message, MessageBoxType.Error, replaceCurrents);
    }
}