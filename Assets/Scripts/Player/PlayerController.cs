using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Transform aimPivot;
        [SerializeField] private Camera mainCamera;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 1.2f;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private Vector2 _aimDirection = Vector2.right;

        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;

        public Vector2 AimDirection => _aimDirection;
        public bool IsDashing => _isDashing;
        public float DashCooldownFraction => dashCooldown <= 0f ? 1f : 1f - Mathf.Clamp01(_dashCooldownTimer / dashCooldown);

        public void SetAimPivot(Transform pivot) => aimPivot = pivot;
        public void SetCamera(Camera camera) => mainCamera = camera;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
            _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            UpdateAim();

            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.LeftShift) && !_isDashing && _dashCooldownTimer <= 0f && _moveInput.sqrMagnitude > 0f)
            {
                StartDash();
            }

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f) _isDashing = false;
            }
        }

        private void FixedUpdate()
        {
            if (_isDashing)
            {
                Vector2 dashDir = _moveInput.sqrMagnitude > 0f ? _moveInput : _aimDirection;
                _rb.velocity = dashDir * dashSpeed;
            }
            else
            {
                _rb.velocity = _moveInput * moveSpeed;
            }
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;
        }

        private void UpdateAim()
        {
            if (mainCamera == null) return;

            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            _aimDirection = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;

            if (aimPivot != null && _aimDirection.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
                aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}
