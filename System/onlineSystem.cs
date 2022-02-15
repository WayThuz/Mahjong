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
    void Start()
    {
        photonview = GetComponent<PhotonView>();
        if(dataSetter == null) dataSetter =  new DataSetter();
        myLocalPlayer = GameObject.Find("localPlayer").GetComponent<localPlayer>();
        Debug.Log("Set local player ID");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player other){
        Debug.LogFormat("{0} 進入遊戲室", other.NickName);
        if(PhotonNetwork.IsMasterClient){
            if(dataSetter.count_IDRemains == playerNumbers){
                assignPlayerData(PhotonNetwork.MasterClient);
                Debug.Log("Set local player ID");
            }
            assignPlayerData(other);
            Debug.LogFormat("我是 Master Client 嗎？ {0}", PhotonNetwork.IsMasterClient);
        }
    }

    void assignPlayerData(Photon.Realtime.Player other){
        int ID = dataSetter.GetID();
        Debug.Log("Assign other player with ID: " + ID.ToString());
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
