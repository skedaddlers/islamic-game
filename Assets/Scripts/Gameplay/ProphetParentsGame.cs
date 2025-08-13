using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using IslamicGame.Core;

namespace IslamicGame.Gameplay
{
    public class ProphetParentsGame : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI instructionText;
        public Transform nameOptionsContainer;
        public Transform dropZonesContainer;
        public Button submitButton;
        public Button hintButton;
        
        [Header("Drop Zones")]
        public DropZone fatherDropZone;
        public DropZone motherDropZone;
        public Image prophetImage;
        public TextMeshProUGUI prophetNameText;
        
        [Header("Name Options")]
        public List<NameOption> availableNames = new List<NameOption>();
        public GameObject namePrefab;
        
        [Header("Feedback")]
        public GameObject correctFeedbackPanel;
        public TextMeshProUGUI feedbackText;
        public ParticleSystem celebrationParticles;
        public GameObject wrongIndicator;
        
        [Header("Game Settings")]
        public Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        public Color wrongColor = new Color(0.8f, 0.2f, 0.2f);
        public float shakeIntensity = 20f;
        public float shakeDuration = 0.5f;
        
        private Dictionary<string, DraggableNameCard> nameCards = new Dictionary<string, DraggableNameCard>();
        private bool isGameComplete = false;
        private int attempts = 0;
        
        [System.Serializable]
        public class NameOption
        {
            public string arabicName;
            public string englishName;
            public bool isCorrectFather;
            public bool isCorrectMother;
        }
        
        public void StartGame()
        {
            InitializeGame();
            AnimateGameStart();
        }
        
        void InitializeGame()
        {
            // Setup instruction text
            if (instructionText != null)
            {
                instructionText.text = "Put the name of Prophet Muhammad's mom and dad into the boxes";
                instructionText.DOFade(0f, 0f);
                instructionText.DOFade(1f, 0.5f);
            }
            
            // Create name cards
            CreateNameCards();
            
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
            
            // Setup Prophet image
            if (prophetImage != null)
            {
                prophetImage.transform.localScale = Vector3.zero;
                prophetImage.transform.DOScale(1f, 0.5f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.3f);
            }
        }
        
