using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleHealth;
using BattleBullet;

public class BulletTest : MonoBehaviour
{
    private Rigidbody bulletRigidbody;
    public float speed;
    public LayerMask destroyOnCollisionWith; // 选择要销毁的层级
    private bool hasHit = false; // 是否已经命中
    public float damage = 10f; // 子弹伤害值
    public LayerMask hitLayers; // 自定义的射线检测层级
    private float rayLength = 10f; // 自定义射线长度
    private GameObject hitObject; // 保存命中的游戏对象

    //以下参数是测试参数
    private VirtualHP virtualhp;
    private HitOrNot hitornot;

    //子弹命中后的特效
    [SerializeField] private GameObject vfxHitYellow;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        bulletRigidbody.velocity = transform.forward * speed;
    }

    private void Update()
    {
        // 如果已经命中，不再进行射线检测
        if (hasHit) return;

        ShootRay();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (destroyOnCollisionWith == (destroyOnCollisionWith | (1 << collision.gameObject.layer)))
        {
            // 如果碰撞对象的层级与 destroyOnCollisionWith 匹配
            // 启动延迟销毁协程
            //StartCoroutine(DestroyBulletAfterFrames(1));
            // 在销毁子弹之前获取目标物体的信息
            if (hitObject != null)
            {
                DodingDamage();
                SetHitted();
                // 在这里可以处理目标物体的信息
                Debug.Log("销毁前获取到目标物体信息：" + hitObject.name);
            }

            Instantiate(vfxHitYellow,transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("Destory");
    //    Destroy(gameObject);
    //}

    //private IEnumerator DestroyBulletAfterFrames(int frameCount)
    //{
    //    for (int i = 0; i < frameCount; i++)
    //    {
    //        yield return null; // 等待一帧
    //    }

    //    // 在三帧后销毁子弹
    //    Destroy(gameObject);
    //}

    void ShootRay()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, rayLength, hitLayers))
        {
            // 获取命中的游戏对象
            hitObject = hitInfo.collider.gameObject;
            hasHit = true; // 设置已命中标志
            virtualhp = hitObject.GetComponent<VirtualHP>();
            hitornot = hitObject.GetComponent<HitOrNot>();
            // 在此处可以进行命中的处理，例如播放音效、添加命中效果、处理伤害等
            Debug.Log("命中了：" + hitObject.name);



            // 立刻返回或执行其他逻辑
            return;
        }
    }

    private void DodingDamage()
    {
        if (virtualhp != null)
        {
            virtualhp.AddDamage(damage); // 将伤害值传递给目标脚本的方法
        }
    }

    private void SetHitted()
    {
        if (hitornot != null)
        {
            Debug.Log("1111");
            hitornot.hitted = true;
        }
    }

    public void SetBulletSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetBulletHitLayer(LayerMask HitLayer)
    {
        destroyOnCollisionWith = HitLayer;
    }

    public void SetRayLength(float newRayLength)
    {
        rayLength = newRayLength;
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

#if UNITY_EDITOR
    // 在编辑器中使用Gizmos显示射线
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Ray ray = new Ray(transform.position, transform.forward);
        Gizmos.DrawRay(ray);
    }
#endif
}
