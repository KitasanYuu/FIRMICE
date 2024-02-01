using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace BattleShoot
{
    public class ShootController : MonoBehaviour
    {
        public bool isAiming = false;
        public bool Fire = false;

        [SerializeField] private GameObject raycastOrigin; // 新增的字段，用于指定射线的出发点
        [SerializeField] private Transform debugTransform;
        [SerializeField] private Material customMaterial;

        private GameObject debugSphere;

        public float transitionSpeed = 0.5f;
        public bool isBlocked = false;

        private Animator _animator;
        private int _animIDEnterAiming;
        private int _animIDAimStatus;

        private void Start()
        {
            ComponentInit();
            _animator = GetComponent<Animator>();
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

                //Vector3 playerDirection = (PlayerPosition() - transform.position).normalized;
                //transform.forward = Vector3.Lerp(transform.forward, playerDirection, Time.deltaTime * 5f);
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
            // 初始化NPC组件，可以根据需要扩展
        }

        private Vector3 PlayerPosition()
        {
            return Vector3.zero;
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

                debugTransform = debugSphere.transform;
            }
        }
    }
}
