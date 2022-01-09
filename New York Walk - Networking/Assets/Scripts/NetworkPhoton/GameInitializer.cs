using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Dependant Objects")]
    [SerializeField] private MultiplayerBoard multiplayerBoardPrefab;
    private MultiplayerGameController controller;

    [Header("Scene references")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Transform BoardAnchor;
    [SerializeField] private UIManager UIManager;

    private void Awake()
    {
        controller = GetComponent<MultiplayerGameController>();
    }
    public void CreateMultiplayerBoard()
    {
        if(!networkManager.IsRoomFull()) //only the first playe instantiates the board
        {
            PhotonNetwork.Instantiate(multiplayerBoardPrefab.name, BoardAnchor.position, BoardAnchor.rotation);
        }
    }

    public void InitializeMultiplayerGameController()
    {
        AudioManager._instance.SetPositioningMusicClip();
        MultiplayerBoard mBoard = FindObjectOfType<MultiplayerBoard>();//try not to use findobject
        controller.SetDependencies(UIManager, mBoard);
        networkManager.SetController(controller);
    }

}
