using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
[RequireComponent(typeof(PhotonView))]
public class MultiplayerBoard : MonoBehaviour
{
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SelectedTokenMoved(new Vector2 (UnityEngine.Random.Range(0, 50), UnityEngine.Random.Range(0, 50)));
        }
    }

    //this will be called when all the board array checks are done // and then the client recieving it will update his array according to it ????
    public void SelectedTokenMoved(Vector2 coords) //maybe hem de passar el identifier i la posicio
    {
        photonView.RPC(nameof(RPC_SelectedTokenMove), RpcTarget.AllBuffered, new object[] { coords });
    }


    /// <summary>
    /// here we replicate the move made by the other player
    /// </summary>
    /// <param name="coords"></param>
    [PunRPC]
    private void RPC_SelectedTokenMove(Vector2 coords) //example
    {
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y)); //example
    }
}
