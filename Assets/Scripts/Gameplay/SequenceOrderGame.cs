using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using IslamicGame.Core;

namespace IslamicGame.Gameplay
{
    public class SequenceOrderGame : MonoBehaviour
    {
        public Level1AController levelController;
        [Header("UI Elements")]
        public TextMeshProUGUI instructionText;
        public Transform optionsContainer;
        public Button submitButton;
        public Button hintButton;
        public Button nextLevelButton;

        [Header("Drop Zones")]
        public List<DropZone> dropZones = new List<DropZone>();

        [Header("Image Choices")]
        public List<DraggableObject> choiceImages = new List<DraggableObject>();
        public List<int> correctOrder = new List<int>();

        [Header("Feedback")]
        public GameObject correctFeedbackPanel;
        public TextMeshProUGUI feedbackText;

        [Header("Game Settings")]
        public Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        public Color wrongColor = new Color(0.8f, 0.2f, 0.2f);
        public float shakeIntensity = 20f;
        public float shakeDuration = 0.5f;

        private bool isGameComplete = false;
        private int attempts = 0;


        public void StartGame()
        {
            InitializeGame();
        }

        void InitializeGame()
        {
            // Setup instruction text
            if (instructionText != null)
            {
                instructionText.text = "Let's put the pictures in the right timeline order!";
                instructionText.DOFade(0f, 0f);
                instructionText.DOFade(1f, 0.5f);
            }

            // Setup drop zones
            SetupDropZones();

            // Setup submit button
            if (submitButton != null)
            {
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(OnSubmitClicked);
                submitButton.interactable = false; // Initially disabled
            }

            // Setup hint button
            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(ShowHint);
            }

            foreach (var option in choiceImages)
            {
                option.Initialize();
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveAllListeners();
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
                nextLevelButton.gameObject.SetActive(false); // Initially hidden
            }
        }

        void SetupDropZones()
        {

            foreach (var dropZone in dropZones)
            {
                dropZone.onObjectDropped += OnChoiceDropped;
            }

            // animate dropzone on start
            foreach (var dropZone in dropZones)
            {
                dropZone.transform.localScale = Vector3.zero;
                dropZone.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
            }

        }

        void OnChoiceDropped(DraggableObject droppedObject)
        {
            CheckIfCanSubmit();
        }

        void CheckIfCanSubmit()
        {
            bool canSubmit = true;

            foreach (var dropZone in dropZones)
            {
                if (!dropZone.HasObject())
                {
                    canSubmit = false;
                    break;
                }
            }

            if (submitButton != null)
            {
                submitButton.interactable = canSubmit;

                if (submitButton.interactable)
                {
                    // Pulse animation when ready
                    submitButton.transform.DOScale(1.1f, 0.3f)
                        .SetLoops(2, LoopType.Yoyo);
                }
            }
        }

        void OnSubmitClicked()
        {
            if (isGameComplete) return;

            attempts++;
            AudioManager.Instance.PlayUISound("submit");

            // Check answers
            bool answerCorrect = CheckDropZone();

            if (answerCorrect)
            {
                HandleCorrectAnswer();
            }
            else
            {
                HandleWrongAnswer();
            }
        }

        bool CheckDropZone()
        {
            bool isCorrect = true;
            for (int i = 0; i < dropZones.Count; i++)
            {
                DropZone zone = dropZones[i];
                if (zone.HasObject())
                {
                    DraggableObject obj = zone.GetCurrentObject();
                    if (choiceImages[correctOrder[i]] != obj)
                    {
                        HighlightWrongZone(zone);
                        isCorrect = false;
                    }
                }
                else
                {
                    isCorrect = false; // Empty zone
                }
            }

            return isCorrect;
        }

        void HandleCorrectAnswer()
        {
            isGameComplete = true;
            AudioManager.Instance.PlaySound("success");

            // Disable further interaction
            submitButton.interactable = false;
            // foreach (var card in nameCards.Values)
            // {
            //     card.SetDraggable(false);
            // }

            // Show success feedback
            StartCoroutine(ShowSuccessFeedback());
        }

        IEnumerator ShowSuccessFeedback()
        {

            // Show "Great Job!" text
            if (correctFeedbackPanel != null)
            {
                correctFeedbackPanel.SetActive(true);
                feedbackText.text = "Great Job!";

                // Animate feedback
                correctFeedbackPanel.transform.localScale = Vector3.zero;
                correctFeedbackPanel.transform.DOScale(1.2f, 0.3f)
                    .SetEase(Ease.OutBack);

                feedbackText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 3);
            }

            // Play celebration particles
            // if (celebrationParticles != null)
            // {
            //     celebrationParticles.Play();
            // }

            // Add score
            GameManager.Instance.AddScore(100 - (attempts - 1) * 10);

            yield return new WaitForSeconds(2f);

            // Show additional feedback
            if (feedbackText != null)
            {
                feedbackText.DOFade(0f, 0.3f)
                    .OnComplete(() =>
                    {
                        feedbackText.text = "You know the events in the right order!";
                        feedbackText.DOFade(1f, 0.3f);
                    });
            }

            yield return new WaitForSeconds(3f);

            // Complete level
            CompleteLevel();
        }

        void HandleWrongAnswer()
        {
            AudioManager.Instance.PlaySound("wrong");

            // Show hint after 2 wrong attempts
            if (attempts >= 2 && hintButton != null)
            {
                hintButton.transform.DOScale(1.2f, 0.3f)
                    .SetLoops(3, LoopType.Yoyo);
            }
        }

        void HighlightWrongZone(DropZone zone)
        {
            Image zoneImage = zone.GetComponent<Image>();

            // Flash red
            zoneImage.DOColor(wrongColor, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    zoneImage.color = Color.white;
                });

            // Shake
            zone.transform.DOShakePosition(shakeDuration, shakeIntensity, 10, 90, false, true);

            // Return card to original position
            if (zone.HasObject())
            {
                DraggableObject dragObj = zone.GetCurrentObject();
                dragObj.ReturnToOriginal();
                zone.RemoveObject();
            }
        }

        void ShowHint()
        {
            AudioManager.Instance.PlayUISound("hint");

            // // Highlight correct name cards
            // foreach (var kvp in nameCards)
            // {
            //     var card = kvp.Value;
            //     if (card.nameData.isCorrectFather || card.nameData.isCorrectMother)
            //     {
            //         // Glow effect
            //         Image cardImage = card.GetComponent<Image>();
            //         cardImage.DOColor(Color.yellow, 0.5f)
            //             .SetLoops(2, LoopType.Yoyo);

            //         // Pulse
            //         card.transform.DOScale(1.1f, 0.5f)
            //             .SetLoops(2, LoopType.Yoyo);
            //     }
            // }
        }

        void CompleteLevel()
        {
            nextLevelButton.gameObject.SetActive(true);
            // animate next level button
            nextLevelButton.transform.DOScale(1.1f, 0.3f)
                .SetLoops(2, LoopType.Yoyo);
            nextLevelButton.interactable = true;

            // Mark level as complete
            // GameManager.Instance.ChangeGameState(GameState.Victory);

            // // Unlock next level
            // PlayerPrefs.SetInt("Level_1A_Complete", 1);
            // PlayerPrefs.Save();

        }

        void OnNextLevelClicked()
        {
            if (levelController != null)
            {
                levelController.NextLevel(2);
            }
        }
    }
}