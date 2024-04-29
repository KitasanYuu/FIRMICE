using CustomInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalObjectPathSetting", menuName = "Global/GlobalObjectPathSetting", order = 1)]
public class GlobalObjectPathSetting:ScriptableObject
{
    [HorizontalLine("Data", 2, FixedColor.Gray)]
    [AssetPath]
    public FolderPath CsvDataPath = new FolderPath();

    [HorizontalLine("UI", 2, FixedColor.Gray)]
    [AssetPath]
    public FolderPath UI_HealthBar = new FolderPath();
    public FolderPath UI_Player = new FolderPath();

    [HorizontalLine("Audio", 2, FixedColor.Gray)]
    [AssetPath]
    public FolderPath UIAudioClip = new FolderPath();
    public FolderPath WeaponAudioClip = new FolderPath();

    [HorizontalLine("RenderUsage", 2, FixedColor.Gray)]
    [AssetPath]
    public FolderPath WeaponSnapShot = new FolderPath();
    public FolderPath RenderBox = new FolderPath();

    [HorizontalLine("Unclassified", 2, FixedColor.Gray)]
    [AssetPath]
    public FolderPath BulletEffect = new FolderPath();

}
