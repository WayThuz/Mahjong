using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

using Method;

public class MahjongTable : MonoBehaviourPunCallbacks
{   
    private PhotonView photonview;
    public static MahjongTable current;
    [SerializeField] private GameObject cardImage;
    [SerializeField] private GameObject cardModel;

    [SerializeField] private float lengthBetweenCards;
    [SerializeField] private float rowLength;
    [SerializeField] private float heightOfCard = 20;
    [SerializeField] private Vector3 yOffSet;
    private Vector3[] centerDeckStartPosition = new Vector3[4];
    private Vector3[] cardInRowOffset = new Vector3[4];

    [SerializeField] private float borderLength = 400f; 
    
    private Transform tableTransform;
    private Stack<GameObject> cardOnTable = new Stack<GameObject>(); 
    private List<GameObject> centerDeck = new List<GameObject>();
    private const int cardNumber = 144;
    void Awake(){
        if(current == null){
            current = this;
        }
        else{
            if(current != this){
                current = null;
                current = this;
            }
        }
        tableTransform = this.transform;
        photonview = GetComponent<PhotonView>();
        setCenterDeckStartPosition(rowLength);
        setRowOffset(lengthBetweenCards);
        createCenterDeck();
    }

    void setCenterDeckStartPosition(float rowLength){
        centerDeckStartPosition[0] = new Vector3(rowLength/2f, 0, -rowLength/2f);
        centerDeckStartPosition[1] = new Vector3(-rowLength/2f, 0, -rowLength/2f);
        centerDeckStartPosition[2] = new Vector3(-rowLength/2f, 0, rowLength/2f);
        centerDeckStartPosition[3] = new Vector3(rowLength/2f, 0, rowLength/2f);
    }
    void setRowOffset(float lengthBetweenCards){  
        cardInRowOffset[0] = new Vector3(-lengthBetweenCards, 0, 0);
        cardInRowOffset[1] = new Vector3(0, 0, lengthBetweenCards);
        cardInRowOffset[2] = new Vector3(lengthBetweenCards, 0, 0);
        cardInRowOffset[3] = new Vector3(0, 0, -lengthBetweenCards);  
    }

    void createCenterDeck(){
        for(int i = 0; i < cardNumber; i++){
            GameObject model = GameObject.Instantiate(cardModel);
            centerDeck.Add(model);
        }

        for(int i = 0; i < 4; i++){       
            Vector3 flatPosition = centerDeckStartPosition[i];
            Vector3 eulerAngles = new Vector3(-90, 0, 90*i);
            for(int j = 0; j < cardNumber/4; j++){    
                Vector3 heightOffset = (j%2 == 0) ? yOffSet*2 : yOffSet;
                Vector3 position =  flatPosition + heightOffset;
                setModel(centerDeck[i*cardNumber/4 + j], position, eulerAngles);  
                if(j%2 != 0) flatPosition += cardInRowOffset[i];      
            }
        }       
    }

    void setModel(GameObject model, Vector3 position, Vector3 eulerAngles){
        model.transform.SetParent(tableTransform);
        model.transform.localPosition = position;  
        model.transform.eulerAngles = eulerAngles;
    }

    public void setCardPlayed(int cardPlayedOrder, Vector3 playerPosition, Vector3 eulerAngles){   
        Vector3 dirVector = playerPosition - this.transform.position;     
        Vector3 randomPos = TableMethod.randomCoordinate(dirVector, borderLength/2, heightOfCard);
        float randomRotaY = eulerAngles.y + Random.Range(-45, 45);
        photonview.RPC("visualizedSetCardPlayed", RpcTarget.AllBuffered, cardPlayedOrder, randomPos, randomRotaY);
    }

    [PunRPC]
    void visualizedSetCardPlayed(int cardPlayedOrder, Vector3 randomPos, float randomRotaY){
        GameObject cardGameObject = GameObject.Instantiate(cardImage);
        TableMethod.initializeCard(cardGameObject, tableTransform, cardPlayedOrder, randomPos, new Vector3(90,randomRotaY,0));
        cardOnTable.Push(cardGameObject);
    }

    public void pickCardOnTable(){
        photonview.RPC("visualizedPickCardOnTable", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void visualizedPickCardOnTable(){
        GameObject card = cardOnTable.Pop();
        Destroy(card);
    }

    public void playerDrawCardInCenterDeck(){
        photonview.RPC("visualizedPlayerDrawCardInCenterDeck", RpcTarget.AllBuffered);
    } 

    [PunRPC]
    void visualizedPlayerDrawCardInCenterDeck(){
        GameObject cardModel = centerDeck[0];
        centerDeck.RemoveAt(0);
        cardModel.SetActive(false);
    } 

    public GameObject setMeldGameObject(Transform meldParent, Vector3 meldPosition, int meldLength, int meldOrder){
        GameObject meldGameObject = TableMethod.setMeldGameObject(meldParent, meldPosition, meldLength, meldOrder);
        return meldGameObject;
    }
}

