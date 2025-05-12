using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownInit : MonoBehaviour
{
    // Simple script to have same gamemode selected after each playthrough
    void Start()
    {
        gameObject.GetComponent<TMP_Dropdown>().value = GameObject.Find("Player").GetComponent<SimData>().environment;
    }
}
