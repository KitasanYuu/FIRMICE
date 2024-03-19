using System;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    /// <summary>
    /// Class helping handling any type of needed variable for animation designs
    /// </summary>
    [System.Serializable]
    public partial class ADVariable
    {
        public string ID = "";
        public string DisplayName = "";
        public string Tooltip = "";

        /// <summary> x is Top Spacing, y is bottom spacing </summary>
        public Vector2 GUISpacing = Vector2.zero;

        [SerializeField] private Vector4 v4Val = Vector3.zero;
        [SerializeField] private string str = "";
        [SerializeField] private UnityEngine.Object unityObj = null;
        [SerializeField] private AnimationCurve animCurve = null;


        /// <summary> Curve to support parameter changes during animation clip time </summary>
        [SerializeField] private AnimationCurve blendingCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        public bool DisplayBlendingCurve = false;
        /// <summary> Time Start, Value Start, Time End, Value End </summary>
        public Vector4 _BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f);

        /// <summary> Implement it in your module to support parameter changes during animation clip time </summary>
        public float GetBlendEvaluation(float progress)
        {
            if (blendingCurve == null) return 1f;
            if (blendingCurve.keys.Length < 2) return 1f;
            return blendingCurve.Evaluate(progress);
        }


        public bool HideFlag = false;

        [SerializeField] private int nameHash;
        public int NameHash { get { return nameHash; } }

        [SerializeField] private Vector4 rangeHelper = Vector4.zero;

        public ADVariable(string name, object value)
        {
            ID = name;
            SetValue(value);
            nameHash = ID.GetHashCode();
        }

        public enum EVarType { None, Number, Bool, Vector3, Vector2, Vector4, ProjectObject, String, Curve }
        [HideInInspector] public EVarType ValueType = EVarType.None;
        public enum EVarFloatingSwitch { Float, Int }
        [HideInInspector] public EVarFloatingSwitch FloatSwitch = EVarFloatingSwitch.Float;


        public float Float { get { return v4Val.x; } set { SetValue(value); } }
        public float GetBlendedFloat(float progress) { return v4Val.x * GetBlendEvaluation(progress); }

        public int IntV { get { return Mathf.RoundToInt(v4Val.x); } set { SetValue(value); } }

        public int GetIntValue() { return Mathf.RoundToInt(v4Val.x); }
        public float GetFloatValue() { return v4Val.x; }
        public bool GetBoolValue() { return v4Val.x > 0; }
        public Vector2 GetVector2Value() { return new Vector2(v4Val.x, v4Val.y); }
        public Vector3 GetVector3Value() { return new Vector3(v4Val.x, v4Val.y, v4Val.z); ; }
        public Vector4 GetVector4Value() { return v4Val; }

        /// <summary>
        /// Range for curve is: xyzw = startTime, startValue, endTime, endValue
        /// </summary>
        public void SetRangeHelperValue(Vector4 val)
        {
            rangeHelper = val;
        }

        public void SetRangeHelperValue(float min, float max)
        {
            rangeHelper = new Vector4(min, max, 0f, 0f);
        }

        public Vector4 RangeHelperValue { get { return rangeHelper; } }


        public string GetStringValue() { return str; }
        public UnityEngine.Object GetUnityObjRef() { return unityObj; }
        [NonSerialized] public Type ForceType = null;

        public AnimationCurve GetCurve() { return animCurve; }

        public void SetValue(int value) { v4Val.x = value; ValueType = EVarType.Number; FloatSwitch = EVarFloatingSwitch.Int; UpdateVariable(); }
        public void SetValue(float value) { v4Val.x = value; ValueType = EVarType.Number; FloatSwitch = EVarFloatingSwitch.Float; UpdateVariable(); }
        public void SetValue(bool value) { v4Val.x = value ? 1 : 0; ValueType = EVarType.Bool; UpdateVariable(); }
        public void SetValue(Vector4 value) { v4Val = value; ValueType = EVarType.Vector4; FloatSwitch = EVarFloatingSwitch.Float; UpdateVariable(); }
        public void SetValue(Vector3 value) { v4Val = value; ValueType = EVarType.Vector3; FloatSwitch = EVarFloatingSwitch.Float; UpdateVariable(); }
        public void SetValue(Vector2 value) { v4Val = value; ValueType = EVarType.Vector2; FloatSwitch = EVarFloatingSwitch.Float; UpdateVariable(); }
        public void SetValue(string value) { str = value; ValueType = EVarType.String; UpdateVariable(); }
        public void SetValue(UnityEngine.Object value) { unityObj = value; ValueType = EVarType.ProjectObject; UpdateVariable(); }
        public void SetValue(AnimationCurve value) { animCurve = value; ValueType = EVarType.Curve; UpdateVariable(); }


        public void ClampVector3_01()
        {
            v4Val.x = Mathf.Clamp(v4Val.x, 0f, 1f);
            v4Val.y = Mathf.Clamp(v4Val.y, 0f, 1f);
            v4Val.z = Mathf.Clamp(v4Val.z, 0f, 1f);
        }

        public void ClampVectorAxis_01(bool x, bool y, bool z)
        {
            if (x) v4Val.x = Mathf.Clamp(v4Val.x, 0f, 1f);
            if (y) v4Val.y = Mathf.Clamp(v4Val.y, 0f, 1f);
            if (z) v4Val.z = Mathf.Clamp(v4Val.z, 0f, 1f);
        }

        public void SetValue(object value)
        {
            if (value is int)
            {
                SetValue(Convert.ToInt32(value));
            }
            else if (value is float)
            {
                SetValue(Convert.ToSingle(value));
            }
            else if (value is bool)
            {
                SetValue(Convert.ToBoolean(value));
            }
            else if (value is Vector2)
            {
                SetValue((Vector2)value);
            }
            else if (value is Vector3)
            {
                SetValue((Vector3)value);
            }
            else if (value is Vector4)
            {
                SetValue((Vector4)value);
            }
            else if (value is string)
            {
                SetValue((string)value);
            }
            else if (value is AnimationCurve)
            {
                SetValue((AnimationCurve)value);
            }
            else if (value is UnityEngine.Object)
            {
                SetValue((UnityEngine.Object)value);
            }
            else
            {
                ValueType = EVarType.None;
            }

            UpdateVariable();
        }

        public object GetValue()
        {
            switch (ValueType)
            {
                case EVarType.Number:
                    if (FloatSwitch == EVarFloatingSwitch.Float) return GetFloatValue();
                    else return GetIntValue();

                case EVarType.Bool: return GetBoolValue();

                case EVarType.Vector3:
                    return GetVector3Value();

                case EVarType.Vector4:
                    return GetVector4Value();

                case EVarType.Vector2:
                    return GetVector2Value();

                case EVarType.ProjectObject: return GetUnityObjRef();

                case EVarType.String: return GetStringValue();

                case EVarType.Curve: return GetCurve();
            }

            return -1;
        }

        public void SetValue(ADVariable value)
        {
            if (value == null) return;

            switch (value.ValueType)
            {
                case EVarType.Number:
                    if (value.FloatSwitch == EVarFloatingSwitch.Float)
                        SetValue(value.GetFloatValue());
                    else
                        SetValue(value.GetIntValue());
                    break;

                case EVarType.Bool: SetValue(value.GetBoolValue()); break;
                case EVarType.Vector2: SetValue(value.GetVector2Value()); break;
                case EVarType.Vector3: SetValue(value.GetVector3Value()); break;
                case EVarType.Vector4: SetValue(value.GetVector4Value()); break;
                case EVarType.ProjectObject: SetValue(value.GetUnityObjRef()); break;
                case EVarType.String: SetValue(value.GetStringValue()); break;
                case EVarType.Curve: SetValue(value.GetCurve()); break;
            }

            UpdateVariable();
        }

        public void UpdateVariable() { }

        public ADVariable Copy()
        {
            ADVariable f = (ADVariable)MemberwiseClone();
            f.ValueType = ValueType;
            f.ID = ID;
            f.str = str;
            f.v4Val = v4Val;
            f.animCurve = animCurve;
            f.unityObj = unityObj;
            return f;
        }

    }
}
