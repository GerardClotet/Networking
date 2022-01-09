using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SetSpaceCubes : MonoBehaviour
{
    private GameObject restrictedSpaceCubePrefab;
    private GameObject availableCubePrefab;

    private List<GameObject> restrictedCubeList = new List<GameObject>();
    private List<GameObject> availablePosCubeList = new List<GameObject>();
    private List<GameObject> boardPosSquares = new List<GameObject>();

    private const string restrictedCubePath = "RestrictedSpaceCubePrefab";
    private const string availableCubePath = "AvailableSpaceCubePrefab";

    public static SetSpaceCubes _instance { get; private set; }

    private void Awake()
    {
        restrictedSpaceCubePrefab = Resources.Load(restrictedCubePath) as GameObject;
        availableCubePrefab = Resources.Load(availableCubePath) as GameObject;
        _instance = this;
    }

    public void DefineBoardPos(List<GameObject> boardPosSquares)
    {
        this.boardPosSquares = boardPosSquares;
    }

    public void SetRestrictedSpaceCubes(int[] restricted_pos)
    {
        for (int i = 0; i < restricted_pos.Length; i++)
        {
            GameObject go = GameObject.Instantiate(restrictedSpaceCubePrefab);
            go.transform.position = boardPosSquares[restricted_pos[i]].transform.position;
            restrictedCubeList.Add(go);
        }
    }

    public void EraseRestrictedSpaceCubes()
    {
        for (int i = 0; i < restrictedCubeList.Count(); i++)
        {
            Destroy(restrictedCubeList[i]);
        }
        restrictedCubeList.Clear();
    }

    public void SetAvailableCubes(int currentpos)
    {
        if (currentpos + 5 <= 24) //we are inside of boundaries
        {
            GameObject go = Instantiate(availableCubePrefab);
            go.transform.position = boardPosSquares[currentpos + 5].transform.position;
            availablePosCubeList.Add(go);
        }
        if (currentpos - 5 >= 0)
        {
            GameObject go = Instantiate(availableCubePrefab);
            go.transform.position = boardPosSquares[currentpos - 5].transform.position;
            availablePosCubeList.Add(go);
        }
        if (currentpos % 5 != 0) //means we aren't on the right side 
        {
            GameObject go = Instantiate(availableCubePrefab);
            go.transform.position = boardPosSquares[currentpos - 1].transform.position; //one to the right
            availablePosCubeList.Add(go);
        }
        if ((currentpos + 1) % 5 != 0)//means we are on the left side
        {
            GameObject go = Instantiate(availableCubePrefab);
            go.transform.position = boardPosSquares[currentpos + 1].transform.position; //one to the left
            availablePosCubeList.Add(go);
        }
    }
    public void EraseAvailableSquares()
    {
        for (int i = 0; i < availablePosCubeList.Count(); i++)
        {
            Destroy(availablePosCubeList[i]);
        }
        availablePosCubeList.Clear();
    }

}
