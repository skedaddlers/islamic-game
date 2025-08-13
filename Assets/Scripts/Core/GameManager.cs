using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

namespace IslamicGame.Core
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("Game Settings")]
        public GameState currentState = GameState.Menu;
        public int currentLevel = 1;
        public int totalLevels = 10;
        
        [Header("Player Progress")]
        public int playerScore = 0;
        public int starsEarned = 0;
        public List<LevelData> levelProgress = new List<LevelData>();
        
        [Header("Game Configuration")]
        public float transitionDuration = 0.5f;
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        
        public delegate void OnGameStateChanged(GameState newState);
        public event OnGameStateChanged onGameStateChanged;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeDOTween();
            LoadPlayerProgress();
        }
        
        void InitializeDOTween()
        {
            DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
            DOTween.SetTweensCapacity(500, 50);
            DOTween.defaultEaseType = Ease.OutQuad;
        }
        
        public void ChangeGameState(GameState newState)
        {
            currentState = newState;
            onGameStateChanged?.Invoke(newState);
            
            switch (newState)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    HandleGameOver();
                    break;
                case GameState.Victory:
                    HandleVictory();
                    break;
            }
        }
        
        public void LoadLevel(int levelNumber)
        {
            currentLevel = levelNumber;
            StartCoroutine(LoadLevelAsync($"Level_{levelNumber}"));
        }
        
        IEnumerator LoadLevelAsync(string sceneName)
        {
            UIManager.Instance.ShowLoadingScreen(true);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                UIManager.Instance.UpdateLoadingProgress(progress);
                yield return null;
            }
            
            UIManager.Instance.ShowLoadingScreen(false);
            ChangeGameState(GameState.Playing);
        }
        
        public void RestartLevel()
        {
            LoadLevel(currentLevel);
        }
        
        public void NextLevel()
        {
            if (currentLevel < totalLevels)
            {
                LoadLevel(currentLevel + 1);
            }
            else
            {
                // All levels completed
                UIManager.Instance.ShowCompletionScreen();
            }
        }
        
        public void AddScore(int points)
        {
            playerScore += points;
            UIManager.Instance.UpdateScore(playerScore);
        }
        
        void HandleVictory()
        {
            SaveLevelProgress();
            UIManager.Instance.ShowVictoryPopup(CalculateStars());
        }
        
        void HandleGameOver()
        {
            UIManager.Instance.ShowGameOverPopup();
        }
        
        int CalculateStars()
        {
            // Simple star calculation based on score
            if (playerScore >= 1000) return 3;
            if (playerScore >= 700) return 2;
            if (playerScore >= 400) return 1;
            return 0;
        }
        
        void SaveLevelProgress()
        {
            PlayerPrefs.SetInt($"Level_{currentLevel}_Score", playerScore);
            PlayerPrefs.SetInt($"Level_{currentLevel}_Stars", CalculateStars());
            PlayerPrefs.SetInt("UnlockedLevel", Mathf.Max(currentLevel + 1, PlayerPrefs.GetInt("UnlockedLevel", 1)));
            PlayerPrefs.Save();
        }
        
        void LoadPlayerProgress()
        {
            for (int i = 1; i <= totalLevels; i++)
            {
                LevelData data = new LevelData
                {
                    levelNumber = i,
                    isUnlocked = i <= PlayerPrefs.GetInt("UnlockedLevel", 1),
                    highScore = PlayerPrefs.GetInt($"Level_{i}_Score", 0),
                    stars = PlayerPrefs.GetInt($"Level_{i}_Stars", 0)
                };
                levelProgress.Add(data);
            }
        }
        
        public void ToggleSound()
        {
            soundEnabled = !soundEnabled;
            AudioManager.Instance.SetSoundEnabled(soundEnabled);
            PlayerPrefs.SetInt("SoundEnabled", soundEnabled ? 1 : 0);
        }
        
        public void ToggleMusic()
        {
            musicEnabled = !musicEnabled;
            AudioManager.Instance.SetMusicEnabled(musicEnabled);
            PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        }
    }
    
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Victory,
        Loading
    }
    
    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public bool isUnlocked;
        public int highScore;
        public int stars;
    }
}
