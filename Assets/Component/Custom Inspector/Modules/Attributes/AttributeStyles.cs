using System;
using UnityEngine;

namespace CustomInspector
{
    public enum Size
    {
        small,
        medium,
        big,
        max,
    }
    public enum FixedColor
    {
        BabyBlue,
        Black,
        Blue,
        CherryRed,
        CloudWhite,
        Cyan,
        DarkGray,
        DustyBlue,
        Gray,
        Green,
        IceWhite,
        Magenta,
        Orange,
        PressedBlue,
        Purple,
        Red,
        Yellow,
    }
    public enum LabelStyle
    {
        /// <summary>
        /// No label. draws the value over full width
        /// </summary>
        NoLabel,
        /// <summary>
        /// No label, but draws the value only in the value Area
        /// </summary>
        EmptyLabel,
        /// <summary>
        /// Makes the label as small as the label text
        /// </summary>
        NoSpacing,
        /// <summary>
        /// The common: label has default labelwith and value field starts in next column
        /// </summary>
        FullSpacing,
    }

    public enum DisabledStyle
    {
        /// <summary> greys out and forbids editing </summary>
        GreyedOut,
        /// <summary> hides the field completely </summary>
        Invisible
    }

    public enum InspectorIcon
    {
        AddFolder,
        AreaLight,
        Arrow_left,
        Arrow_right,
        AssetLabel,
        Camera,
        Error,
        Warning,
        Wrench,
        Help,
        Menu,
        Settings,
        Account,
        Lock,
        Lightbaking,
        Lightbaking_Off,
        Server_connected,
        Server_disabled,
        Server_disconnected,
        Cross,
        Cloud,
        Picker,
        Info_console,
        Info_Inactive,
        Script,
        CustomSorting,
        Debug,
        DebuggerAttached,
        DebuggerDisabled,
        DebuggerEnabled,
        DefaultAsset,
        Edit_collision,
        Edit_constraints,
        Exposure,
        Favorite,
        Filter_by_label,
        Filter_by_type,
        Folder,
        Folder_favorite,
        Folder_opened,
        Frame_capture,
        GameObject,
        Gizmos,
        GridAndSnap,
        GridAxisY,
        GridAxisY_On,
        Lighting,
        Linked,
        Unlinked,
        Material,
        Mesh,
        More,
        MoreOptions,
        Move,
        Minus,
        Minus_filled,
        Minus_act,
        Plus,
        Plus_filled,
        Plus_act,
        Orientation,
        PackageManager,
        Pause,
        Pick,
        Play,
        Loop,
        Prefab,
        Context,
        Ticked,
        Rect,
        Rotate,
        Scale,
        Scene,
        pickable,
        not_pickable,
        pickable_mixed,
        not_pickable_mixed,
        _2D,
        Sound,
        Sound_off,
        Sound_muted,
        SceneCamera,
        SceneView,
        Lighting_Off,
        Snap,
        ScriptableObject,
        Search,
        Search_menu,
        Search_jump,
        Shaded,
        ShadedWireframe,
        Shortcut,
        SnapIncrement,
        Step,
        TextAsset,
        Center,
        Global,
        Local,
        Pivot,
        ToolSettings,
        Transform,
        Undo,
        Console,
        Game,
        Info,
        Hierarchy,
        ViewOptions,
        View_Move,
        Eye,
        Zoom,
        Loading,
        Wireframe,
        Light,
        Download,
        Lock_opened,
        Ticked_green,
        Lens_flare,
        Light_probe,
        Package,
        Particle_system,
        Force_field,
        PointLight,
        Reflection_Probe,
        Refresh,
        SpotLight,
        Unity,
        Update,
        Visual_effect,
        WindZone
    }
}
namespace CustomInspector.Extensions
{
    public static class StylesConvert
    {
        public static Color ToColor(this FixedColor c)
        {
            return c switch
            {
                FixedColor.CloudWhite => new Color(.93f, .93f, .93f, 1),
                FixedColor.IceWhite => Color.white,
                FixedColor.Black => Color.black,
                FixedColor.Gray => Color.gray,
                FixedColor.DarkGray => new Color(.1f, .1f, .1f, 1),
                FixedColor.Blue => Color.blue,
                FixedColor.PressedBlue => new Color(.27f, .38f, .49f) * 2.5f,
                FixedColor.BabyBlue => new Color(.73f, .89f, .96f, 1),
                FixedColor.DustyBlue => new Color(.31f, .4f, .5f, 1),
                FixedColor.Purple => new Color(.44f, .13f, .51f, 1),
                FixedColor.Red => Color.red,
                FixedColor.CherryRed => new Color(.8f, 0, .1f, 1),
                FixedColor.Orange => new Color(.95f, .55f, .09f, 1),
                FixedColor.Cyan => Color.cyan,
                FixedColor.Green => Color.green,
                FixedColor.Magenta => Color.magenta,
                FixedColor.Yellow => Color.yellow,
                _ => throw new System.NotImplementedException($"{c} currently not supported")
            };
        }
        public static InternalLabelStyle ToInteralStyle(this LabelStyle labelStyle)
        {
            return labelStyle switch
            {
                LabelStyle.NoLabel => InternalLabelStyle.NoLabel,
                LabelStyle.EmptyLabel => InternalLabelStyle.EmptyLabel,
                LabelStyle.NoSpacing => InternalLabelStyle.NoSpacing,
                LabelStyle.FullSpacing => InternalLabelStyle.FullSpacing,
                _ => throw new NotImplementedException($"{labelStyle} not found"),
            };
        }
        public static string ToInternalIconName(this InspectorIcon icon)
            => ToInternalIconName(icon.ToString());
        public static string ToInternalIconName(string iconName)
        {
            return iconName switch
            {
                nameof(InspectorIcon.AddFolder) => "Add-Available",
                nameof(InspectorIcon.AreaLight) => "AreaLight Gizmo",
                nameof(InspectorIcon.Arrow_left) => "ArrowNavigationLeft",
                nameof(InspectorIcon.Arrow_right) => "ArrowNavigationRight",
                nameof(InspectorIcon.AssetLabel) => "AssetLabelIcon",
                nameof(InspectorIcon.Camera) => "Camera Gizmo",
                nameof(InspectorIcon.Error) => "console.erroricon.sml",
                nameof(InspectorIcon.Warning) => "console.warnicon.sml",
                nameof(InspectorIcon.Wrench) => "Customized",
                nameof(InspectorIcon.Help) => "d__Help",
                nameof(InspectorIcon.Menu) => "d__Menu",
                nameof(InspectorIcon.Settings) => "d_Settings",
                nameof(InspectorIcon.Account) => "d_account",
                nameof(InspectorIcon.Lock) => "d_AssemblyLock",
                nameof(InspectorIcon.Lightbaking) => "d_AutoLightbakingOn",
                nameof(InspectorIcon.Lightbaking_Off) => "d_AutoLightbakingOff",
                nameof(InspectorIcon.Server_connected) => "d_CacheServerConnected",
                nameof(InspectorIcon.Server_disabled) => "d_CacheServerDisabled",
                nameof(InspectorIcon.Server_disconnected) => "d_CacheServerDisconnected",
                nameof(InspectorIcon.Cross) => "d_clear",
                nameof(InspectorIcon.Cloud) => "d_CloudConnect",
                nameof(InspectorIcon.Picker) => "d_color_picker",
                nameof(InspectorIcon.Script) => "d_cs Script Icon",
                nameof(InspectorIcon.CustomSorting) => "d_CustomSorting",
                nameof(InspectorIcon.Debug) => "d_debug",
                nameof(InspectorIcon.DebuggerAttached) => "d_DebuggerAttached",
                nameof(InspectorIcon.DebuggerDisabled) => "d_DebuggerDisabled",
                nameof(InspectorIcon.DebuggerEnabled) => "d_DebuggerEnabled",
                nameof(InspectorIcon.DefaultAsset) => "d_DefaultAsset Icon",
                nameof(InspectorIcon.Edit_collision) => "d_editcollision_16",
                nameof(InspectorIcon.Edit_constraints) => "d_editconstraints_16",
                nameof(InspectorIcon.Exposure) => "d_Exposure",
                nameof(InspectorIcon.Favorite) => "d_Favorite",
                nameof(InspectorIcon.Filter_by_label) => "d_FilterByLabel",
                nameof(InspectorIcon.Filter_by_type) => "d_FilterByType",
                nameof(InspectorIcon.Folder) => "d_Folder Icon",
                nameof(InspectorIcon.Folder_favorite) => "d_FolderFavorite Icon",
                nameof(InspectorIcon.Folder_opened) => "d_FolderOpened Icon",
                nameof(InspectorIcon.Frame_capture) => "d_FrameCapture",
                nameof(InspectorIcon.GameObject) => "d_GameObject Icon",
                nameof(InspectorIcon.Gizmos) => "d_GizmosToggle",
                nameof(InspectorIcon.GridAndSnap) => "d_GridAndSnap",
                nameof(InspectorIcon.GridAxisY) => "d_GridAxisY",
                nameof(InspectorIcon.GridAxisY_On) => "d_GridAxisY On",
                nameof(InspectorIcon.Lighting) => "d_Lighting",
                nameof(InspectorIcon.Linked) => "d_Linked",
                nameof(InspectorIcon.Unlinked) => "d_Unlinked",
                nameof(InspectorIcon.Material) => "d_Material Icon",
                nameof(InspectorIcon.Mesh) => "d_Mesh Icon",
                nameof(InspectorIcon.More) => "d_more",
                nameof(InspectorIcon.MoreOptions) => "d_MoreOptions",
                nameof(InspectorIcon.Move) => "d_MoveTool on",
                nameof(InspectorIcon.Minus) => "d_Toolbar Minus",
                nameof(InspectorIcon.Minus_filled) => "d_ol_minus",
                nameof(InspectorIcon.Minus_act) => "d_ol_minus_act",
                nameof(InspectorIcon.Plus) => "d_Toolbar Plus",
                nameof(InspectorIcon.Plus_filled) => "d_ol_plus",
                nameof(InspectorIcon.Plus_act) => "d_ol_plus_act",
                nameof(InspectorIcon.Orientation) => "d_OrientationGizmo",
                nameof(InspectorIcon.PackageManager) => "d_Package Manager",
                nameof(InspectorIcon.Pause) => "d_PauseButton On",
                nameof(InspectorIcon.Pick) => "d_pick",
                nameof(InspectorIcon.Play) => "d_PlayButton On",
                nameof(InspectorIcon.Loop) => "d_preAudioLoopOff",
                nameof(InspectorIcon.Prefab) => "d_Prefab Icon",
                nameof(InspectorIcon.Context) => "d_Preset.Context",
                nameof(InspectorIcon.Ticked) => "d_Progress",
                nameof(InspectorIcon.Rect) => "d_RectTool On",
                nameof(InspectorIcon.Rotate) => "d_RotateTool On",
                nameof(InspectorIcon.Scale) => "d_ScaleTool On",
                nameof(InspectorIcon.Scene) => "d_Scene",
                nameof(InspectorIcon.pickable) => "d_scenepicking_pickable_hover",
                nameof(InspectorIcon.not_pickable) => "d_scenepicking_notpickable_hover",
                nameof(InspectorIcon.pickable_mixed) => "d_scenepicking_pickable-mixed_hover",
                nameof(InspectorIcon.not_pickable_mixed) => "d_scenepicking_notpickable-mixed_hover",
                nameof(InspectorIcon._2D) => "d_SceneView2D On",
                nameof(InspectorIcon.Sound) => "d_SceneViewAudio On",
                nameof(InspectorIcon.Sound_off) => "AudioSource Gizmo",
                nameof(InspectorIcon.Sound_muted) => "d_SceneViewAudio",
                nameof(InspectorIcon.SceneView) => "d_SceneViewCamera",
                nameof(InspectorIcon.Lighting_Off) => "d_SceneViewLighting",
                nameof(InspectorIcon.Snap) => "d_SceneViewSnap",
                nameof(InspectorIcon.ScriptableObject) => "d_ScriptableObject Icon",
                nameof(InspectorIcon.Search) => "d_search_icon",
                nameof(InspectorIcon.Search_menu) => "d_search_menu",
                nameof(InspectorIcon.Search_jump) => "d_SearchJump Icon",
                nameof(InspectorIcon.Shaded) => "d_Shaded",
                nameof(InspectorIcon.ShadedWireframe) => "d_ShadedWireframe",
                nameof(InspectorIcon.Shortcut) => "d_Shortcut Icon",
                nameof(InspectorIcon.SnapIncrement) => "d_SnapIncrement",
                nameof(InspectorIcon.Step) => "d_StepButton",
                nameof(InspectorIcon.TextAsset) => "d_TextAsset Icon",
                nameof(InspectorIcon.Center) => "d_ToolHandleCenter",
                nameof(InspectorIcon.Global) => "d_ToolHandleGlobal",
                nameof(InspectorIcon.Local) => "d_ToolHandleLocal",
                nameof(InspectorIcon.Pivot) => "d_ToolHandlePivot",
                nameof(InspectorIcon.ToolSettings) => "d_ToolSettings",
                nameof(InspectorIcon.Transform) => "d_Transform Icon",
                nameof(InspectorIcon.Undo) => "d_UndoHistory",
                nameof(InspectorIcon.Console) => "d_UnityEditor.ConsoleWindow",
                nameof(InspectorIcon.Game) => "d_UnityEditor.GameView",
                nameof(InspectorIcon.Info) => "d_UnityEditor.InspectorWindow",
                nameof(InspectorIcon.Hierarchy) => "d_UnityEditor.SceneHierarchyWindow",
                nameof(InspectorIcon.ViewOptions) => "d_ViewOptions",
                nameof(InspectorIcon.View_Move) => "d_ViewToolMove On",
                nameof(InspectorIcon.Eye) => "d_scenevis_visible_hover",
                nameof(InspectorIcon.Zoom) => "d_ViewToolZoom On",
                nameof(InspectorIcon.Loading) => "d_WaitSpin02",
                nameof(InspectorIcon.Wireframe) => "d_wireframe",
                nameof(InspectorIcon.Light) => "DiscLight Gizmo",
                nameof(InspectorIcon.Download) => "Download-Available",
                nameof(InspectorIcon.Ticked_green) => "Installed",
                nameof(InspectorIcon.Lens_flare) => "LensFlare Gizmo",
                nameof(InspectorIcon.Light_probe) => "LightProbeGroup Gizmo",
                nameof(InspectorIcon.Package) => "package_installed",
                nameof(InspectorIcon.Particle_system) => "ParticleSystem Gizmo",
                nameof(InspectorIcon.Force_field) => "ParticleSystemForceField Gizmo",
                nameof(InspectorIcon.PointLight) => "PointLight Gizmo",
                nameof(InspectorIcon.Reflection_Probe) => "ReflectionProbe Gizmo",
                nameof(InspectorIcon.Refresh) => "Refresh",
                nameof(InspectorIcon.SpotLight) => "SpotLight Gizmo",
                nameof(InspectorIcon.Unity) => "d_Scene",
                nameof(InspectorIcon.Update) => "Update-Available",
                nameof(InspectorIcon.Visual_effect) => "VisualEffect Gizmo",
                nameof(InspectorIcon.WindZone) => "WindZone Gizmo",
                _ => iconName,
            };
        }

        public static float ToButtonHeight(Size size)
        {
            return size switch
            {
                Size.small => 17,
                Size.medium => 30,
                Size.big => 40,
                Size.max => 60,

                _ => throw new System.NotImplementedException(size.ToString()),
            };
        }
        public static float ToButtonWidth(float availableWidth, GUIContent buttonLabel, Size size)
        {
            return Mathf.Min(availableWidth, Math.Max(GUI.skin.label.CalcSize(buttonLabel).x + 20, size switch
            {
                Size.small => 100,
                Size.medium => 50 + availableWidth / 4,
                Size.big => 50 + availableWidth / 2,
                Size.max => float.MaxValue,
                _ => throw new System.NotImplementedException(size.ToString()),
            }));
        }
    }
}