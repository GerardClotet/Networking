using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenMaterial : MonoBehaviour
{
    [SerializeField] private Material[] materialList;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
    }

    public void AssignMaterial(int material_n)
    {
        for(int i =0; i < materialList.Length; i++)
        {
            if(i == material_n)
            {
                rend.sharedMaterial = materialList[i];
            }
        }
    }
}
