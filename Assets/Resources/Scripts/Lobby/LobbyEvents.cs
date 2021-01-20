using System;
using System.Collections;
using System.Collections.Generic;
using PsionicsCardGameProto;
using UnityEngine;

public class LobbyEvents : MonoBehaviour
{
    public static LobbyEvents instance;

    public LobbyEvents(){
        instance = this;
    }

    public event Action onUpdateConnectedClients;
    public event Action onUpdateAvailableGames;
    public event Action<UpdateActiveGames> onUpdateAvailableGamesAfterJoin;
    public event Action<ChatMessage> onChatMessageReceived;
    public event Action<GameCreated> onGameCreatedReceived;
    public event Action<GameMessage> onJoinGame;
    public event Action<GameIsReadyToStart> onGameReady;
    public void UpdateConnectedClients(){
        onUpdateConnectedClients?.Invoke();
    }

    public void UpdateAvailableGames(){
        onUpdateAvailableGames?.Invoke();
    }

    public void UpdateAvailableGamesAfterJoin(UpdateActiveGames updateActiveGames){
        onUpdateAvailableGamesAfterJoin?.Invoke(updateActiveGames);
    }
    public void ChatMessageReceived(ChatMessage chatMessage){
        onChatMessageReceived?.Invoke(chatMessage);
    }

    public void GameCreatedReceived(GameCreated gameCreated){
        onGameCreatedReceived?.Invoke(gameCreated);
    } 

    public void JoinGame(GameMessage gameMessage){
        onJoinGame?.Invoke(gameMessage);
    }

    public void GameReady(GameIsReadyToStart gameIsReadyToStart){
        onGameReady?.Invoke(gameIsReadyToStart);
    }
}
