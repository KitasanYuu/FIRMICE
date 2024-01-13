using UnityEngine;

public class CursorClickDetection : MonoBehaviour
{
    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 发射一条射线从摄像机的位置到鼠标点击的位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 如果射线击中了物体
            if (Physics.Raycast(ray, out hit))
            {
                // 获取被击中的物体
                GameObject clickedObject = hit.collider.gameObject;

                // 在这里可以添加处理点击物体的逻辑
                Debug.Log("Clicked on: " + clickedObject.name);
            }
        }
    }
}
