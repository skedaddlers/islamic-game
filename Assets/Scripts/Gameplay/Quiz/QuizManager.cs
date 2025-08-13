using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using IslamicGame.Core;

namespace IslamicGame.Gameplay
{
    public class QuizManager : MonoBehaviour
    {
        [Header("Quiz UI Elements")]
        public TextMeshProUGUI questionText;
        public Transform answersContainer;
        public GameObject answerButtonPrefab;
        public Image characterImage;
        
        [Header("Feedback UI")]
        public GameObject correctFeedback;
        public GameObject wrongFeedback;
        public ParticleSystem starsParticles;
        
        [Header("Quiz Data")]
        public List<QuizQuestion> questions = new List<QuizQuestion>();
        public int currentQuestionIndex = 0;
        
        [Header("Settings")]
        public float feedbackDuration = 1.5f;
        public int pointsPerCorrectAnswer = 100;
        public Color correctColor = Color.green;
        public Color wrongColor = Color.red;
        
        private List<Button> answerButtons = new List<Button>();
        private bool isAnswering = false;
        
        void Start()
        {
            if (questions.Count > 0)
                DisplayQuestion(currentQuestionIndex);
        }
        
        public void DisplayQuestion(int index)
        {
            if (index >= questions.Count)
            {
                CompleteQuiz();
                return;
            }
            
            currentQuestionIndex = index;
            QuizQuestion question = questions[index];
            
            // Animate question text
            questionText.text = question.questionText;
            
            // Display character if available
            if (question.characterSprite != null && characterImage != null)
            {
                characterImage.sprite = question.characterSprite;
                characterImage.transform.localScale = Vector3.zero;
                characterImage.transform.DOScale(1f, 0.5f)
                    .SetEase(Ease.OutBack);
            }
            
            // Clear previous answers
            ClearAnswerButtons();
            
            // Create answer buttons
            StartCoroutine(CreateAnswerButtons(question));
        }
        
        IEnumerator CreateAnswerButtons(QuizQuestion question)
        {
            yield return new WaitForSeconds(0.5f);
            
            for (int i = 0; i < question.answers.Count; i++)
            {
                GameObject btnObj = Instantiate(answerButtonPrefab, answersContainer);
                Button btn = btnObj.GetComponent<Button>();
                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                
                // Set answer text
                btnText.text = question.answers[i];
                
                // Animate button entrance
                btnObj.transform.localScale = Vector3.zero;
                btnObj.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.1f);
                
                // Add click listener
                int answerIndex = i;
                btn.onClick.AddListener(() => OnAnswerSelected(answerIndex));
                
                answerButtons.Add(btn);
            }
        }
        
        void OnAnswerSelected(int answerIndex)
        {
            if (isAnswering) return;
            isAnswering = true;
            
            QuizQuestion question = questions[currentQuestionIndex];
            bool isCorrect = (answerIndex == question.correctAnswerIndex);
            
            // Visual feedback on button
            Button selectedBtn = answerButtons[answerIndex];
            Image btnImage = selectedBtn.GetComponent<Image>();
            
            if (isCorrect)
            {
                // Correct answer
                btnImage.DOColor(correctColor, 0.3f);
                selectedBtn.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
                ShowCorrectFeedback();
                GameManager.Instance.AddScore(pointsPerCorrectAnswer);
                AudioManager.Instance.PlaySound("correct_answer");
            }
            else
            {
                // Wrong answer
                btnImage.DOColor(wrongColor, 0.3f);
                selectedBtn.transform.DOShakePosition(0.5f, 10f);
                ShowWrongFeedback();
                
                // Highlight correct answer
                if (question.correctAnswerIndex < answerButtons.Count)
                {
                    Image correctBtnImage = answerButtons[question.correctAnswerIndex].GetComponent<Image>();
                    correctBtnImage.DOColor(correctColor, 0.5f).SetDelay(0.5f);
                }
                
                AudioManager.Instance.PlaySound("wrong_answer");
            }
            
            // Disable all buttons
            foreach (var btn in answerButtons)
            {
                btn.interactable = false;
            }
            
            // Move to next question
            StartCoroutine(NextQuestionDelay());
        }
        
        IEnumerator NextQuestionDelay()
        {
            yield return new WaitForSeconds(feedbackDuration);
            
            // Fade out current question
            foreach (var btn in answerButtons)
            {
                btn.transform.DOScale(0f, 0.3f);
            }
            
            yield return new WaitForSeconds(0.3f);
            
            isAnswering = false;
            DisplayQuestion(currentQuestionIndex + 1);
        }
        
        void ShowCorrectFeedback()
        {
            if (correctFeedback != null)
            {
                correctFeedback.SetActive(true);
                correctFeedback.transform.localScale = Vector3.zero;
                correctFeedback.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack);
                correctFeedback.transform.DOScale(0f, 0.3f)
                    .SetDelay(feedbackDuration - 0.3f);
            }
            
            if (starsParticles != null)
                starsParticles.Play();
        }
        
        void ShowWrongFeedback()
        {
            if (wrongFeedback != null)
            {
                wrongFeedback.SetActive(true);
                wrongFeedback.transform.localScale = Vector3.zero;
                wrongFeedback.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack);
                wrongFeedback.transform.DOScale(0f, 0.3f)
                    .SetDelay(feedbackDuration - 0.3f);
            }
        }
        
        void ClearAnswerButtons()
        {
            foreach (var btn in answerButtons)
            {
                Destroy(btn.gameObject);
            }
            answerButtons.Clear();
        }
        
        void CompleteQuiz()
        {
            Debug.Log("Quiz Completed!");
            GameManager.Instance.ChangeGameState(GameState.Victory);
        }
        
        public void RestartQuiz()
        {
            currentQuestionIndex = 0;
            DisplayQuestion(0);
        }
    }
    
    [System.Serializable]
    public class QuizQuestion
    {
        public string questionText;
        public List<string> answers = new List<string>();
        public int correctAnswerIndex;
        public Sprite characterSprite;
        public AudioClip questionAudio;
    }
}