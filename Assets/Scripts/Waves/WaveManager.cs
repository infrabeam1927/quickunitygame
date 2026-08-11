using System;
using System.Collections;
using Game.Core;
using Game.Enemies;
using UnityEngine;

namespace Game.Waves
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private WaveData[] waves;
        [SerializeField] private GameObject chaserPrefab;
        [SerializeField] private GameObject shooterPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform player;

        public int CurrentWaveIndex { get; private set; } = -1;
        public int TotalWaves => waves != null ? waves.Length : 0;
        public int AliveEnemies { get; private set; }

        public event Action<int, string> OnWaveStarted; // index, flavor text
        public event Action OnAllWavesCleared;

        public void Configure(WaveData[] waveData, GameObject chaser, GameObject shooter, Transform[] points, Transform playerTransform)
        {
            waves = waveData;
            chaserPrefab = chaser;
            shooterPrefab = shooter;
            spawnPoints = points;
            player = playerTransform;
        }

        private void Start()
        {
            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            for (int i = 0; i < waves.Length; i++)
            {
                CurrentWaveIndex = i;
                WaveData wave = waves[i];

                yield return new WaitForSeconds(wave.delayBeforeWave);
                OnWaveStarted?.Invoke(i, wave.flavorText);

                yield return StartCoroutine(SpawnWave(wave));

                yield return new WaitUntil(() => AliveEnemies <= 0);
            }

            GameManager.Instance?.ReportAllWavesCleared();
            OnAllWavesCleared?.Invoke();
        }

        private IEnumerator SpawnWave(WaveData wave)
        {
            foreach (var entry in wave.enemies)
            {
                for (int n = 0; n < entry.count; n++)
                {
                    SpawnEnemy(entry.kind);
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }
        }

        private void SpawnEnemy(EnemyKind kind)
        {
            GameObject prefab = kind == EnemyKind.Chaser ? chaserPrefab : shooterPrefab;
            if (prefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

            Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            GameObject enemyGO = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            var ai = enemyGO.GetComponent<EnemyAI>();
            ai.SetTarget(player);
            ai.OnDeath += HandleEnemyDeath;
            AliveEnemies++;
        }

        private void HandleEnemyDeath(EnemyAI enemy)
        {
            AliveEnemies--;
            GameManager.Instance?.AddScore(enemy.ScoreValue);
        }
    }
}
