using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    [System.Serializable]
    public class ADArmaturePose
    {
        public List<ADBoneKeyPose> BonesCoords = new List<ADBoneKeyPose>();


        internal void CopyWith(ADArmatureSetup armature)
        {
            BonesCoords.Clear();

            for (int i = 0; i < armature.BonesSetup.Count; i++)
            {
                if (armature.BonesSetup[i] == null) continue;
                if (armature.BonesSetup[i].TempTransform == null) continue;
                ADBoneKeyPose key = new ADBoneKeyPose(armature.BonesSetup[i]);
                if (armature.LatestAnimator) key.DefineDepth(armature.LatestAnimator.transform);
                BonesCoords.Add(key);
            }
        }


        internal void RestoreOn(ADArmatureSetup armature)
        {
            for (int i = 0; i < armature.BonesSetup.Count; i++)
            {
                var armatureBone = armature.BonesSetup[i];
                for (int b = 0; b < BonesCoords.Count; b++)
                {
                    var bone = BonesCoords[b];
                    if (bone.BoneName == armatureBone.BoneName)
                        if (bone.BoneDepth == armatureBone.Depth)
                        {
                            armatureBone.ApplyCoords(bone);
                        }
                }
            }
        }

    }
}