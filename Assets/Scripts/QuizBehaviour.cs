using System;
using System.Collections.Generic;
using System.Linq;
using Blocks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizBehaviour : MonoBehaviour
{
   [SerializeField] private MainLogic _mainLogic;
   public enum BlockType
   {
      StaticText,
      StaticNumber,
      StaticDate,
      DropdownNumber,
      CalculateNumber,
      InputText,
      CoefficientK,
      CoefficientB,
      CheckText,
   }
   
   [SerializeField] private List<string> _staticBlockNames;
   [SerializeField] private GameObject _staticTextBlock;
   [SerializeField] private GameObject _staticNumberBlock;
   [SerializeField] private GameObject _dropdownNumberBlock;
   [SerializeField] private GameObject _inputTextBlock;
   
   [SerializeField] private Transform _gridTransform;
   [SerializeField] private GridLayoutGroup _gridLayoutGroup;
   
   [SerializeField] private TextMeshProUGUI _textKz;
   [SerializeField] private TextMeshProUGUI _textBz;
   [SerializeField] private TextMeshProUGUI _textCheck;
   
   [SerializeField] private Button _buttonCalculate;
   [SerializeField] private Button _buttonSend;

   [SerializeField] private TMP_InputField _textAuthor;
   [SerializeField] private TMP_InputField _textSize;
   
   private Block[,] _blocks;
   private int _systems;
   private string _author;

   private void Start()
   {
      _textAuthor.onValueChanged.AddListener(delegate
      {
         CheckAuthorAndSize();
      });
      _textSize.onValueChanged.AddListener(delegate
      {
         CheckAuthorAndSize();
      });
   }
   
   public void SendTable()
   {
      string[][] table = new string[_systems][];
      for (int i = 1; i < _systems + 1; i++)
      {
         table[i - 1] = new string[_systems + 6];
         for (int j = 1; j < _systems + 7; j++)
         {
            table[i - 1][j - 1] = _blocks[i, j].ToString();
         }
      }

      if(MainLogic.Mode)
      {
         _mainLogic.SendData(_author, _blocks[1, _systems + 6].ToString(), _systems + 7, table);
      }
      else
      {
         _mainLogic.AddData(_author, _blocks[1, _systems + 6].ToString(), _systems + 7, table);
      }
      
      this.gameObject.SetActive(false);
   }

   public void CalculateBlocks()
   {
      _author = _textAuthor.text;
      var size = Convert.ToInt32(_textSize.text);
      SpawnBlocks(size);
   }
   private void SpawnBlocks(int size)
   {
      if(size < 2)
         return;

      _textKz.text = "?";
      _textBz.text = "?";
      _textCheck.text = "?";
      _buttonSend.interactable = false;
      
      _systems = size;
      int columns = (size + 7);
      _gridLayoutGroup.constraintCount = columns;

      Clear();
      _blocks = new Block[size + 1, columns];

      SpawnFirstRaw();
      
      for (int i = 1; i < _blocks.GetLength(0); i++)
      {
         for (int j = 0; j < columns; j++)
         {
            if (j == 0)
            {
               var go = Instantiate(_staticNumberBlock, _gridTransform);
               _blocks[i, j] = new BlockText(BlockType.StaticNumber, go, go.GetComponentInChildren<TextMeshProUGUI>());
               _blocks[i, j].Text.text = i.ToString();
            }
            else if (j == size + 1)
            {
               var go = Instantiate(_staticTextBlock, _gridTransform);
               _blocks[i, j] = new BlockText(BlockType.CoefficientK, go, go.GetComponentInChildren<TextMeshProUGUI>());
               _blocks[i, j].Text.text = "?";
            }
            else if (j == size + 2)
            {
               var go = Instantiate(_staticTextBlock, _gridTransform);
               _blocks[i, j] = new BlockText(BlockType.CoefficientB, go, go.GetComponentInChildren<TextMeshProUGUI>());
               _blocks[i, j].Text.text = "?";
            }
            else if (j == size + 3)
            {
               var go = Instantiate(_staticTextBlock, _gridTransform);
               _blocks[i, j] = new BlockText(BlockType.CheckText, go, go.GetComponentInChildren<TextMeshProUGUI>());
               _blocks[i, j].Text.text = "?";
            }
            else if (j == columns - 1)
            {
               var go = Instantiate(_staticTextBlock, _gridTransform);
               _blocks[i, j] = new BlockText(BlockType.StaticDate, go, go.GetComponentInChildren<TextMeshProUGUI>());
               _blocks[i, j].Text.text = "?";
            }
            else if (j > i && j < size + 1)
            {
               var go = Instantiate(_dropdownNumberBlock, _gridTransform);
               _blocks[i, j] = new BlockDropdown(BlockType.DropdownNumber, go, go.GetComponentInChildren<TMP_Dropdown>(), i, j);
               var dropdown = _blocks[i, j].Dropdown;
               var block = _blocks[i, j];
               dropdown.interactable = false;
               dropdown.onValueChanged.AddListener(delegate
               {
                  OnValueChanged((BlockDropdown)block);
               });
            }
            else if (j < i && j < size + 1)
            {
               var go = Instantiate(_staticTextBlock, _gridTransform);
               _blocks[i, j] = new BlockText(BlockType.CalculateNumber, go, go.GetComponentInChildren<TextMeshProUGUI>());
               _blocks[i, j].Text.text = "?";
            }
            else if (j == i && j < size + 1)
            {
               var go = Instantiate(_staticTextBlock, _gridTransform);
               _blocks[i, j] = new BlockText(BlockType.CalculateNumber, go, go.GetComponentInChildren<TextMeshProUGUI>());
               _blocks[i, j].Text.text = "1";
            }
            else
            {
               var go = Instantiate(_inputTextBlock, _gridTransform);
               _blocks[i, j] = new BlockInput(BlockType.InputText, go, go.GetComponentInChildren<TMP_InputField>());
            }
         }
      }

      _blocks[1, 2].Dropdown.interactable = true;
      
      void SpawnFirstRaw()
      {
         var go = Instantiate(_staticTextBlock, _gridTransform);
         _blocks[0, 0] = new BlockText(BlockType.StaticText, go, go.GetComponentInChildren<TextMeshProUGUI>());
         _blocks[0, 0].Text.text = _staticBlockNames[0];
         
         for (int i = 1; i < size + 1; i++)
         {
            go = Instantiate(_staticNumberBlock, _gridTransform);
            _blocks[0, i] = new BlockText(BlockType.StaticNumber, go, go.GetComponentInChildren<TextMeshProUGUI>());
            _blocks[0, i].Text.text = i.ToString();
         }
         
         for (int i = size + 1; i < size + 7; i++)
         {
            go = Instantiate(_staticTextBlock, _gridTransform);
            _blocks[0, i] = new BlockText(BlockType.StaticNumber, go, go.GetComponentInChildren<TextMeshProUGUI>());
            _blocks[0, i].Text.text = _staticBlockNames[i - size];
         }
      }
      void Clear()
      {
         var children = _gridTransform.GetComponentsInChildren<Transform>().ToList();
         children.Remove(_gridTransform);
         foreach (var child in children)
            Destroy(child.gameObject);
      }
   }
   private void Calculate(int posI, int posJ)
   {
      int calculationPosI = posI + 1;
      int calculationPosJ = posJ;

      while (calculationPosI < calculationPosJ)
      {
         for (int i = 1; i < calculationPosI; i++)
         {
            int result = Compare(_blocks[i, calculationPosI].Dropdown.value, _blocks[i, calculationPosJ].Dropdown.value);
            if(result != 0)
            {
               _blocks[calculationPosI, calculationPosJ].Dropdown.SetValueWithoutNotify(result);
               _blocks[calculationPosJ, calculationPosI].Text.text = Reflect(result).ToString();
               break;
            }
         }
         //_blocks[calculationPosI, calculationPosJ].Dropdown.SetValueWithoutNotify(2);
         calculationPosI++;
      }

      int Compare(int firstToSecond, int firstToThird)
      {
         if (firstToSecond == 0 || firstToThird == 0)
            return 0;
         // first - 1 система ко 2
         // second - 1 система к 3
         var table = new[,] {
            {0, 3, 3},
            {1, 2, 3},
            {1, 1, 0}
         };

         return table[firstToSecond - 1, firstToThird - 1];
      }
   }
   private void CalculationComplete()
   {
      int bAll = (int)Mathf.Pow(_systems, 2);

      int bMax = 1 + 2 * (_systems - 1);
      int bMin = 1;
      float kMax = (1 + 2 * (_systems - 1)) / Mathf.Pow(_systems, 2);
      float kMin = 1 / Mathf.Pow(_systems, 2);

      float kSum = 0;
      int bSum = 0;
      
      for (int i = 1; i < _systems + 1; i++)
      {
         int bZ = 0;
         for (int j = 1; j < _systems + 1; j++)
         {
            if (j > i)
               bZ += _blocks[i, j].Dropdown.value - 1;
            else
            {
               bZ += int.Parse(_blocks[i, j].Text.text);
            }
         }

         float kZ = (float) bZ / bAll;

         _blocks[i, _systems + 1].Text.text = kZ.ToString();
         _blocks[i, _systems + 2].Text.text = bZ.ToString();

         bSum += bZ;
         kSum += kZ;

         if (kZ <= kMax && kZ >= kMin && bZ >= bMin && bZ <= bMax)
            _blocks[i, _systems + 3].Text.text = "OK";
         else
            _blocks[i, _systems + 3].Text.text = "NO";
      }
      
      _textKz.text = kSum.ToString();
      _textBz.text = bSum.ToString();
      
      if (Math.Abs(kSum - 1.0f) < 0.001 && bSum == bAll) 
         _textCheck.text = "OK";
      else
         _textCheck.text = "NO";
      _buttonSend.interactable = true;

      for (int i = 1; i < _systems + 1; i++)
      {
         _blocks[i, _systems + 6].Text.text = DateTime.Now.ToString();
      }
   }
   
   private void OnValueChanged(BlockDropdown block)
   {
      _blocks[block.PosJ, block.PosI].Text.text = Reflect(block.Dropdown.value).ToString();
      Calculate(block.PosI, block.PosJ);
      
      int size = _blocks.GetLength(0);
      for (int i = 1; i < size; i++)
      {
         for (int j = 1; j < size; j++)
         {
            if(j <= i)
               continue;

            if (_blocks[i, j].Dropdown.value == 0)
            {
               _blocks[i, j].Dropdown.interactable = true;
               block.Dropdown.interactable = false;
               return;
            }
         }
      }
      
      block.Dropdown.interactable = false;
      CalculationComplete();
   }
   private int Reflect(int result)
   {
      if (result == 1)
         return 2;
      if (result == 3)
         return 0;
      return 1;
   }
   private void CheckAuthorAndSize()
   {
      if (_textAuthor.text.Length == 5 && _textSize.text != "")
         _buttonCalculate.interactable = true;
      else
         _buttonCalculate.interactable = false;
   }
}
