using UnityEngine;

namespace Game.Combat
{
    // Empty marker so projectiles can recognize arena boundaries without custom tags.
    [RequireComponent(typeof(Collider2D))]
    public class Wall : MonoBehaviour
    {
    }
}
