using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textPro;

    void Start()
    {
        if(textPro != null) textPro.text = PlayerPrefs.GetString("PlayerName");
    }

    
}
