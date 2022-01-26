using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager _instance { get; private set; }

    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameInitializer gameInitializer;

    private MultiplayerGameController controller;

    private const string STARTING_TURN = "turn";
    private const int MAX_PLAYERS = 2;
    private const string GAME_STATE = "gameState";
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        _instance = this;
    }
    private void Update()
    {
        uiManager.SetConnectionStattus(PhotonNetwork.NetworkClientState.ToString());
    }
    private void Start()
    {
      Debug.developerConsoleVisible = true;
    }

    public void SetController(MultiplayerGameController controller)
    {
        this.controller = controller;
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom(null, MAX_PLAYERS);
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom(null, MAX_PLAYERS);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"Joining random room failed because of {message}. Creating new one");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = MAX_PLAYERS;
        PhotonNetwork.CreateRoom(null, roomOptions);
    
    }

    public override void OnJoinedRoom()
    {
        uiManager.Connected();
        Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined the room");

        if(PhotonNetwork.IsMasterClient)
        {
            if (!IsRoomFull())
            {
                if (PhotonNetwork.CurrentRoom != null)
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { GAME_STATE, gameInitializer.GetController().GetGameState() } });
                gameInitializer.CreateMultiplayerBoard();
            }
        }
        else
        {
            //check game state
            switch((GameState)PhotonNetwork.CurrentRoom.CustomProperties[GAME_STATE])
            {
                case GameState.Init: //Default
                    SetTurn();
                    break;
                case GameState.Game:
                    //Do function
                    break;
                case GameState.Finish:
                    //Do function
                    break;
            }
        }
    }

    public bool IsRoomFull()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.ActorNumber} joined the room");
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log($"Player {targetPlayer.ActorNumber} has changed his property");

        if (PhotonNetwork.LocalPlayer == targetPlayer)
        {
            uiManager.SetTurnType((bool)PhotonNetwork.LocalPlayer.CustomProperties[STARTING_TURN]);

            if (controller == null)
                gameInitializer.InitializeMultiplayerGameController();

            controller.SetGameState(GameState.Game);

            if ((bool)targetPlayer.CustomProperties[STARTING_TURN])
            {
                GetRandomCards getRandomCards = new GetRandomCards();
                controller.SetRandomCards(getRandomCards.GenerateRandom());
                controller.SetTurnState(GameTurn.MyTurnSetUp);
                controller.SetTeam();
            }
        }
    }

    private void SetTurn() 
    {
        int tmp = Random.Range(1, 3);
        foreach(Player p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == tmp)
            {
                p.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { STARTING_TURN, true } });

            }
            else
                p.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { STARTING_TURN, false } });
        }
    }

    public void RestartGame()
    {

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if((bool)p.CustomProperties[STARTING_TURN])
            {
                p.CustomProperties[STARTING_TURN] = false;
                p.SetCustomProperties(new ExitGames.Client.Photon.Hashtable(){ { STARTING_TURN,false} });
            }
            else
            {
                p.CustomProperties[STARTING_TURN] = true;
                p.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { STARTING_TURN, true } });
            }
        }
    }
    #endregion

    public void SetGameStateToRoomProperty(GameState state)
    {
        if(PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.CustomProperties[GAME_STATE] = state;
        }
        else
        {
            Debug.LogError("Room is null, you aren't connected");
        }
    }
}
