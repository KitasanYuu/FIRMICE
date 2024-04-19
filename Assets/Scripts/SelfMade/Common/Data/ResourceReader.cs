using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DataManager
{
    public class ResourceReader
    {
        public string GainPath(string ObjectType, string ObjectName)
        {
            GlobalObjectPathSetting GOPS = Resources.Load<GlobalObjectPathSetting>("GlobalSettings/GlobalObjectPathSetting");
            System.Reflection.FieldInfo field = GOPS.GetType().GetField(ObjectType);
            FolderPath folderPath = (FolderPath)field.GetValue(GOPS);
            string resourcesFolderPath = "Assets/Resources/";
            string FolderPath = folderPath.path;

            // 使用 Path.Combine() 来组合路径
            string FullPath = Path.Combine(FolderPath, ObjectName);
            FullPath = FullPath.Replace("\\", "/");
            // 为了得到相对路径，去除资源路径前缀
            string relativePath = FullPath.Replace(resourcesFolderPath, "");

            //Debug.Log(relativePath);
            return relativePath;
        }

        public static PersistenceDataEncryptionSettings GetEncryptionOption()
        {
            PersistenceDataEncryptionSettings PDES = Resources.Load<PersistenceDataEncryptionSettings>("GlobalSettings/PersistenceDataEncryptionSettings");
            if(PDES != null)
            {
                return PDES;
            }
            else
            {
                return null;
            }
        }

        public Color GetColor(string colorUsage)
        {
            // 加载GlobalColorSetting ScriptableObject
            GlobalColorSetting gcs = Resources.Load<GlobalColorSetting>("GlobalSettings/GlobalColorSetting");

            if (gcs == null)
            {
                //Debug.LogError("GlobalColorSetting not found.");
                return Color.white; // 如果找不到GCS，返回白色
            }

            // 使用反射根据colorUsage获取颜色
            FieldInfo colorField = typeof(GlobalColorSetting).GetField(colorUsage, BindingFlags.Public | BindingFlags.Instance);

            if (colorField != null && colorField.FieldType == typeof(Color))
            {
                //Debug.Log("Color field found or not a Color: " + colorUsage+ (Color)colorField.GetValue(gcs));
                return (Color)colorField.GetValue(gcs); // 如果找到字段且为Color类型，返回对应颜色
            }
            else
            {
                //Debug.LogError("Color field not found or not a Color: " + colorUsage);
                return Color.white; // 如果字段不存在或不是Color类型，返回白色
            }
        }

        public AudioClip GetUIAudioClip(string AudioName)
        {
            AudioClip returnclip = null;
            string ClipPath = GainPath("UIAudioClip", AudioName);

            returnclip = Resources.Load<AudioClip>(ClipPath);

            return returnclip;
        }

        public AudioClip GetWeaponAudioClip(string weaponName,string audioType)
        {
            AudioClip returnClip = null;

            string clipName = weaponName + "_" + audioType;
            string ObjectWeaponPath = GainPath("WeaponAudioClip", weaponName)+"/"+clipName;
            //Debug.Log(ObjectWeaponPath);

            returnClip = Resources.Load<AudioClip>(ObjectWeaponPath);

            //Debug.Log(returnClip);

            return returnClip;
        }

        public Sprite GetPlaceHolderSprite(string spriteName)
        {
            Sprite sprite = null;

            // 使用反射动态获取 DefaultSpritePlaceHolder 类型的实例
            DefaultSpritePlaceHolder dsp = Resources.Load<DefaultSpritePlaceHolder>("GlobalSettings/DefaultSpritePlaceHolder");

            if (dsp != null)
            {
                // 使用反射获取 ItemNotAvaible 字段的值
                FieldInfo fieldInfo = typeof(DefaultSpritePlaceHolder).GetField(spriteName, BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo != null && fieldInfo.FieldType == typeof(Sprite))
                {
                    sprite = (Sprite)fieldInfo.GetValue(dsp);
                }
                else
                {
                    Debug.LogError("Sprite not found in DefaultSpritePlaceHolder: " + spriteName);
                }
            }
            else
            {
                Debug.LogError("DefaultSpritePlaceHolder not found!");
            }

            return sprite;
        }

        public TextAsset GetCSVFile(string CSVName)
        {
            TextAsset csv = null;
            string CsvResourcePath = GainPath("DataPath", CSVName);
            csv = Resources.Load<TextAsset>(CsvResourcePath);
            return csv;
        }

        public GameObject GetGameObject(string ObjectType, string ObjectName)
        {
            GameObject ReturnGameObject = null;
            string ObjectResourcePath = GainPath(ObjectType, ObjectName);
            ReturnGameObject = Resources.Load<GameObject>(ObjectResourcePath);
            return ReturnGameObject;
        }

        public SubSelectIdentity GetPanelIDData(string PanelID, int page = 0)
        {
            UIPanelID uiPanelID = Resources.Load<UIPanelID>("GlobalSettings/UIPanelID");
            UIIdentity uiInfo = uiPanelID.UIID.FirstOrDefault(ui => ui.PanelID == PanelID);
            if (uiInfo != null)
            {
                return uiInfo.SubIdentity.FirstOrDefault(subs => subs.Page == page);
            }
            return null;
        }
    }
}