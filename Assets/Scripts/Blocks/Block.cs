using TMPro;
using UnityEngine;

namespace Blocks
{
    public class Block
    {
        public QuizBehaviour.BlockType Type { get; protected set; }
        public GameObject GameObject{ get; protected set; }

        public TMP_Dropdown Dropdown{ get; protected set; }
        public TextMeshProUGUI Text{ get; protected set; }
        public TMP_InputField Input{ get; protected set; }
    
    
    }
}
