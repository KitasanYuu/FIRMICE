using System.Collections.Generic;

namespace CustomInspector.Documentation
{
    public static class CustomInspectorAttributeDescriptions
    {
        public static Dictionary<NewPropertyD, string> descriptions = new()
        {
            // ----------------DecoratorDrawer-------------


            { NewPropertyD.HorizontalLineAttribute,
            "Adds a horizontal line to divide and structure the inspector." },


            { NewPropertyD.MessageBoxAttribute,
            "Displays an informational message in the inspector." +
            "\nUseful for providing context or details about a property." +
            "\nThe message box icon is determined by the MessageType parameter." },


            // -------------- PropertyDrawer-------------------

            { NewPropertyD.AsButtonAttribute,
            "Displays a bool, int, string or InspectorButtonState as a clickable button in the inspector." +
            "\nUse the 'staysPressed' parameter to specify if the button should remain pressed after being clicked." },

            { NewPropertyD.AsRangeAttribute,
            "Interpret a Vector2 as a range between given minLimit and maxLimit. " +
            "Useful for selecting values within a custom range." },


            { NewPropertyD.AssetsOnlyAttribute,
            "Prevents scene objects from being assigned to the property." +
            "\nOnly assets, such as prefabs or imported meshes, can be assigned to the property." },


            { NewPropertyD.ButtonAttribute,
            "Executes a function when the button is clicked." +
            "\nUseful for editor scripts or frequently-used functions that should be easily accessible." },


            { NewPropertyD.CopyPasteAttribute,
            "Provides buttons for copying and pasting of variable values between programs using the system clipboard." },


            { NewPropertyD.DecimalsAttribute,
            "Limits the number of decimal places that can be entered for a property value." +
            "\nUseful for ensuring accuracy when using floats or similar data types." },

            { NewPropertyD.Delayed2Attribute,
            "The equivalent to unitys DelayedAttribute, but also works on vectors." +
            "\nDescription:\n" +
            "Fields will not return a new value until the user has pressed enter or focus is moved away from the field." },

            { NewPropertyD.DisplayAutoPropertyAttribute,
            "Displays auto-properties in the inspector, which Unity does not serialize by default." +
            "\nValues can only be changed at runtime in the inspector, as they are not saved with the scene or project." },

            { NewPropertyD.FixedValuesAttribute,
            "Limits the input of a property to a specific set of values." +
            "\nUseful for restricting input to valid options." },

            { NewPropertyD.FoldoutAttribute,
            "Adds a foldout option to see more information on other MonoBehaviours or ScriptableObjects." },

            { NewPropertyD.ForceFillAttribute,
            "Indicates that a field must be filled out and can be used anywhere." +
            "\nThe forbidden values can be defined using the ToString() format for every type (e.g., Vector3 -> (1, 1, 1))." +
            "\nTo check whether all fields has been filled out, use the CheckForceFilled function: ForceFill.CheckForceFilled(this)." +
            "\nThis function is automatically excluded in the build." },


            { NewPropertyD.GetSetAttribute,
            "Of course, you can validate input afterwards, but you can also add a getter and setter directly here." +
            "\r\nWarning: If you don't make any changes to serialized fields on the actual object in the setter, " +
            "e.g. if you only change other objects in the scene or static fields, " +
            "then it will not be saved because unity thinks nothing has been changed" },


            { NewPropertyD.GUIColorAttribute,
            "Changes the color of the GUI of one field or the entire GUI." },


            { NewPropertyD.HideFieldAttribute,
            "Hides only the fields in the inspector that are serialized by default, unlike the build-in [HideInInspector] also hiding everything attached to the fields too." +
            "\nYou can see in the example below, that HideInInspector hides other attributes too but HideField keeps previous Attributes (like the [Header]-attribute)." +
            "\nHideInInspector will hide a whole list, but HideField affects the elements of a list."},

            { NewPropertyD.HookAttribute,
            "Calls a method, if the value was changed in the inspector. " +
            "Given method can be without parameters or with 2 parameters (oldValue, newValue) that share the same type of the field." +
            "\nHookAttribute can be used as a custom setter: If you set 'useHookOnly', inspector inputs will *only* call the hook-method and will not apply themselves." +
            "\nWarning: If you change values of non-serialized fields/properties (like for example statics) they wont be saved."},


            { NewPropertyD.BackgroundColorAttribute,
            "This attribute can be used to highlight certain fields" },


            { NewPropertyD.HorizontalGroupAttribute,
            "Surely everyone has already thought about placing input fields next to each other in Unity so that they take up less space. " +
            "It is also very useful for structuring or for comparing two classes. " +
            "Note: You begin a new HorizontalGroup by setting the parameter beginNewGroup=true" +
            "\n- Does not work with reordable lists" },


            { NewPropertyD.IndentAttribute,
            "For indenting or un-indenting your fields."},

            { NewPropertyD.InspectorIconAttribute,
            "Inserts an icon in front of the label or appends it at the end" },

            { NewPropertyD.LabelSettingsAttribute,
            "Edit where the input field is and if the label is shown"},


            { NewPropertyD.LayerAttribute,
            "If an integer represents a layer, it is very difficult to tell in the inspector which number belongs to which layer. " +
            "This attribute facilitates the assignment - you can select a single layer from an enum dropdown." },


            { NewPropertyD.PreviewAttribute,
            "Filenames can be long and sometimes assets are not easy to identify in the inspector. " +
            "With a preview you can see directly what kind of asset it is" },

            { NewPropertyD.ProgressBarAttribute,
            "Use on floats and ints to show a progressbar that finishes at given max." +
            "\nThe progressbar can be edited by dragging over it if you dont set the 'isReadOnly'." },


            { NewPropertyD.ReadOnlyAttribute,
            "Want to see everything? Knowledge is power, but what if you don't want that variable to be edited in the inspector?" +
            "\nWith this attribute you can easily make fields visible in the inspector without later wondering if you should change this value"},


            { NewPropertyD.RequireTypeAttribute,
            "Anyone who masters C# will eventually get to the point that they are working with inheritance. " +
            "Since c# doesn't support multi-inheritance, there are interfaces. " +
            "Unfortunately, a field with type of interface is not shown in the inspector. " +
            "With this attribute you can easily restrict object references to all types and they will still be displayed"},

            { NewPropertyD.RichTextAttribute,
            "Display text using unitys html-style markup-format." +
            "\nYou can edit the raw text if you foldout the textfield." },

            { NewPropertyD.SelfFillAttribute,
            "If you have components where you know they are on your own object and don't want to write GetComponent every time, you can now write [SelfFill] in front of it. " +
            "With this attribute, the fields are already saved in the editor and no longer consume any performance at runtime (because after the first OnValidate or OnGUI call in editor it will fill with GetComponent). " +
            "\nThe fields will hide if they are filled if you set the parameter hideIfFilled=true (they will still show an error if they didnt find themselves a suitable component). " +
            "\nYou can even use SelfFill.CheckSelfFilled to test whether all components have been found" +
            "\nPro Tip: Very useful for inner classes and list elements to get informations of the local Monobehaviour you are inside." +
            "\nYou can also put SelfFill on a gameObject or Transform so you get the gameObject or Transform in an inner classes" },

            { NewPropertyD.ShowAssetReferenceAttribute,
            "Provides a way to quickly locate and edit references to generic C# classes" +
            "\nIf the file-name does not match the type of your generic class, you can insert a custom fileName to locate the file" },


            { NewPropertyD.ShowIfAttribute,
            "Opposite of the [ShowIfNot]-attribute.\n" +
            "Some variables are simply not needed in certain constellations. " +
            "Instead of making your inspector unnecessarily confusing, you can simply hide them. " +
            "\nYou can use nameof() to reference booleans/methods. " +
            "\nYou can use certain special conditions with the" + nameof(StaticConditions) + "-enum (like "+ nameof(StaticConditions.IsPlaying) +" or " + nameof(StaticConditions.IsActiveAndEnabled) + ")" +
            "\nThe opposite of ShowIfNotAttribute." +
            "\nIndents field automatically, but you can revert with Indent(-1)" },


            { NewPropertyD.ShowIfIsAttribute,
            "Opposite of the [ShowIfIsNot]-attribute.\n" +
            "Similar to the ShowIfAttribute, but instead of passing references you pass one reference and one actual value. " +
            "It is then tested whether they have the same value. " +
            "Mostly you want to use ShowIfAttribute instead, because you cannot use functions, you are restricted to only comparing two and you can only pass constants as 2nd attribute parameter:" +
            "\nbools, numbers, strings, Types, enums, and arrays of those types" },

            { NewPropertyD.ShowIfIsNotAttribute,
            "Opposite of the [ShowIfIs]-attribute.\n" +
            "-> Read the description of the [ShowIfIs]-attribute to get a deeper explanation." },


            { NewPropertyD.ShowIfNotAttribute,
            "Opposite of the [ShowIf]-attribute.\n" +
            "-> Read the description of the [ShowIf]-attribute to get a deeper explanation." },

            { NewPropertyD.ShowMethodAttribute,
            "This attribute can be used to display return values from methods. Field is updated on each OnGUI call (e.g. when you hover over menu buttons on the left)" +
            "\nThe name shown in the inspector can be given custom or is the name of the get-function without (if found) the word \"get\"" },

            { NewPropertyD.ShowPropertyAttribute,
            "Displays a property additionally at current position." +
            "\nThe [HideField] and [HideInInspector] attributes will be removed for this property." },

            { NewPropertyD.Space2Attribute,
            "A Variation to unitys buildin SpaceAttribute that is more compatible with other attributes." +
            "\nThe parameter is the distance in pixels." },

            { NewPropertyD.TabAttribute,
            "An easy way to divide propertys in groups. Fields with same (attribute parameter) groupName share the same group" },


            { NewPropertyD.TagAttribute,
            "Makes you select tags from an enum dropdown." },

            { NewPropertyD.TitleAttribute,
            "An alternative to the [Header]-attribute that does not always draws first,\ngiving you a more flexible draw order." +
            "\nIt also allows for defining a tooltip and a custom font-size" },


            { NewPropertyD.ToolbarAttribute,
            "A normal toggle or enum dropdown is very small and unobtrusive. " +
            "This display is much more noticeable"},

            { NewPropertyD.TooltipBoxAttribute,
            "Especially if you rarely use tooltips, this way you can make it more clear that there is an explanation." +
            "\nFirst TooltipBox (in code) will be the outermost." },

            { NewPropertyD.UnfoldAttribute,
            "Always ticks the foldout on generics, so they are always open." +
            "\nUse the [Unwrap] to hide the foldout completely"},


            { NewPropertyD.UnitAttribute,
            "Make the current unit clear so that you can better assess the values"},

            { NewPropertyD.UnwrapAttribute,
            "Shows the serialized fields of the class instead of it wrapped with a foldout"},


            { NewPropertyD.URLAttribute,
            "Displays a clickable url in the inspector"},


            { NewPropertyD.ValidateAttribute,
            "If you only want to allow certain values, this attribute is perfect to make it clear what is allowed or not directly when entering it in the inspector" },


            { NewPropertyD.MaskAttribute,
            "Everyone has seen the constraints on the rigidbody as 3 toggles next to each other and maybe thought of some kind of horizontal alignment, but it's a mask. " +
            "A LayerMask is also a Mask. " +
            "A mask is a number where each bit of the number is interpreted as yes/no. " +
            "Then you can pack a lot of booleans into one number. To access the 3rd bit later, you can use bitshift for example. " +
            "Now you can easily show Masks in the inspector as what they are. Note: On integers you should specify how many bits are displayed (default=3)" },


            { NewPropertyD.MaxAttribute,
            "The counterpart to unitys buildin MinAttribute: Cap the values of numbers or components of vectors to a given maximum" },


            { NewPropertyD.MultipleOfAttribute,
            "It allows only multiples of a given number. The number can be passed by value or by name/path of field" },

            { NewPropertyD.Min2Attribute,
            "Extension to unitys buildin MinAttribute: You can also pass other members names to have a dynamic min value" },

            // --------------------- Types ----------------

            { NewPropertyD.Array2D,
            "To display a two-dimensional array as a table in the inspector."},

            { NewPropertyD.DynamicSlider,
            "The built-in range slider is very nice and handy; " +
            "But what if you don't want unchangable fixed min-max limits. " +
            "In this way, the designer remains flexible to change the values if necessary, but has a defined default range." +
            "\nNote: Since type drawers are not compatible to attributes by default, you have to add [DynamicSlider] attribute if you add other attributes" },

            { NewPropertyD.FilePath,
            "In a project I once ran DeleteAssets on a path defined by a string. " +
            "Clumsily, the string was initialized to \"Assets\". " +
            "The whole project had been deleted. That'll never happen again with this type. " +
            "If the path does not end on a specified type (which is never a Folder!), GetPath() throws a NullReferenceException" +
            "\nNote: Since type drawers are not compatible to attributes by default, you have to add [AssetPath] attribute if you add other attributes" },


            { NewPropertyD.FolderPath,
            "Since FilePath cannot hold Folders, this is a type that only holds paths leading to folders. " +
            "Invalid paths return NullReferenceExceptions.\n(Also look at FilePath)" +
            "\nNote: Since type drawers are not compatible to attributes by default, you have to add [AssetPath] attribute if you add other attributes" },

            { NewPropertyD.LineGraph,
            "Used to make a graph out of linear connecting points." +
            "\nThis is an easy to compute and an easy to understand method." +
            "\nThe black lines are the x-axis and the y-axis." +
            "\n\nAn usecase would be for example the damage drop-off in shooter games." },

            { NewPropertyD.MessageDrawer,
            "If you want to write something in the inspector at runtime instead of in the console. For non-runtime messages use the MessageBoxAttribute" },


            { NewPropertyD.ReorderableDictionary,
            "A serializable dictionary that is shown and reordable in the inspector." +
            "\nDuplicate keys are marked in the inspector and won't be added to the dictionary." +
            "\nThe reorder-ability is just cosmetic and has no effect in code/game." +
            "\nReorderableDictionary is derived from the System.Dictionary." +
            "\nTime complexity: access = O(log(n)), add/remove = O(n)" },

            { NewPropertyD.SerializableDateTime,
            "For displaying time in the unity-inspector." +
            "\nYou can edit the inspector appealing in the [SerializableDateTime]-attribute." },

            { NewPropertyD.SerializableDictionary,
            "A serializable dictionary that can be shown in the inspector" +
            "\nIf you are using generic keys, you should override the equals-method " +
            "because the default implementation is based on a reference value that changes during serialization." +
            "\nTips: -The foldout's text for generics is based on the ToString-Method." +
            "\n -Change the key-value-ratio with the parameter 'keySize' on the [Dictionary]-attribute" +
            "\nTime complexity: add/remove/access = O(n)" +
            "\nUse SerializableSortedDictionary for better complexity/performance" },


            { NewPropertyD.SerializableSortedDictionary,
            "A serializable implementation of System.SortedDictionary that can be shown in the inspector." +
            "\nKey has to implement the interface System.IComparable." +
            "\nTime complexity: access = O(log(n)) , add/remove = O(n)" },


            { NewPropertyD.SerializableSet,
            "A list, with no duplicates possible. Adding a duplicate will lead to an ArgumentException" +
            "\nIf you are it with generic types, you should override the equals-method " +
            "because the default implementation is based on a reference value that changes during serialization." +
            "\nPro Tip: The foldout's text for generics is based on the ToString-Method." +
            "\nTime complexity: add/remove/access = O(n)" },

            { NewPropertyD.SerializableSortedSet,
            "The equivalent to the System.SortedSet but can be serialized and shown in the unity-inspector" +
            "\nTime complexity: access = O(log(n)), add/remove = O(n)" },


            { NewPropertyD.SerializableTuple,
            "A serializable version of a Tuple" },

            { NewPropertyD.StaticsDrawer,
            "Static variables are all well and good, but unity doesn't show them in the inspector. " +
            "Place the serialized StaticsDrawer anywhere in your class and the inspector will show all static variables of your class. " +
            "Since unity does not save statics, values can only be changed runtime in the inspector (you can test it by entering playmode)" },


            // ------UNITYS--------

            { NewPropertyD.DelayedAttribute,
            "Unity Documentation:\n" +
            "\"When this attribute is used, the float, int, or text field will not return a new value until the user has pressed enter or focus is moved away from the field.\"" },

            { NewPropertyD.HeaderAttribute,
            "Unity Documentation:\n" +
            "\"Use this PropertyAttribute to add a header above some fields in the Inspector.\"" },

            { NewPropertyD.HideInInspectorAttribute,
            "Not only hides a property from the inspector, but also hides everything that belongs to it." +
            "\n->To hide full list instead of only the elements of the list." +
            "\nUse the [HideField]-Attribute if you only want to hide the field only." },

            { NewPropertyD.MinAttribute,
            "Obsolete: use [Min2]-attribute instead\n" +
            "\nThe counterpart to the MaxAttribute." +
            "\nCap the values of numbers or components of vectors to a given minimum\n" +
            "Warning: it will only cap new inputs in the inspector: not set values by script" },

            { NewPropertyD.MultilineAttribute,
            "Unity Documentation:\n" +
            "\"Attribute to make a string be edited with a multi-line textfield.\"" },

            { NewPropertyD.NonReorderableAttribute,
            "Unity Documentation:\n" +
            "\"Disables reordering of an array or list in the Inspector window.\"" },

            { NewPropertyD.NonSerializedAttribute,
            "Prevents the unityEditor to serialize this property so it doesnt save any values for it." +
            "\nIf you just hide the property it will still have a saved value in the background." },

            { NewPropertyD.RangeAttribute,
            "Draws a slider in the inspector where you can choose values from\n" +
            "\nUnity Documentation:\n" +
            "Attribute used to make a float or int variable in a script be restricted to a specific range." },

            { NewPropertyD.SpaceAttribute,
            "Unity Documentation:\n" +
            "\"Use this PropertyAttribute to add some spacing in the Inspector.\"" +
            "\n\n" +
            "Use the [Space2]-attribute instead, if you encounter sorting problems. [Space2] has better compatibility with other attributes." },

            { NewPropertyD.TooltipAttribute,
            "Adds a tooltip that you appears by hovering over the given field in the inspector AND in your visual studio editor." },

            { NewPropertyD.TextAreaAttribute,
            "Unity Documentation:\n" +
            "\"Attribute to make a string be edited with a height-flexible and scrollable text area.\n" +
            "You can specify the minimum and maximum lines for the TextArea, and the field will expand according to the size of the text. A scrollbar will appear if the text is bigger than the area available.\"" },
        };
    }
}