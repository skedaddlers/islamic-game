using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using IslamicGame.Core;
using IslamicGame.Environment;

namespace IslamicGame.Gameplay
{
    public class Level1AController : MonoBehaviour
    {

        [Header("Scene Panels")]
        public RectTransform scenesContainer; // Parent container for all scenes
        public List<GameObject> storyScenes = new List<GameObject>(); // All story panels
        public List<bool> useSequence = new List<bool>();
        public GameObject gameplayScene; // The actual game panel
        public GameObject gameplayScene2;
        public GameObject finalGameScene; // The final game panel
        public GameObject winScene;

        [Header("Title Screen")]
        public GameObject titlePanel;
        public Button playButton;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI subtitleText;

        [Header("Story Elements")]
        public Button nextButton;
        public TextMeshProUGUI nextButtonText;
        public Button skipButton;
        public float slideTransitionDuration = 0.7f;
        public int gameInterruptionIndex = 5;
        public Ease slideEase = Ease.OutQuart;

        [Header("Game Elements")]
        public Button restartButton;
        public GameObject gameInstructionPanel;
        public Button startGameButton;

        private int currentSceneIndex = -1;
        private float sceneWidth = 1920f; // Default scene width

        void Start()
        {
            InitializeLevel();
            DisableAllTexts();

            AddTweenToButtons();
        }

        void InitializeLevel()
        {
            // Get actual scene width
            if (scenesContainer != null)
            {
                sceneWidth = scenesContainer.rect.width;
            }

            // Setup play button
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayClicked);

                // Pulse animation for play button
                playButton.transform.DOScale(1.1f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            // Setup navigation buttons
            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(GoToNextScene);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(SkipToGame);
            }

            // Animate title entrance
            AnimateTitleScreen();

            // Set Next Button to be the first scene child
            if (nextButton != null && storyScenes.Count > 0)
            {
                nextButton.transform.SetParent(storyScenes[0].transform, false);
            }

