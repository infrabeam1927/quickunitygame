using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Button restartButton;

        public void Configure(GameObject panelObject, Text title, Text finalScore, Button restart)
        {
            panel = panelObject;
            titleText = title;
            finalScoreText = finalScore;
            restartButton = restart;
        }

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
        }

        // Subscribing in Start (not OnEnable/Awake) guarantees GameManager.Instance is
        // already assigned, since every object's Awake has run before any Start.
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameWon += HandleWon;
                GameManager.Instance.OnGameLost += HandleLost;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameWon -= HandleWon;
                GameManager.Instance.OnGameLost -= HandleLost;
            }
        }

        private void HandleWon() => Show("Garden Secured");
        private void HandleLost() => Show("The Seed-Core Has Fallen");

        private void Show(string title)
        {
            if (panel != null) panel.SetActive(true);
            if (titleText != null) titleText.text = title;
            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = $"Final Score: {GameManager.Instance.Score}";
            Time.timeScale = 0f;
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
