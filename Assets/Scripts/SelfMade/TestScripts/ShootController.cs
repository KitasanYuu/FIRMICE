using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.Rendering.HighDefinition;

namespace BattleShoot
{
    public class ShootController : MonoBehaviour
    {
        public GameObject Target;

        public bool isAiming = false;
        public bool Fire = false;

        [SerializeField] private GameObject raycastOrigin; // 新增的字段，用于指定射线的出发点
        [SerializeField] private Transform debugTransform;
        [SerializeField] private Material customMaterial;

        private GameObject debugSphere;

        public float transitionSpeed = 0.5f;
        public bool isBlocked = false;

        private Animator _animator;
        private AimIK _aimIK;

        private bool _hasAnimator;
        private int AimIKParameter;
        private int _animIDEnterAiming;
        private int _animIDAimStatus;

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
            FIRE();
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
                Vector3 DTargetPosition = Target.transform.position;
                DTargetPosition.y = 0f;
                Vector3 DSelfPosition = transform.position;
                DSelfPosition.y = 0f;
                Vector3 TargetDir = (DTargetPosition - DSelfPosition).normalized;
                transform.forward = Vector3.Lerp(transform.forward, TargetDir, Time.deltaTime * 10f);

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDEnterAiming, true);
                    _animator.SetFloat(_animIDAimStatus, 0);
                }

                if (AimIKParameter == 1)
                {
                    AimIKParameter = 1;
                    _aimIK.enabled = true;
                    //AimIKParameter = 0;
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

        private void FIRE()
        {
            if (isAiming)
            {
                //Fire = true;
            }
            else
            {
                //Fire = false;
            }
        }

        private void ComponentInit()
        {
            _aimIK = GetComponent<AimIK>();
        }


        private void RaycastDetection()
        {
            if (raycastOrigin != null)
            {
                Ray ray = new Ray(raycastOrigin.transform.position, transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    if (debugSphere != null)
                    {
                        debugSphere.transform.position = hit.point;
                    }
                }
            }
            else
            {
                Debug.LogError("Raycast origin is not set. Please assign a GameObject to the 'raycastOrigin' field.");
            }
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
