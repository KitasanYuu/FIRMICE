using System.Collections.Generic;
using System.Linq;


namespace CustomInspector.Documentation
{
    public enum NewPropertyD
    {
        //attributes
        AsButtonAttribute,
        AsRangeAttribute,
        AssetsOnlyAttribute,
        BackgroundColorAttribute,
        ButtonAttribute,
        CopyPasteAttribute,
        DecimalsAttribute,
        Delayed2Attribute,
        DisplayAutoPropertyAttribute,
        FixedValuesAttribute,
        FoldoutAttribute,
        ForceFillAttribute,
        GetSetAttribute,
        GUIColorAttribute,
        HideFieldAttribute,
        HookAttribute,
        HorizontalGroupAttribute,
        HorizontalLineAttribute,
        IndentAttribute,
        InspectorIconAttribute,
        LabelSettingsAttribute,
        LayerAttribute,
        MaskAttribute,
        MaxAttribute,
        MessageBoxAttribute,
        Min2Attribute,
        MultipleOfAttribute,
        PreviewAttribute,
        ProgressBarAttribute,
        ReadOnlyAttribute,
        RequireTypeAttribute,
        RichTextAttribute,
        SelfFillAttribute,
        ShowAssetReferenceAttribute,
        ShowIfAttribute,
        ShowIfIsAttribute,
        ShowIfIsNotAttribute,
        ShowIfNotAttribute,
        ShowMethodAttribute,
        ShowPropertyAttribute,
        Space2Attribute,
        TabAttribute,
        TagAttribute,
        TitleAttribute,
        ToolbarAttribute,
        TooltipBoxAttribute,
        UnfoldAttribute,
        UnitAttribute,
        UnwrapAttribute,
        URLAttribute,
        ValidateAttribute,
        //Types
        Array2D,
        DynamicSlider,
        FilePath,
        FolderPath,
        LineGraph,
        MessageDrawer,
        ReorderableDictionary,
        SerializableDateTime,
        SerializableDictionary,
        SerializableSortedDictionary,
        SerializableSet,
        SerializableSortedSet,
        SerializableTuple,
        StaticsDrawer,
        //Unity
        DelayedAttribute,
        HeaderAttribute,
        HideInInspectorAttribute,
        MinAttribute,
        MultilineAttribute,
        NonReorderableAttribute,
        NonSerializedAttribute,
        RangeAttribute,
        SpaceAttribute,
        TooltipAttribute,
        TextAreaAttribute,
    }

    /// <summary>
    /// Groups propertys and defines custom order
    /// </summary>
    public static class PropertyDList
    {
        /// <summary>
        /// Spacing before each header
        /// </summary>
        public const float headerSpacing = 20;
        /// <summary>
        /// Height of header label
        /// </summary>
        public const float headerHeight = 20;
        /// <summary>
        /// The y-position distance addition a header does
        /// </summary>
        public const float headerDistance = headerSpacing + headerHeight;
        /// <summary>
        /// Spacing between entrys
        /// </summary>
        public const float entrySpacing = 7;
        /// <summary>
        /// Height of a single entry
        /// </summary>
        public const float entryHeight = 40;
        /// <summary>
        /// The y-position distance between two entrys
        /// </summary>
        public const float entryDistance = entrySpacing + entryHeight;
        

        //------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The sections of NewPropertyD
        /// </summary>
        public static readonly (string header, List<NewPropertyD> entrys)[] Sections;
        /// <summary>
        /// The height of all headers and entrys and their spacing
        /// </summary>
        public static readonly float TotalHeight;
        /// <summary>
        /// The height of the whole section (header plus entrys)
        /// </summary>
        public static readonly float MostUsedHeight;

        /// <summary>
        /// Constructor
        /// </summary>
        static PropertyDList()
        {
            Sections = new (string header, List<NewPropertyD> entrys)[]
            {
                ("Most used:", mostUsed),
                ("Attributes:", attributes),
                ("Types:", types),
                ("unitys field-attributes:", unityBuildIn)
            };

            TotalHeight = Sections.Length * headerDistance
                + Sections.Select(_ => _.entrys.Count).Sum() * entryDistance;

            MostUsedHeight = headerDistance + mostUsed.Count * entryDistance;
        }

