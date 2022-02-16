using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class onlineSystem : MonoBehaviourPunCallbacks
{   
    private PhotonView photonview;
    private static DataSetter dataSetter;
    private static localPlayer myLocalPlayer;
    [SerializeField] private cameraMove CameraMove;
    [SerializeField] private int playerNumbers = 4;
    void Awake()
    {
        photonview = GetComponent<PhotonView>();
        if(dataSetter == null) dataSetter =  new DataSetter();
        myLocalPlayer = GameObject.Find("localPlayer").GetComponent<localPlayer>();
        if(PhotonNetwork.IsMasterClient){
            if(dataSetter.count_IDRemains == playerNumbers){
                assignPlayerData(PhotonNetwork.MasterClient);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player other){
        Debug.LogFormat("{0} 進入遊戲室", other.NickName);
        if(PhotonNetwork.IsMasterClient){
            if(dataSetter.count_IDRemains == playerNumbers){
                assignPlayerData(PhotonNetwork.MasterClient);
            }
            assignPlayerData(other);
        }
    }

    void assignPlayerData(Photon.Realtime.Player other){
        int ID = dataSetter.GetID();
        photonview.RPC("SetPlayerID", other, ID);
        photonview.RPC("SetLocalCamera", other, ID);
    }

    [PunRPC]
    void SetPlayerID(int ID){
        if(myLocalPlayer != null) myLocalPlayer.SetPlayerData(ID);
        else Debug.Log("myLocalPlayer is null");
    }

    [PunRPC]
    void SetLocalCamera(int ID){
        CameraMove.setCameraTransform(ID);
    }
}

public class DataSetter{
    private List<int> playerIDs = new List<int>{0,1,2,3};
    public int GetID(){
        if(playerIDs.Count > 0){
            int ID = playerIDs[0];
            playerIDs.RemoveAt(0);
            return ID;
        }
        else return -100;
    }

    public int count_IDRemains{
        get{
            return playerIDs.Count;
        }
    }

}
