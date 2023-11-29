using UnityEngine;

namespace AimAvoidedL
{
    public class AimAviodL : MonoBehaviour
    {
        public bool isBlockedL = false;
        [SerializeField] public LayerMask AvoidColliderLayerMask;
        private int collisionCount = 0; // ���ڼ�¼�ض��㼶����ײ������

        private void Update()
        {
            isBlockedL = collisionCount > 0; // �������ײ�崦��ָ���㼶��isBlocked Ϊ true������Ϊ false
        }

        private void OnCollisionEnter(Collision collision)
        {
            if ((AvoidColliderLayerMask & (1 << collision.gameObject.layer)) != 0) // ����Ƿ���ָ���㼶
            {
                collisionCount++; // ����ָ���㼶����ײ����������
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if ((AvoidColliderLayerMask & (1 << collision.gameObject.layer)) != 0) // ����Ƿ���ָ���㼶
            {
                collisionCount--; // �뿪ָ���㼶����ײ����������
                collisionCount = Mathf.Max(0, collisionCount); // ȷ����������С����
            }
        }
    }
}
