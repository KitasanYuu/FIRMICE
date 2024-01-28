using UnityEngine;

namespace TargetDirDetec
{

    public class HitCollisionDetection : MonoBehaviour
    {
        public LayerMask targetLayer;
        public int HitDir;

        private void Update()
        {
            //HitDir = 0;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if ((targetLayer.value & 1 << collision.gameObject.layer) != 0)
            {
                // 将相对位置转换到本地坐标系
                Vector3 relativePosition = transform.InverseTransformPoint(collision.contacts[0].point);
                Debug.Log(collision.gameObject);
                CalculateHitDir(relativePosition);

            }
        }

        private void CalculateHitDir(Vector3 relativePosition)
        {
            Quaternion rotation = Quaternion.LookRotation(relativePosition, Vector3.up);
            float angle = rotation.eulerAngles.y;
            //Debug.Log(angle);

            if (angle >= 0f && angle < 45f)
            {
                HitDir = 1;
            }
            else if (angle >= 45f && angle < 135f)
            {
                HitDir = -2;
            }
            else if (angle >= 135f && angle < 225f)
            {
                HitDir = -1;
            }
            else if (angle >= 225f && angle < 315f)
            {
                HitDir = -2;
            }
            else if (angle >= 315f || angle < 0f)
            {
                HitDir = 1;
            }

            // You can use hitDir as needed in your game logic
            //Debug.Log("HitDir: " + HitDir);
        }

        public void HitDirStatus(int HitDirStatus)
        {
            HitDir = HitDirStatus;
            Debug.Log(HitDirStatus);
        }
    }
}