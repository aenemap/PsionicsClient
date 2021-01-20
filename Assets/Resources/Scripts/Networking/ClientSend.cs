using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PsionicsCardGameProto;
using Google.Protobuf;
public class ClientSend : MonoBehaviour
{
    private static void SendTCPDataProto(byte[] data, int size)
    {
        Client.instance.tcp.SendDataProto(data, size);
    }

    private static void SendUDPData(byte[] data, int size)
    {
        // _packet.WriteLength();
        Client.instance.udp.SendData(data, size);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        WelcomeReceived welcomeReceived = new WelcomeReceived{
            Id = Client.instance.myId,
            ClientName = Client.instance.name,
            Username = UIConnectToServer.instance.GetUsername()
        };

        ClientRequest clientRequest = new ClientRequest{
            WelcomeReceived = welcomeReceived
        };

        int byteLength = clientRequest.CalculateSize();
        byte[] data = new byte[byteLength];
        clientRequest.WriteTo(data);
        SendTCPDataProto(data, byteLength);
    }

    public static void PlayerMovement(bool[] _inputs)
    {
        PlayerMovement playerMovement = new PlayerMovement();
        foreach(bool input in _inputs)
        {
            playerMovement.Inputs.Add(input);
        }
        playerMovement.ClientId = Client.instance.myId;
        if (GameManager.players.ContainsKey(Client.instance.myId))
        {
            playerMovement.PlayerRotation = new PlayerRotation{
            X = GameManager.players[Client.instance.myId].transform.rotation.x,
            Y = GameManager.players[Client.instance.myId].transform.rotation.y,
            Z = GameManager.players[Client.instance.myId].transform.rotation.z,
            W = GameManager.players[Client.instance.myId].transform.rotation.w,
            };
            ClientRequest clientRequest = new ClientRequest{
                PlayerMovement = playerMovement
            };


            int byteLength = clientRequest.CalculateSize();
            byte[] data = new byte[byteLength];
            clientRequest.WriteTo(data);

            SendUDPData(data, byteLength);
        }
    }

    public static void ChangeScene(string sceneName)
    {
        ChangeScene changeScene = new ChangeScene{
            ClientId = Client.instance.myId,
            SceneName = sceneName
        };

        ClientRequest clientRequest = new ClientRequest{
            ChangeScene = changeScene
        };

        int byteLength = clientRequest.CalculateSize();
        byte[] data = new byte[byteLength];
        clientRequest.WriteTo(data);

        SendTCPDataProto(data, byteLength);

    }

    public static void SendChatMessage(string msg, string username){
        ChatMessage chatMessage = new ChatMessage{
            ClientId = Client.instance.myId,
            Username = username,
            Message = msg
        };

        ClientRequest clientRequest = new ClientRequest{
            ChatMessage = chatMessage
        };

        int byteLength = clientRequest.CalculateSize();
        byte[] data = new byte[byteLength];
        clientRequest.WriteTo(data);

        SendTCPDataProto(data, byteLength);
    }

    public static void CreateGame(string gameName){
        Debug.Log($"PLAYER USERNAME:{GameManager.lobbyPlayers[Client.instance.myId].Username}");
        CreateGame createGame = new CreateGame{
            GameName = gameName,
            CreatedById = Client.instance.myId,
            CreatedByUsername = GameManager.lobbyPlayers[Client.instance.myId].Username
        };

        ClientRequest clientRequest = new ClientRequest{
            CreateGame = createGame
        };

        int byteLength = clientRequest.CalculateSize();
        byte[] data = new byte[byteLength];
        clientRequest.WriteTo(data);

        SendTCPDataProto(data, byteLength);
    }

    public static void JoinGame(){
        JoinGame joinGame = new JoinGame{
            GameId = GameManager.lobbyPlayers[Client.instance.myId].JoinedGameId,
            ClientId = Client.instance.myId
        };

        ClientRequest clientRequest = new ClientRequest{
            JoinGame = joinGame
        };

        int byteLength = clientRequest.CalculateSize();
        byte[] data = new byte[byteLength];
        clientRequest.WriteTo(data);

        SendTCPDataProto(data, byteLength);
    }

    public static void StartGame(string gameId){
        SendIntoGame sendIntoGame = new SendIntoGame{
            GameId = gameId
        };

        ClientRequest clientRequest = new ClientRequest{
            SendIntoGame = sendIntoGame
        };

        int byteLength = clientRequest.CalculateSize();
        byte[] data = new byte[byteLength];
        clientRequest.WriteTo(data);

        SendTCPDataProto(data, byteLength);
    }

    #endregion
}
