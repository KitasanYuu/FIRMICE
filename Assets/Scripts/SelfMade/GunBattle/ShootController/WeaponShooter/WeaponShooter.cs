using UnityEngine;
using AvatarMain;
using playershooting;
using BattleShoot;
using CustomInspector;
using BattleHealth;
using System.Collections;
using TestField;
using DataManager;
using TargetDirDetec;

namespace Battle
{
    [RequireComponent(typeof(WeaponIdentity))]
    public class WeaponShooter : MonoBehaviour
    {
        [SerializeField, ReadOnly] private string WeaponID;
        [ReadOnly, SerializeField] private bool UsingAIControl;
        [ReadOnly, SerializeField] private bool UsingMasterControl;

        [HideInInspector] public Vector3 AimDir;

        [SerializeField] private bool RayMethod = true;
        [SerializeField] private bool InstanceMethod;
        [Space2(20)]

        [SerializeField] private bool Semi;
        [SerializeField] private bool LimitAmmo = true;
        [SerializeField] private bool needReload;
        [SerializeField, ShowIf(nameof(LimitAmmo))] private int MaxAmmoCarry;
        [SerializeField, ShowIf(nameof(LimitAmmo))] private int AmmoPreMag;
        [ReadOnly, SerializeField, ShowIf(nameof(LimitAmmo))] private int CurrentBulletCount;

        [Space2(20)]
        public GameObject Shooter;
        // 子弹生成位置
        [SerializeField] private Transform spawnBulletPosition;
        // 子弹生成后的速度
        [Tooltip("决定了射击精度，数值越大精度越偏"), Range(0, 1)] public float shootingAccuracy = 0.3f; // 调整这个值来控制射击的精度

        [HorizontalLine("WeaponSettings", 2, FixedColor.Gray)]
        public float fireRate = 0.5f; // 0.5秒为例，可以根据需要调整射速
        [Space2(20)]
        public float reloadDuration = 2.0f; // 将重新加载的持续时间设为公共属性
        public float FastreloadDuration = 1.0f; // 将重新加载的持续时间设为公共属性

        [HorizontalLine("BulletSettings", 2, FixedColor.Gray)]
        // 子弹预制件或游戏对象
        [SerializeField, ReadOnly] private GameObject bulletPrefab;
        //子弹命中后的特效
        [SerializeField, ReadOnly] private GameObject VFXHitEffect;
        [Space2(20)]
        [SerializeField, ReadOnly] public float bulletspeed;
        [SerializeField, ReadOnly] public float BulletDamage;
        [SerializeField, ReadOnly] public float ArmorBreak;
        [SerializeField, ReadOnly] public float DetectRayLength;
        [SerializeField, ReadOnly] public LayerMask DestoryLayer;
        //子弹速度与射线检测的比值 射线长度 = 子弹速度*SRR
        private float CSRR = 0.0385f;
        private float LSRR = 0.05f;
        private float SRR;

        private float shootingAccuracyScale = 0.1f; // 缩放因子，用于将用户输入的值缩放到 0 到 1 的范围内

        float lastShootTime = 0f;

        private bool CanFireNow = false;

        private float PreviousBulletSpeed = 0;

        private bool Reloading = false;
        private bool reloadingInProgress = false;


        //获取脚本
        private BasicInput _input;
        private ShootController shootController;
        private TPSShootController tpsShootController;
        private WeaponIdentity WID;
        private bool Basicinput;
        private bool Tpsshootcontroller;
        private bool shootcontroller;

        private GameObject BulletContainer;

        [HideInInspector]
        public Vector3 PredictedAimPoint;

        // Start is called before the first frame update
        void Start()
        {
            ComponemetInit();
            SOInit();
            ContainerInit();
            ParameterInit();
            SRRSelecting();

        }

        // Update is called once per frame
        void Update()
        {
            ComponentRetake();
            BulletSpeedWatcher();
            FireAction();
            ReloadProgress();
        }

        private void OnEnable()
        {
            ContainerInit();
        }

