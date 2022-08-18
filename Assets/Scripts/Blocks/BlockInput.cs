using TMPro;
using UnityEngine;

namespace Blocks
{
    public class BlockInput : Block
    {
        public BlockInput(QuizBehaviour.BlockType type, GameObject gameObject, TMP_InputField input)
        {
            Type = type;
            GameObject = gameObject;
            Input = input;
        }

        public override string ToString()
        {
            if (Input.text == "")
                return "Пусто";//MainLogic.EmptyError;
            return Input.text;
        }
    }
}