using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CustomInspector.Extensions;
using System.Linq;
using System;
using CustomInspector.Helpers;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(HorizontalGroupAttribute))]
    public class HorizontalGroupAttributeDrawer : PropertyDrawer
    {
        /// <summary>
        /// Range: [0, 1]
        /// 1 means label takes whole width and field is zero width
        /// </summary>
        const float labelFieldProportion = 0.4f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                PropertyIdentifier id = new(property);
                if (!savedInfos.TryGetValue(id, out InspectorInfo info))
                {
                    info = new InspectorInfo(position, property, (HorizontalGroupAttribute)attribute);
                    savedInfos.Add(id, info);
                }

                position = EditorGUI.IndentedRect(position);
                Rect rect = new()
                {
                    x = info.startX(position),
                    y = position.y,
                    width = info.width(position.width),
                    height = EditorGUI.GetPropertyHeight(property, label),
                };

                using (new NewIndentLevel(0))
                {
                    if (info.errorMessage == null)
                    {
                        using (new LabelWidthScope(rect.width * labelFieldProportion))
                        {
                            DrawProperties.PropertyField(rect, label, property);
                        }
                    }
                    else
                    {
                        DrawProperties.DrawPropertyWithMessage(rect, label, property, info.errorMessage, info.errorType);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
            {
                PropertyIdentifier id = new(property);
                GroupMember[] group = GetGroup(property);


                // Get height
                if (group[^1].id == id) //is last
                {
                    //My infos
                    storedHeights.Push((id, GetHeight()));

                    //Try-catch because pushed item MUST be popped
                    try
                    {
                        //let themselves enter in heights dict if they are bigger
                        foreach (var groupMember in group.Take(group.Length - 1)) //skip last, cuz its self
                        {
                            SerializedProperty prop = property.serializedObject.FindProperty(groupMember.id.propertyPath);
                            DrawProperties.GetPropertyHeight(prop);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    //dispose variable
                    (_, float maxHeight) = storedHeights.Pop();
                    //return biggest
                    return maxHeight;
                }
                else
                {
                    //wenn der letzte gerade sucht, dann meine dazu
                    if (storedHeights.Any()
                        && storedHeights.Peek().last.propertyPath == group[^1].id.propertyPath)
                    {
                        (PropertyIdentifier last, float maxHeight) current = storedHeights.Pop();
                        storedHeights.Push((current.last, Mathf.Max(GetHeight(), current.maxHeight)));
                    }

                    return -EditorGUIUtility.standardVerticalSpacing;
                }

                float GetHeight()
                {
                    bool hasMessage;
                    if (savedInfos.TryGetValue(id, out InspectorInfo info))
                        hasMessage = info.errorMessage != null;
                    else
                        hasMessage = property.IsArrayElement() || group.Length == 1;

                    if (hasMessage)
                        return DrawProperties.GetPropertyWithMessageHeight(label, property);
                    else
                        return DrawProperties.GetPropertyHeight(label, property);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return 0;
            }
        }
        /// <summary> This is a Stack, because there can be horizontal groups inside other horizontal groups and the last is always the current interest </summary>
        readonly static Stack<(PropertyIdentifier last, float maxHeight)> storedHeights = new();

        // For performance, the property can access its informations out of Dictionary instead of recalculate each time
        readonly static Dictionary<PropertyIdentifier, InspectorInfo> savedInfos = new();
        readonly static Dictionary<PropertyIdentifier, GroupMember[]> propertyGroups = new();

        class InspectorInfo
        {
            //If error
            public string errorMessage = null;
            public MessageType errorType;

            //position
            /// <summary> Returns the position_x, if you input the original position </summary>
            public Func<Rect, float> startX;
            /// <summary> Returns the width, if you input the position.width </summary>
            public Func<float, float> width;

            public InspectorInfo(Rect position, SerializedProperty property, HorizontalGroupAttribute attribute)
            {
                //Check if not list element
                if (property.IsArrayElement()) //is element in a list
                {
                    errorMessage = "HorizontalGroup not valid on lists";
                    errorType = MessageType.Error;
                    startX = (r) => r.x;
                    width = (w) => w;

                    return;
                }

                //Get values
                //List<(SerializedProperty prop, float size)> props = GetAllPropsInGroup(property);
                GroupMember[] group = GetGroup(property);

                //Check again
                if (group.Length == 1) //alone in group
                {
                    if (attribute.beginNewGroup)
                        errorMessage = $"{property.name} is alone in a horizontal group. Maybe set \"beginNewGroup\"=false to let him join the previous group";
                    else
                        errorMessage = $"unnecessary assignment of HorizontalGroupAttribute ({property.name} is alone in a horizontal group). All members of the group must have the attribute and stand behind each other in the code";
                    errorType = MessageType.Warning;
                    startX = (r) => r.x;
                    width = (w) => w;
                    return;
                }

                //Draw
                float propertysSpacing = position.width / 25f; // Distance between two properties in same horizontal group

                float customTotalWidth = group.Select(_ => _.size).Sum();
                Func<float, float> realWidth = (positionWidth) => (positionWidth - (group.Length - 1) * propertysSpacing);
                width = (w) => (realWidth(w) / customTotalWidth * attribute.size); //withPer = (width - spacing) / amount * myAmount
                int myIndex = group.ToList().FindIndex(_ => _.id.propertyPath == property.propertyPath);

                float previousSizes = group.Take(myIndex).Sum(_ => _.size);
                startX = (r) => (r.x + realWidth(r.width) / customTotalWidth * previousSizes + myIndex * propertysSpacing);
            }
        }
        public class GroupMember
        {
            public readonly float size;
            //public SerializedProperty property => null;
            public readonly PropertyIdentifier id;

            private GroupMember(PropertyIdentifier id, float size)
            {
                this.id = id;
                this.size = size;
            }
            public static GroupMember[] GetGroup(SerializedProperty property)
            {
                if (property == null)
                    throw new NullReferenceException("property is null");

                if (property.IsArrayElement()) //arr element is not allowed
                    return new[] { new GroupMember(new PropertyIdentifier(property), 1) };

                var owner = property.GetOwnerAsFinder();

                List<(SerializedProperty prop, float size)> props = new();

                bool foundMyGroup = false; //we are looking for the group the property is in

                foreach (SerializedProperty prop in owner.GetAllPropertys(false))
                {
                    //Check is in group
                    HorizontalGroupAttribute hg = new DirtyValue(prop).GetAttribute<HorizontalGroupAttribute>();
                    if (hg is null)
                    {
                        if (foundMyGroup)
                            break;
                        else
                        {
                            props.Clear();
                            continue;
                        }
                    }
                    if (hg.beginNewGroup == true)
                    {
                        if (foundMyGroup)
                            break;
                        else
                            props.Clear();
                    }
                    //Add
                    props.Add((prop, hg.size));
                    if (prop.name == property.name)
                        foundMyGroup = true;
                }
                //Test
                Debug.Assert(props.Count > 0, "Property group is empty");
                //Return
                return props.Select(_ => new GroupMember(new PropertyIdentifier(_.prop), _.size)).ToArray();
            }
        }
        public static GroupMember[] GetGroup(SerializedProperty property)
        {
            PropertyIdentifier id = new(property);
            if (!propertyGroups.TryGetValue(id, out GroupMember[] group))
            {
                group = GroupMember.GetGroup(property);
                propertyGroups.Add(id, group);
            }
            return group;
        }
    }
}