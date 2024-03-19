using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class ADArmatureBakeHelper
    {
        public ADArmatureSetup Armature;
        public Transform Root { get { return Armature.Root; } }
        public ADArmatureSetup Ar { get { return Armature; } }
        public Transform anim { get { return Ar.LatestAnimator; } }

        private HumanPose humanoidPose = new HumanPose();
        private HumanPoseHandler humanoidPoseHandler;

        public Vector3 bodyPosition { get; private set; }
        public Quaternion bodyRotation { get; private set; }
        public Quaternion? lastBodyRotation { get; private set; }

        public Vector3 initBodyPosition { get; private set; }
        public Vector3 initRootBonePosition { get; private set; }
        public Quaternion initRootBoneRotation { get; private set; }


        private float[] muscles = new float[0];
        private ADHumanoidMuscle[] muscleHelpers;
        //internal Vector3 bodyMovementOffset = Vector3.zero;

        public AnimationClip OriginalBakedClip { get; private set; }
        public bool Humanoid { get; private set; }
        public bool BakeRoot { get; private set; }
        /// <summary> Humanoid Animator Human Scale </summary>
        public float HumanScale { get; private set; }
        /// <summary> Scale used in few baking placements to correct humanoid rigs, for generic this value is = 1f </summary>
        public float BakeHumanScale { get; private set; }
        public bool OriginalClipWithRootMotionPos { get; private set; }
        public bool OriginalClipWithRootMotionRot { get; private set; }
        public bool OriginalClipWithAnyRootMotion { get; private set; }
        public ADClipSettings_Main BakeMain { get; private set; }
        public AnimationDesignerSave Save { get; private set; }

        public ADArmatureBakeHelper(ADArmatureSetup armature, AnimationClip originalClip, ADClipSettings_Main main, AnimationDesignerSave save)
        {
            Armature = armature;
            OriginalBakedClip = originalClip;
            HumanScale = 1f;
            BakeMain = main;
            Save = save;

            if (originalClip)
            {
                OriginalClipWithRootMotionPos = ADRootMotionBakeHelper.ClipContainsRootPositionCurves(originalClip);
                if (! OriginalClipWithRootMotionPos) OriginalClipWithRootMotionPos = ADRootMotionBakeHelper.ClipContainsRootPositionCurves(originalClip, "Root");
                OriginalClipWithRootMotionRot = ADRootMotionBakeHelper.ClipContainsRootRotationCurves(originalClip);
                if (!OriginalClipWithRootMotionRot) OriginalClipWithRootMotionRot = ADRootMotionBakeHelper.ClipContainsRootRotationCurves(originalClip, "Root");
                OriginalClipWithAnyRootMotion = OriginalClipWithRootMotionPos || OriginalClipWithRootMotionRot;
                if (main != null) if (main.Export_ForceRootMotion) OriginalClipWithAnyRootMotion = true;
            }
        }


        public void PrepareAndDefine()
        {
            BakeRoot = false;
            Humanoid = anim.IsHuman();

            if (AnimationDesignerWindow._forceExportGeneric) Humanoid = false;

            if (Humanoid)
            {
                BakeRoot = true;

                Animator an = anim.GetAnimator();

                if (an)
                {
                    HumanScale = an.humanScale;
                    BakeHumanScale = an.humanScale;

                    if (OriginalClipWithAnyRootMotion)
                    {
                        BakeHumanScale = 1f;
                    }
                }

                muscles = new float[HumanTrait.MuscleCount];

                muscleHelpers = new ADHumanoidMuscle[HumanTrait.MuscleCount];
                for (int i = 0; i < muscleHelpers.Length; i++) muscleHelpers[i] = new ADHumanoidMuscle(i);

                Transform rootBone = anim;
                if (Ar.UseRootBoneForAvatar)
                {
                    rootBone = Ar.RootBoneReference.TempTransform;
                }

                Avatar av = anim.GetAvatar();

                humanoidPoseHandler = new HumanPoseHandler(av, rootBone);
                initRootBoneRotation = Armature.Root.rotation;
            }
            else
            {
                BakeHumanScale = 1f;

                //if (OriginalBakedClip.hasRootCurves)
                {
                    BakeRoot = true;
                }

                //initRootBoneRotation = FEngineering.QToLocal(Root.parent.rotation, Armature.Root.rotation);
                initRootBoneRotation = Quaternion.FromToRotation(Armature.Root.InverseTransformDirection(anim.right), Vector3.right);
                initRootBoneRotation *= Quaternion.FromToRotation(Armature.Root.InverseTransformDirection(anim.up), Vector3.up);
            }

            lastBodyRotation = null;
            initBodyPosition = bodyPosition;
            //bodyMovementOffset = Vector3.zero;
            //lastBodyPos = null;
        }

        //Vector3? lastBodyPos = null;
        public void UpdateHumanoidBodyPose()
        {
            initRootBonePosition = Armature.Root.position;
            humanoidPoseHandler.GetHumanPose(ref humanoidPose);

            //if (lastBodyPos != null) bodyMovementOffset = humanoidPose.bodyPosition - initBodyPosition;

            //lastBodyPos = humanoidPose.bodyPosition;
            bodyPosition = humanoidPose.bodyPosition;
            bodyRotation = humanoidPose.bodyRotation;

            for (int i = 0; i < humanoidPose.muscles.Length; i++)
            {
                muscles[i] = humanoidPose.muscles[i];
            }
        }

        public void CaptureArmaturePoseFrame(float elapsed)
        {

            if (Humanoid)
            {
                UpdateHumanoidBodyPose();
                for (int i = 0; i < muscleHelpers.Length; i++) muscleHelpers[i].SetKeyframe(elapsed, muscles);
            }
            else
            {
                bodyPosition = Root.position - initRootBonePosition;
                //bodyRotation = Quaternion.Inverse(Root.rotation) * (initRootBoneRotation);
                //bodyRotation = FEngineering.QToLocal(anim.rotation, bodyRotation);
                //Quaternion diff = FEngineering.QToLocal(Root.parent.rotation, Root.rotation);
                //bodyRotation = Quaternion.Inverse(diff) * (initRootBoneRotation);
                bodyRotation = (Root.rotation) * Quaternion.Inverse(initRootBoneRotation);
                //bodyRotation = FEngineering.QToLocal(anim.rotation,Root.rotation) * (initRootBoneRotation);
                //UnityEngine.Debug.Log("initrot = " + initRootBoneRotation.eulerAngles + " vs curr " + Root.eulerAngles + " rootbodyrot = " + bodyRotation.eulerAngles);
            }

            if (lastBodyRotation != null) AnimationGenerateUtils.EnsureQuaternionContinuity(lastBodyRotation.Value, bodyRotation);
            lastBodyRotation = bodyRotation;

        }

        public void SaveHumanoidCurves(ref AnimationClip clip, float reduction, float legsReductionMul)
        {
            if (Humanoid)
            {
                for (int i = 0; i < muscleHelpers.Length; i++)
                {
                    muscleHelpers[i].SaveCurves(BakeMain.Export_WrapLoopBakeMode, ref clip, reduction, legsReductionMul);
                }
            }
        }


    }
}