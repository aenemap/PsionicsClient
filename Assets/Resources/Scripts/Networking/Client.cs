using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Xml;
using System.Text;
using PsionicsCardGameProto;
using Google.Protobuf;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start() {
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnApplicationQuit() {
        Disconnect();
    }
    
    public void ConnectToServer()
    {
        tcp = new TCP();
        udp = new UDP();

        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if(!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }


        public void SendDataProto(byte[] data, int size)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(data, 0, size, null, null);
                }
            }
            catch(Exception ex)
            {
                Debug.Log($"Error sending data ti server vua TCP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }
                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                // receivedData.Reset(HandleData(_data));

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    ClientHandle.HandleServerResponse(_data);
                    // ServerResponse serverResponse = ServerResponse.Parser.ParseFrom(_data);
                    // switch(serverResponse.ResultCase){
                    //     case ServerResponse.ResultOneofCase.Welcome:
                    //         ClientHandle.Welcome(serverResponse.Welcome);
                    //         break;
                    //     case ServerResponse.ResultOneofCase.Player:
                    //         ClientHandle.SpawnPlayer(serverResponse.Player);
                    //         break;
                    //     case ServerResponse.ResultOneofCase.PlayerPosition:
                    //         ClientHandle.PlayerPosition(serverResponse.PlayerPosition);
                    //         break;
                    //     case ServerResponse.ResultOneofCase.PlayerRotation:
                    //         ClientHandle.PlayerRotation(serverResponse.PlayerRotation);
                    //         break;
                    //     case ServerResponse.ResultOneofCase.ChangeScene:
                    //         ClientHandle.ChangeScene(serverResponse.ChangeScene);
                    //         break;
                    //     default:
                    //         Debug.Log("Received unknown message");
                    //         break;
                    // }
                });
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiveing TCP data: {ex}");
                Disconnect();
            }
        }

        private void Disconnect()
        {
            instance.Disconnect();
            stream = null;
            receiveBuffer = null;
            socket = null;
            GameManager.players.Remove(Client.instance.myId);
            
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            UdpWelcome udpWelcome  = new UdpWelcome{
                ClientId = instance.myId
            };
            ClientRequest clientRequest = new ClientRequest{
                UdpWelcome = udpWelcome
            };

            int byteLength = clientRequest.CalculateSize();
            byte[] data = new byte[byteLength];
            clientRequest.WriteTo(data);
            SendData(data, byteLength);
            // using (Packet _packet = new Packet())
            // {
            //     SendData(_packet);
            // }
        }

        public void SendData(byte[] data, int size)
        {
            try
            {
                // _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(data, size, null, null);
                }
            }
            catch(Exception ex)
            {
                Debug.Log($"Error sending data to server via UDP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }
                // HandleData(_data);
                
                ThreadManager.ExecuteOnMainThread(() => {
                    ClientHandle.HandleServerResponse(_data);
                    // ServerResponse serverResponse = ServerResponse.Parser.ParseFrom(_data);
                    // switch (serverResponse.ResultCase)
                    // {
                    //     case ServerResponse.ResultOneofCase.PlayerPosition:
                    //         ClientHandle.PlayerPosition(serverResponse.PlayerPosition);
                    //         break;
                    //     case ServerResponse.ResultOneofCase.PlayerRotation:
                    //         ClientHandle.PlayerRotation(serverResponse.PlayerRotation);
                    //         break;
                    //     default:
                    //         break;
                    // }
                });
            }
            catch
            {
                Disconnect();
            }
        }
        private void Disconnect()
        {
            instance.Disconnect();
            endPoint = null;
            socket = null;
            GameManager.players.Remove(Client.instance.myId);
        }



    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();
            GameManager.players.Remove(Client.instance.myId);
        }

        Debug.Log("Disconnected from server");
    }

    private static T ReadMessage<T>(byte[] data) where T : IMessage, new()
    {
        T message = new T();
        message.MergeFrom(data);
        return message;
    }
}