            if(restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlayUISound("button_click");
                    SceneManagerUtility.ReloadCurrentScene();
                });
            }

        }



        void AnimateTitleScreen()
        {
            if (titleText != null)
            {
                titleText.transform.localScale = Vector3.zero;
                titleText.transform.DOScale(1f, 0.5f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.2f);
            }

            if (subtitleText != null)
            {
                subtitleText.DOFade(0f, 0f);
                subtitleText.DOFade(1f, 0.5f)
                    .SetDelay(0.5f);
            }

            Sequence seq = DOTween.Sequence();
            for (int i = titlePanel.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = titlePanel.transform.GetChild(i);
                ForegroundAnimator animator = child.GetComponent<ForegroundAnimator>();
                if (animator != null)
                {
                    seq.Append(animator.AnimateInTween());
                }
            }
            seq.Play();


            // Animate clouds (if you have cloud objects)
            // AnimateClouds();
        }

        void OnPlayClicked()
        {
            AudioManager.Instance.PlayUISound("game_start");

            // Stop play button animation
            playButton.transform.DOKill();

            // Hide title panel and start story
            titlePanel.transform.DOScale(0.8f, 0.3f)
                .SetEase(Ease.InBack);
            titlePanel.GetComponent<CanvasGroup>().DOFade(0f, 0.3f)
                .OnComplete(() =>
                {
                    titlePanel.SetActive(false);
                    scenesContainer.gameObject.SetActive(true);
                    StartStorySequence();
                });
        }

        void StartStorySequence()
        {
            currentSceneIndex = 0;
            ShowStoryScene(0);
        }

        void ShowStoryScene(int index)
        {
            // Debug.Log($"Showing story scene {index}");
            GameObject scene = storyScenes[index];
            scene.SetActive(true);

            nextButton.transform.SetParent(scene.transform, false);

            // Slide in from right
            // RectTransform sceneRect = scene.GetComponent<RectTransform>();
            // sceneRect.anchoredPosition = new Vector2(sceneWidth, 0);
            // sceneRect.DOAnchorPosX(0, slideTransitionDuration)
            //     .SetEase(slideEase)
            //     .OnComplete(() => AnimateSceneContent(scene));

            // fade in current scene
            if (index > gameInterruptionIndex)
            {
                scene.GetComponent<CanvasGroup>().DOFade(0f, 0f);
                scene.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
            }
            AnimateSceneContent(scene);

            // Hide previous scene if exists
            if (index > 0)
            {
                GameObject prevScene = storyScenes[index - 1];
                prevScene.SetActive(false);

                if (index > gameInterruptionIndex)
                {
                    RectTransform prevRect = prevScene.GetComponent<RectTransform>();
                    // fade out previous scene
                    prevScene.GetComponent<CanvasGroup>().DOFade(0f, 0.5f)
                        .OnComplete(() => prevScene.SetActive(false));
                }
            }
        }

        void DisableAllTexts()
        {
            foreach (var scene in storyScenes)
            {
                TextMeshProUGUI[] texts = scene.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    if (text != nextButtonText)
                        text.DOFade(0f, 0f);
                }
            }
        }

        void AnimateSceneContent(GameObject scene)
        {
            // Animate text elements
            TextMeshProUGUI[] texts = scene.GetComponentsInChildren<TextMeshProUGUI>();

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != nextButtonText)
                {
                    texts[i].DOFade(0f, 0f);
                    texts[i].DOFade(1f, 0.5f)
                        .SetDelay(i * 0.2f);
                }
            }

            // Animate images
            Image[] images = scene.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.gameObject.name.Contains("Character") ||
                    img.gameObject.name.Contains("Icon"))
                {
                    img.transform.localScale = Vector3.zero;
                    img.transform.DOScale(1f, 0.5f)
                        .SetEase(Ease.OutBack)
                        .SetDelay(0.3f);
                }
            }

            // Slide in foreground
            int checkScene = currentSceneIndex;
            if (currentSceneIndex > gameInterruptionIndex)
            {
                checkScene--;
            }
            if (!useSequence[checkScene])
            {
                foreach (Transform child in scene.transform)
                {
                    ForegroundAnimator animator = child.GetComponent<ForegroundAnimator>();
                    if (animator != null)
                    {
                        animator.AnimateIn();
                    }
                }
            }
            else
            {
                // Use sequence for foreground animations
                Sequence seq = DOTween.Sequence();
                foreach (Transform child in scene.transform)
                {
                    ForegroundAnimator animator = child.GetComponent<ForegroundAnimator>();
                    if (animator != null)
                    {
                        seq.Append(animator.AnimateInTween());
                    }
                }
                seq.Play();
            }

            // Play narration if available
            PlayNarration(currentSceneIndex);
        }

        void GoToNextScene()
        {
            AudioManager.Instance.PlayUISound("button_click");
            currentSceneIndex++;
            if (currentSceneIndex == gameInterruptionIndex)
            {
                // Interrupt the game to show instructions
                ShowGameInstructions();
                return;
            }
            else if (currentSceneIndex >= storyScenes.Count + 1)
            {
                NextLevel(3);
            }
            else if (currentSceneIndex > gameInterruptionIndex)
            {
                ShowStoryScene(currentSceneIndex - 1);
            }
            else
            {
                ShowStoryScene(currentSceneIndex);
            }
        }

        void SkipToGame()
        {
            AudioManager.Instance.PlayUISound("button_click");

            // Hide all story scenes
            foreach (var scene in storyScenes)
            {
                scene.SetActive(false);
            }

            ShowGameInstructions();
        }

        void ShowGameInstructions()
        {
            if (gameInstructionPanel != null)
            {
                gameInstructionPanel.SetActive(true);

                // Animate instruction panel
                gameInstructionPanel.transform.localScale = Vector3.zero;
                gameInstructionPanel.transform.DOScale(1f, 0.5f)
                    .SetEase(Ease.OutBack);

                // Setup start game button
                if (startGameButton != null)
                {
                    startGameButton.onClick.RemoveAllListeners();
                    startGameButton.onClick.AddListener(StartGame);

                    // Pulse animation
                    startGameButton.transform.DOScale(1.1f, 0.5f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            }
            else
            {
                // Go directly to game if no instruction panel
                StartGame();
            }
        }

        void StartGame()
        {
            AudioManager.Instance.PlayUISound("game_start");

            if (gameInstructionPanel != null)
            {
                gameInstructionPanel.transform.DOScale(0f, 0.3f)
                    .OnComplete(() => gameInstructionPanel.SetActive(false));
            }

            // Show gameplay scene
            if (gameplayScene != null)
            {
                gameplayScene.SetActive(true);

                // Initialize the drag and drop game
                var dragDropGame = gameplayScene.GetComponent<ProphetParentsGame>();
                if (dragDropGame != null)
                {
                    dragDropGame.StartGame();
                }
            }
        }

        // void AnimateClouds()
        // {
        //     // Find and animate cloud objects
        //     GameObject[] clouds = GameObject.FindGameObjectsWithTag("Cloud");
        //     foreach (var cloud in clouds)
        //     {
        //         CloudMover mover = cloud.GetComponent<CloudMover>();
        //         if (mover == null)
        //         {
        //             mover = cloud.AddComponent<CloudMover>();
        //             mover.moveSpeed = Random.Range(0.5f, 2f);
        //             mover.moveDistance = 30f;
        //         }
        //     }
        // }

        void PlayNarration(int sceneIndex)
        {
            // Play narration audio for the current scene
            string narrationClip = $"narration_scene_{sceneIndex}";
            AudioManager.Instance.PlaySound(narrationClip);
        }

        public void NextLevel(int index)
        {
            if (index == 1)
            {
                gameplayScene.SetActive(false);
                gameplayScene2.SetActive(true);
                gameplayScene2.GetComponent<SpeechBubbleGame>().StartGame();
            }
            else if (index == 2)
            {
                gameplayScene2.SetActive(false);
                GoToNextScene();
            }
            else if (index == 3)
            {
                // Show final game scene
                finalGameScene.SetActive(true);
                finalGameScene.GetComponent<SequenceOrderGame>().StartGame();
            }
            else if (index == 4)
            {
                finalGameScene.transform.DOScale(0f, 0.5f).SetEase(Ease.OutBack)
                    .OnComplete(() => finalGameScene.SetActive(false));

                winScene.SetActive(true);
            }
        }
        
        void AddTweenToButtons()
        {
            // Find all Button components in this scene (you can narrow this to specific parents if you want)
            Button[] allButtons = FindObjectsOfType<Button>(true); // include inactive buttons

            foreach (var btn in allButtons)
            {
                Transform btnTransform = btn.transform;

                // Make sure starting scale is stored
                Vector3 originalScale = btnTransform.localScale;

                // Handle hover (requires EventTrigger or pointer events)
                UnityEngine.EventSystems.EventTrigger trigger = btn.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null)
                {
                    trigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                }

                // Pointer Enter
                UnityEngine.EventSystems.EventTrigger.Entry entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) =>
                {
                    btnTransform.DOKill(); // stop ongoing tweens
                    btnTransform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack);
                });
                trigger.triggers.Add(entryEnter);

                // Pointer Exit
                UnityEngine.EventSystems.EventTrigger.Entry entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) =>
                {
                    btnTransform.DOKill();
                    btnTransform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack);
                });
                trigger.triggers.Add(entryExit);

                // On Click
                btn.onClick.AddListener(() =>
                {
                    btnTransform.DOKill();
                    btnTransform.DOScale(originalScale * 0.9f, 0.1f).SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            btnTransform.DOScale(originalScale, 0.1f).SetEase(Ease.InOutSine);
                        });
                });
            }
        }

    }
}