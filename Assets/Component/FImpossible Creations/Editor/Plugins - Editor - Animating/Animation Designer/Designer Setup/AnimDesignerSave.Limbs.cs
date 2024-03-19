using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    /// <summary>
    /// Core Data file for the AnimationDesigner
    /// Containing:
    /// Armature (base skeleton setup)
    /// Limbs Setups (chains of transforms ids)
    /// Some Main Settings for all edited AnimationClips
    /// Separated Settings sets (elasticness, ik etc.) for each modified AnimationClip file
    /// And some additional data
    /// </summary>
    public partial class AnimationDesignerSave : ScriptableObject
    {

        #region Bones Utils

        public List<Transform> GetAllArmatureBonesList
        {
            get
            {
                List<Transform> allBones = new List<Transform>();
                for (int i = 0; i < Armature.BonesSetup.Count; i++)
                {
                    if (LatestAnimator) if (Armature.BonesSetup[i].TempTransform == null) Armature.BonesSetup[i].GatherTempTransform(LatestAnimator.transform);
                    if (Armature.BonesSetup[i].TempTransform == null) continue;
                    allBones.Add(Armature.BonesSetup[i].TempTransform);
                }

                return allBones;
            }
        }

        public List<Transform> GetHumanoidBonesList
        {
            get
            {
                List<Transform> humanoid = new List<Transform>();
                Animator a = LatestAnimator.GetAnimator();
                if (a == null) return humanoid;

                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                {
                    Transform t = a.GetBoneTransform((HumanBodyBones)i);
                    if (t) humanoid.Add(t);
                }

                return humanoid;
            }
        }

        public List<Transform> GetNonHumanoidBonesList
        {
            get
            {
                List<Transform> humanoid = GetHumanoidBonesList;

                List<Transform> nonHumanoid = new List<Transform>();
                for (int i = 0; i < Armature.BonesSetup.Count; i++) nonHumanoid.Add(Armature.BonesSetup[i].TempTransform);
                if (Armature != null) if (Armature.RootBoneReference != null) if (SkelRootBone) if (nonHumanoid.Contains(SkelRootBone) == false) nonHumanoid.Add(SkelRootBone);

                for (int i = 0; i < humanoid.Count; i++)
                {
                    if (humanoid[i] == null) continue;
                    if (nonHumanoid.Contains(humanoid[i]) == false) continue;
                    nonHumanoid.Remove(humanoid[i]);
                }

                return nonHumanoid;
            }
        }

        public List<Transform> GetAllAnimatorTransforms
        {
            get
            {
                List<Transform> allBones = new List<Transform>();
                for (int i = 0; i < Armature.BonesSetup.Count; i++)
                {
                    if (LatestAnimator) if (Armature.BonesSetup[i].TempTransform == null) Armature.BonesSetup[i].GatherTempTransform(LatestAnimator.transform);
                    if (Armature.BonesSetup[i].TempTransform == null) continue;
                    allBones.Add(Armature.BonesSetup[i].TempTransform);
                }

                if (Armature.RootBoneReference != null)
                    if (Armature.RootBoneReference.TempTransform)
                    {
                        var extras = Armature.RootBoneReference.TempTransform.GetComponentsInChildren<Transform>(true);
                        for (int i = 0; i < extras.Length; i++)
                        {
                            var t = extras[i];
                            if (allBones.Contains(t) == false) allBones.Add(t);
                        }
                    }

                return allBones;
            }
        }


        #endregion


        public List<ADArmatureLimb> Limbs = new List<ADArmatureLimb>();

        // For some reason, private list was serialized so I needed to add [NonSerialized] to make it work as it should o_O 
        [NonSerialized] private List<ADArmatureLimb> _AltExecutionOrderLimbs = new List<ADArmatureLimb>();

        public void ChangeLimbsOrder(ADArmatureLimb limb, bool moveRight)
        {
            int index = limb.Index;
            int targetIndex = index;
            if (moveRight) targetIndex += 1; else targetIndex -= 1;

            if (targetIndex >= Limbs.Count) targetIndex = 0; else if (targetIndex < 0) targetIndex = Limbs.Count - 1;

            ADArmatureLimb swapMemory = Limbs[targetIndex];
            Limbs[targetIndex] = limb;
            limb.Index = targetIndex;
            Limbs[index] = swapMemory;
            swapMemory.Index = index;
        }

        public void PrepareAutoLimbs(ADArmatureSetup armature)
        {
            if (Limbs.Count > 0) return;
            if (armature.LatestAnimator == null) return;
            Animator anim = armature.LatestAnimator.GetAnimator();
            if (!anim) return;
            if (anim.isHuman == false) return;

            ADArmatureLimb limb = new ADArmatureLimb();
            limb.LimbName = "Left Arm";
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.LeftShoulder));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.LeftUpperArm));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.LeftLowerArm));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.LeftHand));
            Limbs.Add(limb);
            limb.LimbType = ADArmatureLimb.ELimbType.Arm;

            limb.Bones[0].ElasticnessBlend = 0.5f;
            limb.Bones[1].ElasticnessBlend = 1f;
            limb.Bones[2].ElasticnessBlend = 0.75f;
            limb.Bones[3].ElasticnessBlend = 0.25f;

            limb = new ADArmatureLimb();
            limb.LimbName = "Right Arm";
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.RightShoulder));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.RightUpperArm));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.RightLowerArm));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.RightHand));
            Limbs.Add(limb);
            limb.LimbType = ADArmatureLimb.ELimbType.Arm;

            limb.Bones[0].ElasticnessBlend = 0.5f;
            limb.Bones[1].ElasticnessBlend = 1f;
            limb.Bones[2].ElasticnessBlend = 0.75f;
            limb.Bones[3].ElasticnessBlend = 0.25f;

            limb = new ADArmatureLimb();
            limb.LimbName = "Spine Chain";
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.Spine));
            if (anim.GetBoneTransform(HumanBodyBones.Chest)) limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.Chest));
            if (anim.GetBoneTransform(HumanBodyBones.UpperChest)) limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.UpperChest));
            if (anim.GetBoneTransform(HumanBodyBones.Neck)) limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.Neck));
            if (anim.GetBoneTransform(HumanBodyBones.Head)) limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.Head));
            Limbs.Add(limb);
            limb.LimbType = ADArmatureLimb.ELimbType.Spine;

            limb.Bones[0].ElasticnessBlend = 0.5f;
            if (limb.Bones.Count > 1) limb.Bones[1].ElasticnessBlend = 1f;
            if (limb.Bones.Count > 3) limb.Bones[limb.Bones.Count - 2].ElasticnessBlend = 0.65f;
            limb.Bones[limb.Bones.Count - 1].ElasticnessBlend = 0.2f;

            limb = new ADArmatureLimb();
            limb.LimbName = "Left Leg";
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.LeftUpperLeg));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.LeftLowerLeg));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.LeftFoot));
            Limbs.Add(limb);
            limb.LimbType = ADArmatureLimb.ELimbType.Leg;

            limb.Bones[limb.Bones.Count - 1].ElasticnessBlend = 0.2f;

            limb = new ADArmatureLimb();
            limb.LimbName = "Right Leg";
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.RightUpperLeg));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.RightLowerLeg));
            limb.Bones.Add(new ADBoneID(armature, HumanBodyBones.RightFoot));
            Limbs.Add(limb);
            limb.LimbType = ADArmatureLimb.ELimbType.Leg;

            limb.Bones[limb.Bones.Count - 1].ElasticnessBlend = 0.2f;
        }

        public void RefreshLimbsReferences(ADArmatureSetup armature)
        {
            for (int l = 0; l < Limbs.Count; l++)
            {
                Limbs[l].RefresTransformReferences(armature);
            }
        }

    }
}
