using Game.Combat;
using UnityEngine;

namespace Game.Player
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimPivot;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireRate = 6f; // shots per second
        [SerializeField] private float projectileSpeed = 14f;
        [SerializeField] private float projectileDamage = 12f;

        [Header("Heat")]
        [SerializeField] private float maxHeat = 100f;
        [SerializeField] private float heatPerShot = 9f;
        [SerializeField] private float heatCooldownRate = 45f; // per second
        [SerializeField] private float overheatLockDuration = 1.2f;

        private float _fireCooldown;
        private float _heat;
        private bool _overheated;
        private float _overheatTimer;

        public float HeatFraction => _heat / maxHeat;
        public bool IsOverheated => _overheated;

        public void SetProjectilePrefab(GameObject prefab) => projectilePrefab = prefab;
        public void SetFirePoint(Transform point) => firePoint = point;
        public void SetAimPivot(Transform pivot) => aimPivot = pivot;

        private void Update()
        {
            if (_fireCooldown > 0f) _fireCooldown -= Time.deltaTime;

            if (_overheated)
            {
                _overheatTimer -= Time.deltaTime;
                if (_overheatTimer <= 0f)
                {
                    _overheated = false;
                    _heat = 0f;
                }
            }
            else
            {
                _heat = Mathf.Max(0f, _heat - heatCooldownRate * Time.deltaTime);
            }

            if (Input.GetMouseButton(0) && !_overheated && _fireCooldown <= 0f)
            {
                Fire();
            }
        }

        private void Fire()
        {
            if (projectilePrefab == null || firePoint == null) return;

            _fireCooldown = 1f / fireRate;
            _heat += heatPerShot;
            if (_heat >= maxHeat)
            {
                _heat = maxHeat;
                _overheated = true;
                _overheatTimer = overheatLockDuration;
            }

            Vector2 direction = aimPivot != null ? (Vector2)aimPivot.right : Vector2.right;
            GameObject go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            if (go.TryGetComponent(out Projectile projectile))
            {
                projectile.Launch(direction, projectileSpeed, projectileDamage, gameObject, ProjectileFaction.Player);
            }
        }
    }
}
