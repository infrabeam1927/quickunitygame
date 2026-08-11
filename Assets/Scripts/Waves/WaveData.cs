using UnityEngine;

namespace Game.Waves
{
    public enum EnemyKind { Chaser, Shooter }

    [System.Serializable]
    public struct EnemySpawnEntry
    {
        public EnemyKind kind;
        public int count;
    }

    // Authoring asset for one wave's encounter composition.
    // Create via Assets > Create > Game > Wave Data.
    [CreateAssetMenu(fileName = "Wave_", menuName = "Game/Wave Data")]
    public class WaveData : ScriptableObject
    {
        public string waveName = "Wave";
        public EnemySpawnEntry[] enemies;
        [Tooltip("Seconds between individual enemy spawns within this wave.")]
        public float spawnInterval = 0.6f;
        [Tooltip("Seconds to wait before this wave begins spawning.")]
        public float delayBeforeWave = 2f;
        [TextArea] public string flavorText;
    }
}
