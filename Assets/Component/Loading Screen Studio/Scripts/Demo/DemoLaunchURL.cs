using UnityEngine;

namespace Michsky.LSS.Demo
{
    public class DemoLaunchURL : MonoBehaviour
    {
        public void OpenURL(string url)
        {
            Application.OpenURL(url);
        }
    }
}