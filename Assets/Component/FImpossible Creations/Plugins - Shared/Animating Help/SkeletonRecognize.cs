using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public static class SkeletonRecognize
    {

        public enum EWhatIsIt
        {
            Unknown, Humanoidal, Quadroped, Creature
        }


        public class SkeletonInfo
        {
            public Transform AnimatorTransform;
            public float LowestVsHighestLen;
            public float MostLeftVsMostRightLen;
            public float MostForwVsMostBackLen;
            public float AverageLen;

            public Transform ProbablyRootBone;
            public Transform ProbablyHips;
            public Transform ProbablyChest;
            public Transform ProbablyHead;

            public List<Transform> TrReachingGround = new List<Transform>();
            public List<Transform> TrReachingSides = new List<Transform>();
            public List<Transform> TrEnds = new List<Transform>();

            public List<Transform> ProbablySpineChain = new List<Transform>();
            public List<Transform> ProbablySpineChainShort = new List<Transform>();
            public List<List<Transform>> ProbablyRightArms = new List<List<Transform>>();
            public List<List<Transform>> ProbablyLeftArms = new List<List<Transform>>();
            public List<List<Transform>> ProbablyLeftLegs = new List<List<Transform>>();
            public List<Transform> ProbablyLeftLegRoot = new List<Transform>();
            public List<List<Transform>> ProbablyRightLegs = new List<List<Transform>>();
            public List<Transform> ProbablyRightLegRoot = new List<Transform>();

            public Vector3 LocalSpaceHighest = Vector3.zero;
            public Vector3 LocalSpaceMostRight = Vector3.zero;
            public Vector3 LocalSpaceMostForward = Vector3.zero;
            public Vector3 LocalSpaceMostBack = Vector3.zero;
            public Vector3 LocalSpaceMostLeft = Vector3.zero;
            public Vector3 LocalSpaceLowest = Vector3.zero;

            public int SpineChainLength { get { return ProbablySpineChain.Count; } }
            public int LeftArms { get { return ProbablyLeftArms.Count; } }
            public int LeftLegs { get { return ProbablyLeftLegs.Count; } }
            public int RightArms { get { return ProbablyRightArms.Count; } }
            public int RightLegs { get { return ProbablyRightLegs.Count; } }
            public int Legs { get { return RightLegs + LeftLegs; } }
            public int Arms { get { return LeftArms + RightArms; } }

            public EWhatIsIt WhatIsIt = EWhatIsIt.Unknown;

            public SkeletonInfo(Transform t, List<Transform> checkOnly = null, Transform pelvisHelp = null)
            {
                AnimatorTransform = t;

                Transform[] childTransforms;

                if (checkOnly != null)
                {
                    childTransforms = new Transform[checkOnly.Count];
                    for (int i = 0; i < checkOnly.Count; i++)
                    {
                        childTransforms[i] = checkOnly[i];
                    }
                }
                else
                    childTransforms = AnimatorTransform.GetComponentsInChildren<Transform>(true);


                if (childTransforms.Length > 0)
                {
                    Vector3 l = AnimatorTransform.InverseTransformPoint(childTransforms[0].position);
                    LocalSpaceHighest = l;
                    LocalSpaceMostRight = l;
                    LocalSpaceMostForward = l;
                    LocalSpaceMostBack = l;
                    LocalSpaceMostLeft = l;
                    LocalSpaceLowest = l;
                }

                List<Transform> childT = new List<Transform>();
                for (int i = 0; i < childTransforms.Length; i++)
                {
                    Transform c = childTransforms[i];
                    SkinnedMeshRenderer skin = c.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (skin != null) continue;
                    childT.Add(c);
                }

                //UnityEngine.Debug.Log("ChildTransforms = " + childTransforms.Length);

                #region Defining Skeleton Bounds Guides


                for (int i = 0; i < childT.Count; i++)
                {
                    Transform c = childT[i];

                    if (c.GetComponent<SkinnedMeshRenderer>()) continue;

                    Vector3 local = AnimatorTransform.InverseTransformPoint(c.position);

                    if (local.x > LocalSpaceMostRight.x) LocalSpaceMostRight = local;
                    else if (local.x < LocalSpaceMostLeft.x) LocalSpaceMostLeft = local;

                    if (local.z > LocalSpaceMostForward.z) LocalSpaceMostForward = local;
                    else if (local.z < LocalSpaceMostBack.z) LocalSpaceMostBack = local;

                    if (local.y > LocalSpaceHighest.y) LocalSpaceHighest = local;
                    else if (local.y < LocalSpaceLowest.y) LocalSpaceLowest = local;
                }


                #endregion


                // Helper Measures
                LowestVsHighestLen = Mathf.Abs(LocalSpaceLowest.y - LocalSpaceHighest.y);
                MostLeftVsMostRightLen = Mathf.Abs(LocalSpaceMostLeft.x - LocalSpaceMostRight.x);
                MostForwVsMostBackLen = Mathf.Abs(LocalSpaceMostForward.z - LocalSpaceMostBack.z);
                AverageLen = (LowestVsHighestLen + MostLeftVsMostRightLen + MostForwVsMostBackLen) / 3f;


                float limbMinimumLength = LowestVsHighestLen * 0.55f;



                #region Initial finding name based


                // arms
                for (int c = 0; c < childT.Count; c++)
                {
                    Transform ct = childT[c];
                    if (NameContains(ct.name, ShouldersNames))
                    {
                        Transform getCh = GetBottomMostChildTransform(ct);
                        if (NotContainedYetByLimbs(getCh)) TrReachingSides.Add(getCh);
                    }
                    else
                    {
                        if (NameContains(ct.name, ElbowNames))
                        {
                            Transform getCh = GetBottomMostChildTransform(ct);
                            if (NotContainedYetByLimbs(getCh)) TrReachingSides.Add(getCh);
                        }
                    }
                }

                // legs
                for (int c = 0; c < childT.Count; c++)
                {
                    Transform ct = childT[c];
                    if (NameContains(ct.name, UpperLegNames))
                    {
                        Transform getCh = GetBottomMostChildTransform(ct);
                        if (NotContainedYetByLimbs(getCh)) TrReachingGround.Add(getCh);
                    }
                    else
                    {
                        if (NameContains(ct.name, KneeNames))
                        {
                            Transform getCh = GetBottomMostChildTransform(ct);
                            if (NotContainedYetByLimbs(getCh)) TrReachingGround.Add(getCh);
                        }
                    }
                }

                // pelvis
                bool hipsByName = false;

                for (int c = 0; c < childT.Count; c++)
                {
                    Transform ct = childT[c];
                    if (NameContains(ct.name, PelvisNames))
                    {
                        hipsByName = true;
                        ProbablyHips = ct;
                        break;
                    }
                }

                // chest
                bool chestByName = false;

                for (int c = 0; c < childT.Count; c++)
                {
                    Transform ct = childT[c];
                    if (NameContains(ct.name, ChestNames))
                    {
                        chestByName = true;
                        ProbablyChest = ct;
                        break;
                    }
                }

                // head

                bool headByName = false;

                for (int c = 0; c < childT.Count; c++)
                {
                    Transform ct = childT[c];
                    if (NameContains(ct.name, HeadNames))
                    {
                        headByName = true;
                        ProbablyHead = ct;
                        break;
                    }
                }

                if (ProbablyHead != null)
                    if (ProbablyHips != null)
                    {
                        if (IsChildOf(ProbablyHead, ProbablyHips) == false)
                        {
                            ProbablyHead = null;
                        }
                    }

                // root

                for (int c = 0; c < childT.Count; c++)
                {
                    Transform ct = childT[c];
                    if (NameContains(ct.name, RootNames))
                    {
                        ProbablyRootBone = ct;
                        break;
                    }
                }



                #endregion




                #region Defining End Transforms for Arms / Legs / Head


                if (childT.Count > 2)
                {
                    for (int i = 1; i < childT.Count; i++)
                    {
                        Transform tr = childT[i];

                        if (tr.childCount == 0)
                        {
                            TrEnds.Add(tr);

                            Vector3 l = Loc(tr);

                            if (l.y < LocalSpaceLowest.y + LowestVsHighestLen * 0.1f)
                            {
                                if (NotContainedYetByLimbs(tr)) TrReachingGround.Add(tr);
                            }
                            else
                            {
                                if (l.y > LocalSpaceLowest.y + LowestVsHighestLen * 0.2f)
                                {
                                    if (l.x < MostLeftVsMostRightLen * -0.1f || l.x > MostLeftVsMostRightLen * 0.1f)
                                    {
                                        if (NotContainedYetByLimbs(tr)) TrReachingSides.Add(tr);
                                    }
                                }
                            }
                        }

                    }
                }


                #endregion




                #region Chest Basing on Left / Right Sides Limbs

                if (!chestByName)
                {
                    List<Transform> probablyChestOnes = new List<Transform>();
                    for (int i = 0; i < TrReachingSides.Count; i++)
                    {
                        if (childT[i].GetComponent<SkinnedMeshRenderer>()) continue;

                        Transform par = TrReachingSides[i].parent;

                        while (par != null)
                        {
                            if (par.childCount > 2)
                            {
                                Vector3 loc = Loc(par);
                                if (loc.x > -MostLeftVsMostRightLen * 0.03f && loc.x < MostLeftVsMostRightLen * 0.03f)
                                {
                                    probablyChestOnes.Add(par);
                                    break;
                                }
                            }

                            par = par.parent;
                        }
                    }

                    if (probablyChestOnes.Count == 1) ProbablyChest = probablyChestOnes[0];
                    else if (probablyChestOnes.Count > 1)
                    {
                        if (probablyChestOnes[0] == probablyChestOnes[1])
                            ProbablyChest = probablyChestOnes[0];
                    }
                }

                #endregion


                #region Pelvis Basing On Left / Right Low Limbs


                if (!hipsByName)
                {
                    List<Transform> probablyHipsOnes = new List<Transform>();

                    for (int i = 0; i < TrReachingGround.Count; i++)
                    {
                        Transform par = TrReachingGround[i].parent;

                        while (par != null)
                        {
                            if (par.childCount > 2)
                            {
                                Vector3 loc = Loc(par);
                                if (loc.y > LocalSpaceLowest.y + LowestVsHighestLen * 0.04f)
                                    if (loc.x > -MostLeftVsMostRightLen * 0.02f && loc.x < MostLeftVsMostRightLen * 0.02f)
                                    {
                                        probablyHipsOnes.Add(par);
                                        break;
                                    }
                            }

                            par = par.parent;
                        }
                    }

                    if (probablyHipsOnes.Count == 1) ProbablyChest = probablyHipsOnes[0];
                    else if (probablyHipsOnes.Count > 1)
                    {
                        if (probablyHipsOnes[0] == probablyHipsOnes[1])
                            ProbablyHips = probablyHipsOnes[0];
                    }
                }

                if (ProbablyHips == null) ProbablyHips = pelvisHelp;

                #endregion


                #region correcting chest if required


                if (ProbablyChest == null || ProbablyChest == ProbablyHips || (ProbablyHips != null && IsChildOf(ProbablyChest, ProbablyHips) == false))
                {
                    if (ProbablyHips) if (ProbablyHead)
                        {
                            Transform checkT = ProbablyHead.parent;
                            bool found = false;

                            while (checkT.parent != null && checkT.parent != ProbablyHips)
                            {
                                if (checkT.childCount > 2)
                                {
                                    // Check if some side limbs are child bones of chest check bone
                                    for (int s = 0; s < TrReachingSides.Count; s++)
                                    {
                                        Transform side = TrReachingSides[s];

                                        if (IsChildOf(side, checkT))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }

                                if (found) break;

                                checkT = checkT.parent;
                            }

                            if (found) ProbablyChest = checkT;
                        }
                }

                if (ProbablyHips == null) ProbablyHips = pelvisHelp;

                #endregion


                // Probably correctly detected chest and hips
                if (ProbablyChest && ProbablyHips)
                {

                    if (MostForwVsMostBackLen > LowestVsHighestLen * 0.9f) // If forward legth is bigger than model's height
                    {
                        // In most cases chest is more in front than hips
                        if (Loc(ProbablyChest).z < Loc(ProbablyHips).z) // Chest is behind hips - swap!
                        {
                            Transform swap = ProbablyChest;
                            ProbablyChest = ProbablyHips;
                            ProbablyHips = swap;
                            UnityEngine.Debug.Log("Hips - Chest - Reversed Detection Swap!");
                        }
                    }


                    #region Trying To Detect Head

                    if (!headByName)
                    {
                        Vector3 highestForHead = Vector3.zero;
                        for (int c = 0; c < ProbablyChest.childCount; c++)
                        {
                            // checking all probably chest child transforms
                            Transform ch = ProbablyChest.GetChild(c);
                            Vector3 lc;

                            if (ch.childCount > 0) // Going through 
                            {
                                for (int c2 = 0; c2 < ch.childCount; c2++)
                                {
                                    Transform ch2 = ch.GetChild(c2);
                                    lc = Loc(ch2);

                                    if (lc.x > -MostLeftVsMostRightLen * 0.04f && lc.x < MostLeftVsMostRightLen * 0.04f)
                                    {
                                        if (Loc(ch2).y > highestForHead.y)
                                        { highestForHead = Loc(ch2); ProbablyHead = ch2; }
                                    }
                                }
                            }

                            lc = Loc(ch);
                            if (lc.x > -MostLeftVsMostRightLen * 0.04f && lc.x < MostLeftVsMostRightLen * 0.04f)
                                if (lc.y > highestForHead.y)
                                { highestForHead = Loc(ch); ProbablyHead = ch; }
                        }


                        if (ProbablyChest && ProbablyHead && ProbablyHips)
                        {
                            float chestToPelvis = Vector3.Distance(Loc(ProbablyChest), Loc(ProbablyHips));

                            if ((ProbablyChest.childCount < 3 || chestToPelvis < AverageLen * 0.12f) && ProbablyHead.childCount > 1)
                            {
                                ProbablyChest = ProbablyHead;
                                ProbablyHead = GetHighestChild(ProbablyHead, AnimatorTransform, MostLeftVsMostRightLen * 0.05f);
                                if (ProbablyHead == ProbablyChest) ProbablyHead = ProbablyChest.GetChild(0);
                            }
                        }

                    }


                    #endregion


                    #region Eliminating wrong detected arms (it can be ear bones)

                    if (ProbablyHead)
                    {
                        for (int i = TrReachingSides.Count - 1; i >= 0; i--)
                        {
                            if (IsChildOf(TrReachingSides[i], ProbablyHead)) TrReachingSides.RemoveAt(i);
                        }
                    }

                    for (int i = TrReachingSides.Count - 1; i >= 0; i--)
                    {
                        if (GetDepth(TrReachingSides[i], AnimatorTransform) < 5)
                        {
                            TrReachingSides.RemoveAt(i);
                        }
                    }

                    #endregion


                    #region Detecting Spine Chain

                    Transform headC = null;
                    if (ProbablyHead)
                    {
                        //if (ProbablyHead.parent) headC = ProbablyHead.parent;
                        ProbablySpineChain.Add(ProbablyHead);
                        headC = ProbablyHead.parent;
                    }

                    while (headC != null && headC != ProbablyHips)
                    {
                        ProbablySpineChain.Add(headC);
                        headC = headC.parent;
                    }

                    ProbablySpineChain.Reverse();

                    for (int i = 0; i < Mathf.Min(4, ProbablySpineChain.Count); i++)
                    {
                        ProbablySpineChainShort.Add(ProbablySpineChain[i]);
                    }

                    #endregion


                    #region Detecting Legs

                    List<Transform> confirmedLegs = new List<Transform>();

                    for (int i = 0; i < TrReachingGround.Count; i++)
                    {
                        Transform start = TrReachingGround[i];
                        Vector3 startLoc = Loc(start);

                        List<Transform> fullChain = new List<Transform>();

                        Transform untilHips = start;
                        while (untilHips != null && (untilHips != ProbablyHips && untilHips != ProbablyChest))
                        {
                            fullChain.Add(untilHips);
                            untilHips = untilHips.parent;
                        }

                        if (fullChain.Count >= 3)
                        {
                            List<Transform> legChain = new List<Transform>();
                            legChain.Add(fullChain[fullChain.Count - 1]);
                            legChain.Add(fullChain[fullChain.Count - 2]);
                            legChain.Add(fullChain[fullChain.Count - 3]);

                            confirmedLegs.Add(start);

                            if (startLoc.x < MostLeftVsMostRightLen * 0.02f)
                            {
                                ProbablyLeftLegs.Add(legChain);
                                ProbablyLeftLegRoot.Add(untilHips);
                            }
                            else
                            {
                                ProbablyRightLegs.Add(legChain);
                                ProbablyRightLegRoot.Add(untilHips);
                            }
                        }
                    }

                    #endregion


                    #region Detecting Arms


                    for (int i = 0; i < TrReachingSides.Count; i++)
                    {
                        Transform start = TrReachingSides[i];
                        Vector3 startLoc = Loc(start);

                        List<Transform> fullChain = new List<Transform>();

                        Transform untilChest = start;
                        while (untilChest != null && untilChest != ProbablyChest)
                        {
                            fullChain.Add(untilChest);
                            untilChest = untilChest.parent;
                        }

                        if (fullChain.Count >= 4)
                        {
                            List<Transform> armChain = new List<Transform>();
                            armChain.Add(fullChain[fullChain.Count - 1]);
                            armChain.Add(fullChain[fullChain.Count - 2]);
                            armChain.Add(fullChain[fullChain.Count - 3]);
                            armChain.Add(fullChain[fullChain.Count - 4]);

                            if (startLoc.x < MostLeftVsMostRightLen * 0.02f)
                                ProbablyLeftArms.Add(armChain);
                            else
                                ProbablyRightArms.Add(armChain);
                        }
                    }

                    #endregion

                    #region Removing Duplicates (resulting by fingers counts)

                    ClearDuplicates(ProbablyLeftArms, null);
                    ClearDuplicates(ProbablyRightArms, null);
                    ClearDuplicates(ProbablyLeftLegs, ProbablyLeftLegRoot);
                    ClearDuplicates(ProbablyRightLegs, ProbablyRightLegRoot);

                    #endregion


                    if (Legs == 2 && Arms == 2)
                    {
                        WhatIsIt = EWhatIsIt.Humanoidal;
                    }
                    else if (Legs == 4 && Arms == 0)
                    {
                        WhatIsIt = EWhatIsIt.Quadroped;
                    }
                    else if (Legs > 0 || Arms > 0)
                    {
                        WhatIsIt = EWhatIsIt.Creature;
                    }
                    else
                    {
                        WhatIsIt = EWhatIsIt.Unknown;
                    }

                }


                float middleHeight = Mathf.Lerp(LocalSpaceLowest.y, LocalSpaceHighest.y, 0.5f);
                UnityEngine.Debug.DrawLine(t.TransformPoint(new Vector3(LocalSpaceMostLeft.x, LocalSpaceHighest.y, LocalSpaceMostForward.z)), t.TransformPoint(new Vector3(LocalSpaceMostLeft.x, LocalSpaceLowest.y, LocalSpaceMostForward.z)), Color.green, 12);
                UnityEngine.Debug.DrawLine(t.TransformPoint(new Vector3(LocalSpaceMostLeft.x, middleHeight, LocalSpaceMostForward.z)), t.TransformPoint(new Vector3(LocalSpaceMostRight.x, middleHeight, LocalSpaceMostForward.z)), Color.red, 12);
                UnityEngine.Debug.DrawLine(t.TransformPoint(new Vector3(LocalSpaceMostRight.x, middleHeight, LocalSpaceMostForward.z)), t.TransformPoint(new Vector3(LocalSpaceMostRight.x, middleHeight, LocalSpaceMostBack.z)), Color.blue, 12);

            }


            bool NotContainedYetByAny(Transform t)
            {
                return (!TrReachingSides.Contains(t) && !TrReachingGround.Contains(t) && !TrEnds.Contains(t)
                    && t != ProbablyChest && t != ProbablyHips && t != ProbablyHead && t != ProbablyChest && t != ProbablyRootBone && t != AnimatorTransform);
            }

            bool NotContainedYetByLimbs(Transform t)
            {
                return (!TrReachingSides.Contains(t) && !TrReachingGround.Contains(t));
            }

            public Transform GetHighestChild(Transform t, Transform root, float inCenterRangeFactor)
            {
                if (t == null) return null;

                Transform highT = t;
                Vector3 highest = root.InverseTransformPoint(t.position);
                foreach (var ct in t.GetComponentsInChildren<Transform>(true))
                {
                    Vector3 pos = root.InverseTransformPoint(ct.position);

                    if (pos.x > -inCenterRangeFactor && pos.x < inCenterRangeFactor)
                        if (pos.y > highest.y)
                        {
                            highest.y = pos.y;
                            highT = ct;
                        }
                }

                return highT;
            }

            //float ComputeLength(Transform p, int parentBack)
            //{
            //    float len = 0f;

            //    if (p != null)
            //        for (int i = 0; i < parentBack; i++)
            //        {
            //            if (p.parent != null)
            //            {
            //                len += Vector3.Distance(p.position, p.parent.position);
            //                p = p.parent;
            //            }
            //            else
            //                break;
            //        }

            //    return len;
            //}

            void ClearDuplicates(List<List<Transform>> limbs, List<Transform> roots)
            {
                if (limbs.Count > 1)
                {
                    for (int main = 0; main < limbs.Count; main++) // Checking all limb chains
                    {
                        if (main >= limbs.Count) return;

                        var limb = limbs[main];

                        // Checking if some other limbs contains duplicate bones of each other
                        // It can be caused by finger bones - how many fingers -> that many hands detected
                        for (int i = limbs.Count - 1; i >= 0; i--)
                        {
                            if (i == main) continue; // Don't check self

                            var otherLimb = limbs[i];

                            bool remove = false;

                            for (int p = 0; p < otherLimb.Count; p++)
                            {

                                if (limb.Contains(otherLimb[p]))
                                {
                                    remove = true;
                                    break;
                                }

                            }

                            if (remove)
                            {
                                limbs.RemoveAt(i);
                            }
                        }
                    }

                }
            }


            Vector3 Loc(Transform t)
            {
                return AnimatorTransform.InverseTransformPoint(t.position);
            }

            #region Debug Log Report

            public string GetLog()
            {
                string log = "< " + AnimatorTransform.name + " >\n";

                log += "\nGenerate Guides:\n";

                log += "Highest: " + LocalSpaceHighest + "     ";
                log += "Lowest: " + LocalSpaceLowest + "     ";
                log += "Left: " + LocalSpaceMostLeft + "     ";
                log += "Right: " + LocalSpaceMostRight + "     ";
                log += "Forward: " + LocalSpaceMostForward + "     ";
                log += "Back: " + LocalSpaceMostBack + "     ";


                log += "\n\nGenerated Helper Measurements: \n";
                log += "UpDown: " + LowestVsHighestLen + "     ";
                log += "LeftRight: " + MostLeftVsMostRightLen + "     ";
                log += "ForwBack: " + MostForwVsMostBackLen + "     ";
                log += "Avr: " + AverageLen + "     ";

                log += "\n\nDetected Propabilities: \n";
                log += "ProbablyHips: " + ProbablyHips + "     ";
                log += "ProbablyChest: " + ProbablyChest + "     ";
                log += "ProbablyHead: " + ProbablyHead + "     ";

                log += "\n\nLimb End Detections: \n";
                log += "Reaching Ground: " + TrReachingGround.Count + "     ";
                log += "Reaching Sides: " + TrReachingSides.Count + "     ";
                log += "Spine Chain Length: " + ProbablySpineChain.Count + " (" + ProbablySpineChainShort.Count + ")     ";

                log += "\n\nDetected Propabilities: \n";
                log += "Probably Left Arms: " + ProbablyLeftArms.Count + "     ";
                log += "Probably Right Arms: " + ProbablyRightArms.Count + "     ";
                log += "Probably Left Legs: " + ProbablyLeftLegs.Count + "     ";
                log += "Probably Right Legs: " + ProbablyRightLegs.Count + "     ";

                log += "\n\n\nTr Ends: \n";
                for (int i = 0; i < TrEnds.Count; i++)
                {
                    if (TrEnds[i] == null) continue;
                    log += TrEnds[i].name + "     ";
                }
                log += "\n\nTr Reaching Ground: \n";
                for (int i = 0; i < TrReachingGround.Count; i++)
                {
                    if (TrReachingGround[i] == null) continue;
                    log += TrReachingGround[i].name + "     ";
                }
                log += "\n\nTr Reaching Sides: \n";
                for (int i = 0; i < TrReachingSides.Count; i++)
                {
                    if (TrReachingSides[i] == null) continue;
                    log += TrReachingSides[i].name + "     ";
                }


                if (ProbablyLeftArms.Count > 0)
                {
                    log += "\n\nDebug Left Arms: \n";
                    for (int i = 0; i < ProbablyLeftArms.Count; i++)
                    {
                        if (ProbablyLeftArms[i] == null) continue;
                        log += "[" + i + "] ";

                        for (int l = 0; l < ProbablyLeftArms[i].Count; l++)
                        {
                            log += ProbablyLeftArms[i][l].name + "  ";
                        }

                        log += "\n";
                    }
                }

                if (ProbablySpineChainShort.Count > 0)
                {
                    log += "\n\nDebug Spine Chain: \n";
                    for (int i = 0; i < ProbablySpineChainShort.Count; i++)
                    {
                        if (ProbablySpineChainShort[i] == null) continue;
                        log += ProbablySpineChainShort[i].name + "  ";
                    }
                }

                log += "\n\n";

                return log;
            }

            #endregion


            public static int GetDepth(Transform t, Transform skelRootBone)
            {
                int depth = 0;
                if (t == skelRootBone) return 0;
                if (t == null) return 0;
                if (t.parent == null) return 0;

                while (t != null && t != skelRootBone)
                {
                    t = t.parent;
                    depth += 1;
                }

                return depth;
            }
        }



        #region Transforms Utils


        public static bool IsChildOf(Transform child, Transform parent)
        {
            Transform p = child;
            while (p != null)
            {
                if (p == parent) return true;
                p = p.parent;
            }

            return false;
        }

        public static Transform GetBottomMostChildTransform(Transform parent)
        {
            var allCh = parent.GetComponentsInChildren<Transform>(true);
            int lowest = 0;
            Transform lowestT = parent;
            
            for (int c = 0; c < allCh.Length; c++)
            {
                if (allCh[c] == parent) continue;

                Transform ch = allCh[c];
                int depth = 0;

                while (ch.parent != parent && ch.parent != null)
                {
                    depth += 1;
                    ch = ch.parent;
                }

                if (depth > lowest)
                {
                    lowest = depth;
                    lowestT = allCh[c];
                }
            }

            return lowestT;
        }

        #endregion


        #region Name Based Search Utils

        public static readonly string[] SpineNames = new string[] { "spine" };
        public static readonly string[] NeckNames = new string[] { "neck" };
        public static readonly string[] HeadNames = new string[] { "head" };
        public static readonly string[] RootNames = new string[] { "root", "origin", "skel" };
        public static readonly string[] PelvisNames = new string[] { "pelvis", "hips", "pelv" };
        public static readonly string[] ChestNames = new string[] { "chest", "upperspine" };
        public static readonly string[] ShouldersNames = new string[] { "shoulde", "collarbon", "clavicl" };
        public static readonly string[] UpperLegNames = new string[] { "upperleg", "thigh" };
        public static readonly string[] KneeNames = new string[] { "knee", "calf", "lowerleg" };
        public static readonly string[] ElbowNames = new string[] { "elbow", "lowerarm" };

        public static bool NameContains(string name, string[] names)
        {
            string nm = name.ToLower();
            nm = nm.Replace("-", "");
            nm = nm.Replace(" ", "");
            nm = nm.Replace("_", "");
            nm = nm.Replace("|", "");
            nm = nm.Replace("@", "");

            for (int n = 0; n < names.Length; n++)
            {
                if (nm.Contains(names[n])) return true;
            }

            return false;
        }

        #endregion

    }
}
