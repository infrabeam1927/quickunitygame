using Game.Core;
using Game.Enemies;
using Game.Player;
using UnityEngine;

namespace Game.Combat
{
    public enum ProjectileFaction { Player, Enemy }

    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private GameObject impactVfxPrefab;

        private Rigidbody2D _rb;
        private float _damage;
        private GameObject _owner;
        private ProjectileFaction _faction;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
        }

        public void SetImpactVfx(GameObject prefab) => impactVfxPrefab = prefab;

        public void Launch(Vector2 direction, float speed, float damage, GameObject owner, ProjectileFaction faction)
        {
            _damage = damage;
            _owner = owner;
            _faction = faction;
            _rb.velocity = direction.normalized * speed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == _owner) return;

            if (other.GetComponent<Wall>() != null)
            {
                SpawnImpact();
                Destroy(gameObject);
                return;
            }

            bool isValidTarget = _faction == ProjectileFaction.Player
                ? other.GetComponent<EnemyAI>() != null
                : other.GetComponent<PlayerHealth>() != null;

            if (isValidTarget && other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage, transform.position, _rb.velocity.normalized);
                SpawnImpact();
                Destroy(gameObject);
            }
        }

        private void SpawnImpact()
        {
            if (impactVfxPrefab != null)
            {
                Instantiate(impactVfxPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
