using System.Collections;
using System.Collections.Generic;
using PsionicsCardGameProto;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, LobbyPlayer> lobbyPlayers = new Dictionary<int, LobbyPlayer>();
    public static List<GameMessage> availableGames = new List<GameMessage>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

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

    

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        players.Add(_id, _player.GetComponent<PlayerManager>());
        // UIManager.instance.WriteToDebugText(players.Count.ToString());

    }

    public void SendPlayerToLobby(PlayerToLobby playerToLobby){
        Debug.Log($"New lobby Player ID: {playerToLobby.Player.Id} , Player UserName: {playerToLobby.Player.Username}, ClientId: {playerToLobby.ClientId}");
        LobbyPlayer lobbyPlayer = new LobbyPlayer{
            Id = playerToLobby.Player.Id,
            Username = playerToLobby.Player.Username
        };             
        
        lobbyPlayers.Add(playerToLobby.Player.Id, lobbyPlayer);
        availableGames.Clear();
        foreach(GameMessage gm in playerToLobby.ActiveGames){
            availableGames.Add(gm);
        }
        if (SceneManager.GetActiveScene().name != "Lobby"){
            ChangeScene(playerToLobby.SceneName);
        } else {
            LobbyEvents.instance.UpdateConnectedClients();
            LobbyEvents.instance.UpdateAvailableGames();
        }
        
    }

    public void ChangeScene(string sceneName)
    {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneName);
        loadScene.completed += LoadSceneCompleted;
    }

    private void LoadSceneCompleted(AsyncOperation ao)
    {
         ao.allowSceneActivation = true;  
         switch (SceneManager.GetActiveScene().name)
         {
            case "Lobby":
                LobbyEvents.instance.UpdateConnectedClients();
                LobbyEvents.instance.UpdateAvailableGames();
                break;
            case "MainGameScene":
                Debug.Log("MAIN GAME SCENE");
                break;
             default:
                Debug.Log("Uknown scene");
                break;
         }
    }
}
