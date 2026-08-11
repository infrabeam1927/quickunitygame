using System.Collections;
using UnityEngine;

namespace Game.UI
{
    public class ScreenShake : MonoBehaviour
    {
        public static ScreenShake Instance { get; private set; }

        private Vector3 _originalLocalPos;
        private Coroutine _shakeRoutine;

        private void Awake()
        {
            Instance = this;
            _originalLocalPos = transform.localPosition;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Shake(float duration = 0.15f, float magnitude = 0.15f)
        {
            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float damper = 1f - (elapsed / duration);
                Vector2 offset = UnityEngine.Random.insideUnitCircle * magnitude * damper;
                transform.localPosition = _originalLocalPos + (Vector3)offset;
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = _originalLocalPos;
            _shakeRoutine = null;
        }
    }
}