        void CreateNameCards()
        {
            for (int i = 0; i < availableNames.Count; i++)
            {
                GameObject cardObj = Instantiate(namePrefab, nameOptionsContainer);
                DraggableNameCard card = cardObj.GetComponent<DraggableNameCard>();
                // Debug.Log($"Creating name card for {availableNames[i].englishName}");
                if (card == null)
                {
                    card = cardObj.AddComponent<DraggableNameCard>();
                }
                
                // Setup card data
                
                // Position cards
                RectTransform rect = cardObj.GetComponent<RectTransform>();
                float yPos = (i % 4) * 100f - 200f;
                float xPos = -(i / 4) * 100f;
                Vector3 targetPosition = new Vector2(xPos, yPos);
                Vector3 targetScale = Vector3.one;

                card.Initialize(targetPosition, targetScale);
                card.SetupCard(availableNames[i]);
                card.onDropped += OnNameCardDropped;

                // Animate entrance
                cardObj.transform.localScale = Vector3.zero;
                cardObj.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.1f);
                
                nameCards[availableNames[i].englishName] = card;
            }
        }
        
        void SetupDropZones()
        {
            // Setup father drop zone
            if (fatherDropZone != null)
            {
                fatherDropZone.acceptedTag = "FatherName";
                fatherDropZone.onObjectDropped += OnNameDroppedToZone;
            }
            
            // Setup mother drop zone
            if (motherDropZone != null)
            {
                motherDropZone.acceptedTag = "MotherName";
                motherDropZone.onObjectDropped += OnNameDroppedToZone;
            }
        }
        
        void OnNameCardDropped(DraggableNameCard card, DropZone zone)
        {
            // Check if both zones have names
            CheckIfCanSubmit();
        }
        
        void OnNameDroppedToZone(DraggableObject droppedObject)
        {
            CheckIfCanSubmit();
        }
        
        void CheckIfCanSubmit()
        {
            bool fatherHasName = fatherDropZone != null && fatherDropZone.HasObject();
            bool motherHasName = motherDropZone != null && motherDropZone.HasObject();
            
            if (submitButton != null)
            {
                submitButton.interactable = fatherHasName && motherHasName;
                
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
            bool fatherCorrect = CheckDropZone(fatherDropZone, true, false);
            bool motherCorrect = CheckDropZone(motherDropZone, false, true);
            
            if (fatherCorrect && motherCorrect)
            {
                // Both correct!
                HandleCorrectAnswer();
            }
            else
            {
                // Show wrong feedback
                HandleWrongAnswer(fatherCorrect, motherCorrect);
            }
        }
        
        bool CheckDropZone(DropZone zone, bool checkFather, bool checkMother)
        {
            if (zone == null || !zone.HasObject()) return false;
            
            DraggableNameCard card = zone.GetCurrentObject().GetComponent<DraggableNameCard>();
            if (card == null) return false;
            
            if (checkFather)
                return card.nameData.isCorrectFather;
            else if (checkMother)
                return card.nameData.isCorrectMother;
            
            return false;
        }
        
        void HandleCorrectAnswer()
        {
            isGameComplete = true;
            AudioManager.Instance.PlaySound("success");
            
            // Disable further interaction
            submitButton.interactable = false;
            foreach (var card in nameCards.Values)
            {
                card.SetDraggable(false);
            }
            
            // Show success feedback
            StartCoroutine(ShowSuccessFeedback());
        }
        
        IEnumerator ShowSuccessFeedback()
        {
            // Highlight correct answers
            fatherDropZone.GetComponent<Image>().DOColor(correctColor, 0.5f);
            motherDropZone.GetComponent<Image>().DOColor(correctColor, 0.5f);
            
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
            if (celebrationParticles != null)
            {
                celebrationParticles.Play();
            }
            
            // Add score
            GameManager.Instance.AddScore(100 - (attempts - 1) * 10);
            
            yield return new WaitForSeconds(2f);
            
            // Show additional feedback
            if (feedbackText != null)
            {
                feedbackText.DOFade(0f, 0.3f)
                    .OnComplete(() => {
                        feedbackText.text = "Abdullah and Aminah are the parents of Prophet Muhammad (pbuh)";
                        feedbackText.DOFade(1f, 0.3f);
                    });
            }
            
            yield return new WaitForSeconds(3f);
            
            // Complete level
            CompleteLevel();
        }
        
        void HandleWrongAnswer(bool fatherCorrect, bool motherCorrect)
        {
            AudioManager.Instance.PlaySound("wrong");
            
            // Shake and highlight wrong answers
            if (!fatherCorrect && fatherDropZone != null)
            {
                HighlightWrongZone(fatherDropZone);
            }
            
            if (!motherCorrect && motherDropZone != null)
            {
                HighlightWrongZone(motherDropZone);
            }
            
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
                .OnComplete(() => {
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
            
            // Highlight correct name cards
            foreach (var kvp in nameCards)
            {
                var card = kvp.Value;
                if (card.nameData.isCorrectFather || card.nameData.isCorrectMother)
                {
                    // Glow effect
                    Image cardImage = card.GetComponent<Image>();
                    cardImage.DOColor(Color.yellow, 0.5f)
                        .SetLoops(2, LoopType.Yoyo);
                    
                    // Pulse
                    card.transform.DOScale(1.1f, 0.5f)
                        .SetLoops(2, LoopType.Yoyo);
                }
            }
        }
        
        void AnimateGameStart()
        {
            // Animate game elements entrance
            if (prophetNameText != null)
            {
                prophetNameText.text = "Muhammad ﷺ";
                prophetNameText.DOFade(0f, 0f);
                prophetNameText.DOFade(1f, 1f);
            }
            
            // Draw connection lines (optional visual enhancement)
            DrawFamilyConnections();
        }
        
        void DrawFamilyConnections()
        {
            // You can add line renderers or UI lines here to connect the elements
            // This is optional but adds visual appeal
        }
        
        void CompleteLevel()
        {
            // Mark level as complete
            GameManager.Instance.ChangeGameState(GameState.Victory);
            
            // Unlock next level
            PlayerPrefs.SetInt("Level_1A_Complete", 1);
            PlayerPrefs.Save();
        }
    }
}