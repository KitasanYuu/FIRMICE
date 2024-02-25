using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.Rendering.HighDefinition;

namespace BattleShoot
{
    public class ShootController : MonoBehaviour
    {
        public bool UsingTrajectoryPredict = true;

        public GameObject Target;

        public bool isAiming = false;
        public bool Fire = false;

        [SerializeField] private LayerMask aimColliderLayerMask;
        [SerializeField] private GameObject raycastOrigin; // 新增的字段，用于指定射线的出发点
        [SerializeField] private Transform debugTransform;
        [SerializeField] private Material customMaterial;

        [SerializeField] private float SelfRotateSpeed=10f;

        private GameObject debugSphere;

        private Animator _animator;
        private AimIK _aimIK;

        private bool _hasAnimator;
        private int AimIKParameter;
        private int _animIDEnterAiming;
        private int _animIDAimStatus;

        private Vector3 previousHitpoint;
        private Vector3 currentHitpoint;

        private WeaponShooter _weaponShooter;

        [HideInInspector]
        public Vector3 hitpoint;
        [HideInInspector]
        public Vector3 hitpointVelocity;

        private void Start()
        {
            ComponentInit();
            _hasAnimator = TryGetComponent(out _animator);
            AssignAnimationIDs();

            if (customMaterial == null)
            {
                customMaterial = new Material(Shader.Find("HDRP/Lit"));
            }

            if (debugTransform == null)
            {
                GenerateDebugSphere();
            }
        }

        private void Update()
        {
            AIM();
            CanIFire();
        }

        private void AssignAnimationIDs()
        {
            _animIDEnterAiming = Animator.StringToHash("EnterAiming");
            _animIDAimStatus = Animator.StringToHash("AimStatus");
        }

        private void AIM()
        {
            if (isAiming)
            {
                RaycastDetection();
                if (Target != null)
                {

                    Vector3 DTargetPosition = Target.transform.position;
                    DTargetPosition.y = 0f;
                    Vector3 DSelfPosition = transform.position;
                    DSelfPosition.y = 0f;
                    Vector3 TargetDir = (DTargetPosition - DSelfPosition).normalized;
                    transform.forward = Vector3.Lerp(transform.forward, TargetDir, Time.deltaTime * SelfRotateSpeed);
                }

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDEnterAiming, true);
                    _animator.SetFloat(_animIDAimStatus, 0);
                }

                if (AimIKParameter == 1)
                {
                    //AimIKParameter = 1;
                    _aimIK.enabled = true;
                    AimIKParameter = 0;
                }
            }
            else
            {
                _aimIK.enabled = false;
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDEnterAiming, false);
                }
            }
        }

        private void ComponentInit()
        {
            _aimIK = GetComponent<AimIK>();
            _weaponShooter = GetComponent<WeaponShooter>();
        }


        private void RaycastDetection()
        {
            if (raycastOrigin != null)
            {
                HitpointVelocyCalcu();
                if (Target != null)
                {
                    Ray ray = new Ray(raycastOrigin.transform.position, (Target.transform.position - raycastOrigin.transform.position).normalized);
                    if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimColliderLayerMask))
                    {
                        //debugSphere.transform.position = hit.point;
                        //Debug.Log(hit.collider.gameObject);
                        hitpoint = hit.point;
                        currentHitpoint = hit.point;
                    }
                }
                else
                {
                    Ray ray = new Ray(raycastOrigin.transform.position, transform.forward.normalized);
                    if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimColliderLayerMask))
                    {
                        //debugSphere.transform.position = hit.point;
                        hitpoint = hit.point;
                        currentHitpoint = hit.point;
                    }
                }
                if (debugSphere != null)
                {
                    DebugSphereSet();
                }
            }
            else
            {
                Debug.LogError("Raycast origin is not set. Please assign a GameObject to the 'raycastOrigin' field.");
            }
        }

        private void CanIFire()
        {
            if (raycastOrigin != null)
            {
                if (Target != null)
                {
                    // 创建一个射线，从源到目标
                    Ray ray = new Ray(raycastOrigin.transform.position, (Target.transform.position - raycastOrigin.transform.position).normalized);

                    // 设定射线的最大长度为源和目标之间的距离
                    float maxDistance = Vector3.Distance(raycastOrigin.transform.position, Target.transform.position);

                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, maxDistance))
                    {
                        // 判断击中的目标是否就是预期的Target 或者其父物体
                        Transform currentTransform = hitInfo.transform;
                        while (currentTransform != null)
                        {
                            if (currentTransform == Target.transform)
                            {
                                Fire = true;
                                Debug.Log("Can Fire");
                                return;
                            }
                            currentTransform = currentTransform.parent;
                        }
                        Fire = false;
                        Debug.Log("Cannot Fire");
                    }
                    else
                    {
                        Fire = false;
                        Debug.Log("Cannot Fire");
                    }
                }
            }
        }

        private void DebugSphereSet()
        {
            if (!UsingTrajectoryPredict)
            {
                debugSphere.transform.position = hitpoint;
            }
            else if (UsingTrajectoryPredict)
            {
                debugSphere.transform.position = _weaponShooter.PredictedAimPoint;
            }
        }

        private void HitpointVelocyCalcu()
        {
            // 计算速度
            hitpointVelocity = (currentHitpoint - previousHitpoint) / Time.deltaTime;

            // 存储当前帧的目标点位置
            previousHitpoint = currentHitpoint;
        }

        private void GenerateDebugSphere()
        {
            if (debugSphere == null)
            {
                debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                // 使用HDRP的材质球组件
                var hdrpMaterial = new Material(customMaterial.shader);
                hdrpMaterial.CopyPropertiesFromMaterial(customMaterial);
                debugSphere.GetComponent<Renderer>().material = hdrpMaterial;
                Destroy(debugSphere.GetComponent<Collider>());
                // 将生成的 Sphere 添加到脚本所在物体的子集
                debugSphere.transform.parent = transform;
                debugSphere.SetActive(false);
                _aimIK.solver.target = debugSphere.transform;
                debugTransform = debugSphere.transform;
            }
        }

        public void AimIKStatus(int newValue)
        {
            AimIKParameter = newValue;
        }
    }
}
