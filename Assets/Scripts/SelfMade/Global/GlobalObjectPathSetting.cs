using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalObjectPathSetting", menuName = "Global/GlobalObjectPathSetting", order = 1)]
public class GlobalObjectPathSetting:ScriptableObject
{
    public FolderPath BulletEffect = new FolderPath();

    public FolderPath DataPath = new FolderPath();

    public FolderPath HealthBar = new FolderPath();

}
