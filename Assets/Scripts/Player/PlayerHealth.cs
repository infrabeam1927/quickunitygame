using Game.Core;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(Health))]
    public class PlayerHealth : MonoBehaviour
    {
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            _health.OnDied -= HandleDied;
        }

        private void HandleDied()
        {
            GameManager.Instance?.ReportPlayerDied();
            gameObject.SetActive(false);
        }
    }
}
