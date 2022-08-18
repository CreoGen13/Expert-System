using UnityEngine;
using UnityEngine.UI;

namespace Blocks
{
    public class DiagramBlock : MonoBehaviour
    {
        [SerializeField] private LayoutElement _layoutElementFull;
        [SerializeField] private LayoutElement _layoutElementEmpty;

        public void SetBlock(int full, int empty)
        {
            _layoutElementFull.flexibleHeight = full;
            _layoutElementEmpty.flexibleHeight = empty;
        }
    }
}
