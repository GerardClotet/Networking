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
}
