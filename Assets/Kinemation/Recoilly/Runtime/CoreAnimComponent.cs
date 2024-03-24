using System;
using UnityEngine;

namespace Kinemation.Recoilly.Runtime
{
    public class CoreAnimComponent : MonoBehaviour
    {
        [Header("Rig")] [Tooltip("Doesn't use Target and Hint")] [SerializeField]
        private DynamicBone masterDynamic;

        [SerializeField] private DynamicBone rightHand;
        [SerializeField] private DynamicBone leftHand;

        [Header("Misc")] [Tooltip("Used for mesh space calculations")] [SerializeField]
        private Transform rootBone;

        [SerializeField] private RecoilAnimation recoilAnimation;

        public Transform recoilPivot;
        [NonSerialized] public Vector3 handsOffset;
        public LocRot pointAimOffset;

        private float _layerAlpha;

        private void Awake()
        {
            recoilAnimation = GetComponent<RecoilAnimation>();
        }

        private void Retarget()
        {
            //Master is retargeted manually as it requires non-character bone

            if (recoilPivot != null)
            {
                masterDynamic.obj.transform.position = recoilPivot.position;
                masterDynamic.obj.transform.rotation = recoilPivot.rotation;
            }

            rightHand.Retarget();
            leftHand.Retarget();
        }

        private void ApplyProceduralLayer()
        {
            _layerAlpha = AnimToolkitLib.Glerp(_layerAlpha, recoilAnimation.isAiming ? 1f : 0f, 9f);

            Vector3 aimOffset = Vector3.Lerp(Vector3.zero, pointAimOffset.position, _layerAlpha);
            Quaternion handsRot = Quaternion.Slerp(Quaternion.identity, pointAimOffset.rotation,
                _layerAlpha);

            AnimToolkitLib.MoveInBoneSpace(masterDynamic.obj.transform, masterDynamic.obj.transform,
                recoilAnimation.OutLoc + aimOffset + handsOffset);

            AnimToolkitLib.RotateInBoneSpace(masterDynamic.obj.transform.rotation, masterDynamic.obj.transform,
                Quaternion.Euler(recoilAnimation.OutRot) * handsRot);
        }

        private void ApplyIK()
        {
            Transform lowerBone = rightHand.target.parent;

            AnimToolkitLib.SolveTwoBoneIK(lowerBone.parent, lowerBone, rightHand.target,
                rightHand.obj.transform, rightHand.hintTarget, 1f, 1f, 1f);

            lowerBone = leftHand.target.parent;

            AnimToolkitLib.SolveTwoBoneIK(lowerBone.parent, lowerBone, leftHand.target,
                leftHand.obj.transform, leftHand.hintTarget, 1f, 1f, 1f);
        }

        // If bone transform is the same - zero frame
        // Use cached data to prevent continuous translation/rotation

        public void LateUpdate()
        {
            Retarget();
            ApplyProceduralLayer();
            ApplyIK();
        }

        private void SetupIKBones(Transform head)
        {
            if (masterDynamic.obj == null)
            {
                var boneObject = head.transform.Find("MasterIK");

                if (boneObject != null)
                {
                    masterDynamic.obj = boneObject.gameObject;
                }
                else
                {
                    masterDynamic.obj = new GameObject("MasterIK");
                    masterDynamic.obj.transform.parent = head;
                    masterDynamic.obj.transform.localPosition = Vector3.zero;
                }
            }
            
            if (rightHand.obj == null)
            {
                var boneObject = masterDynamic.obj.transform.Find("RightHandIK");

                if (boneObject != null)
                {
                    rightHand.obj = boneObject.gameObject;
                }
                else
                {
                    rightHand.obj = new GameObject("RightHandIK");
                }

                rightHand.obj.transform.parent = masterDynamic.obj.transform;
                rightHand.obj.transform.localPosition = Vector3.zero;
            }
            
            if (leftHand.obj == null)
            {
                var boneObject = masterDynamic.obj.transform.Find("LeftHandIK");

                if (boneObject != null)
                {
                    leftHand.obj = boneObject.gameObject;
                }
                else
                {
                    leftHand.obj = new GameObject("LeftHandIK");
                }

                leftHand.obj.transform.parent = masterDynamic.obj.transform;
                leftHand.obj.transform.localPosition = Vector3.zero;
            }
        }

