using TMPro;
using UnityEngine;

public class HistoryBlockBehaviour : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textAuthor;
    [SerializeField] private TextMeshProUGUI _textDate;
    [SerializeField] private TextMeshProUGUI _textSize;

    private MainLogic _mainLogic;
    public int Index { get; set; }

    public void Delete()
    {
        _mainLogic.DeleteCurrent(Index);
        Destroy(this.gameObject);
    }

    public void SetText(string author, string date, int size, MainLogic mainLogic)
    {
        _textAuthor.text = author;
        _textDate.text = date;
        _textSize.text = size.ToString();

        _mainLogic = mainLogic;
    }
}