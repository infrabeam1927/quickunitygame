using Game.Core;
using Game.UI;
using UnityEngine;

namespace Game.Combat
{
    // Wires a Health component's events to hit-flash / screen-shake / death VFX juice.
    [RequireComponent(typeof(Health))]
    public class DamageFeedback : MonoBehaviour
    {
        [SerializeField] private HitFlash hitFlash;
        [SerializeField] private bool shakeCameraOnHit;
        [SerializeField] private GameObject deathVfxPrefab;

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        public void SetDeathVfx(GameObject prefab) => deathVfxPrefab = prefab;
        public void SetHitFlash(HitFlash flash) => hitFlash = flash;
        public void SetShakeCameraOnHit(bool shake) => shakeCameraOnHit = shake;

        private void OnEnable()
        {
            _health.OnDamaged += HandleDamaged;
            _health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            _health.OnDamaged -= HandleDamaged;
            _health.OnDied -= HandleDied;
        }

        private void HandleDamaged(Vector2 hitPoint, Vector2 hitDirection)
        {
            if (hitFlash != null) hitFlash.Flash();
            if (shakeCameraOnHit) ScreenShake.Instance?.Shake();
        }

        private void HandleDied()
        {
            if (deathVfxPrefab != null)
            {
                Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            }
            ScreenShake.Instance?.Shake(0.2f, 0.2f);
        }
    }
}
