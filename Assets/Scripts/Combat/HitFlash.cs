using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.08f;

        private SpriteRenderer _renderer;
        private Color _originalColor;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _originalColor = _renderer.color;
        }

        public void Flash()
        {
            if (!gameObject.activeInHierarchy) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _renderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            _renderer.color = _originalColor;
            _flashRoutine = null;
        }
    }
}
