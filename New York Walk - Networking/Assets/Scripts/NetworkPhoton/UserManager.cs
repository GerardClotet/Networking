using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class UserManager : MonoBehaviour
{
    public static UserManager _instance { get; private set; }
    private int[] cards = new int[3];

    [SerializeField] private TokenScript tokenScriptPrefab;
    [SerializeField] private List<GameObject> cardAnchors;
    [SerializeField] private GameObject cardGameObjectPrefab;
    private int tokenCounter = 0;
    private MultiplayerGameController controller;

    private List<GameObject> boardSquares = new List<GameObject>();

    private const int DEFAULT_SQUARE_VALUE = -2;
    private const int RESTRICTED_SQUARE_VALUE = -1;
    private const int SQUARES = 25;
    private int[] boardArray = new int[SQUARES];

    private List<GameObject> tokenList = new List<GameObject>();
    private JSONReader.Citizen currentCitizen;
    private TokenScript currentToken;

    private bool blueTeam = false;
    private int winCounter = 0;
    private const int WIN_CONDITION = 3;
    private void Awake()
    {
        _instance = this;
        for(int i = 0; i < SQUARES; i++)
        {
            boardArray[i] = -2;
        }
    }

    void Update()
    {
        if(controller.CanPerformMove()&& Input.GetKeyDown(KeyCode.Mouse0))
        {
            switch(controller.GetGameTurn())
            {
                case GameTurn.MyTurn:
                    UpdateTokenPos();
                    break;
                case GameTurn.MyTurnSetUp:
                    SetInitialTokenPos();
                    break;
                default:
                    break;
            }
        }
    }

    public void SetController(MultiplayerGameController controller)
    {
        this.controller = controller;
    }

    public void SetCards(List<int> cards)
    {
        this.cards = cards.ToArray();
    }

    public void SetUpToken()
    {
        currentCitizen = JSONReader._instance.GetCitizenCardInfo(cards[tokenCounter]);
        SetSpaceCubes._instance.SetRestrictedSpaceCubes(currentCitizen.unavailableSquares);
        CreateRestrictedSpace(currentCitizen.unavailableSquares);
        SpawnCard(cardAnchors[tokenCounter].transform,cards[tokenCounter]);
    }

    public void FillBoardSquares(BoxCollider[] boxCollider)
    {
        for(int i =0; i < boxCollider.Length; i++)
        {
            boardSquares.Add(boxCollider[i].gameObject);
        }
        SetSpaceCubes._instance.DefineBoardPos(boardSquares);
    }

    public Vector3 GetBoardSquaresPos(int pos)
    {
        return boardSquares[pos].transform.position;
    }

    public int GetTokenCounter()
    {
        return tokenCounter;
    }

    public void UpdateToken()
    {
        SetCurrentToken();
        SetSpaceCubes._instance.SetAvailableCubes(Array.IndexOf(boardArray, currentToken.GetID()));
        currentToken.MyTurn();
    }

    void SetCurrentToken()
    {
        if (currentToken == null)
        {
            currentToken = tokenList[0].GetComponent<TokenScript>();
            currentToken.StartIdleAnimation();
            return;
        }

        for (int i = 0; i < tokenList.Count(); i++)
        {
            if (currentToken == tokenList[i].GetComponent<TokenScript>())
            {
                if (i == 2)
                {
                    currentToken = tokenList[0].GetComponent<TokenScript>(); ;
                    break;
                }
                else
                {
                    currentToken = tokenList[i + 1].GetComponent<TokenScript>(); ;
                    break;
                }
            }
        }
        if(winCounter == WIN_CONDITION)
        {
            //endgame 
            return;
        }
        if(currentToken.GetTokenState() == TokenState.Win)
        {
            SetCurrentToken();
            return;
        }
        currentToken.StartIdleAnimation();
    }

    private void UpdateTokenPos()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            for(int i = 0; i < SQUARES; i++)
            {
                if (boardSquares[i].name == hit.collider.name)
                {
                    if (!currentToken.CheckAdjacentSquares(i, boardArray) || boardArray[i] != DEFAULT_SQUARE_VALUE)
                        return;

                    currentToken.StopIdleAnimation();
                    boardArray[Array.IndexOf(boardArray, currentToken.GetID())] = DEFAULT_SQUARE_VALUE;//Clean previous boardArray pos
                    boardArray[i] = currentToken.GetID();
                    SetSpaceCubes._instance.EraseAvailableSquares();
                    currentToken.GetComponent<TokenScript>().UpdateboardArrayPos(i);
                    currentToken.GetComponent<TokenScript>().UpdatePosition(boardSquares[i].transform.position);
                    if (winCounter == WIN_CONDITION)
                    {
                        controller.EndGame();
                        return;
                    }
                    controller.EndTurn();
                }
            }
        }
    }
    //the first player has the blue team
    public void SetBlueTeamTrue()
    {
        blueTeam = true;
    }
    public bool GetTeam()
    {
        return blueTeam;
    }

    private void SetInitialTokenPos()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray,out hit))
        {
            for(int i =0; i < SQUARES; i++)
            {
                if(boardSquares[i].name == hit.collider.name)
                {
                    //check if the position is already occupied
                    if (boardArray[i] != DEFAULT_SQUARE_VALUE)
                        return;

                    GameObject tmp_token = PhotonNetwork.Instantiate(tokenScriptPrefab.name, boardSquares[i].transform.position,boardSquares[i].transform.rotation);
                    tmp_token.GetComponent<TokenScript>().SetMaterial(tokenCounter);
                    tmp_token.GetComponent<TokenScript>().SetCitizenCard(currentCitizen);
                    tmp_token.GetComponent<TokenScript>().SetID_BoardArrayPos(cards[tokenCounter],i,tokenCounter);
                    tmp_token.GetComponent<TokenScript>().SetPickUpPosition(boardSquares[currentCitizen.pickUpID].transform.position);
                    tmp_token.GetComponent<TokenScript>().SetDestiny(boardSquares[currentCitizen.destinyID].transform.position, tokenCounter);
                    tokenList.Add(tmp_token);

                    CleanRestrictedSpace();
                    SetSpaceCubes._instance.EraseRestrictedSpaceCubes();
                    
                    tokenCounter++;
                    if (tokenCounter == 3)
                        AudioManager._instance.SetInGameMusicClip();

                    //last function to call
                    controller.EndSetUpTurn();
                }
            }
        }
    }

    public void PassTurn()
    {
        if (currentToken != null)
        {
            currentToken.StopIdleAnimation();
            SetSpaceCubes._instance.EraseAvailableSquares();
            currentToken.EndMyTurn();
        }
    }

    private void CreateRestrictedSpace(int[] noavailablepos) 
    {
        for (int i = 0; i < noavailablepos.Length; ++i)
        {
            if (boardArray[noavailablepos[i]] == DEFAULT_SQUARE_VALUE)
                boardArray[noavailablepos[i]] = RESTRICTED_SQUARE_VALUE;
        }    
    }

    private void CleanRestrictedSpace()
    {
        for (int j = 0; j < SQUARES; j++)
        {
            if (boardArray[j] == RESTRICTED_SQUARE_VALUE)
                boardArray[j] = DEFAULT_SQUARE_VALUE;
        }
    }

    public void UpdateBoardArray(int id, int boardArrayPos)
    {
        boardArray[boardArrayPos] = id;
    }
    public void Clean_UpdateBoardArray(int id, int boardArrayPos)
    {
        boardArray[Array.IndexOf(boardArray, id)] = DEFAULT_SQUARE_VALUE;//Clean previous boardArray pos
        boardArray[boardArrayPos] = id;
    }

    public void SpawnCard(Transform trans,int id)
    {
        GameObject go = Instantiate(cardGameObjectPrefab);
        //go.transform.position = trans.position;
        go.GetComponent<CitizenMaterial>().AssignMaterial(id);
        go.GetComponent<CardMovement>().SetDestinationPos(trans.position);
    }

    public void TokenDone()
    {
        winCounter++;
        if(winCounter == 2)
        {
            AudioManager._instance.SetlastCitizenMusicClip();
        }
    }

    public void ResetAll()
    {
        //delete tokens
        currentToken = null;
        foreach(GameObject go in tokenList)
        {
            go.GetComponent<TokenScript>().DeleteAll();
            PhotonNetwork.Destroy(go);
        }
        SetSpaceCubes._instance.EraseAvailableSquares();
        tokenList.Clear();

        tokenCounter = 0;
        winCounter = 0;
        blueTeam = false;

        //set board to defaul values
        for (int i = 0; i < SQUARES; i++)
        {
            boardArray[i] = -2;
        }

        //delete cards
        CardMovement[] cards = FindObjectsOfType<CardMovement>();
        foreach(CardMovement c in cards)
        {
            Destroy(c.gameObject);
        }
    }

    public int GetWinCounter()
    {
        return winCounter;
    }
    public List<GameObject> GetTokenList() => tokenList;

    public int[] GetCards() => cards;

    public void ModifyBoardValue(int boardPos, int boardValue)
    {
        boardArray[boardPos] = boardValue;
    }
}
