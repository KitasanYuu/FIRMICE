using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using CustomInspector.Helpers;


namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(LineGraph))]
    [CustomPropertyDrawer(typeof(LineGraphAttribute))]
    public class LineGraphDrawer : PropertyDrawer
    {
        //static
        const string pointsPropertyName = "points";

        [Min(0.1f)] const float outlineThickness = 1;
        readonly static Vector2 graphPadding = new Vector2(20, 10); //distance of graphs maximum and the outline
        [Min(0.1f)] const float graphPointSize = 10; //size of the specific points in the inspector

        public float GraphHeight => (attribute != null) ? ((LineGraphAttribute)attribute).graphHeight : LineGraphAttribute.defaultGraphHeight;

        readonly static Color outlineColor = new(0.1f, 0.1f, 0.1f, a: 1);

        //runtime
        int? selectedPoint = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            label = PropertyValues.RepairLabel(label, property);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);

            if (!property.isExpanded)
                return;

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            position.height = GraphHeight;

            using (new EditorGUI.IndentLevelScope(1))
            {
                position = EditorGUI.IndentedRect(position);
            }
            using (new NewIndentLevel(0)) //Draw the graph
            {
                //get points
                SerializedProperty pointsProperty = property.FindPropertyRelative(pointsPropertyName);
                List<Vector2> points = pointsProperty.GetAllPropertys(false).Select<SerializedProperty, Vector2>(x => x.vector2Value).ToList();

                if (points.Count > 0)
                {
                    //outline + background
                    Rect backgroundRect = new(position);
                    EditorGUI.DrawRect(backgroundRect, outlineColor);
                    backgroundRect.x += outlineThickness;
                    backgroundRect.y += outlineThickness;
                    backgroundRect.width -= 2 * outlineThickness;
                    backgroundRect.height -= 2 * outlineThickness;
                    EditorGUI.DrawRect(backgroundRect, new Color(.2f, .2f, .2f, a: 1));

                    //Draw graph
                    Rect graphRect = new()
                    {
                        x = backgroundRect.x + graphPadding.x,
                        y = backgroundRect.y + graphPadding.y,
                        width = backgroundRect.width - 2 * graphPadding.x,
                        height = backgroundRect.height - 2 * graphPadding.y,
                    };

                    Vector2 min = new Vector2(Mathf.Min( points[0].x, -0.1f), Mathf.Min( points.Min(p => p.y), -0.1f)); //the min
                    Vector2 max = new Vector2(Mathf.Max( points[^1].x, 0.1f), Mathf.Max(points.Max(p => p.y), 0.1f)); //and max is so that graph is never stuck 

                    Vector2 TransformInRange(Vector2 pointValue) //from points into gui-position
                    {
                        float x = Common.FromRangeToRange(value: pointValue.x,
                                                         minA: min.x,
                                                         maxA: max.x,
                                                         minB: graphRect.x,
                                                         maxB: graphRect.x + graphRect.width);
                        float y = Common.FromRangeToRange(value: pointValue.y,
                                                        minA: min.y,
                                                        maxA: max.y,
                                                        minB: graphRect.y + graphRect.height,
                                                        maxB: graphRect.y);
                        return new(x, y);
                    }
                    Vector2 TransformFromRange(Vector2 uiPosition)
                    {
                        float x = Common.FromRangeToRange(value: uiPosition.x,
                                        minA: graphRect.x,
                                        maxA: graphRect.x + graphRect.width,
                                        minB: min.x,
                                        maxB: max.x);
                        float y = Common.FromRangeToRange(value: uiPosition.y,
                                        minA: graphRect.y + graphRect.height,
                                        maxA: graphRect.y,
                                        minB: min.y,
                                        maxB: max.y);
                        return new(x, y);
                    }

                    {
                        //coordinate system
                        Vector2 originPosition = TransformInRange(Vector2.zero);
                        //x axis
                        if (originPosition.y > backgroundRect.y && originPosition.y < backgroundRect.y + backgroundRect.height)
                            EditorGUI.DrawRect(new Rect(x: backgroundRect.x,
                                                        y: originPosition.y,
                                                        width: backgroundRect.width,
                                                        height: 1), Color.black);
                        //y axis
                        if (originPosition.x > backgroundRect.x && originPosition.x < backgroundRect.x + backgroundRect.width)
                            EditorGUI.DrawRect(new Rect(x: originPosition.x,
                                                            y: backgroundRect.y,
                                                            width: 1,
                                                            height: backgroundRect.height), Color.black);
                    }

                    //line (through the points)
                    {
                        Handles.BeginGUI();

                        List<Vector2> guiPointPositions = points.Select(p => TransformInRange(p)).ToList();
                        guiPointPositions.Insert(0, new Vector2(backgroundRect.x, TransformInRange(points[0]).y)); //outside startpoint
                        guiPointPositions.Add(new Vector2(backgroundRect.x + backgroundRect.width, TransformInRange(points[^1]).y)); //outside endpoint

                        Handles.DrawAAPolyLine(guiPointPositions.Select(p => (Vector3)p).ToArray());

                        Handles.EndGUI();
                    }



                    {
                        //draw buttons
                        Rect buttonRect = new() { width = graphPointSize, height = graphPointSize };
                        List<Vector2> buttonPositions = points.Select(point => TransformInRange(point) - Vector2.one * (graphPointSize / 2f)).ToList();

                        foreach (Vector2 buttonPosition in buttonPositions)
                        {
                            buttonRect.position = buttonPosition;
                            GUI.Button(position: buttonRect, content: GUIContent.none);
                        }
                        //drag buttons

                        //select first matching
                        if (Event.current.type == EventType.Used) //this is an Mouse -Down & -Up & -Drag on a button
                        {
                            for (int i = 0; i < points.Count; i++)
                            {
                                buttonRect.position = buttonPositions[i];
                                if (buttonRect.Contains(Event.current.mousePosition))
                                {
                                    selectedPoint = i;
                                    break; //select only first
                                }
                            }

                            //drag
                            if (selectedPoint.HasValue && backgroundRect.Contains(Event.current.mousePosition))
                            {
                                //apply+save
                                points[selectedPoint.Value] = PreventExplosion(TransformFromRange(Event.current.mousePosition));

                                points = points.OrderBy(point => point.x).ToList();
                                for (int i = 0; i < points.Count; i++)
                                {
                                    pointsProperty.GetArrayElementAtIndex(i).vector2Value = points[i];
                                }
                                property.serializedObject.ApplyModifiedProperties();
                                EditorWindow.focusedWindow.Repaint(); //display changes

                                static Vector2 PreventExplosion(Vector2 v) //if user dragged too far
                                {
                                    if (float.IsNaN(v.x) || float.IsInfinity(v.x))
                                        v.x = 0;
                                    if (float.IsNaN(v.y) || float.IsInfinity(v.y))
                                        v.y = 0;
                                    return v;
                                }
                            }
                        }


                        //delesect
                        if (!backgroundRect.Contains(Event.current.mousePosition)
                            || Event.current.type == EventType.MouseDown //mouse down cannot be on a button -> it would be a Event.type.Used instead then
                            || Event.current.type == EventType.MouseUp)
                            selectedPoint = null;
                    }
                }
                else
                {
                    EditorGUI.HelpBox(position, "No points added to the graph!", MessageType.Error);
                    pointsProperty.InsertArrayElementAtIndex(0);
                }


                //draw points below
                GUIContent pointsLabel = new("Points");
                Rect pointsRect = new()
                {
                    x = position.x,
                    y = position.y + position.height + EditorGUIUtility.standardVerticalSpacing,
                    width = position.width,
                    height = EditorGUI.GetPropertyHeight(pointsProperty, pointsLabel, true),
                };
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(pointsRect, pointsProperty, true);
                if (EditorGUI.EndChangeCheck())
                {
                    //sort changed values
                    var sortedPoints = pointsProperty.GetAllPropertys(false).Select(x => x.vector2Value).OrderBy(p => p.x).ToList();
                    for (int i = 0; i < sortedPoints.Count; i++)
                    {
                        pointsProperty.GetArrayElementAtIndex(i).vector2Value = sortedPoints[i];
                    }
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                SerializedProperty pointsProperty = property.FindPropertyRelative(pointsPropertyName);
                Debug.Assert(pointsProperty != null, $"Property with name '{pointsPropertyName}' not found in 'LineGraph'");
                return EditorGUIUtility.singleLineHeight
                    + GraphHeight
                    + 2 * EditorGUIUtility.standardVerticalSpacing
                    + EditorGUI.GetPropertyHeight(pointsProperty, new("points"), true);
            }
            else
                return EditorGUIUtility.singleLineHeight;
        }
    }
}