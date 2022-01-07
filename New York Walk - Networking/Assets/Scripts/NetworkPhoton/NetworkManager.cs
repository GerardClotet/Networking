using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameInitializer gameInitializer;

    private MultiplayerGameController controller;

    private const string STARTING_TURN = "turn";
    private const int MAX_PLAYERS = 2;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

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
            Debug.LogError($"Connected to server from connect");
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
        Debug.LogError($"Connected to server. Looking for a random room");
        PhotonNetwork.JoinRandomRoom(null, MAX_PLAYERS);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError($"Joining random room failed because of {message}. Creating new one");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = MAX_PLAYERS;
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        uiManager.Connected();
        Debug.LogError($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined the room");

        if (!IsRoomFull())
        {
            gameInitializer.CreateMultiplayerBoard();
        }
        else SetTurn();
    }

    public bool IsRoomFull()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.LogError($"Player {newPlayer.ActorNumber} joined the room");
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.LogError($"Player {targetPlayer.ActorNumber} has changed his property");

        if (PhotonNetwork.LocalPlayer == targetPlayer)
        {
            uiManager.SetTurnType((bool)PhotonNetwork.LocalPlayer.CustomProperties[STARTING_TURN]); //maybe we should update this in another way
            gameInitializer.InitializeMultiplayerGameController();

            if((bool)targetPlayer.CustomProperties[STARTING_TURN])
            {
                Debug.LogError($"Entered here as player {targetPlayer.ActorNumber}");
                GetRandomCards getRandomCards = new GetRandomCards();
                controller.SetRandomCards(getRandomCards.GenerateRandom());
                controller.SetTurnState(GameTurn.MyTurnSetUp);
                //here we must set our game state
            }
        }
    }

    private void SetTurn() //here we decide whos turn first
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
    #endregion
}
