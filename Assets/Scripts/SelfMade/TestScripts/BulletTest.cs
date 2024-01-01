using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTest : MonoBehaviour
{
    private Rigidbody bulletRigidbody;
    public float speed;
    public LayerMask destroyOnCollisionWith; // 选择要销毁的层级
    [SerializeField] private Transform vfxHitYellow;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        bulletRigidbody.velocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (destroyOnCollisionWith == (destroyOnCollisionWith | (1 << collision.gameObject.layer)))
        {
            // 如果碰撞对象的层级与 destroyOnCollisionWith 匹配
            // 启动延迟销毁协程
            //StartCoroutine(DestroyBulletAfterFrames(1));
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

    public void SetBulletSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetBulletHitLayer(LayerMask HitLayer)
    {
        destroyOnCollisionWith = HitLayer;
    }
}
