#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace Deenote.Library
{
    public static class UnityWithUtils
    {
        public static void WithLocalPositionX(this Transform transform, float x)
            => transform.localPosition = transform.localPosition with { x = x };

        public static void WithLocalPositionZ(this Transform transform, float z)
            => transform.localPosition = transform.localPosition with { z = z };

        public static void WithLocalPositionXZ(this Transform transform, float x, float z)
            => transform.localPosition = transform.localPosition with { x = x, z = z };

        public static void WithLocalScaleX(this Transform transform, float x)
            => transform.localScale = transform.localScale with { x = x };

        public static void WithLocalScaleY(this Transform transform, float y)
            => transform.localScale = transform.localScale with { y = y };

        public static void WithLocalScaleXY(this Transform transform, float x, float y)
            => transform.localScale = transform.localScale with { x = x, y = y };

        public static void WithAnchoredMinMaxX(this RectTransform transform, float x)
        {
            transform.anchorMin = transform.anchorMin with { x = x };
            transform.anchorMax = transform.anchorMax with { x = x };
        }

        public static void WithColorAlpha(this SpriteRenderer spriteRenderer, float alpha)
            => spriteRenderer.color = spriteRenderer.color with { a = alpha };

        public static void WithColorSolid(this SpriteRenderer spriteRenderer, Color color)
            => spriteRenderer.color = color with { a = spriteRenderer.color.a };

        public static void WithColorAlpha(this Graphic graphic, float alpha)
            => graphic.color = graphic.color with { a = alpha };
    }
}