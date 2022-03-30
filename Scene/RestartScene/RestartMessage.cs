using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestartMessage : MonoBehaviour
{
    private TextMeshPro message;
    void Start(){
        if(message == null) message = GameObject.Find("RestartMessage").GetComponent<TextMeshPro>();
        showMessage();
    }

    void showMessage(){
        string restartMessage = PlayerPrefs.GetString("restartMessage");
        if(String.IsNullOrEmpty(restartMessage)){
            Debug.LogWarning("restartMessage is not stored in PlayPrefs");
        }
        else{
            message.text = restartMessage;
        }
        
    }

    
}
