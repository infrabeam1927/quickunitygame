using UnityEngine;

namespace Game.Utils
{
    // Generates flat-vector-style sprites (solid shapes, no shading) at runtime
    // so the vertical slice needs no external art files.
    public static class ShapeSpriteFactory
    {
        public enum Shape { Circle, Triangle, Diamond, Square, Ring }

        public static Sprite Create(Shape shape, int size, Color color, int pixelsPerUnit = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var clear = new Color(0f, 0f, 0f, 0f);
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f) / size * 2f - 1f;
                    float ny = (y + 0.5f) / size * 2f - 1f;
                    float r2 = nx * nx + ny * ny;

                    bool inside = shape switch
                    {
                        Shape.Circle => r2 <= 1f,
                        Shape.Ring => r2 <= 1f && r2 >= 0.5f,
                        Shape.Diamond => Mathf.Abs(nx) + Mathf.Abs(ny) <= 1f,
                        Shape.Square => Mathf.Abs(nx) <= 0.9f && Mathf.Abs(ny) <= 0.9f,
                        Shape.Triangle => IsInsideTriangle(nx, ny),
                        _ => false
                    };

                    pixels[y * size + x] = inside ? color : clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        // Points along +X so it reads as a "ship" facing its forward direction.
        private static bool IsInsideTriangle(float x, float y)
        {
            Vector2 p0 = new Vector2(1f, 0f);
            Vector2 p1 = new Vector2(-0.8f, 0.8f);
            Vector2 p2 = new Vector2(-0.8f, -0.8f);
            Vector2 p = new Vector2(x, y);

            float d1 = Sign(p, p0, p1);
            float d2 = Sign(p, p1, p2);
            float d3 = Sign(p, p2, p0);

            bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
            bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
    }
}
