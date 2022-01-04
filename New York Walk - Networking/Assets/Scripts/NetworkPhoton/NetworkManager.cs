using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private UIManager uiManager;

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

    public void Connect()
    {
        if(PhotonNetwork.IsConnected)
        {
            Debug.LogError($"Connected to server from connect");
            PhotonNetwork.JoinRandomRoom(null,MAX_PLAYERS);
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
        Debug.LogError($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined the room");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.LogError($"Player {newPlayer.ActorNumber} joined the room");
    }
    #endregion
}
