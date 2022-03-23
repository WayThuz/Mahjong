using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class winnerUI : MonoBehaviourPunCallbacks
{
    public static winnerUI current;
    [SerializeField] private PhotonView photonview;
    [SerializeField] private GameObject cardObj;
    private Queue<GameObject> objPool = new Queue<GameObject>();

    [SerializeField] private Vector3 deckStartPos;
    [SerializeField] private Vector3 meldsStartPos;
    [SerializeField] private Vector3 pos_EachCardInDeck;
    [SerializeField] private Vector3 pos_EachMeld;
    [SerializeField] private Vector3 pos_EachCardInMeld;
    void Start()
    {
        if(current == null){
            current = this;
        }
        else if(current != this)
        {
            current = null;
            current = this;
        }

        if(photonview == null) photonview = GetComponent<PhotonView>();

        if(cardObj != null){
            for(int i = 0; i < 17; i++){
                GameObject card = Instantiate(cardObj);
                objPool.Enqueue(card);
                card.SetActive(false);
            }
        }
    }

    public void setWinnerUI(){
        setWinnerDeck();
        setWinnerMelds();
    }

    public void setWinnerDeck(){
        for(int i = 0; i < PlayerPrefs.GetInt("winnerDeckCount"); i++){
            string cardID = PlayerPrefs.GetString("winnerDeck_" + i);
            Vector3 pos = deckStartPos + i*pos_EachCardInDeck;
            photonview.RPC("setCard", RpcTarget.AllBuffered, cardID, pos.x, pos.y, pos.z);
        }
    }

    public void setWinnerMelds(){
        for(int i = 0; i < PlayerPrefs.GetInt("winnerMeldsCount"); i++){
            for(int j = 0; j < PlayerPrefs.GetInt("winnerMeldsCount_" + i); j++){
                string cardID = PlayerPrefs.GetString("winnerMelds_" + i + "_" + j);
                Vector3 pos = meldsStartPos + i*pos_EachMeld + j*pos_EachCardInMeld;
                photonview.RPC("setCard", RpcTarget.AllBuffered, cardID, pos.x, pos.y, pos.z);
            }
        }
    } 

    [PunRPC]
    void setCard(string cardID, float x, float y, float z){
        GameObject card = objPool.Dequeue();
        card.transform.parent = this.transform;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localPosition = new Vector3(x, y, z);
        Image image = card.GetComponent<Image>();
        getCardImage(image, cardID);
    }

    void getCardImage(Image cardImage, string cardID){
        var cardSprite = Resources.Load<Sprite>("deckType/" + cardID);
        if (cardSprite != null) cardImage.sprite = cardSprite;
    }
}
