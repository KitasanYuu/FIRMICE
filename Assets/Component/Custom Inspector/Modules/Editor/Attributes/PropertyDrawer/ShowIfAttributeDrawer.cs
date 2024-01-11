using CustomInspector.Extensions;
using CustomInspector.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalDrawer.OnGUI(position, property, label, (ShowIfAttribute)attribute);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ConditionalDrawer.GetPropertyHeight(property, label, attribute);
        }
    }

    [CustomPropertyDrawer(typeof(ShowIfNotAttribute))]
    public class ShowIfNotAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalDrawer.OnGUI(position, property, label, (ShowIfNotAttribute)attribute);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ConditionalDrawer.GetPropertyHeight(property, label, attribute);
        }
    }



    internal static class ConditionalDrawer
    {

        public static void OnGUI(Rect position, SerializedProperty property, GUIContent label, ShowIfAttribute attribute)
        {
            var a = (ShowIfAttribute)attribute;
            var info = GetInfo(property, a);

            if(info.errorMessage != null)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, info.errorMessage, MessageType.Error);
                return;
            }

            //Display
            using (new EditorGUI.IndentLevelScope(attribute.indent))
            {
                switch (a.style)
                {
                    case DisabledStyle.Invisible:
                        if (info.condition(property))
                        {
                            DrawProperties.PropertyField(position, label, property, true);
                        }
                        break;
                    case DisabledStyle.GreyedOut:
                        using (new EditorGUI.DisabledScope(!info.condition(property)))
                        {
                            DrawProperties.PropertyField(position, label, property, true);
                        }
                        break;
                    default:
                        throw new System.NotImplementedException($"{a.style}");
                }
            }
        }

        public static float GetPropertyHeight(SerializedProperty property, GUIContent label, PropertyAttribute attribute)
        {
            var a = (ShowIfAttribute)attribute;
            var info = GetInfo(property, a);

            if(info.errorMessage != null)
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }
            //If visible
            if(info.condition(property))
            {
                return DrawProperties.GetPropertyHeight(label, property);
            }
            //if not visible
            else
            {
                switch (a.style)
                {
                    case DisabledStyle.Invisible:
                        return -EditorGUIUtility.standardVerticalSpacing;

                    case DisabledStyle.GreyedOut:
                        return DrawProperties.GetPropertyHeight(label, property);

                    default:
                        throw new System.NotImplementedException($"{a.style}");
                };
            }
        }

        readonly static Dictionary<PropertyIdentifier, PropInfo> savedInfos = new();
        static PropInfo GetInfo(SerializedProperty property, ShowIfAttribute attribute)
        {
            PropertyIdentifier id = new(property);
            if (!savedInfos.TryGetValue(id, out PropInfo info))
            {
                info = new(property, attribute);
                savedInfos.Add(id, info);
            }
            return info;
        }
        class PropInfo
        {
            public readonly string errorMessage;
            public readonly Func<SerializedProperty, bool> condition;

            public PropInfo(SerializedProperty property, ShowIfAttribute attribute)
            {
                //Check if not list element
                if (property.IsArrayElement()) //is element in a list
                {
                    errorMessage = "conditional-show not valid on list elements." +
                    "\nHint: Put your list in a (holder-)class, give it this attribute and UnwrapAttribute to hide and unhide the whole list";
                    condition = null;
                    return;
                }

                try
                {
                    condition = GetCondition(property, attribute);
                    errorMessage = null;
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                    condition = null;
                    return;
                }

                static Func<SerializedProperty, bool> GetCondition(SerializedProperty property, ShowIfAttribute conditionalAttribute)
                {
                    string[] allConditionPaths = conditionalAttribute.conditionPaths.Where(_ => !string.IsNullOrEmpty(_)).ToArray();

                    Func<SerializedProperty, List<object>> valuesGetter = GetAllValues(property, allConditionPaths);

                    if (conditionalAttribute.op.HasValue)
                    {
                        if (valuesGetter(property).TryGetFirst(predicate: _ => _ is not bool, match: out var p))
                            throw new ArgumentException($"BoolOperator only works on booleans and not on {p?.GetType()}");

                        return conditionalAttribute.op switch
                        {
                            BoolOperator.And => (p) => (valuesGetter(p).All(_ => (bool)_) ^ conditionalAttribute.Invert),
                            BoolOperator.Or => (p) => (valuesGetter(p).Any(_ => (bool)_) ^ conditionalAttribute.Invert),
                            _ => throw new System.NotImplementedException($"{conditionalAttribute.op}"),
                        };
                    }
                    else if (conditionalAttribute.comOp.HasValue)
                    {
                        return conditionalAttribute.comOp switch
                        {
                            ComparisonOp.Equals => (p) => {
                                var values = valuesGetter(p);
                                object firstValue = values[0];
                                if (firstValue.IsUnityNull())
                                    return values.Skip(1).All(_ => _.IsUnityNull()) ^ conditionalAttribute.Invert;
                                else
                                    return values.Skip(1).All(_ => firstValue.Equals(_)) ^ conditionalAttribute.Invert;
                            }
                            ,
                            ComparisonOp.NotNull => (p) => valuesGetter(p).All(_ => !_.IsUnityNull()) ^ conditionalAttribute.Invert,
                            ComparisonOp.Null => (p) => valuesGetter(p).All(_ => _.IsUnityNull()) ^ conditionalAttribute.Invert,
                            _ => throw new System.NotImplementedException($"{conditionalAttribute.comOp}"),
                        };
                    }
                    else //only a single
                    {
                        if (allConditionPaths.Length > 1)
                            throw new ArgumentException("No operator or comparison provided to evalute multiple paths/names");
                        if (allConditionPaths.Length < 1)
                            throw new ArgumentException($"No condition on {property.name} provided to evaluate");

                        object firstInstantiation = valuesGetter(property)[0];
                        if (firstInstantiation is not bool)
                        {
                            throw new Exceptions.WrongTypeException($"{allConditionPaths[0]} is not type of bool." +
                                        $"\nUse ComparisonOp to define, how to translate value ({firstInstantiation.GetType()}) into a boolean");
                        }

                        return (p) => (bool)valuesGetter(p)[0] ^ conditionalAttribute.Invert;
                    }

                    static Func<SerializedProperty, List<object>> GetAllValues(SerializedProperty property, string[] allConditionPaths)
                    {
                        if (allConditionPaths.Length < 1)
                            throw new ArgumentException($"No conditions on {property.name} provided");

                        //We have to split in 3 parts: serialized_properties_path, on_targetObject_paths, static_conditions_paths | bec they are evaluated differently
                        List<string> static_conditions_paths = new();
                        List<string> serialized_properties_path = new();
                        List<string> on_targetObject_paths = new();

                        var ownerInstantiation = property.GetOwnerAsFinder();
                        foreach (var path in allConditionPaths)
                        {
                            if (Enum.GetNames(typeof(StaticConditions)).Contains(path))
                                static_conditions_paths.Add(path);
                            else if (ownerInstantiation.FindPropertyRelative(path) != null)
                                serialized_properties_path.Add(path);
                            else
                                on_targetObject_paths.Add(path);
                        }

                        //Get the valuesGetter | already affected by
                        Func<SerializedProperty, IEnumerable<object>> static_values = null;
                        Func<SerializedProperty, IEnumerable<object>> serialized_values = null;
                        Func<SerializedProperty, IEnumerable<object>> targetObject_values = null;

                        //Static 
                        if (static_conditions_paths.Any())
                        {
                            List<StaticConditions> parsed = static_conditions_paths.Select(_ => (StaticConditions)Enum.Parse(typeof(StaticConditions), _)).ToList();

                            //Test
                            if (parsed.Any(_ => _ == StaticConditions.IsEnabled)
                                && property.serializedObject.targetObject is not MonoBehaviour)
                            {
                                throw new ArgumentException("StaticConditions.IsEnabled is only valid on MonoBehaviour");
                            }
                            if (parsed.Any(_ => _ == StaticConditions.IsActiveAndEnabled)
                                && property.serializedObject.targetObject is not MonoBehaviour)
                            {
                                throw new ArgumentException("StaticConditions.IsActiveAndEnabled is only valid on MonoBehaviour");
                            }

                            static_values = (p) => parsed.Select<StaticConditions, object>(_ => GetValue(p, _));

                            static bool GetValue(SerializedProperty property, StaticConditions cond)
                            {
                                return cond switch
                                {
                                    StaticConditions.True => true,
                                    StaticConditions.False => false,
                                    StaticConditions.IsPlaying => Application.isPlaying,
                                    StaticConditions.IsNotPlaying => !Application.isPlaying,
                                    StaticConditions.IsEnabled => ((MonoBehaviour)property.serializedObject.targetObject).enabled,
                                    StaticConditions.IsActiveAndEnabled => ((MonoBehaviour)property.serializedObject.targetObject).isActiveAndEnabled,
                                    _ => throw new NotImplementedException(cond.ToString()),
                                };
                            }
                        }

                        //serialized
                        if (serialized_properties_path.Any())
                        {
                            serialized_values = (p) =>
                            {
                                var owner = p.GetOwnerAsFinder();
                                List<SerializedProperty> props = serialized_properties_path.Select(_ => owner.FindPropertyRelative(_)).ToList();
                                return props.Select(_ => _.GetValue());
                            };
                        }

                        //on targetobject
                        if (on_targetObject_paths.Any())
                        {
                            //declare paths
                            (string[] propPaths, string[] methodPaths) = Split(property, on_targetObject_paths);

                            //Fill paths
                            DirtyValue owner = DirtyValue.GetOwner(property);

                            //result
                            if (propPaths == null) //only methods   (there must be props, because on_targetObject_paths.Any was true)
                                targetObject_values = (p) =>
                                {
                                    p.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                    DirtyValue owner = DirtyValue.GetOwner(p);
                                    return methodPaths.Select(_ => InvokableMethod.GetMethod(owner, _).Invoke());
                                };
                            else if (methodPaths == null) //only props
                                targetObject_values = (p) =>
                                {
                                    p.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                    DirtyValue owner = DirtyValue.GetOwner(p);
                                    return propPaths.Select(_ => owner.FindRelative(_).GetValue());
                                };
                            else //both
                                targetObject_values = (p) =>
                                {
                                    p.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                    DirtyValue owner = DirtyValue.GetOwner(p);
                                    return propPaths.Select(_ => owner.FindRelative(_).GetValue())
                                        .Concat(methodPaths.Select(_ => InvokableMethod.GetMethod(owner, _).Invoke()));
                                };

                            static (string[] propPaths, string[] methodPaths) Split(SerializedProperty property, IEnumerable<string> paths)
                            {
                                List<string> propPaths = new();
                                List<string> methodPaths = new();

                                foreach (string path in paths)
                                {
                                    //Add as prop
                                    try
                                    {
                                        DirtyValue.GetOwner(property).FindRelative(path).GetValue();
                                        propPaths.Add(path);
                                        continue;
                                    }
                                    catch (MissingFieldException)
                                    { }

                                    //Add as method
                                    InvokableMethod method;
                                    try
                                    {
                                        method = PropertyValues.GetMethodOnOwner(property, path);
                                    }
                                    catch (MissingMethodException)
                                    {
                                        string pre = property.propertyPath.PrePath(false);
                                        if (!string.IsNullOrEmpty(pre))
                                            pre = "." + pre;
                                        pre = property.serializedObject.targetObject.GetType().Name + pre;
                                        throw new MissingMemberException($"No method or field '{path}' on '{pre}' found");
                                    }

                                    methodPaths.Add(path);
                                }

                                return (propPaths.ToArray(), methodPaths.ToArray());
                            }
                        }

                        Func<SerializedProperty, List<object>> allValues = (p) =>
                        {
                            List<object> res = new();
                            if (static_values != null)
                                res.AddRange(static_values(p));
                            if (serialized_values != null)
                                res.AddRange(serialized_values(p));
                            if (targetObject_values != null)
                                res.AddRange(targetObject_values(p));
                            return res;
                        };

                        return allValues;

                    }
                }
            }
        }
    }
}