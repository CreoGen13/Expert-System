using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Blocks;
using DG.Tweening;
using SFB;
using TMPro;
using UnityEngine;

public class MainLogic : MonoBehaviour
{
    private static readonly string _host = "3.214.214.59"; 
    private static readonly int _port = 8080;
    private static Client _client;
    private readonly struct Diagram
    {
        public Dictionary<string, float> Systems { get; }
        public List<string> Experts { get; }

        public Diagram(int i)
        {
            Systems = new Dictionary<string, float>();
            Experts = new List<string>();
        }

        // internal struct System
        // {
        //     public string Number { get; set; }
        //     public string Value { get; set;}
        //     
        //     public System(string number, string value)
        //     {
        //         Number = number;
        //         Value = value;
        //     }
        // }
    }
    private struct CoefficientDiagramData
    {
        public GameObject GameObject { get; }
        public float Coefficient { get; set; }
        public int Count { get; set; }
        
        public TextMeshProUGUI [] Texts { get; private set; }

        public CoefficientDiagramData(GameObject gameObject, float coefficient)
        {
            GameObject = gameObject;
            Coefficient = coefficient;
            Count = 1;
            Texts = new TextMeshProUGUI [2];
        }
        
        public void SetText(TextMeshProUGUI text1, TextMeshProUGUI text2)
        {
            Texts[0] = text1;
            Texts[1] = text2;
        }
    }
    private struct PointsDiagramData
    {
        public GameObject GameObject { get; }
        public int Points { get; set; }
        public TextMeshProUGUI [] Texts { get; private set; }

        public PointsDiagramData(GameObject gameObject, int points)
        {
            GameObject = gameObject;
            Points = points;
            Texts = new TextMeshProUGUI [2];
        }

        public void SetText(TextMeshProUGUI text1, TextMeshProUGUI text2)
        {
            Texts[0] = text1;
            Texts[1] = text2;
        }
    }
    
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _historyBlock;
    [SerializeField] private GameObject _diagramBlock;
    [SerializeField] private Transform _historyTransform;
    [SerializeField] private Transform _pointsDiagramTransform;
    [SerializeField] private Transform _coefficientDiagramTransform;
    [SerializeField] private TextMeshProUGUI _historyEmptyText;
    [SerializeField] private TextMeshProUGUI _pointsDiagramEmptyText;
    [SerializeField] private TextMeshProUGUI _coefficientDiagramEmptyText;
    
    [SerializeField] private GameObject _leftTab;
    [SerializeField] private Transform _screenshotMenuTransform;

    private List<CoefficientDiagramData> _coefficientDiagramBlocks;
    private List<PointsDiagramData> _pointsDiagramBlocks;
    private List<Packet.Data> _dataList;
    private Diagram[] _diagrams;
    
    private bool _start = true;
    private GameObject _fakeLeftTab;

    public static bool Mode;
    [SerializeField] private TextMeshProUGUI _saveButtonText;
    [SerializeField] private TextMeshProUGUI _deleteButtonText;
    public void Start()
    {
        _dataList = new List<Packet.Data>();
        _pointsDiagramBlocks = new List<PointsDiagramData>();
        _coefficientDiagramBlocks = new List<CoefficientDiagramData>();
        _diagrams = new Diagram[2];
        _diagrams[0] = new Diagram(1);
        _diagrams[1] = new Diagram(2);
        
        LoadData();

        _client = new Client(_host, _port);
    }

