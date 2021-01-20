using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PsionicsCardGameProto;
using System.Linq;

public class UILobbyManager : MonoBehaviour
{

    public static UILobbyManager instance;
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenu = null;
    [SerializeField] private GameObject createGameMenu = null;
    [SerializeField] private GameObject connectedPlayersContent = null;
    [SerializeField] private GameObject connectedPlayerItemPrefab = null;
    [SerializeField] private GameObject chatContent = null;
    [SerializeField] private GameObject chatTextPrefab = null;
    [SerializeField] private Scrollbar chatScrollbar = null;

    [SerializeField] private GameObject availableGamesContent = null;
    [SerializeField] private GameObject availableGamesItemPrefab = null;
    [SerializeField] private Scrollbar availableGamesScrollbar = null;
    [SerializeField] private InputField inputChat = null;

    [Header("Create Game Menu")]
    [SerializeField] private TMP_InputField gameName = null;

    [Header("Join Game Menu")]
    [SerializeField] private GameObject joinGameMenu = null;
    // [SerializeField] private TextMeshProUGUI gameNameText = null;

    [SerializeField] private Text debugText;

    private void OnEnable() {
        LobbyEvents.instance.onUpdateConnectedClients += UpdateConnectedClients;
        LobbyEvents.instance.onUpdateAvailableGames += UpdateAvailableGames;
        LobbyEvents.instance.onChatMessageReceived += OnChatMessageReceived;
        LobbyEvents.instance.onGameCreatedReceived += OnGameCreatedReceived;
        LobbyEvents.instance.onJoinGame += OnJoinGame;
        LobbyEvents.instance.onGameReady += OnGameReady;
        LobbyEvents.instance.onUpdateAvailableGamesAfterJoin += OnUpdateAvailableGamesAfterJoin;
    }