        static readonly List<NewPropertyD> mostUsed = new()
        {
            NewPropertyD.ButtonAttribute,
            NewPropertyD.ForceFillAttribute,
            NewPropertyD.HideFieldAttribute,
            NewPropertyD.HorizontalLineAttribute,
            NewPropertyD.ReadOnlyAttribute,
            NewPropertyD.SelfFillAttribute,
            NewPropertyD.ShowIfAttribute,
        };
        static readonly List<NewPropertyD> attributes = new()
        {
            NewPropertyD.AsButtonAttribute,
            NewPropertyD.AsRangeAttribute,
            NewPropertyD.AssetsOnlyAttribute,
            NewPropertyD.BackgroundColorAttribute,
            NewPropertyD.ButtonAttribute,
            NewPropertyD.CopyPasteAttribute,
            NewPropertyD.DecimalsAttribute,
            NewPropertyD.Delayed2Attribute,
            NewPropertyD.DisplayAutoPropertyAttribute,
            NewPropertyD.FixedValuesAttribute,
            NewPropertyD.FoldoutAttribute,
            NewPropertyD.ForceFillAttribute,
            NewPropertyD.GetSetAttribute,
            NewPropertyD.GUIColorAttribute,
            NewPropertyD.HideFieldAttribute,
            NewPropertyD.HookAttribute,
            NewPropertyD.HorizontalGroupAttribute,
            NewPropertyD.HorizontalLineAttribute,
            NewPropertyD.InspectorIconAttribute,
            NewPropertyD.IndentAttribute,
            NewPropertyD.LabelSettingsAttribute,
            NewPropertyD.LayerAttribute,
            NewPropertyD.MaskAttribute,
            NewPropertyD.MaxAttribute,
            NewPropertyD.MessageBoxAttribute,
            NewPropertyD.Min2Attribute,
            NewPropertyD.MultipleOfAttribute,
            NewPropertyD.PreviewAttribute,
            NewPropertyD.ProgressBarAttribute,
            NewPropertyD.ReadOnlyAttribute,
            NewPropertyD.RequireTypeAttribute,
            NewPropertyD.RichTextAttribute,
            NewPropertyD.SelfFillAttribute,
            NewPropertyD.ShowAssetReferenceAttribute,
            NewPropertyD.ShowIfAttribute,
            NewPropertyD.ShowIfIsAttribute,
            NewPropertyD.ShowIfIsNotAttribute,
            NewPropertyD.ShowIfNotAttribute,
            NewPropertyD.ShowMethodAttribute,
            NewPropertyD.ShowPropertyAttribute,
            NewPropertyD.Space2Attribute,
            NewPropertyD.TabAttribute,
            NewPropertyD.TagAttribute,
            NewPropertyD.TitleAttribute,
            NewPropertyD.ToolbarAttribute,
            NewPropertyD.TooltipBoxAttribute,
            NewPropertyD.UnfoldAttribute,
            NewPropertyD.UnitAttribute,
            NewPropertyD.UnwrapAttribute,
            NewPropertyD.URLAttribute,
            NewPropertyD.ValidateAttribute,
        };
        static readonly List<NewPropertyD> types = new List<NewPropertyD>()
        {
            NewPropertyD.Array2D,
            NewPropertyD.DynamicSlider,
            NewPropertyD.FilePath,
            NewPropertyD.FolderPath,
            NewPropertyD.LineGraph,
            NewPropertyD.MessageDrawer,
            NewPropertyD.ReorderableDictionary,
            NewPropertyD.SerializableDateTime,
            NewPropertyD.SerializableDictionary,
            NewPropertyD.SerializableSortedDictionary,
            NewPropertyD.SerializableSet,
            NewPropertyD.SerializableSortedSet,
            NewPropertyD.SerializableTuple,
            NewPropertyD.StaticsDrawer,
        };
        static readonly List<NewPropertyD> unityBuildIn = new List<NewPropertyD>()
        {
            NewPropertyD.DelayedAttribute,
            NewPropertyD.HeaderAttribute,
            NewPropertyD.HideInInspectorAttribute,
            NewPropertyD.MinAttribute,
            NewPropertyD.MultilineAttribute,
            NewPropertyD.NonReorderableAttribute,
            NewPropertyD.NonSerializedAttribute,
            NewPropertyD.RangeAttribute,
            NewPropertyD.SpaceAttribute,
            NewPropertyD.TooltipAttribute,
            NewPropertyD.TextAreaAttribute,
        };
    }
}