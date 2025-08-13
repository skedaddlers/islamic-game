using UnityEngine;
using UnityEngine.Events;
using IslamicGame.Core;

namespace IslamicGame.Gameplay
{
    public abstract class BaseMiniGame : MonoBehaviour
    {
        [Header("Base Game Settings")]
        public string gameName;
        public int maxAttempts = 3;
        public int pointsForCompletion = 100;
        
        protected int currentAttempts = 0;
        protected bool isGameComplete = false;
        
        public UnityAction onGameComplete;
        public UnityAction<int> onScoreEarned;
        
        public abstract void StartGame();
        public abstract void ResetGame();
        
        protected virtual void CompleteGame()
        {
            if (isGameComplete) return;
            
            isGameComplete = true;
            
            // Award points
            int finalScore = CalculateScore();
            GameManager.Instance.AddScore(finalScore);
            onScoreEarned?.Invoke(finalScore);
            
            // Trigger completion event
            onGameComplete?.Invoke();
        }
        
        protected virtual int CalculateScore()
        {
            // Reduce score based on attempts
            int score = pointsForCompletion - (currentAttempts * 10);
            return Mathf.Max(score, 10); // Minimum 10 points
        }
    }
}