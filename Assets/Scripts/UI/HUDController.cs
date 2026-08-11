using Game.Core;
using Game.Player;
using Game.Waves;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private FillBar healthBar;
        [SerializeField] private FillBar heatBar;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text waveText;
        [SerializeField] private Health playerHealth;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private WaveManager waveManager;

        public void Configure(FillBar health, FillBar heat, Text score, Text wave, Health player, WeaponController weaponController, WaveManager waves)
        {
            healthBar = health;
            heatBar = heat;
            scoreText = score;
            waveText = wave;
            playerHealth = player;
            weapon = weaponController;
            waveManager = waves;
        }

        // Subscriptions happen in Start (not OnEnable/Awake) because GameManager.Instance
        // is only guaranteed to be assigned once every object's Awake has run.
        private void Start()
        {
            if (playerHealth != null) playerHealth.OnHealthChanged += UpdateHealth;
            if (GameManager.Instance != null) GameManager.Instance.OnScoreChanged += UpdateScore;
            if (waveManager != null) waveManager.OnWaveStarted += UpdateWave;

            if (playerHealth != null) UpdateHealth(playerHealth.Current, playerHealth.MaxHealth);
            UpdateScore(0);
            if (waveText != null) waveText.text = $"Wave 0 / {(waveManager != null ? waveManager.TotalWaves : 0)}";
        }

        private void OnDestroy()
        {
            if (playerHealth != null) playerHealth.OnHealthChanged -= UpdateHealth;
            if (GameManager.Instance != null) GameManager.Instance.OnScoreChanged -= UpdateScore;
            if (waveManager != null) waveManager.OnWaveStarted -= UpdateWave;
        }

        private void Update()
        {
            if (heatBar != null && weapon != null)
            {
                heatBar.SetFraction(weapon.HeatFraction);
            }
        }

        private void UpdateHealth(float current, float max)
        {
            if (healthBar != null) healthBar.SetFraction(max > 0f ? current / max : 0f);
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"Score: {score}";
        }

        private void UpdateWave(int index, string flavor)
        {
            if (waveText != null) waveText.text = $"Wave {index + 1} / {waveManager.TotalWaves}";
        }
    }
}
