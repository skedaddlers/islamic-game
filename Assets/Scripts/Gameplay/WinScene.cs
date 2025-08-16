using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using IslamicGame.Core;

namespace IslamicGame.Gameplay
{
    public class WinScene : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI congratulationsText;
        public List<GameObject> slides = new List<GameObject>();
        public Button nextButton;
        public Button homeButton;

        void Start()
        {
            SetupUI();
        }

        void SetupUI()
        {
            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(OnNextClicked);
            }

            if (homeButton != null)
            {
                homeButton.onClick.RemoveAllListeners();
                homeButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlayUISound("button_click");
                    SceneManagerUtility.ReloadCurrentScene();
                });
            }
        }

        void OnNextClicked()
        {
            AudioManager.Instance.PlayUISound("button_click");
            // Show the next slide
            ShowNextSlide();
        }

        void ShowNextSlide()
        {
            GameObject currentSlide = slides.Find(slide => slide.activeSelf);
            if (currentSlide != null)
            {
                int currentIndex = slides.IndexOf(currentSlide);
                int nextIndex = (currentIndex + 1);

                if( nextIndex >= slides.Count - 1)
                {
                    nextButton.gameObject.SetActive(false);
                }

                CanvasGroup currentGroup = currentSlide.GetComponent<CanvasGroup>();
                CanvasGroup nextGroup = slides[nextIndex].GetComponent<CanvasGroup>();

                // Ensure the next slide is prepared for fade in
                slides[nextIndex].SetActive(true);
                nextGroup.alpha = 0f;
                // Fade out current, then hide it AFTER animation
                currentGroup.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    currentSlide.SetActive(false);
                });

                // Fade in next
                nextGroup.DOFade(1f, 0.5f);
            }
        }

    }
}