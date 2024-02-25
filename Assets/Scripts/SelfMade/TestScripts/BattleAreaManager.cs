using UnityEngine;
using CustomInspector;
using System.Collections.Generic;
using System;
using VInspector;

namespace TestField
{
    public class BattleAreaManager : MonoBehaviour
    {
        [Tab("AreaSettings")]
        [Header("AreaID")]
        [SerializeField] private string AreaID = "SearchArea";
        [Space2(10)]
        [Header("Area Size")]
        [SerializeField] private Vector3 SafeZone = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 boxSize = new Vector3(1f, 1f, 1f);

        // 偏移量
        [SerializeField] private Vector3 boxOffset = Vector3.zero;

        [Tab("SearchingActive")]
        [Foldout("TargetINFO")]
        [ReadOnly] public GameObject TargetFound;
        [SerializeField] private LayerMask detectionLayer;
        [Tag, SerializeField] private string detectionTag = "Player";


        [Foldout("PartnerINFO")]
        [SerializeField]
        [FixedValues("Player", "Partner", "Ally", "Enemy", "Neutral", "TrainingTarget", "TestPrototype")]
        private string PartnerMasterID;
        [SerializeField] private LayerMask PartnerLayer;
        [ReadOnly] public List<GameObject> PartnerList = new List<GameObject>();
        private HashSet<GameObject> lastPartnerSet = new HashSet<GameObject>();


        [Foldout("NeutralINFO")]
        [SerializeField]
        [FixedValues("Player", "Partner", "Ally", "Enemy", "Neutral", "TrainingTarget", "TestPrototype")]
        private string NeutralMasterID;
        [SerializeField] private LayerMask NeutralLayer;
        [ReadOnly] public List<GameObject> NeutralList = new List<GameObject>();
        private HashSet<GameObject> lastNeutralSet = new HashSet<GameObject>();

        [Foldout("AIReceiverINFO")]
        [SerializeField]
        [FixedValues("Player", "Partner", "Ally", "Enemy", "Neutral", "TrainingTarget", "TestPrototype")]
        private string ReceiverMasterID;
        [SerializeField] private LayerMask ReceiverLayer;
        [ReadOnly] public List<GameObject> BroadCastReceiver = new List<GameObject>();
        [ReadOnly] public List<GameObject> SBroadCastReceiver = new List<GameObject>();

        [Tab("SearchingStatic")]
        [Foldout("CoverINFO")]
        [SerializeField]
        [FixedValues("Cover")]
        private string CoverIdentity;
        [SerializeField] private LayerMask CoverLayer;
        [ReadOnly] public List<GameObject> FullCover = new List<GameObject>();
        [ReadOnly] public List<GameObject> HalfCover = new List<GameObject>();
        [ReadOnly] public List<GameObject> OccupiedCover = new List<GameObject>();
        [ReadOnly] public List<GameObject> FreeCover = new List<GameObject>();
        // 事件定义
        public event Action<GameObject> TargetFoundChanged;
        public event Action<List<GameObject>> PartnerChanged;
        public event Action<List<GameObject>> NeutralChanged;
        public event Action<List<GameObject>> BroadCastReceiverChanged;
        public event Action<List<GameObject>> SBroadCastReceiverChanged;
        public event Action<List<GameObject>> HalfCoverChanged;
        public event Action<List<GameObject>> FullCoverChanged;
        public event Action<List<GameObject>> OccupiedCoverChanged;
        public event Action<List<GameObject>> FreeCoverChanged;

        private GameObject previousTarget;

        private void OnValidate()
        {
            SafeZone = boxSize - new Vector3(2f, 2f, 2f);
        }

        void Update()
        {
            SearchingReceiverInSafeZone();
            SearchingReceiver();
            PartnerSeeker();
            SearchingTarget();
            CoverSearching();
            NeutralSeeker();
        }


        //用来监测目标物体
        private void SearchingTarget()
        {

            previousTarget = TargetFound;
            TargetFound = null;
            // 清空 TargetFound

            // 获取盒子范围，考虑 GameObject 的位置和偏移量
            Bounds boxBounds = new Bounds(transform.position + boxOffset, boxSize);

            // 检测是否有物体与盒子相交，使用 LayerMask
            Collider[] colliders = Physics.OverlapBox(boxBounds.center, boxBounds.extents, Quaternion.identity, detectionLayer);

            // 处理相交的物体
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag(detectionTag))
                {
                    TargetFound = collider.gameObject;
                    // 找到一个符合条件的物体后退出循环
                    break;
                }
            }

