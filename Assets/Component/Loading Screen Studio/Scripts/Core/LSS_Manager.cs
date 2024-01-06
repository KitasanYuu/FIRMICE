using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Michsky.LSS
{
    [AddComponentMenu("Loading Screen Studio/LSS Manager")]
    public class LSS_Manager : MonoBehaviour
    {
        // Settings
        public LoadingMode loadingMode;
        public string presetName = "Default";
        public bool enableTrigger;
        public bool onTriggerExit;
        public bool loadWithTag;
        public bool startLoadingAtStart;
        public string objectTag;
        public string sceneName;

        // Smooth Audio
        [Range(0, 10)] public float audioFadeDuration = 2;
        public List<AudioSource> audioSources = new List<AudioSource>();

        // Temp Variables
        public Object[] loadingScreens;
        public int selectedLoadingIndex = 0;
        public int selectedTagIndex = 0;

        // Events
        public UnityEvent onLoadingStart;
        public List<GameObject> dontDestroyOnLoad = new List<GameObject>();

        // Additive Only
        [SerializeField] private List<string> loadedScenes = new List<string>();

#if UNITY_EDITOR
        public bool lockSelection = false;
#endif

        public enum LoadingMode { Single, Additive }

        void Start()
        {
            if (startLoadingAtStart == true && loadingMode == LoadingMode.Single) { LoadScene(sceneName); }
            else if (startLoadingAtStart == true && loadingMode == LoadingMode.Additive) { LoadSceneAdditive(sceneName); }
        }

        public void SetPreset(string styleName)
        {
            presetName = styleName;
        }

        public void LoadScene(string sceneName)
        {
            LSS_LoadingScreen.presetName = presetName;
            LSS_LoadingScreen.LoadScene(sceneName);

            for (int i = 0; i < dontDestroyOnLoad.Count; i++) { DontDestroyOnLoad(dontDestroyOnLoad[i]); }
            if (audioSources.Count != 0) 
            {
                foreach (AudioSource asg in audioSources)
                {
                    LSS_AudioSource tempAS = asg.gameObject.AddComponent<LSS_AudioSource>();
                    tempAS.audioSource = asg;
                    tempAS.audioFadeDuration = audioFadeDuration;
                    tempAS.DoFadeOut();
                }
            }

            onLoadingStart.Invoke();
        }

        public void LoadSceneAdditive(string sceneName)
        {
            LSS_LoadingScreen.LoadSceneAdditive(sceneName, presetName);
            loadedScenes.Add(SceneManager.GetSceneByName(sceneName).name);

            if (audioSources.Count != 0)
            {
                foreach (AudioSource asg in audioSources)
                {
                    if (asg == null)
                        continue;

                    LSS_AudioSource tempAS = asg.gameObject.AddComponent<LSS_AudioSource>();
                    tempAS.audioSource = asg;
                    tempAS.audioFadeDuration = audioFadeDuration;
                    tempAS.DoFadeOut();
                }
            }

            onLoadingStart.Invoke();
        }

        public void LoadSceneAdditiveInstant(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            loadedScenes.Add(SceneManager.GetSceneByName(sceneName).name);

            if (audioSources.Count != 0)
            {
                foreach (AudioSource asg in audioSources)
                {
                    if (asg == null)
                        continue;

                    LSS_AudioSource tempAS = asg.gameObject.AddComponent<LSS_AudioSource>();
                    tempAS.audioSource = asg;
                    tempAS.audioFadeDuration = audioFadeDuration;
                    tempAS.DoFadeOut();
                }
            }

            onLoadingStart.Invoke();
        }

        public void UnloadAdditiveScene(string sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
            loadedScenes.Remove(sceneName);
        }

        public void UnloadActiveAdditiveScenes()
        {
            foreach (string tempScene in loadedScenes) { SceneManager.UnloadSceneAsync(tempScene); }
            loadedScenes.Clear();
        }

        public void SetSingle()
        {
            loadingMode = LoadingMode.Single;
        }

        public void SetAdditive()
        {
            loadingMode = LoadingMode.Additive;
        }

        void DoTriggerActions()
        {
            LSS_LoadingScreen.presetName = presetName;

            if (loadingMode == LoadingMode.Single) { LSS_LoadingScreen.LoadScene(sceneName); }
            if (loadingMode == LoadingMode.Additive) { LSS_LoadingScreen.LoadSceneAdditive(sceneName); }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enableTrigger || onTriggerExit) { return; }
            if (loadWithTag && other.gameObject.tag != objectTag) { return; }

            DoTriggerActions();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enableTrigger || onTriggerExit) { return; }
            if (loadWithTag && other.gameObject.tag != objectTag) { return; }

            DoTriggerActions();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!enableTrigger || !onTriggerExit) { return; }
            if (loadWithTag && other.gameObject.tag != objectTag) { return; }

            DoTriggerActions();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enableTrigger || !onTriggerExit) { return; }
            if (loadWithTag && other.gameObject.tag != objectTag) { return; }

            DoTriggerActions();
        }
    }
}