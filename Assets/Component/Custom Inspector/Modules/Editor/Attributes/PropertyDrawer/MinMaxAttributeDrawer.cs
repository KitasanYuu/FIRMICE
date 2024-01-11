using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    public abstract class MinMaxAttributeDrawer : PropertyDrawer
    {
        public abstract void Initialize(SerializedProperty property);
        public abstract bool DependsOnOtherProperty();
        public abstract int CapInt(int i);
        public abstract float CapFloat(float f);


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                Initialize(property);
            }
            catch (Exception e) //e.g. path doesnt exist or invalid cast if wrong path
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, this.GetType().Name + ": " + e.Message, MessageType.Error);
                return;
            }
            EditorGUI.BeginChangeCheck();
            DrawProperties.PropertyField(position, label, property);
            DoCapping();



            void DoCapping()
            {
                if (DependsOnOtherProperty() || EditorGUI.EndChangeCheck())
                {
                    switch (property.propertyType)
                    {
                        case SerializedPropertyType.Integer:
                            property.intValue = CapInt(property.intValue);
                            break;
                        case SerializedPropertyType.Float:
                            property.floatValue = CapFloat(property.floatValue);
                            break;

                        case SerializedPropertyType.Character:
                            property.intValue = CapInt(property.intValue);
                            break;

                        case SerializedPropertyType.Vector2Int:
                            Vector2Int v2i = property.vector2IntValue;
                            property.vector2IntValue = new Vector2Int(CapInt(v2i.x), CapInt(v2i.y));
                            break;
                        case SerializedPropertyType.Vector2:
                            Vector2 v2 = property.vector2Value;
                            property.vector2Value = new Vector2(CapFloat(v2.x), CapFloat(v2.y));
                            break;

                        case SerializedPropertyType.Vector3Int:
                            Vector3Int v3i = property.vector3IntValue;
                            property.vector3IntValue = new Vector3Int(CapInt(v3i.x), CapInt(v3i.y), CapInt(v3i.z));
                            break;
                        case SerializedPropertyType.Vector3:
                            Vector3 v3 = property.vector3Value;
                            property.vector3Value = new Vector3(CapFloat(v3.x), CapFloat(v3.y), CapFloat(v3.z));
                            break;

                        case SerializedPropertyType.Vector4:
                            Vector4 v4 = property.vector4Value;
                            property.vector4Value = new Vector4(CapFloat(v4.x), CapFloat(v4.y), CapFloat(v4.z), CapFloat(v4.w));
                            break;
                    }
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
            {
                Initialize(property);
            }
            catch //e.g. path doesnt exist or invalid cast if wrong path
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }
            return DrawProperties.GetPropertyHeight(label, property);
        }
    }

    [CustomPropertyDrawer(typeof(MaxAttribute))]
    public class MaxAttributeDrawer : MinMaxAttributeDrawer
    {
        MaxAttribute ma;
        float max;
        public override void Initialize(SerializedProperty property)
        {
            ma = (MaxAttribute)attribute;
            if (ma.maxPath == null)
                max = ma.max;
            else
                max = System.Convert.ToSingle(DirtyValue.GetOwner(property).FindRelative(ma.maxPath).GetValue());
        }
        public override int CapInt(int i) => Math.Min(i, (int)max);
        public override float CapFloat(float f) => Mathf.Min(f, max);

        public override bool DependsOnOtherProperty() => ma.DependsOnOtherProperty();
    }


    [CustomPropertyDrawer(typeof(Min2Attribute))]
    public class Min2AttributeDrawer : MinMaxAttributeDrawer
    {
        Min2Attribute ma;
        float min;
        public override void Initialize(SerializedProperty property)
        {
            ma = (Min2Attribute)attribute;
            if (ma.minPath == null)
                min = ma.min;
            else
                min = System.Convert.ToSingle(DirtyValue.GetOwner(property).FindRelative(ma.minPath).GetValue());
        }
        public override int CapInt(int i) => Math.Max(i, (int)min);
        public override float CapFloat(float f) => Mathf.Max(f, min);

        public override bool DependsOnOtherProperty() => ma.DependsOnOtherProperty();
    }
}

