using UnityEditor;
using UnityEngine;
namespace FIMSpace.AnimationTools.CustomModules
{
    public abstract class ADHumanoidMuclesModuleBase : ADCustomModuleBase
    {
        public override bool SupportBlending { get { return true; } }

        protected HumanPoseHandler humanoid = null;
        protected float[] muscles = null;

        protected bool InheritElasticness = true;
        protected Animator LastHumanoidAnimator { get; private set; }

        public override void OnInheritElasticnessUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            base.OnInheritElasticnessUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);

            //if (anim_MainSet.Additional_UseHumanoidMecanimIK == false) return;
            if (s.TargetAvatar == null) return;
            if (InheritElasticness) ApplyHumanoidProcess(animationProgress, deltaTime, S, anim_MainSet, customModules, set);
        }

        public override void OnLateUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            base.OnLateUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);

            //if (anim_MainSet.Additional_UseHumanoidMecanimIK == false) return;
            if (s.TargetAvatar == null) return;
            if (!InheritElasticness) ApplyHumanoidProcess(animationProgress, deltaTime, S, anim_MainSet, customModules, set);
        }


        void ApplyHumanoidProcess(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            LastHumanoidAnimator = null;
            Transform t = s.LatestAnimator;
            if (t == null) return;
            if (s.ReferencePelvis == null) return;

            Animator mecanim = s.LatestAnimator.GetAnimator();
            if (mecanim == null) return;
            if (mecanim.isHuman == false) return;

            LastHumanoidAnimator = mecanim;

            //if (anim_MainSet.Additional_UseHumanoidMecanimIK == false) return;
            if (s.TargetAvatar == null) return;

            if (humanoid == null)
            {
                humanoid = new HumanPoseHandler(s.TargetAvatar, t);
                muscles = new float[HumanTrait.MuscleCount];
            }

            HumanPose pose = new HumanPose();
            humanoid.GetHumanPose(ref pose);

            //string mList = "";
            // Copy muscle parameters of the current pose
            for (int i = 0; i < pose.muscles.Length; i++)
            {
                #region Commented but can be helpful later
                //mList += "protected const int ";
                //string mName = HumanTrait.MuscleName[i];
                //mName = mName.Replace(" ", "");
                //mName = mName.Replace("-", "");
                //mList += "MID_" + mName + "=" + i + ";\n";

                //mList += "protected float ";
                //string mName = HumanTrait.MuscleName[i];
                //mName = mName.Replace(" ", "");
                //mName = mName.Replace("-", "");
                //mList += "M" + mName + " { get { return muscles[" + i + "]; } set { muscles[" + i + "] = value; } }\n";

                #endregion

                muscles[i] = pose.muscles[i];
            }

            HumanoidChanges(mecanim, animationProgress, deltaTime, s, anim_MainSet, customModules, set);

            pose.muscles = muscles;


            // Apply and prevent changing root
            Vector3 preHipsPos = s.ReferencePelvis.position;

            humanoid.SetHumanPose(ref pose);

            s.ReferencePelvis.position = preHipsPos;
        }


        /// <summary> Modify muscles[] values or use properties like MChestLeftRight or methods like MHandOpenClose() to modify animation </summary>
        public abstract void HumanoidChanges(Animator mecanim, float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set);



        #region Editor GUI Related Code


        protected bool GUI_CheckForHumanoidAnimator(AnimationDesignerSave s, ADClipSettings_Main anim_MainSet)
        {
            var mecanim = AnimationDesignerWindow.Get.GetMecanim;

            if (mecanim == null || mecanim.isHuman == false)
            {
                EditorGUILayout.HelpBox("Can't Find Humanoid Animator!", MessageType.Warning);
                return false;
            }

            if (s.TargetAvatar == null)
            {
                EditorGUILayout.HelpBox("Not found Avatar on the animator!", MessageType.Warning);
                return false;
            }

            //if ( anim_MainSet.Additional_UseHumanoidMecanimIK == false)
            //{
            //    EditorGUILayout.HelpBox("You need to enable Humanoid IK to use this module!\nIt's under Setup Tab -> Additional Settings", MessageType.Info);

            //    if (GUILayout.Button("Enable Humanoid IK for the current animation clip"))
            //    {
            //        anim_MainSet.Additional_UseHumanoidMecanimIK = true;
            //        s._SetDirty();
            //    }

            //    return false;
            //}

            return true;
        }


        #endregion


        #region Muscle Utilities

        public void MHandOpenClose(bool rightHand, float value, bool additive, float blend = 1f)
        {
            if (muscles.Length < 1) return;

            int startI = rightHand ? 75 : 55;
            int endI = rightHand ? 94 : 74;

            if (additive)
            {
                for (int i = startI; i <= endI; i++)
                {
                    // Ignore spreads [56,60,64,68,72]
                    if (i == startI + 1 || i == startI + 5 || i == startI + 9 || i == startI + 13 || i == startI + 17) continue;
                    muscles[i] += value * blend;
                }
            }
            else
            {
                for (int i = startI; i <= endI; i++)
                {
                    // Ignore spreads [56,60,64,68,72]
                    if (i == startI + 1 || i == startI + 5 || i == startI + 9 || i == startI + 13 || i == startI + 17) continue;
                    muscles[i] = Mathf.LerpUnclamped(muscles[i], value, blend);
                }
            }
        }

        public void MHandThumbOpenClose(bool rightHand, float value, bool additive, float blend = 1f)
        {
            if (muscles.Length < 1) return;

            int startI = rightHand ? 75 : 55;
            int endI = rightHand ? 78 : 58;

            if (additive)
            {
                for (int i = startI; i <= endI; i++)
                {
                    // Ignore spreads
                    if (i == startI + 1) continue;
                    muscles[i] += value * blend;
                }
            }
            else
            {
                for (int i = startI; i <= endI; i++)
                {
                    // Ignore spreads
                    if (i == startI + 1) continue;
                    muscles[i] = Mathf.LerpUnclamped(muscles[i], value, blend);
                }
            }
        }




        /// <summary>
        /// Useful for lower leg mods etc. (2 params)
        /// </summary>
        protected void ApplyMusclesV2(int startIdx, Vector2 factors, float blend, bool additive)
        {
            if (additive)
            {
                factors *= blend;
                muscles[startIdx] += factors.x;
                muscles[startIdx + 1] += factors.y;
            }
            else
            {
                muscles[startIdx] = Mathf.LerpUnclamped(muscles[startIdx], factors.x, blend);
                muscles[startIdx + 1] = Mathf.LerpUnclamped(muscles[startIdx + 1], factors.y, blend);
            }
        }

        protected void ApplyMusclesV2(int startIdx, float x, float y, float blend, bool additive)
        {
            ApplyMusclesV2(startIdx, new Vector2(x, y), blend, additive);
        }

        /// <summary>
        /// Useful for upper leg, foot mods etc. (3 params)
        /// </summary>
        protected void ApplyMusclesV3(int startIdx, Vector3 factors, float blend, bool additive)
        {
            if (additive)
            {
                factors *= blend;
                muscles[startIdx] += factors.x;
                muscles[startIdx + 1] += factors.y;
                muscles[startIdx + 2] += factors.z;
            }
            else
            {
                muscles[startIdx] = Mathf.LerpUnclamped(muscles[startIdx], factors.x, blend);
                muscles[startIdx + 1] = Mathf.LerpUnclamped(muscles[startIdx + 1], factors.y, blend);
                muscles[startIdx + 2] = Mathf.LerpUnclamped(muscles[startIdx + 2], factors.z, blend);
            }
        }


        protected void ApplyMusclesV3(int startIdx, float x, float y, float z, float blend, bool additive)
        {
            ApplyMusclesV3(startIdx, new Vector3(x, y, z), blend, additive);
        }

        protected float MSpineFrontBack { get { return muscles[0]; } set { muscles[0] = value; } }
        protected float MSpineLeftRight { get { return muscles[1]; } set { muscles[1] = value; } }
        protected float MSpineTwistLeftRight { get { return muscles[2]; } set { muscles[2] = value; } }
        protected float MChestFrontBack { get { return muscles[3]; } set { muscles[3] = value; } }
        protected float MChestLeftRight { get { return muscles[4]; } set { muscles[4] = value; } }
        protected float MChestTwistLeftRight { get { return muscles[5]; } set { muscles[5] = value; } }
        protected float MUpperChestFrontBack { get { return muscles[6]; } set { muscles[6] = value; } }
        protected float MUpperChestLeftRight { get { return muscles[7]; } set { muscles[7] = value; } }
        protected float MUpperChestTwistLeftRight { get { return muscles[8]; } set { muscles[8] = value; } }
        protected float MNeckNodDownUp { get { return muscles[9]; } set { muscles[9] = value; } }
        protected float MNeckTiltLeftRight { get { return muscles[10]; } set { muscles[10] = value; } }
        protected float MNeckTurnLeftRight { get { return muscles[11]; } set { muscles[11] = value; } }
        protected float MHeadNodDownUp { get { return muscles[12]; } set { muscles[12] = value; } }
        protected float MHeadTiltLeftRight { get { return muscles[13]; } set { muscles[13] = value; } }
        protected float MHeadTurnLeftRight { get { return muscles[14]; } set { muscles[14] = value; } }
        protected float MLeftEyeDownUp { get { return muscles[15]; } set { muscles[15] = value; } }
        protected float MLeftEyeInOut { get { return muscles[16]; } set { muscles[16] = value; } }
        protected float MRightEyeDownUp { get { return muscles[17]; } set { muscles[17] = value; } }
        protected float MRightEyeInOut { get { return muscles[18]; } set { muscles[18] = value; } }
        protected float MJawClose { get { return muscles[19]; } set { muscles[19] = value; } }
        protected float MJawLeftRight { get { return muscles[20]; } set { muscles[20] = value; } }
        protected float MLeftUpperLegFrontBack { get { return muscles[21]; } set { muscles[21] = value; } }
        protected float MLeftUpperLegInOut { get { return muscles[22]; } set { muscles[22] = value; } }
        protected float MLeftUpperLegTwistInOut { get { return muscles[23]; } set { muscles[23] = value; } }
        protected float MLeftLowerLegStretch { get { return muscles[24]; } set { muscles[24] = value; } }
        protected float MLeftLowerLegTwistInOut { get { return muscles[25]; } set { muscles[25] = value; } }
        protected float MLeftFootUpDown { get { return muscles[26]; } set { muscles[26] = value; } }
        protected float MLeftFootTwistInOut { get { return muscles[27]; } set { muscles[27] = value; } }
        protected float MLeftToesUpDown { get { return muscles[28]; } set { muscles[28] = value; } }
        protected float MRightUpperLegFrontBack { get { return muscles[29]; } set { muscles[29] = value; } }
        protected float MRightUpperLegInOut { get { return muscles[30]; } set { muscles[30] = value; } }
        protected float MRightUpperLegTwistInOut { get { return muscles[31]; } set { muscles[31] = value; } }
        protected float MRightLowerLegStretch { get { return muscles[32]; } set { muscles[32] = value; } }
        protected float MRightLowerLegTwistInOut { get { return muscles[33]; } set { muscles[33] = value; } }
        protected float MRightFootUpDown { get { return muscles[34]; } set { muscles[34] = value; } }
        protected float MRightFootTwistInOut { get { return muscles[35]; } set { muscles[35] = value; } }
        protected float MRightToesUpDown { get { return muscles[36]; } set { muscles[36] = value; } }
        protected float MLeftShoulderDownUp { get { return muscles[37]; } set { muscles[37] = value; } }
        protected float MLeftShoulderFrontBack { get { return muscles[38]; } set { muscles[38] = value; } }
        protected float MLeftArmDownUp { get { return muscles[39]; } set { muscles[39] = value; } }
        protected float MLeftArmFrontBack { get { return muscles[40]; } set { muscles[40] = value; } }
        protected float MLeftArmTwistInOut { get { return muscles[41]; } set { muscles[41] = value; } }
        protected float MLeftForearmStretch { get { return muscles[42]; } set { muscles[42] = value; } }
        protected float MLeftForearmTwistInOut { get { return muscles[43]; } set { muscles[43] = value; } }
        protected float MLeftHandDownUp { get { return muscles[44]; } set { muscles[44] = value; } }
        protected float MLeftHandInOut { get { return muscles[45]; } set { muscles[45] = value; } }
        protected float MRightShoulderDownUp { get { return muscles[46]; } set { muscles[46] = value; } }
        protected float MRightShoulderFrontBack { get { return muscles[47]; } set { muscles[47] = value; } }
        protected float MRightArmDownUp { get { return muscles[48]; } set { muscles[48] = value; } }
        protected float MRightArmFrontBack { get { return muscles[49]; } set { muscles[49] = value; } }
        protected float MRightArmTwistInOut { get { return muscles[50]; } set { muscles[50] = value; } }
        protected float MRightForearmStretch { get { return muscles[51]; } set { muscles[51] = value; } }
        protected float MRightForearmTwistInOut { get { return muscles[52]; } set { muscles[52] = value; } }
        protected float MRightHandDownUp { get { return muscles[53]; } set { muscles[53] = value; } }
        protected float MRightHandInOut { get { return muscles[54]; } set { muscles[54] = value; } }
        protected float MLeftThumb1Stretched { get { return muscles[55]; } set { muscles[55] = value; } }
        protected float MLeftThumbSpread { get { return muscles[56]; } set { muscles[56] = value; } }
        protected float MLeftThumb2Stretched { get { return muscles[57]; } set { muscles[57] = value; } }
        protected float MLeftThumb3Stretched { get { return muscles[58]; } set { muscles[58] = value; } }
        protected float MLeftIndex1Stretched { get { return muscles[59]; } set { muscles[59] = value; } }
        protected float MLeftIndexSpread { get { return muscles[60]; } set { muscles[60] = value; } }
        protected float MLeftIndex2Stretched { get { return muscles[61]; } set { muscles[61] = value; } }
        protected float MLeftIndex3Stretched { get { return muscles[62]; } set { muscles[62] = value; } }
        protected float MLeftMiddle1Stretched { get { return muscles[63]; } set { muscles[63] = value; } }
        protected float MLeftMiddleSpread { get { return muscles[64]; } set { muscles[64] = value; } }
        protected float MLeftMiddle2Stretched { get { return muscles[65]; } set { muscles[65] = value; } }
        protected float MLeftMiddle3Stretched { get { return muscles[66]; } set { muscles[66] = value; } }
        protected float MLeftRing1Stretched { get { return muscles[67]; } set { muscles[67] = value; } }
        protected float MLeftRingSpread { get { return muscles[68]; } set { muscles[68] = value; } }
        protected float MLeftRing2Stretched { get { return muscles[69]; } set { muscles[69] = value; } }
        protected float MLeftRing3Stretched { get { return muscles[70]; } set { muscles[70] = value; } }
        protected float MLeftLittle1Stretched { get { return muscles[71]; } set { muscles[71] = value; } }
        protected float MLeftLittleSpread { get { return muscles[72]; } set { muscles[72] = value; } }
        protected float MLeftLittle2Stretched { get { return muscles[73]; } set { muscles[73] = value; } }
        protected float MLeftLittle3Stretched { get { return muscles[74]; } set { muscles[74] = value; } }
        protected float MRightThumb1Stretched { get { return muscles[75]; } set { muscles[75] = value; } }
        protected float MRightThumbSpread { get { return muscles[76]; } set { muscles[76] = value; } }
        protected float MRightThumb2Stretched { get { return muscles[77]; } set { muscles[77] = value; } }
        protected float MRightThumb3Stretched { get { return muscles[78]; } set { muscles[78] = value; } }
        protected float MRightIndex1Stretched { get { return muscles[79]; } set { muscles[79] = value; } }
        protected float MRightIndexSpread { get { return muscles[80]; } set { muscles[80] = value; } }
        protected float MRightIndex2Stretched { get { return muscles[81]; } set { muscles[81] = value; } }
        protected float MRightIndex3Stretched { get { return muscles[82]; } set { muscles[82] = value; } }
        protected float MRightMiddle1Stretched { get { return muscles[83]; } set { muscles[83] = value; } }
        protected float MRightMiddleSpread { get { return muscles[84]; } set { muscles[84] = value; } }
        protected float MRightMiddle2Stretched { get { return muscles[85]; } set { muscles[85] = value; } }
        protected float MRightMiddle3Stretched { get { return muscles[86]; } set { muscles[86] = value; } }
        protected float MRightRing1Stretched { get { return muscles[87]; } set { muscles[87] = value; } }
        protected float MRightRingSpread { get { return muscles[88]; } set { muscles[88] = value; } }
        protected float MRightRing2Stretched { get { return muscles[89]; } set { muscles[89] = value; } }
        protected float MRightRing3Stretched { get { return muscles[90]; } set { muscles[90] = value; } }
        protected float MRightLittle1Stretched { get { return muscles[91]; } set { muscles[91] = value; } }
        protected float MRightLittleSpread { get { return muscles[92]; } set { muscles[92] = value; } }
        protected float MRightLittle2Stretched { get { return muscles[93]; } set { muscles[93] = value; } }
        protected float MRightLittle3Stretched { get { return muscles[94]; } set { muscles[94] = value; } }


        protected const int MID_SpineFrontBack = 0;
        protected const int MID_SpineLeftRight = 1;
        protected const int MID_SpineTwistLeftRight = 2;
        protected const int MID_ChestFrontBack = 3;
        protected const int MID_ChestLeftRight = 4;
        protected const int MID_ChestTwistLeftRight = 5;
        protected const int MID_UpperChestFrontBack = 6;
        protected const int MID_UpperChestLeftRight = 7;
        protected const int MID_UpperChestTwistLeftRight = 8;
        protected const int MID_NeckNodDownUp = 9;
        protected const int MID_NeckTiltLeftRight = 10;
        protected const int MID_NeckTurnLeftRight = 11;
        protected const int MID_HeadNodDownUp = 12;
        protected const int MID_HeadTiltLeftRight = 13;
        protected const int MID_HeadTurnLeftRight = 14;
        protected const int MID_LeftEyeDownUp = 15;
        protected const int MID_LeftEyeInOut = 16;
        protected const int MID_RightEyeDownUp = 17;
        protected const int MID_RightEyeInOut = 18;
        protected const int MID_JawClose = 19;
        protected const int MID_JawLeftRight = 20;
        protected const int MID_LeftUpperLegFrontBack = 21;
        protected const int MID_LeftUpperLegInOut = 22;
        protected const int MID_LeftUpperLegTwistInOut = 23;
        protected const int MID_LeftLowerLegStretch = 24;
        protected const int MID_LeftLowerLegTwistInOut = 25;
        protected const int MID_LeftFootUpDown = 26;
        protected const int MID_LeftFootTwistInOut = 27;
        protected const int MID_LeftToesUpDown = 28;
        protected const int MID_RightUpperLegFrontBack = 29;
        protected const int MID_RightUpperLegInOut = 30;
        protected const int MID_RightUpperLegTwistInOut = 31;
        protected const int MID_RightLowerLegStretch = 32;
        protected const int MID_RightLowerLegTwistInOut = 33;
        protected const int MID_RightFootUpDown = 34;
        protected const int MID_RightFootTwistInOut = 35;
        protected const int MID_RightToesUpDown = 36;
        protected const int MID_LeftShoulderDownUp = 37;
        protected const int MID_LeftShoulderFrontBack = 38;
        protected const int MID_LeftArmDownUp = 39;
        protected const int MID_LeftArmFrontBack = 40;
        protected const int MID_LeftArmTwistInOut = 41;
        protected const int MID_LeftForearmStretch = 42;
        protected const int MID_LeftForearmTwistInOut = 43;
        protected const int MID_LeftHandDownUp = 44;
        protected const int MID_LeftHandInOut = 45;
        protected const int MID_RightShoulderDownUp = 46;
        protected const int MID_RightShoulderFrontBack = 47;
        protected const int MID_RightArmDownUp = 48;
        protected const int MID_RightArmFrontBack = 49;
        protected const int MID_RightArmTwistInOut = 50;
        protected const int MID_RightForearmStretch = 51;
        protected const int MID_RightForearmTwistInOut = 52;
        protected const int MID_RightHandDownUp = 53;
        protected const int MID_RightHandInOut = 54;
        protected const int MID_LeftThumb1Stretched = 55;
        protected const int MID_LeftThumbSpread = 56;
        protected const int MID_LeftThumb2Stretched = 57;
        protected const int MID_LeftThumb3Stretched = 58;
        protected const int MID_LeftIndex1Stretched = 59;
        protected const int MID_LeftIndexSpread = 60;
        protected const int MID_LeftIndex2Stretched = 61;
        protected const int MID_LeftIndex3Stretched = 62;
        protected const int MID_LeftMiddle1Stretched = 63;
        protected const int MID_LeftMiddleSpread = 64;
        protected const int MID_LeftMiddle2Stretched = 65;
        protected const int MID_LeftMiddle3Stretched = 66;
        protected const int MID_LeftRing1Stretched = 67;
        protected const int MID_LeftRingSpread = 68;
        protected const int MID_LeftRing2Stretched = 69;
        protected const int MID_LeftRing3Stretched = 70;
        protected const int MID_LeftLittle1Stretched = 71;
        protected const int MID_LeftLittleSpread = 72;
        protected const int MID_LeftLittle2Stretched = 73;
        protected const int MID_LeftLittle3Stretched = 74;
        protected const int MID_RightThumb1Stretched = 75;
        protected const int MID_RightThumbSpread = 76;
        protected const int MID_RightThumb2Stretched = 77;
        protected const int MID_RightThumb3Stretched = 78;
        protected const int MID_RightIndex1Stretched = 79;
        protected const int MID_RightIndexSpread = 80;
        protected const int MID_RightIndex2Stretched = 81;
        protected const int MID_RightIndex3Stretched = 82;
        protected const int MID_RightMiddle1Stretched = 83;
        protected const int MID_RightMiddleSpread = 84;
        protected const int MID_RightMiddle2Stretched = 85;
        protected const int MID_RightMiddle3Stretched = 86;
        protected const int MID_RightRing1Stretched = 87;
        protected const int MID_RightRingSpread = 88;
        protected const int MID_RightRing2Stretched = 89;
        protected const int MID_RightRing3Stretched = 90;
        protected const int MID_RightLittle1Stretched = 91;
        protected const int MID_RightLittleSpread = 92;
        protected const int MID_RightLittle2Stretched = 93;
        protected const int MID_RightLittle3Stretched = 94;



        #endregion



        #region Other Utilities

        public Transform GetShoulder(bool right)
        {
            if (LastHumanoidAnimator == null) return null; return LastHumanoidAnimator.GetBoneTransform(right ? HumanBodyBones.RightShoulder : HumanBodyBones.LeftShoulder);
        }

        public Transform GetUpperArm(bool right)
        {
            if (LastHumanoidAnimator == null) return null; return LastHumanoidAnimator.GetBoneTransform(right ? HumanBodyBones.RightUpperArm : HumanBodyBones.LeftUpperArm);
        }

        public Transform GetLowerArm(bool right)
        {
            if (LastHumanoidAnimator == null) return null; return LastHumanoidAnimator.GetBoneTransform(right ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm);
        }

        public Transform GetHand(bool right)
        {
            if (LastHumanoidAnimator == null) return null; return LastHumanoidAnimator.GetBoneTransform(right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
        }


        #endregion

    }
}
