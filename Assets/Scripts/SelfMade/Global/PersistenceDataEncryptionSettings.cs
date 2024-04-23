using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersistenceDataEncryptionSettings", menuName = "Global/PersistenceDataEncryptionSettings", order = 1)]
public class PersistenceDataEncryptionSettings : ScriptableObject
{
    public string AesKey;
    public bool UsingAes;
    [ShowIf(nameof(UsingAes))] public string EncryptionMark;
    public bool DataNameEncryption;
}
