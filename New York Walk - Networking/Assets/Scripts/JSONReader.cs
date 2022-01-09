using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONReader : MonoBehaviour
{
    [SerializeField] private TextAsset CitizensTXT;
    [SerializeField] private TextAsset CityCardsTXT;

    public static JSONReader _instance { get; private set; }

    [System.Serializable]
    public class Citizen
    {
        public string citizen;
        public string pickUp;
        public string destiny;
        public int pickUpID;
        public int destinyID;
        public int difficulty;
        public int[] unavailableSquares;
    }

    [System.Serializable]
    public class PowerUp
    {
        public string name;
        public string utility;
        public int turns;
    }

    [System.Serializable]
    public class CitizenList
    {
        public Citizen[] citizens;
    }

    [System.Serializable]
    public class PowerUpList
    {
        public PowerUp[] powerUps;
    }

    private CitizenList playableCitizenList = new CitizenList();
    private PowerUpList cityCardsList = new PowerUpList();

    private void Awake()
    {
        _instance = this;
        playableCitizenList = JsonUtility.FromJson<CitizenList>(CitizensTXT.text);
        cityCardsList = JsonUtility.FromJson<PowerUpList>(CityCardsTXT.text);
    }

    public Citizen GetCitizenCardInfo(int card_id)
    {
        Citizen citizen = new Citizen();
        return playableCitizenList.citizens[card_id];
    }
}
