using FIMSpace.FEditor;
using FIMSpace.FTools;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    [System.Serializable]
    /// <summary> Basic data to gather correct transform out of armature by using simple string / id </summary>
    public partial class ADBoneID
    {
        [NonSerialized] Transform GatheredTransform;
        public Transform T { get { return GatheredTransform; } }
        public Vector3 TrVec(Vector3 p) { return T.TransformVector(p); }
        public Vector3 TrPoint(Vector3 p) { return T.TransformPoint(p); }
        public Transform ChildT { get { if (GatheredTransform == null) return null; if (GatheredTransform.childCount == 0) return null; return GatheredTransform.GetChild(0); } }
        public Vector3 pos { get { return T.position; } }
        public Quaternion rot { get { return T.rotation; } }

        [NonSerialized] public Quaternion InitLocalRot = Quaternion.identity;
        [NonSerialized] public Vector3 InitLocalPos = Vector3.zero;

        public string BoneName;
        public string BonePath;
        public HumanBodyBones BodyBoneId = HumanBodyBones.LastBone;
        public float AnimationBlend = 1f;

        public float ElasticnessBlend = 1f;
        public float LimbEffectsBlend = 1f;

        public Vector3 ForwardInRoot { get; private set; }
        public Vector3 UpInRoot { get; private set; }
        public Vector3 RightInRoot { get; private set; }


        public ADBoneID(string name, string bonePath = "", float effBlend = 1f)
        {
            BoneName = name;
            BonePath = bonePath;
            AnimationBlend = 1f;
            ElasticnessBlend = effBlend;
            LimbEffectsBlend = effBlend;
        }

        public void AssignTransform(Transform t, Transform skelRoot = null)
        {
            if (t == null) return;
            GatheredTransform = t;
            BoneName = t.name;
            if (skelRoot != null) BonePath = GetBonePath(skelRoot);
            CalculateAxis(skelRoot);
        }

        public void ClearTransform()
        {
            GatheredTransform = null;
            BoneName = "";
            BonePath = "";
        }

        public ADBoneID(Transform t, Transform skelRoot = null, float effBlend = 1f)
        {
            if (t == null)
            {
                UnityEngine.Debug.Log("[Animation Designer] Tried to create Bone ID out of null Transform!");
                return;
            }

            GatheredTransform = t;
            BoneName = t.name;
            BonePath = GetBonePath(skelRoot);
            LimbEffectsBlend = effBlend;

            CalculateAxis(skelRoot);
        }

        public ADBoneID(ADArmatureSetup armature, HumanBodyBones id)
        {
            BoneName = id.ToString();
            BodyBoneId = id;
            RefreshTransformReference(armature, false);

            if (id == HumanBodyBones.Spine) LimbEffectsBlend = 0.5f;
            if (id == HumanBodyBones.Chest) LimbEffectsBlend = 0.75f;
            if (id == HumanBodyBones.UpperChest) LimbEffectsBlend = 0.4f;
            if (id == HumanBodyBones.Neck) LimbEffectsBlend = 0.15f;
            if (id == HumanBodyBones.Head) LimbEffectsBlend = 0.1f;

            if (id == HumanBodyBones.LeftShoulder) LimbEffectsBlend = 0.4f;
            if (id == HumanBodyBones.LeftUpperArm) LimbEffectsBlend = 0.7f;
            if (id == HumanBodyBones.LeftLowerArm) LimbEffectsBlend = 0.5f;
            if (id == HumanBodyBones.LeftHand) LimbEffectsBlend = 0.2f;

            if (id == HumanBodyBones.RightShoulder) LimbEffectsBlend = 0.4f;
            if (id == HumanBodyBones.RightUpperArm) LimbEffectsBlend = 0.7f;
            if (id == HumanBodyBones.RightLowerArm) LimbEffectsBlend = 0.5f;
            if (id == HumanBodyBones.RightHand) LimbEffectsBlend = 0.2f;

            if (id == HumanBodyBones.LeftUpperLeg) LimbEffectsBlend = 0.7f;
            if (id == HumanBodyBones.LeftLowerLeg) LimbEffectsBlend = 0.5f;
            if (id == HumanBodyBones.LeftFoot) LimbEffectsBlend = 0.25f;

            if (id == HumanBodyBones.RightUpperLeg) LimbEffectsBlend = 0.7f;
            if (id == HumanBodyBones.RightLowerLeg) LimbEffectsBlend = 0.5f;
            if (id == HumanBodyBones.RightFoot) LimbEffectsBlend = 0.25f;
        }


        public string GetBonePath(Transform rootTransform)
        {
            if (GatheredTransform == null) return "";
            return AnimationUtility.CalculateTransformPath(GatheredTransform, rootTransform);
        }

        public void SaveInitialCoords()
        {
            if (GatheredTransform == null) return;
            InitLocalPos = GatheredTransform.localPosition;
            InitLocalRot = GatheredTransform.localRotation;
        }


        #region Gizmos Related

        internal void DrawBoxGizmo(float size)
        {
            if (T == null) return;
            Handles.CubeHandleCap(0, T.position, T.rotation, size, EventType.Repaint);
        }

        internal void DrawBoneGizmo(Transform child, float boneFatness)
        {
            if (child == null) return;
            if (T == null) return;
            FGUI_Handles.DrawBoneHandle(T.position, child.position, boneFatness, true);
        }

        #endregion

        public void RefreshTransformReference(ADArmatureSetup armature, bool forceIfNull = true)
        {
            if (armature == null)
            {
                UnityEngine.Debug.Log("[Animation Designer] No Armature!");
                return;
            }

            if (armature.LatestAnimator == null)
            {
                UnityEngine.Debug.Log("[Animation Designer] No Armature Animator!");
                return;
            }

            if (BodyBoneId != HumanBodyBones.LastBone) // Humanoid skeleton rig bone
            {
                if (GatheredTransform == null)
                {
                    GatheredTransform = armature.LatestAnimator.GetAnimator().GetBoneTransform(BodyBoneId);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(BonePath) == false)
                {
                    if (GatheredTransform == null)
                        GatheredTransform = armature.LatestAnimator.transform.Find(BonePath);
                }

                if (forceIfNull)
                    if (GatheredTransform == null)
                    {
                        GatheredTransform = FTransformMethods.FindChildByNameInDepth(BoneName, armature.LatestAnimator.transform);
                    }
            }

            CalculateAxis(armature.LatestAnimator);
        }

        public void CalculateAxis(Transform root)
        {
            if (T == null) return;
            if (root == null) return;
            RightInRoot = T.InverseTransformDirection(root.right);
            UpInRoot = T.InverseTransformDirection(root.up);
            ForwardInRoot = T.InverseTransformDirection(root.forward);
        }
    }
}