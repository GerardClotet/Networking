using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class TokenScript : MonoBehaviour
{
    TokenState tokenState = TokenState.BaseState;
    private PhotonView photonView;
    private JSONReader.Citizen citizenCard = new JSONReader.Citizen();
    private int id = -1;
    private int boardArrayPos;
    private JSONReader.Citizen citizen;
    private TokenAnimationMovement tokenAnimation;

    [Header("Materials List")]
    [SerializeField] private List<Material> blueMaterialList;
    [SerializeField] private List<Material> redMaterialList;
    [SerializeField] private List<Material> blueDestinnyMaterialList;
    [SerializeField] private List<Material> redDestinnyMaterialList;

    private const float myTurnAlphaValue = 1.0f;
    private const float notMyTurnAlphaValue = 0.45f;
    private const string destinyPrefabPath = "FlagPrefab";
    private GameObject destinationPrefab;
    private GameObject destinationGO;
    private GameObject pickUpGO;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        tokenAnimation = GetComponent<TokenAnimationMovement>();
        destinationPrefab = Resources.Load(destinyPrefabPath) as GameObject;
    }

    public void SetMaterial(int materialCounter)
    {
        if (UserManager._instance.GetTeam())
            GetComponent<MeshRenderer>().material = blueMaterialList[materialCounter];

        else if (!UserManager._instance.GetTeam())
            GetComponent<MeshRenderer>().material = redMaterialList[materialCounter];
    }

    public void SetID_BoardArrayPos(int id, int boardArrayPos,int materialCounter)
    {
        this.id = id;
        this.boardArrayPos = boardArrayPos;
        LastTokenSetted(materialCounter);
    }

    public int GetID()
    {
        return id;
    }

    public void StartIdleAnimation()
    {
        tokenAnimation.SetIdleAnimation(true);
    }

    public void StopIdleAnimation()
    {
        tokenAnimation.SetIdleAnimation(false);
    }

    public void LastTokenSetted(int materialCounter) 
    {
        photonView.RPC(nameof(RPC_LastTokenSetted),RpcTarget.AllBuffered, new object[] { id,boardArrayPos,UserManager._instance.GetTeam(),materialCounter});
    }

    [PunRPC]
    private void RPC_LastTokenSetted(int id,int boardArrayPos,bool team,int materialCounter)
    {
        UserManager._instance.UpdateBoardArray(id, boardArrayPos);
        if(team)
            GetComponent<MeshRenderer>().material = blueMaterialList[materialCounter];
        
        else if(!team)
            GetComponent<MeshRenderer>().material = redMaterialList[materialCounter];

    }

    public void TokenUpdate()
    {
        photonView.RPC(nameof(RPC_TokenUpdate), RpcTarget.AllBuffered, new object[] { id, boardArrayPos});
    }

    [PunRPC]
    private void RPC_TokenUpdate(int id, int boardArrayPos)
    {
        UserManager._instance.Clean_UpdateBoardArray(id, boardArrayPos);
    }

    public void UpdatePosition(Vector3 newPos)
    {
        tokenAnimation.SetDestPos(newPos);
        TokenUpdate();

        if (tokenState == TokenState.BaseState)
            CheckPickUp();
        else
        {
            CheckDestinationComplete();
            if(tokenState == TokenState.Win)
            {
                UserManager._instance.TokenDone();
            }
        }
        EndMyTurn();
    }

    public void SetCitizenCard(JSONReader.Citizen citizen)
    {
        this.citizen = citizen;
    }
    public void SetPickUpPosition(Vector3 pickUpPos)
    {
        pickUpGO = Instantiate(destinationPrefab);
        pickUpPos.x -= 1;
        pickUpGO.transform.position = pickUpPos;
        pickUpGO.SetActive(false);
    }

    public void UpdateboardArrayPos(int boardArrayPos)
    {
        this.boardArrayPos = boardArrayPos;
    }

    public void SetDestiny(Vector3 destinyPos, int materialCounter)
    {
        destinationGO = Instantiate(destinationPrefab);
        destinationGO.transform.position = destinyPos;
        //SetDestinyPosition(destinyPos);
        SetDestinyMaterial(materialCounter);
        ChangeDestinyAlphaMaterial(notMyTurnAlphaValue);
    }
    private void SetDestinyPosition(Vector3 destinyPos)
    {
        destinationPrefab.transform.position = destinyPos;
    }
    private void SetDestinyMaterial(int materialCounter)
    {
        destinationGO.GetComponent<MeshRenderer>().material = UserManager._instance.GetTeam() ? blueDestinnyMaterialList[materialCounter] : redDestinnyMaterialList[materialCounter];
    }
    private void ChangeDestinyAlphaMaterial(float alpha)
    {
        Color color = destinationGO.GetComponent<MeshRenderer>().material.color;
        color.a = alpha;
        destinationGO.GetComponent<MeshRenderer>().material.color = color;
    }

    public void MyTurn()
    {
        ChangeDestinyAlphaMaterial(myTurnAlphaValue);
        if(pickUpGO!= null)
        pickUpGO.SetActive(true);
    }

    public void EndMyTurn()
    {
        ChangeDestinyAlphaMaterial(notMyTurnAlphaValue);
        if (pickUpGO !=null)
            pickUpGO.SetActive(false);
    }

    private void CheckPickUp()
    {
        if(boardArrayPos == citizen.pickUpID)
        {
            AudioManager._instance.SetPickupMusicClip();
            Destroy(pickUpGO);
            tokenState = TokenState.Pickup;
            //here we could launch some particles
        }
    }

    public void CheckDestinationComplete()
    {
        if(boardArrayPos == citizen.destinyID)
        {
            tokenState = TokenState.Win;
            //set some particles & tell userManager we are done
        }
    }

    public TokenState GetTokenState()
    {
        return tokenState;
    }

    /// <summary>
    /// This function validates that the clicked board pos is adjacent to the current token, not diagonals though
    /// </summary>
    /// <param name="clicked_pos"></param>
    /// <returns></returns>
    public bool CheckAdjacentSquares(int clicked_pos, int[] boardArray)
    {
        //Get our current pos in the array/board
        int idx = Array.IndexOf(boardArray, id);
        int row_offset = 5;
        int col_offset = 1;

        if ((idx % 5) == 0 && idx - col_offset == clicked_pos) //it means is on the right edge of the board and clicked on the left edge
        {
            Debug.Log("right to left false");
            return false;
        }
        else if (((idx + 1) % 5) == 0 && idx + col_offset == clicked_pos) //it means is on the left edge of the board and clicked on the right edge
        {
            Debug.Log("left to right false");
            return false;
        }

        //here we validate that the clicked position is adjacent to the current token //diagonals don't count
        if (idx + row_offset == clicked_pos || idx - row_offset == clicked_pos ||
            idx + col_offset == clicked_pos || idx - col_offset == clicked_pos)
        {
            //check that we don't jump directly from right to left
            if (idx % 5 == 0 && (idx + row_offset) % 5 != 0)
            {
                Debug.Log("Double check return false 1");
                return false;
            }
            //check that we don't jump directly from left to right
            else if (idx + 1 % 5 == 0 && (idx - row_offset + 1) % 5 != 0)
            {
                Debug.Log("Double check return false 2");
                return false;
            }
            return true;
        }
        return false;
    }

    public void DeleteAll()
    {
        if (pickUpGO != null)
            Destroy(pickUpGO);
        if (destinationGO != null)
            Destroy(destinationGO);
    }
}
