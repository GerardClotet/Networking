using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class MultiplayerGameController : MonoBehaviour, IOnEventCallback
{
    private GameTurn turnState;
    private GameState gameState;
    private const byte SET_TURN_STATE_EVENT_CODE = 1;
    private const byte GIVE_CARDS = 2;
    private const byte GAME_STATE = 4;
    private const byte RESET_GAME = 3;
    private const byte DELETINGPLAYER = 5;//wip
    private UserManager userManager;

    private UIManager uiManager;
    private MultiplayerBoard board;
    private bool restartGame = false;
    private const string crossPath = "Cross";
    [SerializeField] private GameObject crossPrefab;
    private void Awake()
    {
        turnState = GameTurn.OtherTurn;
        gameState = GameState.Init;
        userManager = GetComponent<UserManager>();
        userManager.SetController(this);
    }

    public bool CanPerformMove()
    {
        if(!IsLocalPlayerTurn() && !IsGameInProgress())
        {
            return false;
        }
        return true;
    }

    private bool IsLocalPlayerTurn()
    {
        return turnState == GameTurn.MyTurnSetUp || turnState == GameTurn.MyTurn;
    }
    
    private bool IsGameInProgress()
    {
        return gameState == GameState.Game;
    }

    public void EndTurn()
    {
        ChangeOtherGameState(GameTurn.MyTurn);
        //end current token animation
    }

    public void EndSetUpTurn()
    {
        ChangeOtherGameState(GameTurn.MyTurnSetUp);
    }

    public GameTurn GetGameTurn()
    {
        return turnState;
    }

    public void EndGame()
    {
        gameState = GameState.Finish;
        NetworkManager._instance.SetGameStateToRoomProperty(gameState);
        turnState = GameTurn.OtherTurn;
        uiManager.WonGame();
        FinishGame();
    }

    public void SetDependencies(UIManager uiManager, MultiplayerBoard multi_board)
    {
        restartGame = false;
        this.uiManager = uiManager;
        board = multi_board;
        userManager.FillBoardSquares(board.GetComponentsInChildren<BoxCollider>());
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += RecieveRandomCards;
        PhotonNetwork.NetworkingClient.EventReceived += FinishGameEvent;
        PhotonNetwork.NetworkingClient.EventReceived += ResetAllEvent;
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= RecieveRandomCards;
        PhotonNetwork.NetworkingClient.EventReceived -= FinishGameEvent;
        PhotonNetwork.NetworkingClient.EventReceived -= ResetAllEvent;

        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void FinishGame()
    {
        object[] content = new object[] { (int)GameState.Finish};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //recieverGroup Maybe just other?
        PhotonNetwork.RaiseEvent(GAME_STATE, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void FinishGameEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if(eventCode == GAME_STATE)
        {
            object[] data = (object[])photonEvent.CustomData;
            gameState = (GameState)data[0];
            //reset or quit application TODO
            turnState = GameTurn.OtherTurn;
            uiManager.LoseGame();
        }
    }

    public void ResetAll()
    {
        turnState = GameTurn.OtherTurn;
        gameState = GameState.Init;
        NetworkManager._instance.SetGameStateToRoomProperty(gameState);
        userManager.ResetAll();
        if (!restartGame)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //recieverGroup Maybe just other?
            PhotonNetwork.RaiseEvent(RESET_GAME, null, raiseEventOptions, SendOptions.SendReliable);
        }
        else
        {
            Debug.Log($"Reset secondtime");
            restartGame = false;
            NetworkManager._instance.RestartGame();
        }
    }

    public void ResetAllEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == RESET_GAME)
        {
            restartGame = true;
        }
    }

    public void OnEvent(EventData photonEvent) 
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == SET_TURN_STATE_EVENT_CODE)
        {
            object[] data = (object[])photonEvent.CustomData;
            GameTurn state = (GameTurn)data[0];
            this.turnState = state;
            if (turnState == GameTurn.MyTurnSetUp && userManager.GetTokenCounter() < 3)
                userManager.SetUpToken();

            else if (userManager.GetTokenCounter() == 3)
            {
                turnState = GameTurn.MyTurn;
                userManager.UpdateToken();
            }
        }
    }

    public void SetTurnState(GameTurn turnState)
    {
        this.turnState = turnState;
    }

    public void SetTeam()
    {
        userManager.SetBlueTeamTrue();
    }
    //bassically here we tell the other player its his turn
    private void ChangeOtherGameState(GameTurn state)
    {
        this.turnState = GameTurn.OtherTurn;
        object[] content = new object[] { (int)state };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //recieverGroup Maybe just other?
        PhotonNetwork.RaiseEvent(SET_TURN_STATE_EVENT_CODE, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void CrossesBlock(int[] positions)
    {
        Debug.LogError($"Croos instantiated");
        PhotonNetwork.InstantiateRoomObject(crossPrefab.name, userManager.GetBoardSquaresPos(positions[0]), Quaternion.Euler(-90, 0, 0));
        userManager.ModifyBoardValue(positions[0],-3);
        PhotonNetwork.InstantiateRoomObject(crossPrefab.name, userManager.GetBoardSquaresPos(positions[1]), Quaternion.Euler(-90, 0, 0));
        userManager.ModifyBoardValue(positions[1], -3);
    }
    public void SetRandomCards(List<int>randomCards)
    {
        //we kept the first half and give the other player the other half
        userManager.SetCards(randomCards.GetRange(0, 3));
        GiveCards(randomCards.GetRange(3, 5));
        //create crosses
        CrossesBlock(randomCards.GetRange(6, 2).ToArray());
        userManager.SetUpToken();
    }

    private void GiveCards(List<int> randomcards)
    {
        object[] content = new object[] { randomcards[0],randomcards[1],randomcards[2],randomcards[3],randomcards[4] }; //token id, token pos
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //recieverGroup Maybe just other?
        PhotonNetwork.RaiseEvent(GIVE_CARDS, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void RecieveRandomCards(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == GIVE_CARDS)
        {
            object[] data = (object[])photonEvent.CustomData;

            List<int> dataRecieved = new List<int>();
            dataRecieved.Add((int)data[0]);
            dataRecieved.Add((int)data[1]);
            dataRecieved.Add((int)data[2]);
            userManager.SetCards(dataRecieved);
            userManager.ModifyBoardValue((int)data[3], -3);
            userManager.ModifyBoardValue((int)data[4], -3);
        }
    }

    public void SetGameState(GameState state)
    {
        gameState = state;
    }

    public GameState GetGameState() => gameState;
    
    public void PassTurn()
    {
        if (CanPerformMove() && turnState != GameTurn.MyTurnSetUp && turnState != GameTurn.OtherTurn )
        {
            userManager.PassTurn();
            EndTurn();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(NetworkManager._instance.IsRoomFull())
            {
                SendAllDataForReconnection();
            }
                Application.Quit();
        }
    }

    //WIP
    public void SendAllDataForReconnection()
    {
        //case scenario player reconnects while in game
        List<object> objs = new List<object>();
        int[] cards = userManager.GetCards();
        objs.Add(cards[0]);
        objs.Add(cards[1]);
        objs.Add(cards[2]);
        objs.Add(userManager.GetTokenCounter());
        objs.Add(userManager.GetWinCounter());
        //tokens info
        List<GameObject> tokenList = userManager.GetTokenList();
        for(int i = 0; i < tokenList.Count; i++)
        {
            objs.Add(tokenList[i].GetComponent<TokenScript>().GetID());
            objs.Add(tokenList[i].GetComponent<TokenScript>().GetBoardPos());
            objs.Add(tokenList[i].GetComponent<TokenScript>().GetPickUp());
        }
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //recieverGroup Maybe just other?
        PhotonNetwork.RaiseEvent(GIVE_CARDS, objs.ToArray(), raiseEventOptions, SendOptions.SendReliable);
    }
}
