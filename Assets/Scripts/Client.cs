using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Blocks;
using UnityEngine;

public class Client
{
    private static string _host;
    private static int _port;
    private static readonly ManualResetEvent ConnectDone =
        new ManualResetEvent(false);
    private static readonly ManualResetEvent SendDone =
        new ManualResetEvent(false);
    private static readonly ManualResetEvent ReceiveDone =
        new ManualResetEvent(false);


    private static Packet _receivedPacket;
    public Client(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public Packet SendMessage(string method, string author, string date, int size, string [][] table)
    {
        try
        {
            ConnectDone.Reset();
            SendDone.Reset();
            ReceiveDone.Reset();
            
            IPAddress ip = IPAddress.Parse(_host);
            IPEndPoint lep = new IPEndPoint(ip, _port);
            Socket client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            client.BeginConnect(lep, ConnectCallback, client);
            ConnectDone.WaitOne();
            
            Packet packet = new Packet(method, new List<Packet.Data>(){new Packet.Data(author, date, size, table)});
            Send(client, packet);
            SendDone.WaitOne();

            Debug.Log("Message called \"" + method + "\" sent to " + _host);
            Receive(client);
            ReceiveDone.WaitOne();

            Debug.Log("Message called \"" + _receivedPacket.Method + "\" received from " + _host);
            Debug.Log("Message called \"" + _receivedPacket.Parameter + "\" received from " + _host);
            
            
            
            client.Shutdown(SocketShutdown.Both);
            client.Close();

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
        return _receivedPacket;
        void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket) ar.AsyncState!;

                // Complete the connection.  
                client.EndConnect(ar);

                Debug.Log("Socket connected to " + client.RemoteEndPoint!);

                // Signal that the connection has been made.  
                ConnectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
    private static void Send(Socket client, Packet packet)
    {
        byte[] byteData = Encoding.ASCII.GetBytes(packet.SerializeItem() + "<EOF>");

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,new AsyncCallback(SendCallback), client);
        void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                //Socket client = (Socket) ar.AsyncState!;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                SendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
    private static void Receive(Socket client)
    {
        try
        {
            StateObject state = new StateObject
            {
                workSocket = client
            };
            
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        
        void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject) ar.AsyncState!;
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    string content = state.sb.ToString();
                    Debug.Log(content);
                    var index = content.IndexOf("<EOF>", StringComparison.Ordinal);
                    if (index > -1)
                    {
                        Debug.Log("READ ALL");
                        content = content.Substring(0, index);
                        _receivedPacket = new Packet("test").DeserializeItem(content);
                        ReceiveDone.Set();
                    }
                    else
                    {
                        Debug.Log("READING AGAIN");
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);  
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
    

    // private static void Read(Socket handler, StateObject state, IAsyncResult ar)
    //     {
    //         int bytesRead = handler.EndReceive(ar);
    //         if (bytesRead > 0) {  
    //             // There  might be more data, so store the data received so far.  
    //             state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));  
    //
    //             // Check for end-of-file tag. If it is not there, read
    //             // more data.  
    //             string content = state.sb.ToString();
    //             var index = content.IndexOf("<EOF>", StringComparison.Ordinal);
    //             if (index > -1)
    //             {
    //                 content = content.Substring(0, index);
    //                 // All the data has been read from the
    //                 // client. Display it on the console.  
    //                 Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",  
    //                     content.Length, content );  
    //                 // Echo the data back to the client.  
    //                 Send(handler, content);  
    //             } else {  
    //                 // Not all data received. Get more.  
    //                 handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  
    //                     new AsyncCallback(ReceiveCallback), state);  
    //             }  
    //         }  
    //         
    //     }
    // private static void Send(Socket client, String data)
    // {
    //     // Convert the string data to byte data using ASCII encoding.  
    //     byte[] byteData = Encoding.ASCII.GetBytes(data);
    //
    //     // Begin sending the data to the remote device.  
    //     client.BeginSend(byteData, 0, byteData.Length, 0,
    //         new AsyncCallback(SendCallback), client);
    // }
}