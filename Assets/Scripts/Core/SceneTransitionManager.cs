using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

namespace IslamicGame.Core
{
    public class SceneTransitionManager : Singleton<SceneTransitionManager>
    {
        [Header("Transition Elements")]
        public Image fadeImage;
        public CanvasGroup transitionCanvas;
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Transition Settings")]
        public float fadeDuration = 0.5f;
        public Color fadeColor = Color.black;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (fadeImage == null)
            {
                CreateFadeImage();
            }
        }
        
        void CreateFadeImage()
        {
            GameObject canvasObj = new GameObject("TransitionCanvas");
            canvasObj.transform.SetParent(transform);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            
            transitionCanvas = canvasObj.AddComponent<CanvasGroup>();
            
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform);
            
            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = fadeColor;
            
            RectTransform rect = imageObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            // Start fully transparent
            transitionCanvas.alpha = 0;
        }
        
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }
        
        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadSceneRoutine(sceneIndex));
        }
        
        IEnumerator LoadSceneRoutine(string sceneName)
        {
            // Fade out
            yield return FadeOut();
            
            // Load scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Fade in
            yield return FadeIn();
        }
        
        IEnumerator LoadSceneRoutine(int sceneIndex)
        {
            // Fade out
            yield return FadeOut();
            
            // Load scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Fade in
            yield return FadeIn();
        }
        
        public Coroutine FadeOut()
        {
            return StartCoroutine(FadeRoutine(0, 1));
        }
        
        public Coroutine FadeIn()
        {
            return StartCoroutine(FadeRoutine(1, 0));
        }
        
        IEnumerator FadeRoutine(float from, float to)
        {
            float elapsed = 0;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float curveValue = fadeCurve.Evaluate(t);
                transitionCanvas.alpha = Mathf.Lerp(from, to, curveValue);
                yield return null;
            }
            
            transitionCanvas.alpha = to;
        }
    }
}