        private void FireAction()
        {
            if (Shooter != null)
            {
                // 开火
                if ((UsingAIControl && shootController.Fire && shootController.isAiming) || (UsingMasterControl && tpsShootController.Fire && tpsShootController.isAiming))
                {
                    if (!LimitAmmo ||(!needReload && (MaxAmmoCarry > 0 || CurrentBulletCount > 0)) || CurrentBulletCount > 0)
                    {
                        // 获取当前时间
                        float currentTime = Time.time;

                        if (currentTime - lastShootTime > fireRate || CanFireNow)
                        {
                            Vector3 aimDir = Vector3.zero;

                            if (Tpsshootcontroller)
                                aimDir = (tpsShootController.TmouseWorldPosition - spawnBulletPosition.position).normalized;
                            else if (shootcontroller && RayMethod && !InstanceMethod)
                            {
                                InstanceMethod = false;
                                shootController.UsingTrajectoryPredict = false;
                                aimDir = (shootController.hitpoint - spawnBulletPosition.position).normalized;
                            }
                            else if (shootcontroller && InstanceMethod && !shootController.UsingTrajectoryPredict)
                            {
                                aimDir = (shootController.hitpoint - spawnBulletPosition.position).normalized;
                            }
                            else if (shootcontroller && InstanceMethod && shootController.UsingTrajectoryPredict)
                            {
                                // 计算射击方向和距离
                                Vector3 shootDirection = shootController.hitpoint - spawnBulletPosition.position;
                                float shootDistance = shootDirection.magnitude;

                                // 计算子弹的飞行时间
                                float bulletTravelTime = shootDistance / bulletspeed;

                                // 预测目标位置，同时考虑 X、Y 和 Z 轴上的移动
                                Vector3 predictedTargetPosition = shootController.hitpoint + bulletTravelTime * shootController.hitpointVelocity;
                                PredictedAimPoint = predictedTargetPosition;

                                // 重新计算 aimDir
                                aimDir = (predictedTargetPosition - spawnBulletPosition.position).normalized;
                            }

                            if (shootController)
                            {
                                float scaledShootingAccuracy = Mathf.Clamp01(shootingAccuracy * shootingAccuracyScale);
                                Vector3 randomOffset = Random.insideUnitSphere * scaledShootingAccuracy;
                                aimDir += randomOffset;

                                // 确保在射击前将 aimDir 归一化
                                aimDir.Normalize();

                                AimDir = aimDir;
                            }


                            if (InstanceMethod)
                            {
                                if (needReload && CurrentBulletCount > 0)
                                {
                                    CurrentBulletCount--;
                                }
                                else if (LimitAmmo && !needReload && (MaxAmmoCarry > 0 || CurrentBulletCount > 0))
                                {
                                    if (CurrentBulletCount > 0)
                                        CurrentBulletCount--;
                                    else
                                        MaxAmmoCarry--;
                                }
                                else if (CurrentBulletCount <= 0)
                                {
                                    CurrentBulletCount = 0;
                                }

                                // 生成子弹实例
                                GameObject bulletInstance = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up), BulletContainer.transform);
                                // 获取子弹脚本并设置速度
                                Bullet bullet = bulletInstance.GetComponent<Bullet>();
                                if (bullet != null)
                                {
                                    bullet?.SetParameter(Shooter, DetectRayLength, DestoryLayer, VFXHitEffect, bulletspeed, BulletDamage, ArmorBreak);
                                    //Debug.Log(bulletspeed * SRR);
                                    //Debug.LogError("BulletSpwaned");
                                }
                                else
                                {
                                    Debug.LogError("BulletTest component not found on instantiated object.");
                                }
                            }
                            else if (RayMethod)
                            {
                                // 在这里执行射线投射
                                Ray shootRay = new Ray(spawnBulletPosition.position, aimDir);
                                if (Physics.Raycast(shootRay, out RaycastHit shootRaycastHit))
                                {
                                    // 获取击中点的坐标
                                    Vector3 hitPoint = shootRaycastHit.point;
                                    GameObject HitObject = shootRaycastHit.collider.gameObject;
                                    RayDamageIn(HitObject);
                                    // 生成特效
                                    Instantiate(VFXHitEffect, hitPoint, Quaternion.identity, BulletContainer.transform);
                                    // 在这里处理射击命中的逻辑，例如对击中物体造成伤害或触发其他效果等
                                    //Debug.Log("射击命中：" + shootRaycastHit.collider.gameObject.name + "，击中坐标：" + hitPoint);

                                    if (needReload && CurrentBulletCount > 0)
                                    {
                                        CurrentBulletCount--;
                                    }
                                    else if (LimitAmmo && !needReload && (MaxAmmoCarry > 0 || CurrentBulletCount > 0))
                                    {
                                        if (CurrentBulletCount > 0)
                                            CurrentBulletCount--;
                                        else
                                            MaxAmmoCarry--;
                                    }
                                    else if (CurrentBulletCount <= 0)
                                    {
                                        CurrentBulletCount = 0;
                                    }
                                }
                            }

                            CanFireNow = false;

                            if (Semi)
                            {
                                if (UsingMasterControl)
                                {
                                    _input.shoot = false;
                                }
                            }


                            lastShootTime = currentTime;
                        }
                    }
                    else
                    {
                        ReloadProgress(true);
                    }
                }
            }
        }

        #region 子弹的Reload逻辑
        private void ReloadProgress(bool ReloadStart = false)
        {
            if (needReload)
            {
                if (!Reloading && !reloadingInProgress && (Input.GetKeyDown(KeyCode.R) || ReloadStart))
                {
                    if (CurrentBulletCount < AmmoPreMag)
                    {
                        StartCoroutine(ReloadCoroutine());
                    }
                }
            }
        }

        private IEnumerator ReloadCoroutine()
        {
            Reloading = true;
            reloadingInProgress = true;

            int missingBulletCount = AmmoPreMag - CurrentBulletCount;

            if (MaxAmmoCarry >= missingBulletCount)
            {
                if (CurrentBulletCount == 0)
                {
                    // 模拟延迟后执行重新加载的逻辑
                    yield return new WaitForSeconds(reloadDuration);

                    MaxAmmoCarry -= AmmoPreMag;
                    CurrentBulletCount = AmmoPreMag;
                }
                else if (CurrentBulletCount > 0)
                {
                    yield return new WaitForSeconds(FastreloadDuration);
                    MaxAmmoCarry -= missingBulletCount;
                    CurrentBulletCount += missingBulletCount;
                }

            }
            else if (MaxAmmoCarry > 0 && MaxAmmoCarry < missingBulletCount)
            {
                if (CurrentBulletCount == 0)
                {
                    // 模拟延迟后执行重新加载的逻辑
                    yield return new WaitForSeconds(reloadDuration);

                    CurrentBulletCount += MaxAmmoCarry;
                    MaxAmmoCarry = 0;
                }
                else if (CurrentBulletCount > 0)
                {
                    yield return new WaitForSeconds(FastreloadDuration);

                    CurrentBulletCount += MaxAmmoCarry;
                    MaxAmmoCarry = 0;
                }

            }

            CanFireNow = true;
            Reloading = false;
            reloadingInProgress = false;
        }

        #endregion

        private void BulletRefresh(int newvalue)
        {
            CurrentBulletCount = newvalue;
        }

        private void RayDamageIn(GameObject rayHitTarget)
        {
            HitCollisionDetection HCD = rayHitTarget.GetComponent<HitCollisionDetection>();
            VirtualHP virtualHp = null;
            GameObject currentTarget = rayHitTarget;

            if (HCD != null)
                HCD.RayBulletHitSet(gameObject);

            // 循环遍历父级对象直到找到VirtualHP组件或没有更多的父级
            while (currentTarget != null && virtualHp == null)
            {
                virtualHp = currentTarget.GetComponent<VirtualHP>();
                if (virtualHp == null)
                {
                    // 如果当前对象上没有VirtualHP组件，则尝试获取其父级对象
                    if (currentTarget.transform.parent != null)
                    {
                        currentTarget = currentTarget.transform.parent.gameObject;
                    }
                    else
                    {
                        // 如果已经没有父级，则退出循环
                        currentTarget = null;
                    }
                }
            }

            // 如果找到了VirtualHP组件，则对其应用伤害
            if (virtualHp != null)
            {
                virtualHp.AddDamage(BulletDamage, ArmorBreak, Shooter);
            }
        }


        //用来追踪BulletSpeed有没有在途中变化
        private void BulletSpeedWatcher()
        {
            if (PreviousBulletSpeed != bulletspeed)
            {
                SRRSelecting();
                PreviousBulletSpeed = bulletspeed;
            }
        }

        private void ComponentRetake()
        {
            if (Shooter == null)
            {
                Shooter = FindFatherObj(true);
                if (Shooter != null)
                {
                    ComponemetInit();
                    ContainerInit();
                    SRRSelecting();
                }

            }
        }

        private void SRRSelecting()
        {
            if (bulletspeed < 1000f)
            {
                SRR = CSRR;
            }
            else if (bulletspeed >= 1000 && bulletspeed <= 2000)
            {
                SRR = LSRR;
            }
            else
            {
                SRR = bulletspeed;
            }

            DetectRayLength = bulletspeed * SRR;
        }

        //寻找带有ShootController的Shooter
        public GameObject FindFatherObj(bool Start = false)
        {
            if (Start)
            {
                // 从当前物体开始查找
                GameObject currentObject = gameObject;

                // 在当前物体以及其父级中查找
                while (currentObject != null)
                {
                    // 在当前物体上查找TPSShootController脚本
                    TPSShootController tpsShootController = currentObject.GetComponent<TPSShootController>();
                    if (tpsShootController != null)
                    {
                        // 如果找到，则返回当前物体
                        return currentObject;
                    }

                    // 在当前物体上查找ShootController脚本
                    ShootController shootController = currentObject.GetComponent<ShootController>();
                    if (shootController != null)
                    {
                        // 如果找到，则返回当前物体
                        return currentObject;
                    }

                    // 继续向上查找
                    currentObject = currentObject.transform.parent?.gameObject;
                }
            }
            // 如果未找到任何包含所需脚本的物体，则返回null
            return null;
        }

        private void ParameterInit()
        {
            WeaponID = WID.WeaponID;
            CurrentBulletCount = AmmoPreMag;
            PreviousBulletSpeed = bulletspeed;

            ResourceReader RR = new ResourceReader();
            CSVReader csv = new CSVReader();
            var WeaponData = csv.GetDataByID("weapons", WeaponID);
            int BulletMethod = (int)WeaponData["BulletFireMode"];
            if (BulletMethod == 1)
                InstanceMethod = true;
            else
                InstanceMethod = false;
            RayMethod = !InstanceMethod;
            LimitAmmo = (bool)WeaponData["IsLimitAmmo"];
            needReload = (bool)WeaponData["needReload"];

            fireRate = (float)WeaponData["FireRate"];

            string HitEffect = (string)WeaponData["HitParticle"];
            VFXHitEffect = RR.GetGameObject("BulletEffect", HitEffect);
            BulletDamage = (float)WeaponData["Damage"];
            ArmorBreak = (float)WeaponData["ArmorBreak"];

            if (InstanceMethod)
            {
                string BulletPrefab = (string)WeaponData["BulletModel"];
                bulletPrefab = RR.GetGameObject("BulletEffect", BulletPrefab);
                bulletspeed = (float)WeaponData["BulletSpeed"];
            }

            if (needReload)
            {
                AmmoPreMag = (int)WeaponData["AmmoPreMag"];
                reloadDuration = (float)WeaponData["ReloadDuration"];
                FastreloadDuration = 0.5f * reloadDuration;
            }

            if(LimitAmmo)
            {
                MaxAmmoCarry = (int)WeaponData["MaxAmmoCarry"];
            }
        }

        private void ComponemetInit()
        {
            WID = GetComponent<WeaponIdentity>();

            if (Shooter != null)
            {
                Basicinput = Shooter.TryGetComponent<BasicInput>(out _input);
                Tpsshootcontroller = Shooter.TryGetComponent<TPSShootController>(out tpsShootController);
                shootcontroller = Shooter.TryGetComponent<ShootController>(out shootController);
            }

            if (Tpsshootcontroller && Basicinput)
            {
                UsingMasterControl = true;
                UsingAIControl = false;
                //Debug.Log("BulletSpwanInitSuccess!" + "  " + gameObject.name + "  " + "CurrentUsing 'TPSMasterControl'");
            }

            if (shootcontroller)
            {
                shootController.GetWeaponShooter(this);
                UsingAIControl = true;
                UsingMasterControl = false;
                //Debug.Log("BulletSpwanInitSuccess!"+ "  " + gameObject.name + "  " + "CurrentUsing 'AIControl'");
            }
        }

        private void SOInit()
        {
            GlobalLayerMaskSetting GLMS = Resources.Load<GlobalLayerMaskSetting>("GlobalSettings/GlobalLayerMaskSetting");
            if (GLMS != null)
                DestoryLayer = GLMS.BulletDestoryLayer;
            else
                Debug.LogError("WeaponShooter Cannot Find GlobalLayerMaskSetting!");
        }

        private void ContainerInit()
        {
            if (Shooter != null)
            {
                string ContainerName = "VFXBulletContainer";

                Transform existingObject = Shooter.transform.Find(ContainerName);
                if (existingObject == null)
                {
                    GameObject VFXBulletContainer = new GameObject(ContainerName);
                    BulletContainer = VFXBulletContainer;
                    VFXBulletContainer.transform.SetParent(Shooter.transform);

                    // 添加组件
                    Identity ID = VFXBulletContainer.AddComponent<Identity>();
                    ID.SetMasterID(ContainerName);
                    Counter counter = VFXBulletContainer.AddComponent<Counter>();

                }
            }
        }
    }
}