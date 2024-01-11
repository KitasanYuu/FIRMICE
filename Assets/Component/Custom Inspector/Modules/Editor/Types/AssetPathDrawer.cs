using CustomInspector.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(AssetPathAttribute))]

    [CustomPropertyDrawer(typeof(AssetPath))]
    [CustomPropertyDrawer(typeof(FilePath))]
    [CustomPropertyDrawer(typeof(FolderPath))]
    public class AssetPathDrawer : PropertyDrawer
    {
#if UNITY_EDITOR
        //1: path full with. 0.5f: path and object picker is half half width
        [Range(0, 1)] const float pathToObjectRatio = 0.7f;
        const float pathToObjectSpacing = 5;

        // The error field that pops above the property
        const float labelToErrorSpacing = 16f;
        const float errorHeight = 40;
        const float minDropRectWidth = 3;
        float fileOpenDialogButtonSize => 0;//EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Check type
            if (!fieldInfo.FieldType.IsSubclassOf(typeof(AssetPath)))
            {
                EditorGUI.HelpBox(position, "AssetPathAttribute only valid on FilePath or FolderPath", MessageType.Error);
                return;
            }


            position.height = EditorGUIUtility.singleLineHeight;

            //Get Properties
            SerializedProperty path = property.FindPropertyRelative("path");
            SerializedProperty assetReference = property.FindPropertyRelative("assetReference");

            DirtyValue dv = new(property);
            dv.FindRelative("RequiredType").TryGetValue(out object resType);

            if(resType == null) //noone called = new() | so unity used some sort of copy to not call the constructor
            {
                position.height = errorHeight;
                EditorGUI.HelpBox(position, $"path '{property.name}' is null!\nPls call the constructor in code: ... = new()", MessageType.Error);
                return;
            }
            Type requiredType = (Type)resType;

            //Test if loaded
            Debug.Assert(path != null, $"path not found");
            Debug.Assert(assetReference != null, "assetReference not found");
            Debug.Assert(requiredType != null, "RequiredType not found");

            //new guiContent showing type
            if (requiredType == typeof(Object)) { }
            else if(requiredType == typeof(Folder))
                label.text += " (Folder)";
            else
                label.text += $" ({requiredType.Name})";

            position = EditorGUI.IndentedRect(position);
            using (new NewIndentLevel(0))
            {
                //warning if not valid
                bool isValid = AssetPath.IsValidPath(path.stringValue, requiredType);
                if (!isValid)
                {
                    Rect errorRect;
                    if (EditorGUIUtility.wideMode)
                    {
                        float xStart = position.x //label start
                                     + Mathf.Min(EditorGUIUtility.labelWidth, GUI.skin.label.CalcSize(label).x) //label guiContent width
                                     + labelToErrorSpacing; //spacing
                        errorRect = new()
                        {
                            x = xStart,
                            y = position.y,
                            width = (position.x + position.width) - xStart,
                            height = errorHeight
                        };
                    }
                    else
                    {
                        errorRect = new(position)
                        {
                            height = errorHeight,
                        };
                    }

                    if (requiredType == typeof(Folder) && AssetPath.IsValidPath(path.stringValue, typeof(Object))) //Just wrong type
                    {
                        EditorGUI.HelpBox(errorRect, "Path has to end on a folder and not on a file!", MessageType.Error);
                    }
                    else if(AssetPath.IsValidPath(path.stringValue, typeof(Folder)))
                    {
                        EditorGUI.HelpBox(errorRect, $"Path has to end on a file and not a folder", MessageType.Error);
                    }
                    else if (AssetPath.IsValidPath(path.stringValue, typeof(Object)))
                    {
                        EditorGUI.HelpBox(errorRect, $"Loaded file has to be of type {requiredType}", MessageType.Error);
                    }
                    else
                    {
                        IEnumerator<string> parts = path.stringValue.Split('/', '\\').AsEnumerable<string>().GetEnumerator();
                        parts.MoveNext();
                        if (parts.Current == "Assets")
                        {
                            char usedSeperator = path.stringValue[6];
                            string currentPath = "Assets";
                            while (true) //Find what is not found
                            {
                                if(!parts.MoveNext())
                                {
                                    Debug.LogWarning("Wrong guess, that path is wrong");
                                    break;
                                }

                                string next = $"{currentPath}{usedSeperator}{parts.Current}";
                                if (AssetDatabase.LoadAssetAtPath<Object>(next) == null)
                                {
                                    EditorGUI.HelpBox(errorRect, $"'{parts.Current}' not found in '{currentPath}'", MessageType.Error);
                                    break;
                                }
                                currentPath = next;
                            }
                        }
                        else
                        {
                            EditorGUI.HelpBox(errorRect, $"Every path has to start with 'Assets/'", MessageType.Error);
                        }
                    }

                    if (EditorGUIUtility.wideMode)
                        position.y += (errorHeight - EditorGUIUtility.singleLineHeight) + EditorGUIUtility.standardVerticalSpacing;
                    else
                        position.y += errorHeight + EditorGUIUtility.standardVerticalSpacing;
                }


                //Draw label
                EditorGUI.LabelField(position, label);

                //Draw path input field
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                float savedLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 0;
                Rect pathRect = new(position)
                {
                    width = Mathf.Min(position.width * (pathToObjectRatio),
                                      position.width - EditorGUIUtility.singleLineHeight - pathToObjectSpacing - minDropRectWidth),
                };
                EditorGUI.BeginChangeCheck();
                string res = EditorGUI.TextField(pathRect, path.stringValue);
                if (EditorGUI.EndChangeCheck() || (isValid && assetReference.objectReferenceValue == null))
                {
                    if (string.IsNullOrEmpty(res))
                    {
                        path.stringValue = "";
                        assetReference.objectReferenceValue = null;
                    }
                    else
                    {
                        if (res[^1] == '/' || res[^1] == '\\')
                            res = res[..^1];

                        path.stringValue = res;
                        if (AssetPath.IsValidPath(res, requiredType))
                            assetReference.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(res);
                        else
                            assetReference.objectReferenceValue = null;
                    }
                }

                //Object drop
                Rect dropRect = new(position)
                {
                    x = pathRect.x + pathRect.width + pathToObjectSpacing,
                    width = position.width - pathRect.width - pathToObjectSpacing - fileOpenDialogButtonSize,
                };
                Rect fileOpenRect = new(dropRect)
                {
                    x = dropRect.x + dropRect.width,
                    width = fileOpenDialogButtonSize,
                };
                Object droppedObj;
                string absolutePath = null;
                EditorGUI.BeginChangeCheck();

                if (requiredType == typeof(Folder))
                {
                    droppedObj = EditorGUI.ObjectField(position: dropRect, label: GUIContent.none,
                                                           obj: assetReference.objectReferenceValue, objType: typeof(DefaultAsset), allowSceneObjects: false);

/*                    if (GUI.Button(fileOpenRect, "O"))
                    {
                        absolutePath = EditorUtility.OpenFolderPanel("Folder Selection", path.stringValue, "");
                    }*/
                }
                else
                {
                    droppedObj = EditorGUI.ObjectField(position: dropRect, label: GUIContent.none,
                                                                   obj: assetReference.objectReferenceValue, objType: requiredType, allowSceneObjects: false);

/*                    if (GUI.Button(fileOpenRect, "O"))
                    {
                        absolutePath = EditorUtility.OpenFolderPanel("File Selection", path.stringValue, "");
                    }*/
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if(absolutePath == null)
                    {
                        assetReference.objectReferenceValue = droppedObj;
                        path.stringValue = AssetDatabase.GetAssetPath(droppedObj);
                    }
                    else
                    {
                        Debug.Log("choosing from the file open dialog will come in a future update");
                    }
                }
                //Folder icon
                var defaultFolder = AssetDatabase.LoadAssetAtPath<Object>("Assets");
                dropRect.x += dropRect.width - dropRect.height;
                dropRect.width = dropRect.height;
                GUI.DrawTexture(dropRect, AssetPreview.GetMiniThumbnail(defaultFolder));

                EditorGUIUtility.labelWidth = savedLabelWidth;
            }
        }
        Texture folderIcon;
        Texture FolderIcon { get { if (folderIcon == null) folderIcon = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<Object>("Assets"));  return folderIcon; } }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //Check type
            if (!fieldInfo.FieldType.IsSubclassOf(typeof(AssetPath)))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            SerializedProperty path = property.FindPropertyRelative("path");
            DirtyValue dv = new(property);
            dv.FindRelative("RequiredType").TryGetValue(out object resType);
            if (resType == null)
                return errorHeight + EditorGUIUtility.standardVerticalSpacing;

            Type requiredType = (Type)resType;

            if (AssetPath.IsValidPath(path.stringValue, requiredType))
                return 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            else
            {
                if (EditorGUIUtility.wideMode)
                    return errorHeight + EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
                else
                    return errorHeight + 2 * EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
            }
        }
#endif
    }
}