    public void SendData(string author, string date, int size, string [][] table)
    {
        Packet packet = _client.SendMessage("Add", author, date, size, table);
        try
        {
            List<Packet.Data> dataList = packet.DataList;
            if (dataList == null)
                throw new NullReferenceException();
            DeleteAll();

            foreach (var data in dataList)
            {
                AddData(data);
            }
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Received packet is empty");
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
        CalculateDiagrams();
    }
    public void Refresh()
    {
        Packet packet = _client.SendMessage("Show", null, null, 0, null);
        try
        {
            List<Packet.Data> dataList = packet.DataList;
            if(packet.Method == null)
                throw new NullReferenceException();
            DeleteAll();
            if (dataList != null)
            {
                foreach (var data in dataList)
                {
                    AddData(data);
                }
            }
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Received packet is empty");
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
        CalculateDiagrams();
    }
    private void AddData(Packet.Data data)
    {
        _dataList.Add(data);
        AddHistoryBlock(_dataList.Count - 1, data.Author, data.Date, data.Size - 7);
        _historyEmptyText.enabled = false;
    }
    public void AddData(string author, string date, int size, string [][] table)
    {
        _dataList.Add(new Packet.Data(author, date, size, table));
        AddHistoryBlock(_dataList.Count - 1, author, date, size - 7);
        _historyEmptyText.enabled = false;
        
        CalculateDiagrams();
    }
    public void SaveData()
    {
        PlayerPrefs.DeleteAll();
        for (int i = 0; i < _dataList.Count; i++)
        {
            string playerPrefsTable = "";
            foreach (var playerPrefsRaw in _dataList[i].Table)
            {
                foreach (var playerPrefsBlock in playerPrefsRaw)
                {
                    playerPrefsTable += playerPrefsBlock + ";";
                }
            }
            playerPrefsTable = playerPrefsTable.Remove(playerPrefsTable.Length - 1, 1);
            
            PlayerPrefs.SetString("Author" + i, _dataList[i].Author);
            PlayerPrefs.SetString("Date" + i, _dataList[i].Date);
            PlayerPrefs.SetInt("Size" + i, _dataList[i].Size);
            PlayerPrefs.SetString("Table" + i, playerPrefsTable);
        }
    }
    public void DeleteAll()
    {
        _historyEmptyText.enabled = true;
        _pointsDiagramEmptyText.enabled = true;
        _coefficientDiagramEmptyText.enabled = true;
        
        _dataList.Clear();
        ClearDiagrams();

        foreach (var block in _historyEmptyText.gameObject.GetComponentsInChildren<HistoryBlockBehaviour>())
        {
            Destroy(block.gameObject);
        }
    }
    public void DeleteCurrent(int i)
    {
        _dataList.RemoveRange(i, 1);
        if(_dataList.Count == 0)
        {
            _historyEmptyText.enabled = true;
            _pointsDiagramEmptyText.enabled = true;
            _coefficientDiagramEmptyText.enabled = true;
            
            ClearDiagrams();
            return;
        }

        CalculateDiagrams();
    }
    private void LoadData()
    {
        if(!PlayerPrefs.HasKey("Author0"))
        {
            Debug.Log("Data is missing");
            return;
        }
        
        _historyEmptyText.enabled = false;
        int i = 0;
        while (PlayerPrefs.HasKey("Author" + i))
        {
            int size = PlayerPrefs.GetInt("Size" + i) - 7;
            string[][] table = new string[size][];
            for(int k = 0; k < table.Length; k++)
            {
                table[k] = new string[size + 6];
            }
            var playerPrefsBlocks = PlayerPrefs.GetString("Table" + i).Split(';', '\n');
            int n = 0;
            for (int k = 0; k < table.Length; k++)
            {
                for (int j = 0; j < table[0].Length; j++)
                {
                    table[k][j] = playerPrefsBlocks[n];
                    n++;
                }
            }

            _dataList.Add(new Packet.Data(
                PlayerPrefs.GetString("Author" + i),
                PlayerPrefs.GetString("Date" + i), 
                PlayerPrefs.GetInt("Size" + i),
                table
            ));
            AddHistoryBlock(i, _dataList[i].Author, _dataList[i].Date, _dataList[i].Size - 7);
            i++;
        }
    }

    private void ClearDiagrams()
    {
        foreach (var block in _pointsDiagramBlocks)
        {
            Destroy(block.GameObject);
        }
        foreach (var block in _coefficientDiagramBlocks)
        {
            Destroy(block.GameObject);
        }
        _pointsDiagramBlocks.Clear();
        _coefficientDiagramBlocks.Clear();
    }
    private void CalculateDiagrams()
    {
        if(_dataList.Count == 0)
            return;

        if (_start)
            _start = false;
        
        _diagrams[0].Systems.Clear();
        _diagrams[1].Systems.Clear();
        _diagrams[0].Experts.Clear();
        _diagrams[1].Experts.Clear();
        
        _pointsDiagramEmptyText.enabled = false;
        _coefficientDiagramEmptyText.enabled = false;
        ClearDiagrams();
        
        float maxHeight = _pointsDiagramEmptyText.gameObject.GetComponent<RectTransform>().rect.height;
        float maxCoefficient = 0;
        int pointMax = 0;
        
        foreach (var data in _dataList)
        {
            _diagrams[0].Experts.Add(data.Author);
            _diagrams[1].Experts.Add(data.Author);
            for (int i = 0; i < data.Table.Length; i++)
            {
                try
                {
                    var pointsDiagramBlock = _pointsDiagramBlocks[i];
                    var coefficientDiagramBlock = _coefficientDiagramBlocks[i];

                    pointsDiagramBlock.Points += int.Parse(data.Table[i][data.Size - 6]);
                    coefficientDiagramBlock.Coefficient += float.Parse(data.Table[i][data.Size - 7]);
                    coefficientDiagramBlock.Count++;
                    
                    _pointsDiagramBlocks[i] = pointsDiagramBlock;
                    _coefficientDiagramBlocks[i] = coefficientDiagramBlock;
                }
                catch (ArgumentOutOfRangeException)
                {
                    _pointsDiagramBlocks.Add(new PointsDiagramData(Instantiate(_diagramBlock, _pointsDiagramTransform), int.Parse(data.Table[i][data.Size - 6])));
                    _coefficientDiagramBlocks.Add(new CoefficientDiagramData(Instantiate(_diagramBlock, _coefficientDiagramTransform), float.Parse(data.Table[i][data.Size - 7])));
                }
                
                if (_pointsDiagramBlocks[i].Points > pointMax)
                {
                    pointMax = _pointsDiagramBlocks[i].Points;
                }
            }
        }
        for(int i = 0; i < _coefficientDiagramBlocks.Count; i++)
        {
            var coefficientDiagramBlock = _coefficientDiagramBlocks[i];
            coefficientDiagramBlock.Coefficient = coefficientDiagramBlock.Coefficient / coefficientDiagramBlock.Count;

            if (coefficientDiagramBlock.Coefficient > maxCoefficient)
                maxCoefficient = coefficientDiagramBlock.Coefficient;
            
            _coefficientDiagramBlocks[i] = coefficientDiagramBlock;
        }
        
        for(int i = 0; i < _pointsDiagramBlocks.Count; i++)
        {
            //_coefficientDiagramBlocks[i].GameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, _coefficientDiagramBlocks[i].Coefficient / maxCoefficient * maxHeight);
            //_pointsDiagramBlocks[i].GameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (float)_pointsDiagramBlocks[i].Points / pointMax * maxHeight);
            var coefficientHeight = _coefficientDiagramBlocks[i].Coefficient / maxCoefficient * maxHeight;
            var pointsHeight = (float) _pointsDiagramBlocks[i].Points / pointMax * maxHeight;
            _coefficientDiagramBlocks[i].GameObject.GetComponent<DiagramBlock>().SetBlock((int)coefficientHeight, (int)(maxHeight - coefficientHeight));
            _pointsDiagramBlocks[i].GameObject.GetComponent<DiagramBlock>().SetBlock((int)pointsHeight, (int)(maxHeight - pointsHeight));
            
            _pointsDiagramBlocks[i].SetText(_pointsDiagramBlocks[i].GameObject.GetComponentsInChildren<TextMeshProUGUI>()[0], _pointsDiagramBlocks[i].GameObject.GetComponentsInChildren<TextMeshProUGUI>()[1]);
            _pointsDiagramBlocks[i].Texts[0].text = (i + 1).ToString();
            _pointsDiagramBlocks[i].Texts[1].text = _pointsDiagramBlocks[i].Points.ToString();
            
            _coefficientDiagramBlocks[i].SetText(_coefficientDiagramBlocks[i].GameObject.GetComponentsInChildren<TextMeshProUGUI>()[0], _coefficientDiagramBlocks[i].GameObject.GetComponentsInChildren<TextMeshProUGUI>()[1]);
            _coefficientDiagramBlocks[i].Texts[0].text = (i + 1).ToString();
            _coefficientDiagramBlocks[i].Texts[1].text = _coefficientDiagramBlocks[i].Coefficient.ToString("0.0000");
        }
        
        var pointsSortedBlocks = _pointsDiagramBlocks.OrderByDescending(x => x.Points).ToList();
        var coefficientSortedBlocks = _coefficientDiagramBlocks.OrderByDescending(x => x.Coefficient).ToList();

        //Debug.Log("KOL-VO: " + pointsSortedBlocks.Count);
        for (int i = 0; i < pointsSortedBlocks.Count(); i++)
        {
            if (_diagrams[1].Systems.ContainsKey(pointsSortedBlocks[i].Texts[0].text))
                _diagrams[1].Systems[pointsSortedBlocks[i].Texts[0].text] += pointsSortedBlocks[i].Points;
            else
                _diagrams[1].Systems.Add(pointsSortedBlocks[i].Texts[0].text, pointsSortedBlocks[i].Points);
            
            if (_diagrams[0].Systems.ContainsKey(coefficientSortedBlocks[i].Texts[0].text))
                _diagrams[0].Systems[coefficientSortedBlocks[i].Texts[0].text] += coefficientSortedBlocks[i].Coefficient;
            else
                _diagrams[0].Systems.Add(coefficientSortedBlocks[i].Texts[0].text, coefficientSortedBlocks[i].Coefficient);
            pointsSortedBlocks[i].GameObject.transform.SetSiblingIndex(i);
            coefficientSortedBlocks[i].GameObject.transform.SetSiblingIndex(i);
        }
        
        var children = _screenshotMenuTransform.GetComponentsInChildren<Transform>(true).ToList();
        children.Remove(_screenshotMenuTransform.transform);
        foreach (var child in children)
        {
            Destroy(child.gameObject);
        }

        int x = 0;
        DOTween.To(() => x, y => x = y, 1, 1).onComplete = () =>
        {
            _fakeLeftTab = Instantiate(_leftTab, _screenshotMenuTransform);
        
            _fakeLeftTab.SetActive(false);
            var fakeLeftTabRect = _fakeLeftTab.GetComponent<RectTransform>();
            fakeLeftTabRect.anchorMin = new Vector2(0, 0);
            fakeLeftTabRect.anchorMax = new Vector2(1, 1);
        };
    }
    private void AddHistoryBlock(int index, string author, string date, int size)
    {
        var go = Instantiate(_historyBlock, _historyTransform);
        var historyBlock = go.GetComponentInChildren<HistoryBlockBehaviour>();
        historyBlock.Index = index;
        historyBlock.SetText(author, date, size, this);
    }
    public void ShowPointsDiagram()
    {
        if (_start)
        {
            CalculateDiagrams();
            _start = false;
        }
        _coefficientDiagramTransform.gameObject.SetActive(false);
        _pointsDiagramTransform.gameObject.SetActive(true);
        _historyTransform.gameObject.SetActive(false);
    }
    public void ShowCoefficientDiagram()
    {
        if (_start)
        {
            CalculateDiagrams();
            _start = false;
        }
        _coefficientDiagramTransform.gameObject.SetActive(true);
        _pointsDiagramTransform.gameObject.SetActive(false);
        _historyTransform.gameObject.SetActive(false);
    }
    public void ShowHistory()
    {
        _coefficientDiagramTransform.gameObject.SetActive(false);
        _pointsDiagramTransform.gameObject.SetActive(false);
        _historyTransform.gameObject.SetActive(true);
    }
    public void ChooseFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Выберите файл", "", "", false);
        if(paths.Length == 0)
             return;
        
        var path = paths[0];
        try
        {
            using StreamReader sr = new StreamReader(path, Encoding.UTF8);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "")
                    continue;

                string author = line.Split(';')[1];
                string date = sr.ReadLine().Split(';')[1];
                int size = int.Parse((sr.ReadLine() ?? throw new InvalidOperationException()).Split(';')[1]);

                string[][] table = new string[size][];
                for (int i = 0; i < size; i++)
                {
                    table[i] = sr.ReadLine().Split(';');
                }

                AddData(author, date, size + 7, table);
            }
            sr.Close();
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine("Ошибка заполнения данных:");
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Файл не может быть прочтен:");
            Console.WriteLine(e.Message);
        }
    }
    public void SaveFile()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Сохраните файл", "", "save","csv");
        
        try
        {
            using StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);
            foreach (var data in _dataList)
            {
                sw.WriteLine("Эксперт;" + data.Author);
                sw.WriteLine("Дата;" + data.Date);
                sw.WriteLine("Кол-во систем;" + (data.Size - 7));
                
                foreach (var tableRaw in data.Table)
                {
                    for (int i = 0; i < tableRaw.Length; i++)
                    {
                        if(i == tableRaw.Length - 1)
                            sw.WriteLine(tableRaw[i]);
                        else
                            sw.Write(tableRaw[i] + ";");
                    }
                }
                sw.WriteLine();
            }
            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Файл не может быть сохранен:");
            Console.WriteLine(e.Message);
        }
    }
    public void ScreenShot(int numberOfTab)
    {
        if(_screenshotMenuTransform.childCount == 0 || _fakeLeftTab.transform.GetChild(numberOfTab).childCount == 0)
            return;
        
        var path = StandaloneFileBrowser.SaveFilePanel("Сохраните файл", "", "screenshot","png");
        
        if (path == "")
        {
            Debug.Log("User closed the window before saving");
            return;
        }
        
        _fakeLeftTab.SetActive(true);
        _fakeLeftTab.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        _fakeLeftTab.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        
        for(int i = 0; i < 3; i++)
            _fakeLeftTab.transform.GetChild(i).gameObject.SetActive(false);
        _fakeLeftTab.transform.GetChild(numberOfTab).gameObject.SetActive(true);
        
        int width = _mainCamera.pixelWidth;
        int height = _mainCamera.pixelHeight;
        Texture2D texture = new Texture2D(width, height);
            
        RenderTexture targetTexture = RenderTexture.GetTemporary(width, height);
        
        _mainCamera.targetTexture = targetTexture;
        _mainCamera.Render();
            
        RenderTexture.active = targetTexture;
        
        Rect rect = new Rect(0, 0, width, height);
        texture.ReadPixels(rect, 0, 0);
        texture.Apply();
            
        ScreenCapture.CaptureScreenshot(path, 1);
        _mainCamera.targetTexture = null;
        int x = 0;
        DOTween.To(() => x, y => x = y, 1, 1).onComplete = () =>
        {
            _fakeLeftTab.SetActive(false);
        };
    }
    public void TableShot(int numberOfTab)
    {
        if(_screenshotMenuTransform.childCount == 0 || _fakeLeftTab.transform.GetChild(numberOfTab).childCount == 0)
            return;
        
        var path = StandaloneFileBrowser.SaveFilePanel("Сохраните файл", "", "table","csv");

        if (path == "")
        {
            Debug.Log("User closed the window before saving");
            return;
        }
        
        try
        {
            using StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);

            sw.WriteLine("Кол-во Систем;" + _diagrams[numberOfTab].Systems.Count);
            sw.WriteLine("Кол-во Экспертов;" + _diagrams[numberOfTab].Experts.Count);
            sw.WriteLine("Место;Номер;" + (numberOfTab == 1 ? "Количество баллов, Bz" : "Коэффициент, Kz"));

            int k = 0;
            foreach (var pair in  _diagrams[numberOfTab].Systems)
            {
                k++;
                sw.WriteLine(k + ";" + pair.Key + ";" + pair.Value);
            }
            // for (int i = 0; i < _diagrams[numberOfTab].Systems.Count; i++)
            // {
            //     sw.WriteLine((i + 1) + ";" + _diagrams[numberOfTab].Systems[i].Number + ";" + _diagrams[numberOfTab].Systems[i].Value);
            // }

            sw.WriteLine();
            sw.WriteLine("Список экспертов:");
            
            for (int i = 0; i < _diagrams[numberOfTab].Experts.Count; i++)
            {
                sw.WriteLine((i + 1) + ";" + _diagrams[numberOfTab].Experts[i]);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Файл не может быть сохранен:");
            Console.WriteLine(e.Message);
        }
    }

    public void CheckMode(bool mode)
    {
        Mode = mode;
        if (mode)
        {
            _saveButtonText.text = "СОХРАНИТЬ ЛОКАЛЬНО";
            _deleteButtonText.text = "УДАЛИТЬ ВСЕ ЛОКАЛЬНО";
            Refresh();
        }
        else
        {
            _saveButtonText.text = "СОХРАНИТЬ";
            _deleteButtonText.text = "УДАЛИТЬ ВСЕ";
        }
    }
}
