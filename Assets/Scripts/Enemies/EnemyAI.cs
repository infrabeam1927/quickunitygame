using System;
using Game.Combat;
using Game.Core;
using UnityEngine;

namespace Game.Enemies
{
    public enum EnemyBehavior { Chaser, Shooter }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private EnemyBehavior behavior = EnemyBehavior.Chaser;
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float contactDamage = 10f;
        [SerializeField] private float contactCooldown = 0.6f;
        [SerializeField] private int scoreValue = 10;

        [Header("Shooter settings")]
        [SerializeField] private float preferredRange = 5f;
        [SerializeField] private float fireInterval = 1.6f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileDamage = 8f;

        private Rigidbody2D _rb;
        private Health _health;
        private Transform _target;
        private float _contactTimer;
        private float _fireTimer;

        public event Action<EnemyAI> OnDeath;
        public int ScoreValue => scoreValue;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _health = GetComponent<Health>();
            _fireTimer = UnityEngine.Random.Range(0f, fireInterval);
        }

        private void OnEnable()
        {
            _health.OnDied += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDied -= HandleDeath;
        }

        public void SetTarget(Transform target) => _target = target;
        public void SetProjectilePrefab(GameObject prefab) => projectilePrefab = prefab;
        public void SetFirePoint(Transform point) => firePoint = point;
        public void SetBehavior(EnemyBehavior newBehavior) => behavior = newBehavior;

        private void Update()
        {
            if (_target == null) return;

            if (_contactTimer > 0f) _contactTimer -= Time.deltaTime;

            if (behavior == EnemyBehavior.Shooter)
            {
                _fireTimer -= Time.deltaTime;
                if (_fireTimer <= 0f)
                {
                    _fireTimer = fireInterval;
                    FireAtTarget();
                }
            }
        }

        private void FixedUpdate()
        {
            if (_target == null) return;

            Vector2 toTarget = (Vector2)_target.position - _rb.position;
            float distance = toTarget.magnitude;
            Vector2 direction = distance > 0.0001f ? toTarget.normalized : Vector2.zero;

            if (behavior == EnemyBehavior.Chaser)
            {
                _rb.velocity = direction * moveSpeed;
            }
            else
            {
                if (distance > preferredRange + 0.5f)
                    _rb.velocity = direction * moveSpeed;
                else if (distance < preferredRange - 0.5f)
                    _rb.velocity = -direction * moveSpeed;
                else
                    _rb.velocity = Vector2.zero;
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void FireAtTarget()
        {
            if (projectilePrefab == null || firePoint == null || _target == null) return;

            Vector2 direction = ((Vector2)_target.position - (Vector2)firePoint.position).normalized;
            GameObject go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            if (go.TryGetComponent(out Projectile projectile))
            {
                projectile.Launch(direction, projectileSpeed, projectileDamage, gameObject, ProjectileFaction.Enemy);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (behavior != EnemyBehavior.Chaser) return;
            if (_contactTimer > 0f) return;

            if (collision.collider.TryGetComponent(out IDamageable damageable))
            {
                Vector2 hitDir = ((Vector2)collision.transform.position - _rb.position).normalized;
                damageable.TakeDamage(contactDamage, transform.position, hitDir);
                _contactTimer = contactCooldown;
            }
        }

        private void HandleDeath()
        {
            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
