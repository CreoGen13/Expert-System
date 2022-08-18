using TMPro;
using UnityEngine;

namespace Blocks
{
    public class BlockText : Block
    {
        public BlockText(QuizBehaviour.BlockType type, GameObject gameObject, TextMeshProUGUI text)
        {
            Type = type;
            GameObject = gameObject;
            Text = text;
        }

        public override string ToString()
        {
            return Text.text;
        }
    }
}