            // 检查是否有新的目标被找到
            if (TargetFound != previousTarget)
            {
                // 在设置新值时触发事件
                OnTargetFoundChanged(TargetFound);
            }
        }

        private void PartnerSeeker()
        {
            Bounds boxBounds = new Bounds(transform.position + boxOffset, boxSize);

            Collider[] colliders = Physics.OverlapBox(boxBounds.center, boxBounds.extents, Quaternion.identity, PartnerLayer);

            PartnerList.Clear();

            HashSet<GameObject> currentPartnerSet = new HashSet<GameObject>();

            foreach (Collider collider in colliders)
            {
                Identity identity = collider.GetComponent<Identity>();

                if (identity == null)
                {
                    Transform parent = collider.transform.parent;

                    while (parent != null && identity == null)
                    {
                        identity = parent.GetComponent<Identity>();
                        parent = parent.parent;
                    }
                }

                if (identity != null && identity.MasterID == PartnerMasterID && currentPartnerSet.Add(identity.gameObject))
                {
                    PartnerList.Add(identity.gameObject);
                }
            }

            // 使用SetComparison来比较两个HashSet是否相等
            if (!currentPartnerSet.SetEquals(lastPartnerSet))
            {
                OnPartnerChanged();
            }

            //最后，更新lastPartnerSet
            lastPartnerSet = currentPartnerSet;
        }

        //用于查找该片区域内的中立单位
        private void NeutralSeeker()
        {
            Bounds boxBounds = new Bounds(transform.position + boxOffset, boxSize);

            Collider[] colliders = Physics.OverlapBox(boxBounds.center, boxBounds.extents, Quaternion.identity, NeutralLayer);

            NeutralList.Clear();

            // 使用 HashSet 来确保每个物体只会被添加一次
            HashSet<GameObject> currentNeutralSet = new HashSet<GameObject>();

            foreach (Collider collider in colliders)
            {
                Identity identity = collider.GetComponent<Identity>();

                // 如果当前collider没有Identity脚本，则查找其父级
                if (identity == null)
                {
                    Transform parent = collider.transform.parent;

                    while (parent != null && identity == null)
                    {
                        identity = parent.GetComponent<Identity>();
                        parent = parent.parent;
                    }
                }

                // 添加带有Identity脚本的物体到BroadCastReceiver，确保每个物体只会被添加一次
                if (identity != null && identity.MasterID == NeutralMasterID && currentNeutralSet.Add(identity.gameObject))
                {
                    NeutralList.Add(identity.gameObject);
                }
            }

            // 使用SetComparison来比较两个HashSet是否相等
            if (!currentNeutralSet.SetEquals(lastNeutralSet))
            {
                OnNeutralChanged();
            }

            lastNeutralSet = currentNeutralSet;
        }

        //搜索要向哪些目标设置目标信息
        private void SearchingReceiver()
        {
            Bounds boxBounds = new Bounds(transform.position + boxOffset, boxSize);

            Collider[] colliders = Physics.OverlapBox(boxBounds.center, boxBounds.extents, Quaternion.identity, ReceiverLayer);

            BroadCastReceiver.Clear();

            // 使用 HashSet 来确保每个物体只会被添加一次
            HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

            foreach (Collider collider in colliders)
            {
                Identity identity = collider.GetComponent<Identity>();

                // 如果当前collider没有Identity脚本，则查找其父级
                if (identity == null)
                {
                    Transform parent = collider.transform.parent;

                    while (parent != null && identity == null)
                    {
                        identity = parent.GetComponent<Identity>();
                        parent = parent.parent;
                    }
                }

                // 添加带有Identity脚本的物体到BroadCastReceiver，确保每个物体只会被添加一次
                if (identity != null && identity.MasterID == ReceiverMasterID && uniqueObjects.Add(identity.gameObject))
                {
                    BroadCastReceiver.Add(identity.gameObject);

                    // 发送通知，传递更新后的列表
                    OnBroadCastReceiverChanged();
                }
            }
        }

        private void SearchingReceiverInSafeZone()
        {
            Bounds boxBounds = new Bounds(transform.position + boxOffset, SafeZone);

            Collider[] colliders = Physics.OverlapBox(boxBounds.center, boxBounds.extents, Quaternion.identity, ReceiverLayer);

            SBroadCastReceiver.Clear();

            // 使用 HashSet 来确保每个物体只会被添加一次
            HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

            foreach (Collider collider in colliders)
            {
                Identity identity = collider.GetComponent<Identity>();

                // 如果当前collider没有Identity脚本，则查找其父级
                if (identity == null)
                {
                    Transform parent = collider.transform.parent;

                    while (parent != null && identity == null)
                    {
                        identity = parent.GetComponent<Identity>();
                        parent = parent.parent;
                    }
                }

                // 添加带有Identity脚本的物体到SBroadCastReceiver，确保每个物体只会被添加一次
                if (identity != null && identity.MasterID == ReceiverMasterID && uniqueObjects.Add(identity.gameObject))
                {
                    SBroadCastReceiver.Add(identity.gameObject);
                }
                // 发送通知，传递更新后的列表
                OnSBroadCastReceiverChanged();
            }
        }

        private void CoverSearching()
        {
            Bounds boxBounds = new Bounds(transform.position + boxOffset, boxSize);

            Collider[] colliders = Physics.OverlapBox(boxBounds.center, boxBounds.extents, Quaternion.identity, CoverLayer);

            HalfCover.Clear();
            FullCover.Clear();
            OccupiedCover.Clear();
            FreeCover.Clear();

            // 使用 HashSet 来确保每个物体只会被添加一次
            HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

            foreach (Collider collider in colliders)
            {
                Identity identity = collider.GetComponent<Identity>();

                // 如果当前collider没有Identity脚本，则查找其父级
                if (identity == null)
                {
                    Transform parent = collider.transform.parent;

                    while (parent != null && identity == null)
                    {
                        identity = parent.GetComponent<Identity>();
                        parent = parent.parent;
                    }
                }

                // 添加带有Identity脚本的物体到BroadCastReceiver，确保每个物体只会被添加一次
                if (identity != null && identity.MasterID == CoverIdentity && uniqueObjects.Add(identity.gameObject))
                {
                    if (identity.Covertype == "HalfCover")
                    {
                        HalfCover.Add(identity.gameObject);
                        OnHalfCoverChanged();
                    }
                    else if (identity.Covertype == "FullCover")
                    {
                        FullCover.Add(identity.gameObject);
                        OnFullCoverChanged();
                    }

                    if (identity.CoverOccupied)
                    {
                        // 检查是否已经在列表中
                        if (!OccupiedCover.Contains(identity.gameObject))
                        {
                            OccupiedCover.Add(identity.gameObject);
                            OnOccupiedCoverChanged();
                            OnFreeCoverChanged();
                        }
                    }
                    else if (!identity.CoverOccupied)
                    {
                        // 检查是否已经在列表中
                        if (!FreeCover.Contains(identity.gameObject))
                        {
                            FreeCover.Add(identity.gameObject);
                            OnOccupiedCoverChanged();
                            OnFreeCoverChanged();
                        }
                    }
                }
            }
        }

        private void OnPartnerChanged()
        {
            PartnerChanged?.Invoke(PartnerList);
        }

        private void OnNeutralChanged()
        {
            NeutralChanged?.Invoke(NeutralList);
        }

        private void OnBroadCastReceiverChanged()
        {
            // 检查是否有订阅者，如果有，则调用事件通知它们列表发生了变化
            BroadCastReceiverChanged?.Invoke(BroadCastReceiver);
        }

        private void OnSBroadCastReceiverChanged()
        {
            SBroadCastReceiverChanged?.Invoke(SBroadCastReceiver);
        }

        // 触发事件的方法
        protected virtual void OnTargetFoundChanged(GameObject newTarget)
        {
            TargetFoundChanged?.Invoke(newTarget);
        }

        private void OnHalfCoverChanged()
        {
            HalfCoverChanged?.Invoke(HalfCover);

        }

        private void OnFullCoverChanged()
        {
            FullCoverChanged?.Invoke(FullCover);
        }

        private void OnOccupiedCoverChanged()
        {
            OccupiedCoverChanged?.Invoke(OccupiedCover);

        }

        private void OnFreeCoverChanged()
        {
            FreeCoverChanged?.Invoke(FreeCover);
        }

#if UNITY_EDITOR
        // 在Scene视图中绘制Gizmos
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            // 绘制框，考虑GameObject的位置和偏移量
            Gizmos.DrawWireCube(transform.position + boxOffset, boxSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + boxOffset, SafeZone);
        }
#endif
    }
}
