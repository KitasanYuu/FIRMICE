using UnityEngine;
using System.Collections;
namespace cowsins {
public class SoundManager :MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource src;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.parent = null; 
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(this.gameObject); 

        src = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip,float delay, float pitchAdded, bool randomPitch, float spatialBlend)
    {
        StartCoroutine(Play(clip,delay,pitchAdded,randomPitch,spatialBlend)); 
    }

    private IEnumerator Play(AudioClip clip, float delay, float pitch,bool randomPitch, float spatialBlend)
    {
        yield return new WaitForSeconds(delay);
        src.spatialBlend = spatialBlend;
        float pitchAdded = randomPitch ? Random.Range(-pitch, pitch) : pitch; 
        src.pitch = 1 + pitchAdded;
        src.PlayOneShot(clip); 
        yield return null; 
    }
}
}

