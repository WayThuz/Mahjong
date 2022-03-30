using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    private Button restartButton;
    void Start(){
        if(restartButton == null) restartButton = GameObject.Find("RestartButton").GetComponent<Button>();
    }

    public void Restart(){
        SceneManager.LoadScene(1);
    } 

    
}
