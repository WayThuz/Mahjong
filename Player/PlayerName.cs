using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerName : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI textPro;
    [SerializeField] private PhotonView photonview;
    [SerializeField] private int myOrder;

    void Start(){
        if(photonview == null) photonview = GetComponent<PhotonView>();
        if(textPro == null) textPro = GetComponent<TextMeshProUGUI>();
    }

    public void SetLocalPlayerName(string name, int playerOrder){
        this.gameObject.tag = "localPlayerName";
        if(textPro != null) textPro.text = "";
        photonview.RPC("SetPlayerName", RpcTarget.OthersBuffered, name, playerOrder);
    }

    [PunRPC]
    void SetPlayerName(string name, int playerOrder){
        if(textPro != null && playerOrder == myOrder) textPro.text = name;
    }

    
}
