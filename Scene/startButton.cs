using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class startButton : MonoBehaviour
{
    [SerializeField]private Button startbutton;

    void Update(){
        if(Input.GetKeyDown(KeyCode.Return)){
            if(startbutton != null) startbutton.onClick.Invoke();
        }
    }
}
