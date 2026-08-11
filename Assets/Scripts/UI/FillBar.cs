using UnityEngine;

namespace Game.UI
{
    // Lightweight bar (health/heat) driven by anchor stretch instead of the
    // full Unity Slider hierarchy, so it can be built from two plain Images.
    public class FillBar : MonoBehaviour
    {
        [SerializeField] private RectTransform fillRect;

        public void SetFillRect(RectTransform rect) => fillRect = rect;

        public void SetFraction(float fraction)
        {
            fraction = Mathf.Clamp01(fraction);
            if (fillRect == null) return;
            fillRect.anchorMax = new Vector2(fraction, 1f);
        }
    }
}
