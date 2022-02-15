using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{   
    [SerializeField] private byte maxPlayersPerRoom = 4;
    string gameVersion = "0.0";
    bool isConnecting;
    void Awake(){
        PhotonNetwork.AutomaticallySyncScene = true;
        Screen.SetResolution(640, 480, false);
    }
    
    public void Connect(){
        isConnecting = true;
        if(PhotonNetwork.IsConnected){
            PhotonNetwork.JoinRandomRoom();
            Debug.Log("isConnected = true");
        }
        else{
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();//建立連線
            Debug.Log("isConnected = false");
        }

        
    }

    public override void OnConnectedToMaster(){
        Debug.Log("已連上 Photon Cloud");
        if(isConnecting) PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause){
        Debug.LogWarningFormat("呼叫 OnDisconnected()", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message){
        Debug.Log("隨機加入遊戲室失敗，嘗試自行開啟");
        PhotonNetwork.CreateRoom(null, new RoomOptions{ MaxPlayers = maxPlayersPerRoom});
    }

    public override void OnJoinedRoom(){
        Debug.Log("成功進入遊戲室中");
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1){
            Debug.Log("我是第一個進來的");
            PhotonNetwork.LoadLevel(1);
        }
    }
}
