using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
public class playerData
{
    private string playerName;
    private int playerOrder;
    const string playerNamePrefKey = "PlayerName";
    const string playerIDPrefKey = "PlayerID";
 
    public playerData(string playerName, int playerOrder){
        this.playerName = playerName;
        this.playerOrder = playerOrder; 
    }

    public void SetPlayerData(){
        if(!String.IsNullOrEmpty(playerName)){
            PhotonNetwork.NickName = playerName;
            PlayerPrefs.SetString(playerNamePrefKey, playerName);
        }
        PlayerPrefs.SetInt(playerIDPrefKey, playerOrder);
        Debug.Log("Set playerData: " + playerName + " " + playerOrder.ToString());
    }
 
    public string PlayerName{
        get{
            return playerName;
        }
    }
 
    public int PlayerOrder{
        get{
            return playerOrder;
        }
    }
}
 
public class localPlayer : MonoBehaviour{
    public static playerData localPlayerData;
    private string playerName;
    private int playerOrder;
 
    void Start(){
        DontDestroyOnLoad(this.gameObject);             
    }
 
    public void InputPlayerName(string name){
        playerName = name;   
    }
 
    public void SetPlayerData(int order){ 
        playerOrder = order;
        localPlayerData = new playerData(playerName, playerOrder);
        localPlayerData.SetPlayerData();         
    }
 
} 
