using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    [Header("Scene Dependencies")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private MultiplayerGameController controller;

    [Header("Buttons")]
    [SerializeField] private Button passTurnButton;
    [SerializeField] private Button restartGame;

    [Header("Texts")]
   // [SerializeField] private Text resultText;
    [SerializeField] private Text connectionStatusText;
    [SerializeField] private Text turnStatusText;

    [Header("Screen GameObjects")]
    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private GameObject connectScreen;
    [SerializeField] private GameObject whosTurn;
    [SerializeField] private GameObject winGame;
    [SerializeField] private GameObject loseGame;

    private void Awake()
    {
        passTurnButton.gameObject.SetActive(false);
        restartGame.gameObject.SetActive(false);
        OnGameLaunched();
    }

    private void OnGameLaunched()
    {
        DisableAllScreens();
        connectScreen.SetActive(true);
    }

    private void DisableAllScreens()
    {
        gameoverScreen.SetActive(false);
        connectScreen.SetActive(false);
        whosTurn.SetActive(false);
        winGame.SetActive(false);
        loseGame.SetActive(false);
    }

    public void OnConnect()
    {
        networkManager.Connect();
    }

    public void SetConnectionStattus(string status)
    {
        connectionStatusText.text = status;
    }
    
    public void Connected()
    {
        connectScreen.SetActive(false);
        whosTurn.SetActive(true);
        SetTurnButton(true);
    }

    public void SetTurnType(bool turn)
    {
        turnStatusText.text = turn ? "My Turn" : "Not My Turn";
    }

    public void LoseGame()
    {
        loseGame.SetActive(true);
        passTurnButton.gameObject.SetActive(false);
        restartGame.gameObject.SetActive(true);
    }

    public void WonGame()
    {
        winGame.SetActive(true);
        passTurnButton.gameObject.SetActive(false);
        restartGame.gameObject.SetActive(true);
    }

    public void PassTurn()
    {
        controller.PassTurn();
    }

    public void SetTurnButton(bool active)
    {
        passTurnButton.gameObject.SetActive(active);
    }

    public void RestartButton()
    {
        controller.ResetAll();
        DisableAllScreens();
        restartGame.gameObject.SetActive(false);
    }
}