        public void SetupBones()
        {
            Animator animator = GetComponentInChildren<Animator>();
            
            if (rootBone == null)
            {
                var root = transform.Find("rootBone");

                if (root != null)
                {
                    rootBone = root.transform;
                }
                else
                {
                    var bone = new GameObject("rootBone");
                    bone.transform.parent = transform;
                    rootBone = bone.transform;
                    rootBone.localPosition = Vector3.zero;
                }
            }
            
            if (animator.isHuman)
            {
                rightHand.target = animator.GetBoneTransform(HumanBodyBones.RightHand);
                leftHand.target = animator.GetBoneTransform(HumanBodyBones.LeftHand);

                Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                SetupIKBones(head);
                return;
            }

            var meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogWarning("Core: Skinned Mesh Renderer not found!");
                return;
            }

            var children = meshRenderer.bones;

            bool foundRightHand = false;
            bool foundLeftHand = false;
            bool foundHead = false;

            foreach (var bone in children)
            {
                if (bone.name.ToLower().Contains("ik"))
                {
                    continue;
                }

                bool bMatches = bone.name.ToLower().Contains("hips") || bone.name.ToLower().Contains("pelvis");
                
                bMatches = bone.name.ToLower().Contains("lefthand") || bone.name.ToLower().Contains("hand_l")
                                                                    || bone.name.ToLower().Contains("l_hand")
                                                                    || bone.name.ToLower().Contains("hand l")
                                                                    || bone.name.ToLower().Contains("l hand")
                                                                    || bone.name.ToLower().Contains("l.hand")
                                                                    || bone.name.ToLower().Contains("hand.l")
                                                                    || bone.name.ToLower().Contains("hand_left")
                                                                    || bone.name.ToLower().Contains("left_hand");
                if (!foundLeftHand && bMatches)
                {
                    leftHand.target = bone;

                    if (leftHand.hintTarget == null)
                    {
                        leftHand.hintTarget = bone.parent;
                    }

                    foundLeftHand = true;
                    continue;
                }

                bMatches = bone.name.ToLower().Contains("righthand") || bone.name.ToLower().Contains("hand_r")
                                                                     || bone.name.ToLower().Contains("r_hand")
                                                                     || bone.name.ToLower().Contains("hand r")
                                                                     || bone.name.ToLower().Contains("r hand")
                                                                     || bone.name.ToLower().Contains("r.hand")
                                                                     || bone.name.ToLower().Contains("hand.r")
                                                                     || bone.name.ToLower().Contains("hand_right")
                                                                     || bone.name.ToLower().Contains("right_hand");
                if (!foundRightHand && bMatches)
                {
                    rightHand.target = bone;

                    if (rightHand.hintTarget == null)
                    {
                        rightHand.hintTarget = bone.parent;
                    }

                    foundRightHand = true;
                }
                
                if (!foundHead && bone.name.ToLower().Contains("head"))
                {
                    SetupIKBones(bone);
                    foundHead = true;
                }
            }

            bool bFound = foundRightHand && foundLeftHand && foundHead;

            Debug.Log(bFound ? "All bones are found!" : "Some bones are missing!");
        }
    }

    [Serializable]
    public struct DynamicBone
    {
        public Transform target;
        public Transform hintTarget;
        public GameObject obj;

        public void Retarget()
        {
            if (target == null)
            {
                return;
            }

            obj.transform.position = target.position;
            obj.transform.rotation = target.rotation;
        }
    }
}