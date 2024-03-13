using UnityEngine;
using CustomInspector;
using System.Collections;

namespace Mashimaro
{
    public class MashimaroOffsetMoving : MonoBehaviour
    {
        [SerializeField] private float OffsetRating;
        [ShowIf(nameof(DDisable)), SerializeField] private bool Mashimaro1DOffSet; public bool DDisable() => Mashimaro2DOffSet == false;
        [ShowIf(nameof(Mashimaro1DOffSet)), SerializeField] private float SOffSetRadius;
        [ShowIf(nameof(SDisable)), SerializeField] private bool Mashimaro2DOffSet; public bool SDisable() => Mashimaro1DOffSet == false;
        [ShowIf(nameof(Mashimaro2DOffSet)), SerializeField] private float YChangeRate=2;
        [ShowIf(nameof(Mashimaro2DOffSet)), SerializeField,Min(2)] private Vector2 DOffSetRadius;


        private Vector2 PreviousDOffSetRadius;
        private Vector3 RawMashimaroOriginPosition;
        private Vector3 MashimaroOriginPosition;
        private bool movingTowardsTargetAX = true;
        private bool movingTowardsTargetAY = true;

        // Start is called before the first frame update
        void Start()
        {
            PreviousDOffSetRadius = DOffSetRadius;
            RawMashimaroOriginPosition = transform.position;
            if (Mashimaro2DOffSet)
            {
                transform.position = new Vector3(0f, transform.position.y + DOffSetRadius.y-2f,4.5f);
            }
            MashimaroOriginPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            MashimaroOffset();

            if (Mashimaro2DOffSet && DOffSetRadius != PreviousDOffSetRadius)
            {
                MashimaroOriginPosition.y = RawMashimaroOriginPosition.y + DOffSetRadius.y - 2f;
                PreviousDOffSetRadius = DOffSetRadius;
            }
        }

        private void MashimaroOffset()
        {
            if (Mashimaro1DOffSet)
            {
                float TargetPointAX = MashimaroOriginPosition.x + SOffSetRadius;
                float TargetPointBX = MashimaroOriginPosition.x - SOffSetRadius;

                if (movingTowardsTargetAX)
                {
                    SmoothMoveX(TargetPointAX);
                }
                else
                {
                    SmoothMoveX(TargetPointBX);
                }
            }

            if (Mashimaro2DOffSet)
            {
                float TargetPointAX = MashimaroOriginPosition.x + DOffSetRadius.x;
                float TargetPointAY = MashimaroOriginPosition.y + DOffSetRadius.y;
                float TargetPointBX = MashimaroOriginPosition.x - DOffSetRadius.x;
                float TargetPointBY = MashimaroOriginPosition.y - DOffSetRadius.y;

                //Debug.Log(TargetPointBY);

                if (movingTowardsTargetAX)
                {
                    SmoothMoveX(TargetPointAX);
                }
                else
                {
                    SmoothMoveX(TargetPointBX);
                }

                if (movingTowardsTargetAY)
                {
                    SmoothMoveY(TargetPointAY);
                }
                else
                {
                    SmoothMoveY(TargetPointBY);
                }
            }
        }

        private void SmoothMoveX(float targetX)
        {
            float currentX = transform.position.x;
            float newX = Mathf.Lerp(currentX, targetX, Time.deltaTime * OffsetRating);

            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            //Debug.Log(Mathf.Abs(newX - targetX));
            // 如果物体接近目标点，则切换方向
            if (Mathf.Abs(newX - targetX) < 2f)
            {
                movingTowardsTargetAX = !movingTowardsTargetAX;
            }
        }

        private void SmoothMoveY(float targetY)
        {
            float currentY = transform.position.y;
            float newY = Mathf.Lerp(currentY, targetY, Time.deltaTime * (OffsetRating*YChangeRate));

            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            //Debug.Log(Mathf.Abs(newX - targetX));
            // 如果物体接近目标点，则切换方向
            if (Mathf.Abs(newY - targetY) < 2f)
            {
                movingTowardsTargetAY = !movingTowardsTargetAY;
            }
        }
    }
}
