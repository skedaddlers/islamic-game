using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

namespace IslamicGame.Core
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject gameplayPanel;
        public GameObject pausePanel;
        public GameObject victoryPanel;
        public GameObject gameOverPanel;
        public GameObject loadingPanel;
        public GameObject settingsPanel;
        
        [Header("HUD Elements")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI levelText;
        public Image[] starImages;
        public Button pauseButton;
        
        [Header("Loading Screen")]
        public Image loadingBar;
        public TextMeshProUGUI loadingText;
        
        [Header("Animation Settings")]
        public float panelFadeDuration = 0.3f;
        public float popupScaleDuration = 0.4f;
        public Ease panelEase = Ease.OutQuad;
        
        void Start()
        {
            InitializeUI();
        }
        
        void InitializeUI()
        {
            // Hide all panels except main menu
            HideAllPanels();
            ShowPanel(mainMenuPanel);
            
            // Setup button listeners
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => GameManager.Instance.ChangeGameState(GameState.Paused));
        }
        
        public void ShowPanel(GameObject panel, bool animate = true)
        {
            if (panel == null) return;
            
            panel.SetActive(true);
            
            if (animate)
            {
                CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = panel.AddComponent<CanvasGroup>();
                
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, panelFadeDuration).SetEase(panelEase);
                
                panel.transform.localScale = Vector3.one * 0.8f;
                panel.transform.DOScale(1f, popupScaleDuration).SetEase(Ease.OutBack);
            }
        }
        
        public void HidePanel(GameObject panel, bool animate = true)
        {
            if (panel == null) return;
            
            if (animate)
            {
                CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = panel.AddComponent<CanvasGroup>();
                
                canvasGroup.DOFade(0f, panelFadeDuration)
                    .SetEase(panelEase)
                    .OnComplete(() => panel.SetActive(false));
                
                panel.transform.DOScale(0.8f, panelFadeDuration).SetEase(Ease.InQuad);
            }
            else
            {
                panel.SetActive(false);
            }
        }
        
        public void HideAllPanels()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (gameplayPanel) gameplayPanel.SetActive(false);
            if (pausePanel) pausePanel.SetActive(false);
            if (victoryPanel) victoryPanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);
            if (loadingPanel) loadingPanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);
        }
        
        public void ShowLoadingScreen(bool show)
        {
            if (show)
                ShowPanel(loadingPanel);
            else
                HidePanel(loadingPanel);
        }
        
        public void UpdateLoadingProgress(float progress)
        {
            if (loadingBar)
                loadingBar.fillAmount = progress;
            if (loadingText)
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
        }
        
        public void UpdateScore(int score)
        {
            if (scoreText)
            {
                scoreText.text = score.ToString();
                // Punch scale animation
                scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 2, 0.5f);
            }
        }
        
        public void UpdateLevel(int level)
        {
            if (levelText)
                levelText.text = $"Level {level}";
        }
        
        public void ShowVictoryPopup(int stars)
        {
            ShowPanel(victoryPanel);
            StartCoroutine(AnimateStars(stars));
        }
        
        IEnumerator AnimateStars(int count)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (i < count)
                {
                    yield return new WaitForSeconds(0.3f);
                    starImages[i].transform.localScale = Vector3.zero;
                    starImages[i].gameObject.SetActive(true);
                    starImages[i].transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
                    
                    // Play star sound
                    AudioManager.Instance.PlaySound("star_collect");
                }
            }
        }
        
        public void ShowGameOverPopup()
        {
            ShowPanel(gameOverPanel);
        }
        
        public void ShowCompletionScreen()
        {
            // Show special completion screen for finishing all levels
            Debug.Log("All levels completed!");
        }
        
        // Button callbacks
        public void OnPlayButtonClicked()
        {
            HidePanel(mainMenuPanel);
            GameManager.Instance.LoadLevel(1);
        }
        
        public void OnResumeButtonClicked()
        {
            HidePanel(pausePanel);
            GameManager.Instance.ChangeGameState(GameState.Playing);
        }
        
        public void OnRestartButtonClicked()
        {
            HideAllPanels();
            GameManager.Instance.RestartLevel();
        }
        
        public void OnNextLevelButtonClicked()
        {
            HidePanel(victoryPanel);
            GameManager.Instance.NextLevel();
        }
        
        public void OnMainMenuButtonClicked()
        {
            HideAllPanels();
            ShowPanel(mainMenuPanel);
            GameManager.Instance.ChangeGameState(GameState.Menu);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        
        public void OnSettingsButtonClicked()
        {
            ShowPanel(settingsPanel);
        }
        
        public void OnCloseSettingsButtonClicked()
        {
            HidePanel(settingsPanel);
        }
    }
}
