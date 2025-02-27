#nullable enable

using Deenote.UIFramework.Controls;
using UnityEngine;

namespace Deenote.UIFramework
{
    [CreateAssetMenu(
      fileName = nameof(UIStaticSpriteResources),
      menuName = $"Deenote.UIFramework/{nameof(UIStaticSpriteResources)}")]
    public sealed class UIStaticSpriteResources : ScriptableObject
    {
#nullable disable
        public Sprite SolidRad0;
        public Sprite SolidRad2;
        public Sprite SolidRad4;
        public Sprite SolidRad8;
        public Sprite SolidRad12;
        public Sprite SolidRad0044;
        public Sprite SolidRad8000;
        public Sprite SolidRad0800;
        public Sprite SolidCircle;
        public Sprite BorderRad4;
        public Sprite BorderRad8;
        public Sprite BorderRad12;
        public Sprite BorderCircle;
    }
}