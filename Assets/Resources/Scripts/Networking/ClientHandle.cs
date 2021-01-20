using System.Collections;
using System.Collections.Generic;
using System.Net;
using PsionicsCardGameProto;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Welcome welcome)
    {
        Debug.Log($"Message from server: { welcome.Message }");
        Client.instance.myId = welcome.Id;
        ClientSend.WelcomeReceived();
        // Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(ServerPlayer player)
    {
        GameManager.instance.SpawnPlayer(
            player.Id, 
            player.Username, 
            new Vector3(player.PlayerPosition.X, player.PlayerPosition.Y, player.PlayerPosition.Z), 
            new Quaternion(player.PlayerRotation.X, player.PlayerRotation.Y, player.PlayerRotation.Z, player.PlayerRotation.W)
        );
    }

    public static void PlayerPosition(PlayerPosition playerPosition)
    {
        int _id = playerPosition.ClientId;
        Vector3 _position = new Vector3(
            playerPosition.X,
            playerPosition.Y,
            playerPosition.Z
        );

        if (_id == Client.instance.myId){
            GameManager.players[_id].transform.position = _position;
        } else {
            Vector3 result = Vector3.Lerp(
                GameManager.players[_id].transform.position,
                _position,
                0.75f
            );
            GameManager.players[_id].transform.position = result;
        }
    }

    public static void PlayerRotation(PlayerRotation playerRotation)
    {
        int _id = playerRotation.ClientId;
        Quaternion _rotation = new Quaternion(
            playerRotation.X,
            playerRotation.Y,
            playerRotation.Z,
            playerRotation.W
        );

        GameManager.players[_id].transform.rotation = _rotation;
    }

    public static void ChangeScene(ChangeScene changeScene)
    {
        GameManager.instance.ChangeScene(changeScene.SceneName);
    }

    public static void SendPlayerToLobby(PlayerToLobby playerToLobby){
        GameManager.instance.SendPlayerToLobby(playerToLobby);
    }

    public static void HandleChatMessage(ChatMessage chatMessage){
        LobbyEvents.instance.ChatMessageReceived(chatMessage);
    }

    public static void HandleGameCreated(GameCreated gameCreated){
        LobbyEvents.instance.GameCreatedReceived(gameCreated);
    }

    public static void HandleGameIsReadyToStart(GameIsReadyToStart gameIsReadyToStart){
        LobbyEvents.instance.GameReady(gameIsReadyToStart);
    }

    public static void HandleUpdateActiveGames(UpdateActiveGames updateActiveGames){
        LobbyEvents.instance.UpdateAvailableGamesAfterJoin(updateActiveGames);
    }

    public static void HandleServerResponse(byte[] _data)
    {
        ServerResponse serverResponse = ServerResponse.Parser.ParseFrom(_data);
        switch(serverResponse.ResultCase){
            case ServerResponse.ResultOneofCase.Welcome:
                Welcome(serverResponse.Welcome);
                break;
            case ServerResponse.ResultOneofCase.Player:
                SpawnPlayer(serverResponse.Player);
                break;
            case ServerResponse.ResultOneofCase.PlayerPosition:
                PlayerPosition(serverResponse.PlayerPosition);
                break;
            case ServerResponse.ResultOneofCase.PlayerRotation:
                PlayerRotation(serverResponse.PlayerRotation);
                break;
            case ServerResponse.ResultOneofCase.ChangeScene:
                ChangeScene(serverResponse.ChangeScene);
                break;
            case ServerResponse.ResultOneofCase.PlayerToLobby:
                SendPlayerToLobby(serverResponse.PlayerToLobby);
                break;
            case ServerResponse.ResultOneofCase.ChatMessage:
                HandleChatMessage(serverResponse.ChatMessage);
                break;
            case ServerResponse.ResultOneofCase.GameCreated:
                HandleGameCreated(serverResponse.GameCreated);
                break;
            case ServerResponse.ResultOneofCase.GameIsReadyToStart:
                HandleGameIsReadyToStart(serverResponse.GameIsReadyToStart);
                break;
            case ServerResponse.ResultOneofCase.UpdateActiveGames:
                HandleUpdateActiveGames(serverResponse.UpdateActiveGames);
                break;
            default:
                Debug.Log("Received unknown message");
                break;
        }
    }
}
