using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class getWinnerData : MonoBehaviourPunCallbacks
{   [SerializeField] private PhotonView photonview;
    [SerializeField] private localPlayer myLocalPlayer;
    [SerializeField] private Text winnerMessage;
    void Awake(){
        if(photonview == null) photonview = GetComponent<PhotonView>();
        if(myLocalPlayer == null) myLocalPlayer = GameObject.Find("localPlayer").GetComponent<localPlayer>();
        if(winnerMessage == null) winnerMessage = GameObject.Find("WinnerMessage").GetComponent<Text>();
    }

    void Start(){
        int winnerOrder = PlayerPrefs.GetInt("winnerPlayerIndex");
        if(myLocalPlayer.GetLocalPlayerOrder == winnerOrder){
            string winnerName = myLocalPlayer.GetLocalPlayerName;
            winnerUI.current.setWinnerUI(); 
            photonview.RPC("showWinner", RpcTarget.AllBuffered, winnerName);
        }
    }   
    
    [PunRPC]
    void showWinner(string winnerName){
        winnerMessage.text = winnerName + " wins !!!";
    }

    public void LeaveRoom(){
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }


}