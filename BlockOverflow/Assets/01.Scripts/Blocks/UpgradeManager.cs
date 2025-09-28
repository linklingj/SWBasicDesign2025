using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour 
{
    private Inventory inventory;

    [SerializeField] private Block selectedBlock;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
    }
}
