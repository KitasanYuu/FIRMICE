using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TargetDirDetec
{

    public class HitOrNot : MonoBehaviour
    {
        public bool hitted;
        public float delay;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (hitted)
            {
                StartCoroutine(ResetHittedAfterDelay(delay)); // 启动协程以延迟重置 hitted
            }
        }

        IEnumerator ResetHittedAfterDelay(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds); // 等待 x 秒
            hitted = false; // 在延迟后将 hitted 重置为 false
        }
    }
}
