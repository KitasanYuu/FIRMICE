using UnityEngine;
namespace cowsins {
public class Hitmarker : MonoBehaviour
{

    public AudioClip crosshairSoundEffect; 
    private void Start() =>SoundManager.Instance.PlaySound(crosshairSoundEffect, .08f, .15f,true, 0);
}
}