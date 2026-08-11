using System;
using UnityEngine;

namespace Game.Core
{
    public enum GameState { Playing, Won, Lost }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Playing;
        public int Score { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action OnGameWon;
        public event Action OnGameLost;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void AddScore(int amount)
        {
            if (State != GameState.Playing) return;
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void ReportPlayerDied()
        {
            if (State != GameState.Playing) return;
            State = GameState.Lost;
            OnGameLost?.Invoke();
        }

        public void ReportAllWavesCleared()
        {
            if (State != GameState.Playing) return;
            State = GameState.Won;
            OnGameWon?.Invoke();
        }
    }
}
