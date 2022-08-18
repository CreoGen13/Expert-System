using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

//using System.Text.Json;
//using System.Text.Json.Serialization;

[Serializable]
public class Packet
{
    private JsonSerializer _jsonSerializer = new JsonSerializer();
    public readonly struct Data
    {
        public string Author { get; }
        public string Date { get; }
        public int Size { get; }
        public string [][] Table { get; }
            
        [JsonConstructor]
        public  Data(string author, string date, int size, string [][] table)
        {
            Author = author;
            Date = date;
            Size = size;
            Table = table;
        }

        public override string ToString()
        {
            return Author + " " + Date + " " + Size + " ";
        }
    }
    
    public String Method { get; }
    public String Parameter { get; }
    public List<Data> DataList { get; }
    public String User { get; }
    public String Pass { get; }
    
    public Packet(String method)
    {
        Method = method;
    }

    public Packet(String method, String parameter) :
        this(method)
    {
        Parameter = parameter;
    }

    public Packet(String method, List<Data> dataList) :
        this(method)
    {
        DataList = dataList;
    }
        
    public Packet(String method, String parameter, String user, String pass) :
        this(method, parameter)
    {
        User = user;
        Pass = pass;
    }
        
    [JsonConstructor]
    public Packet(String method, List<Data> dataList, String parameter, String user, String pass)
    {
        Method = method;
        Parameter = parameter;
        DataList = dataList;
        User = user;
        Pass = pass;
    }

    public string SerializeItem()
    {
        //var writer = new System.Xml.Serialization.XmlSerializer(typeof(Packet));
        var sw = new StringWriter();
        //writer.Serialize(sw, this);

        
        _jsonSerializer.Serialize(sw, this);
        return sw.ToString();
        // using(var sww = new StringWriter())
        // {
        //     using(XmlWriter writer = XmlWriter.Create(sww))
        //     {
        //         xsSubmit.Serialize(writer, subReq);
        //         xml = sww.ToString(); // Your XML
        //     }
        // }
    }

    public Packet DeserializeItem(string xml)
    {
        //var reader = new System.Xml.Serialization.XmlSerializer(typeof(Packet));  
        var sr = new StringReader(xml);
        //Packet packet = (Packet)reader.Deserialize(sr)!;
        Packet packet = _jsonSerializer.Deserialize<Packet>(new JsonTextReader(sr))!;
        return packet;
    }
        
        
}