using UnityEngine;
using UnityEngine.UI;

namespace Deenote.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(GridLayoutGroup))]
    public class AutoScalingGrid : MonoBehaviour
    {
        [SerializeField] GridLayoutGroup _layoutGroup;

        // TODO: 
        private void Update()
        {
            switch (_layoutGroup.constraint) {
                case GridLayoutGroup.Constraint.Flexible:
                    return;
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    var width = (transform as RectTransform).rect.width;
                    var cellSize = _layoutGroup.cellSize;
                    var elemTotalWidth = width - _layoutGroup.padding.horizontal - _layoutGroup.spacing.x * (_layoutGroup.constraintCount - 1);
                    cellSize.x = elemTotalWidth / _layoutGroup.constraintCount;
                    _layoutGroup.cellSize = cellSize;
                    break;
                case GridLayoutGroup.Constraint.FixedRowCount:
                    break;
            }
        }
    }
}
