using UnityEngine;


namespace FIMSpace.AnimationTools
{
    public interface INameAndIndex
    {
        string GetName { get; }
        int GetIndex { get; }
        float GUIAlpha { get; }
    }

    public interface IADSettings
    {
        /// <summary> Used for identifying different variants when modifying the same AnimationClip multiple times </summary>
        string SetID { get; }
        /// <summary> Hashed SetID name for faster search </summary>
        int SetIDHash { get; }

        /// <summary> Settings for AnimationClip </summary>
        AnimationClip SettingsForClip { get; }

        /// <summary> Initializing new () IADSettings </summary>
        void OnConstructed(AnimationClip clip, int hash);
    }

}