    private void OnDisable() {
        LobbyEvents.instance.onUpdateConnectedClients -= UpdateConnectedClients;
        LobbyEvents.instance.onUpdateAvailableGames -= UpdateAvailableGames;
        LobbyEvents.instance.onChatMessageReceived -= OnChatMessageReceived;
        LobbyEvents.instance.onGameCreatedReceived -= OnGameCreatedReceived;
        LobbyEvents.instance.onJoinGame -= OnJoinGame;
        LobbyEvents.instance.onGameReady -= OnGameReady;
        LobbyEvents.instance.onUpdateAvailableGamesAfterJoin -= OnUpdateAvailableGamesAfterJoin;
    }

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
        inputChat.onEndEdit.AddListener(message =>{
            if (Input.GetKey(KeyCode.Return) && message.Length > 0){   
                SendChatMessage(message);             
            }
        });
    }

    public void UpdateConnectedClients(){

        foreach(Transform child in connectedPlayersContent.transform){
            Destroy(child.gameObject);
        }

        WriteToDebugText(GameManager.lobbyPlayers.Count.ToString());

        foreach(LobbyPlayer lp in GameManager.lobbyPlayers.Values){
            GameObject instance = Instantiate(connectedPlayerItemPrefab);
            Button buttonInstance = instance.GetComponent<Button>();
            buttonInstance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = lp.Username;
            buttonInstance.AddButtonEventListener(lp.Id, (lobbyPlayerId) => {
                Debug.Log($"The player id is {lobbyPlayerId}");
            });
            instance.transform.SetParent(connectedPlayersContent.transform);
        }
    }

    public void UpdateAvailableGames(){
        foreach(Transform child in availableGamesContent.transform){
            Destroy(child.gameObject);
        }

        foreach(GameMessage gm in GameManager.availableGames){
            InstantiateAvailableGamesItem(gm);
        }
    }

    public void WriteToDebugText(string msg){
        debugText.text += msg;
    }

    public void SendChatMessage(string message){
            string username = GameManager.lobbyPlayers[Client.instance.myId].Username;
            LobbyEvents.instance.ChatMessageReceived(new ChatMessage{
                Message = message,
                Username = username,
                ClientId = Client.instance.myId
            });
            ClientSend.SendChatMessage(message, username);
            inputChat.text = "";
    }

    private void OnChatMessageReceived(ChatMessage chatMessage){
        string message = string.Empty;
        GameObject instance = Instantiate(chatTextPrefab);
        TextMeshProUGUI chatText = instance.GetComponent<TextMeshProUGUI>();
        if (chatMessage.ClientId == Client.instance.myId) 
            message = $"<color=#44BD32>{chatMessage.Username}</color>:{chatMessage.Message}";
        else
            message = $"<color=#FBC531>{chatMessage.Username}</color>:{chatMessage.Message}";

        chatText.text = message;
        instance.transform.SetParent(chatContent.transform);
        chatScrollbar.value = 0;
    }

    private void OnGameCreatedReceived(GameCreated gameCreated){

        GameManager.lobbyPlayers[Client.instance.myId].JoinedGameId = gameCreated.GameId;

        foreach(Transform child in availableGamesContent.transform){
            Destroy(child.gameObject);
        }

        foreach(GameMessage gm in gameCreated.ActiveGames){
            InstantiateAvailableGamesItem(gm);
        }
    }

    private void InstantiateAvailableGamesItem(GameMessage gm){

        GameObject instance = Instantiate(availableGamesItemPrefab);
        instance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = gm.GameName;
        instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Created by {gm.CreatedByUsername}";
        instance.transform.GetChild(2).GetComponent<Button>().AddButtonEventListener(gm, (gameMessage) => {
            LobbyEvents.instance.JoinGame(gameMessage);
        });
        instance.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"<color=#FBC531>Status:</color><color=#4CD137><b>{gm.GameStatus}</b></color>";
        instance.transform.GetChild(7).gameObject.SetActive(true);
        if (gm.GameStatus == Constants.GameStatus.Full){
            instance.transform.GetChild(7).GetComponent<TextMeshProUGUI>().text = "Opponent Found";
            instance.transform.GetChild(2).GetComponent<Button>().gameObject.SetActive(false);
        }
        if (gm.GameStatus == Constants.GameStatus.GameStarted){
            instance.transform.GetChild(7).GetComponent<TextMeshProUGUI>().text = "Game Has Started";
        }
        if(gm.CreatedById == Client.instance.myId){
            instance.transform.GetChild(2).GetComponent<Button>().gameObject.SetActive(false);
        }
        instance.transform.SetParent(availableGamesContent.transform);
        availableGamesScrollbar.value = 0;
    }

    private void OnJoinGame(GameMessage gameMessage){
        Debug.Log($"GameId:{gameMessage.GameId}");
        createGameMenu.SetActive(true);
        createGameMenu.transform.GetChild(0).gameObject.SetActive(false);
        createGameMenu.transform.GetChild(1).gameObject.SetActive(true);
        createGameMenu.transform.GetChild(3).gameObject.SetActive(false);
        createGameMenu.transform.GetChild(4).gameObject.SetActive(true);
        createGameMenu.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = gameMessage.GameName;
        GameManager.lobbyPlayers[Client.instance.myId].JoinedGameId = gameMessage.GameId;
        // gameNameText.text = gameMessage.GameName;
    }

    public void CreateGame(){
        ClientSend.CreateGame(gameName.text);
        createGameMenu.SetActive(false);
        joinGameMenu.SetActive(true);
        joinGameMenu.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = gameName.text;

        //if player has created one game maybe disable the button so he cant make another  
    }

    public void JoinGame(){
        createGameMenu.SetActive(false);
        joinGameMenu.SetActive(true);
        ClientSend.JoinGame();
    }

    private void OnGameReady(GameIsReadyToStart gameIsReadyToStart){
        foreach(PlayersInGame playerInGame in gameIsReadyToStart.PlayersInGame){
            Debug.Log(playerInGame.PlayerName);
        }
        joinGameMenu.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = gameIsReadyToStart.GameName;
        joinGameMenu.transform.GetChild(2).gameObject.SetActive(false);
        joinGameMenu.transform.GetChild(3).gameObject.SetActive(true);
        joinGameMenu.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = gameIsReadyToStart.PlayersInGame.Where(w => w.IsGameCreator == true).Select(s => s.PlayerName).FirstOrDefault();
        joinGameMenu.transform.GetChild(4).gameObject.SetActive(true);
        joinGameMenu.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = gameIsReadyToStart.PlayersInGame.Where(w => w.IsGameCreator != true).Select(s => s.PlayerName).FirstOrDefault();
        joinGameMenu.transform.GetChild(5).gameObject.SetActive(true);
        
        if (Client.instance.myId != gameIsReadyToStart.PlayersInGame.Where(w => w.IsGameCreator == true).Select(s => s.PlayerId).FirstOrDefault()){
            joinGameMenu.transform.GetChild(1).gameObject.SetActive(false);
        }

    }

    private void OnUpdateAvailableGamesAfterJoin(UpdateActiveGames updateActiveGames){
        GameManager.availableGames.Clear();
        foreach(GameMessage gm in updateActiveGames.ActiveGames){
            GameManager.availableGames.Add(gm);
        }

        foreach(Transform child in availableGamesContent.transform){
            Destroy(child.gameObject);
        }

        foreach(GameMessage gm in GameManager.availableGames){
            InstantiateAvailableGamesItem(gm);
        }
    }

    public void StartGame(){
        string gameId = GameManager.availableGames.Where(w => w.CreatedById == Client.instance.myId).Select(s => s.GameId).FirstOrDefault();
        if (gameId != null){
            ClientSend.StartGame(gameId);
        }
    }

}
