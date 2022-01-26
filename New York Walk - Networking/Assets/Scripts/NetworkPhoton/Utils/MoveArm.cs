using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MoveArm : MonoBehaviour
{
    // Start is called before the first frame update
    PhotonView photonView;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PickUpToken(int pos)
    {
        Debug.LogError($"Arm Moved {pos}");
    }

}
