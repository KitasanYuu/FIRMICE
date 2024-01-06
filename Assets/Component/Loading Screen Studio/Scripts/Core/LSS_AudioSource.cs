using System.Collections;
using UnityEngine;

namespace Michsky.LSS
{
    public class LSS_AudioSource : MonoBehaviour
    {
        [HideInInspector] public AudioSource audioSource;
        [HideInInspector] public float audioFadeDuration;

        public void DoFadeOut()
        {
            DontDestroyOnLoad(gameObject);
            StartCoroutine("FadeOutAudioSource");
        }

        IEnumerator FadeOutAudioSource()
        {
            float elapsedTime = 0;

            while (elapsedTime < audioFadeDuration)
            {
                audioSource.volume = Mathf.Lerp(audioSource.volume, 0, elapsedTime / audioFadeDuration);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
