using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
   public List<AudioClip> sources;

    private AudioSource audioSource;

    private void OnDisable()
    {
        audioSource.clip = null;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayClip(int ClipNumber)
    {
        audioSource.clip = sources[ClipNumber];
        audioSource.Play();
    }

}
