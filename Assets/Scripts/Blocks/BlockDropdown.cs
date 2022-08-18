using TMPro;
using UnityEngine;

namespace Blocks
{
    public class BlockDropdown : Block
    {
        public int PosI { get; private set; }
        public int PosJ { get; private set; }
        
        public BlockDropdown(QuizBehaviour.BlockType type, GameObject gameObject, TMP_Dropdown dropdown, int posI, int posJ)
        {
            Type = type;
            GameObject = gameObject;
            Dropdown = dropdown;

            PosI = posI;
            PosJ = posJ;
        }

        public override string ToString()
        {
            return (Dropdown.value - 1).ToString();
        }
    }
}