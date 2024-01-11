using CustomInspector.Extensions;
using CustomInspector.Helpers.Editor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomInspector.Documentation
{
    /// <summary>
    /// This function finds classes in 'CodeSnippets' and returns them nicely for the Guidance window
    /// </summary>
    public class SnippetsReader : ScriptableObject
    {
        /// <summary>
        /// This is the file
        /// </summary>
        [SerializeField] Object codeSnippetsFile;
        
        /// <summary>
        /// This is the file
        /// </summary>
        [SerializeField, TextArea(1, 20), HideInInspector]
        string codeSnippets;


        /// <summary>
        /// Where on codeSnippets a drawer starts and ends (everything between the braces)
        /// </summary>
        [SerializeField, Dictionary, HideInInspector]
        SerializableSortedDictionary<NewPropertyD, StartEnd> snippetPositions;

        [SerializeField, HideInInspector]
        SerializedObject serializedObject = null;

        [Button(nameof(CreatePreviewObj), label = "recreate preview-object", tooltip = "Resets all changes and executes all start functions", size = Size.small)]
        [HideField, SerializeField] bool _;

        [SerializeField]
        CodeSnippets previewObj = new();

        const int padding = 7;

        private void OnValidate()
        {
            if (codeSnippetsFile == null)
                return; // happens too oft - the user shouldnt notice

            string relativePath = AssetDatabase.GetAssetPath(codeSnippetsFile);
            if (string.IsNullOrEmpty(relativePath))
            {
                Debug.LogError("path of codeSnippetsFile could not be loaded");
                return;
            }

            relativePath = relativePath[6..]; //Remove the 'Assets'
            string path = Application.dataPath + relativePath;
            codeSnippets = File.ReadAllText(path);
            if (string.IsNullOrEmpty(codeSnippets))
            {
                Debug.LogError("codeSnippets empty");
                return;
            }
            FillSnippetPositions();
#if UNITY_EDITOR
            foreach (NewPropertyD d in Enum.GetValues(typeof(NewPropertyD)))
            {
                if (!CustomInspectorAttributeDescriptions.descriptions.ContainsKey(d))
                    Debug.LogWarning($"no description for '{d}' found");
            }
#endif
        }

        /// <summary>
        /// Draws the description field of the drawer
        /// </summary>
        /// <param name="position"></param>
        /// <param name="drawer"></param>
        public void DrawDescription(Rect position, NewPropertyD drawer)
        {
            EditorGUI.DrawRect(position, new Color(.93f, .93f, .93f, 1));
            position = GuidanceWindow.Shrinked(position, padding);

            GUIContent content = new GUIContent();
            try
            {
                content.text = CustomInspectorAttributeDescriptions.descriptions[drawer];
            }
            catch (KeyNotFoundException)
            {
                content.text = $"No description for '{drawer}' available";
            }

            GUIStyle style = new();
            style.normal.textColor = FixedColor.DarkGray.ToColor();
            style.wordWrap = true;

            Rect contentRect = new(x: 0, y: 0, width: position.width - scollbarWidth, height: -1);
            contentRect.height = style.CalcHeight(content, contentRect.width) + 15; //the number is some space below

            using (var scrollScope = new GUI.ScrollViewScope(position, descriptionScrollPos, contentRect))
            {
                descriptionScrollPos = scrollScope.scrollPosition;

                EditorGUI.SelectableLabel(contentRect, content.text, style);
            }
        }
        Vector2 descriptionScrollPos = Vector2.zero;
        const float scollbarWidth = 15;
        /// <summary>
        /// This defines, how many lines should be skipped
        /// </summary>
        Vector2 codeScrollPos = Vector2.zero;
        /// <summary>
        /// Draw the example code snippet on given page
        /// </summary>
        /// <param name="position"></param>
        public void DrawCode(Rect position, NewPropertyD drawer)
        {
            EditorGUI.DrawRect(position, new Color(.12f, .12f, .12f, 1));
            position = GuidanceWindow.Shrinked(position, padding);

            //get snippet
            StartEnd snippetCharPosition;
            try
            {
                snippetCharPosition = snippetPositions[drawer];
            }
            catch (KeyNotFoundException)
            {
                EditorGUI.HelpBox(position, $"no code snippet for {drawer} available.", MessageType.Warning);
                return;
            }
            catch (NullReferenceException)
            {
                EditorGUI.HelpBox(position, $"SnippetsReader on the cs-file CodeSnippets is null. Please reopen the window.", MessageType.Warning);
                return;
            }
            (int start, int end) linesPosition = (CharacterIndexToLineIndex(snippetCharPosition.start), CharacterIndexToLineIndex(snippetCharPosition.end));

            //Get code
            string innerPart = codeSnippets[snippetCharPosition.start..snippetCharPosition.end];
            int usedLines = (linesPosition.end - linesPosition.start + 5);
            int availableLines = (int)(position.height / GUI.skin.button.lineHeight);

            //nicer code
            string code = "<i>using CustomInspector;</i>\n"
            + $"\npublic class MyClass : MonoBehaviour"
            + "\n{\n" + innerPart + "\n}";

            GUIContent content = new (AddRichText(code, GetName(drawer)));

            GUIStyle style = new (GUI.skin.label);
            style.richText = true;
            Rect codeRect = new (Vector2.zero, style.CalcSize(content));
            codeRect.width += 10; //slighly more distance
            using (var scrollScope = new GUI.ScrollViewScope(position,
                                                                codeScrollPos,
                                                                codeRect))
            {
                codeScrollPos = scrollScope.scrollPosition;

                EditorGUI.SelectableLabel(codeRect, content.text, style);
            }

            string GetName(NewPropertyD drawer)
            {
                string s = drawer.ToString();
                if(s.Length > 9 && s[^9..] == "Attribute")
                    return s[..^9];
                else
                    return s;
            }
            /// <summary>
            /// This should colorize like visual studio
            /// !Highligting Attributes!
            /// </summary>
            /// <param name="specialWord">It has to get special highlighting</param>
            string AddRichText(string text, string specialWord)
            {
                //Highlight attributes
                string noSBrackets = @"[^\[\]]*";
                string pairOfSBrackets = @$"\[{noSBrackets}\]";
                text = Regex.Replace(text, @$"\[\w({noSBrackets}|{pairOfSBrackets})*\]", "<color=green>$0</color>"); //form e.g.: [any, thing(anything())] or [Header("[Header] attribute")]

                //Highlight special word
                string noRBrackets = @"[^\(\)]*";
                string pairOfRBrackets = @$"\({noRBrackets}\)";
                pairOfRBrackets = @$"(\(({noRBrackets}|{pairOfRBrackets})*\))?";
                return Regex.Replace(text, @$"([^\w])({specialWord})({pairOfRBrackets})([^\w])", "$1<color=yellow><b>$2</b>$3</color>$6"); //the word plus brackets with anything in it | no word character before and after allowed
            }
        }

        Vector2 previewScrollPos = Vector2.zero;
        /// <summary>
        /// Draw the fields how they would look in the inspector
        /// </summary>
        /// <param name="position"></param>
        public void DrawPreview(Rect position, NewPropertyD drawer)
        {
            DrawProperties.DrawBorder(position, false);
            position = GuidanceWindow.Shrinked(position, padding);

            //Create obj
            if (previewObj == null)
            {
                CreatePreviewObj();
            }
            serializedObject ??= new SerializedObject(this);
            
            //Find container that we preview
            string propName = ClassName(drawer);

            SerializedProperty container = serializedObject.FindProperty($"{nameof(previewObj)}.{propName}");
            if (container is null)
            {
                EditorGUI.HelpBox(position, $"no preview for {drawer} available. Please reopen the window.\n({propName} in CodeSnippets missing)", MessageType.Warning);
                return;
            }

            //Draw Propertys
            var props = container.GetAllVisiblePropertys(false);
            Debug.Assert(props.Any(), "No properties found for display");
            float totalHeight = props.Select(_ => DrawProperties.GetPropertyHeight(_)).Sum() + props.Count() * EditorGUIUtility.standardVerticalSpacing + 50; // 50 is some tolerance - space below
            
            Rect previewRect =
            (totalHeight > position.height) ?//if has slider
                new Rect(0, 0, position.width - GuidanceWindow.scrollbarThickness, totalHeight)
                : new Rect(0, 0, position.width, totalHeight);
            using (var scrollScope = new GUI.ScrollViewScope(position,
                                                    previewScrollPos,
                                                    previewRect))
            {
                previewScrollPos = scrollScope.scrollPosition;

                float spacing = 5;
                float height = spacing;
                EditorGUIUtility.labelWidth = position.width * 0.35f;
                EditorGUIUtility.fieldWidth = 50;
                EditorGUI.indentLevel = 0;
                using (new NewWideModeScope(position.width > 350))
                {
                    foreach (SerializedProperty prop in props)
                    {
                        GUIContent label = new(prop.name, prop.tooltip);
                        Rect lineRect = new(x: spacing,
                                            y: height,
                                            width: previewRect.width - 2 * spacing,
                                            height: DrawProperties.GetPropertyHeight(label, prop));

                        DrawProperties.PropertyField(lineRect, label, prop, true);
                        height += lineRect.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }

            string ClassName(NewPropertyD d)
            {
                char[] c = drawer.ToString().ToCharArray();

                c[0] = (char)(c[0] + 'a' - 'A');
                return new string(c) + "Examples";
            }
        }
        void CreatePreviewObj()
        {
            previewObj = new();

            //Call start methods
            var fields = typeof(CodeSnippets).GetFields(PropertyValues.defaultBindingFlags);
            foreach (FieldInfo field in fields)
            {
                InvokableMethod startMethod;
                try
                {
                    object obj = field.GetValue(previewObj);
                    if (obj != null)
                        startMethod = new InvokableMethod(obj, "Start");
                    else
                    {
                        Debug.LogWarning("Attribute examples should not be null. Start could not be executed");
                        continue;
                    }
                }
                catch(MissingMethodException)
                {
                    continue;
                }

                try
                {
                    startMethod.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Fills the start indices of each example snippet
        /// </summary>
        void FillSnippetPositions()
        {
            snippetPositions = new();

            foreach (Match match in Regex.Matches(codeSnippets, @"class\s(\w+)Examples"))
            {
                //Find start
                int startInd;
                int openBrackets = 1;
                for (startInd = match.Index + 1; codeSnippets[startInd] != '{'; startInd++) { } // go to open brace
                do { startInd++; }
                while (codeSnippets[startInd-1] != '\n'); // remove empty line
                //Find end
                int endInd;
                for (endInd = startInd + 1; openBrackets > 0; endInd++) // go to close brace
                {
                    if (codeSnippets[endInd] == '}')
                        openBrackets--;
                    else if (codeSnippets[endInd] == '{')
                        openBrackets++;
                }
                endInd--; //to closing bracket back
                do { endInd--; }
                while (codeSnippets[endInd] != '\n'); // remove last line
                
                //Find enum
                string name = match.Groups[1].Value;
                NewPropertyD res;
                try
                {
                    res = (NewPropertyD)Enum.Parse(typeof(NewPropertyD), name);
                }
                catch (ArgumentException)
                {
                    Debug.LogError($"Invalid CodeSnippetName: {name}");
                    continue;
                }
                //Add to dictionary
                try
                {
                    snippetPositions.Add(res, new StartEnd(startInd, endInd));
                }
                catch (ArgumentException)
                {
                    var prev = snippetPositions[res];
                    Debug.LogError($"Duplicate code snippet for {res}" +
                                   $"\nLine {CharacterIndexToLineIndex(prev.start)} to {CharacterIndexToLineIndex(prev.end)}" +
                                   $" and {CharacterIndexToLineIndex(startInd)}, to {CharacterIndexToLineIndex(endInd)}");

                }
            }
#if UNITY_EDITOR
            foreach (NewPropertyD d in Enum.GetValues(typeof(NewPropertyD)))
            {
                if (!snippetPositions.ContainsKey(d))
                    Debug.LogWarning($"no code snippet for '{d}' found");
            }
#endif
        }
        /// <summary>
        /// Each line first character index
        /// </summary>
        List<int> lineIndices = new List<int>() { 0 };
        
        int LineIndexToCharacterIndex(int index)
        {
            while(index >= lineIndices.Count)
                CalcNextLine();

            return lineIndices[index];
        }
        int CharacterIndexToLineIndex(int index)
        {
            //if not calculated yet
            if (lineIndices.Last() < index)
            {
                while (CalcNextLine() < index) { }
                
                return lineIndices.Count - 1;
            }
            else //in list
            {
                if(lineIndices.Last() == index)
                    return lineIndices.Count + 1;

                return FindLineBetween(0, lineIndices.Count - 1);

                ///<summary>Find the index in lineIndices that is bigger, but clostest to index</summary>
                int FindLineBetween(int min, int max)
                {
                    if(min + 3 >= max)
                    {
                        if (lineIndices[min] > index)
                            return min;
                        else if (lineIndices[min + 1] > index)
                            return min + 1;
                        else if (lineIndices[min + 2] > index)
                            return min + 2;
                        else if (lineIndices[max] > index)
                            return max;
                        else
                            throw new KeyNotFoundException();
                    }

                    int middle = (min + max) / 2;

                    if (index == lineIndices[middle])
                        return middle + 1;
                    else if (index < lineIndices[middle])
                        return FindLineBetween(min, middle);
                    else
                        return FindLineBetween(middle, max);
                }
            }



        }
        IEnumerator<int> enumerator = null;
        int CalcNextLine()
        {
            if (enumerator == null)
                enumerator = GetLineIndices();

            enumerator.MoveNext();
            lineIndices.Add(enumerator.Current);

            return enumerator.Current;

            IEnumerator<int> GetLineIndices() //e.g. Hallo\nHello\n... -> 6, 12...
            {
                for (int i = 0; i < codeSnippets.Length; i++)
                {
                    if (codeSnippets[i] == '\n')
                        yield return i;
                }
                yield return codeSnippets.Length;
                Debug.LogError("Reached file end");
            }
        }
        [System.Serializable]
        class StartEnd
        {
            public int start;
            public int end;

            public StartEnd(int start, int end)
            {
                this.start = start;
                this.end = end;
            }
        }
    }
}