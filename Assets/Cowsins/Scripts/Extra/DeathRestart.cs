using UnityEngine;
using UnityEngine.SceneManagement;
namespace cowsins { 
public class DeathRestart : MonoBehaviour
{
    private void Update()
    {
        if (InputManager.reloading) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
    }
}
}