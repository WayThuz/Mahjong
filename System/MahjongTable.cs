using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MahjongTable : MonoBehaviour
{
    [SerializeField] private GameObject cardImage;
    [SerializeField] private GameObject cardModel;

    [SerializeField] private float lengthBetweenCards;
    [SerializeField] private float rowLength;
    [SerializeField] private Vector3 yOffSet;
    private Vector3[] centerDeckStartPosition = new Vector3[4];
    private Vector3[] cardInRowOffset = new Vector3[4];

    [SerializeField] private float borderLength = 100f; 
    
    private Transform tableTransform;
    private Stack<GameObject> cardOnTable = new Stack<GameObject>(); 
    private List<GameObject> centerDeck = new List<GameObject>();
    private const int cardNumber = 144;
    void Awake(){
        tableTransform = this.transform;
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

    public void setCardPlayed(Card cardPlayed, Vector3 position, Vector3 eulerAngles){         
        GameObject cardGameObject = GameObject.Instantiate(cardImage);
        Vector3 localPosition = tableTransform.InverseTransformPoint(position);
        Vector2 flatLocalPos = new Vector2(localPosition.x, localPosition.z);
        Vector2 vectorToBorder = TableMethod.VectorToBorder(flatLocalPos, borderLength/2);
        Vector3 randomPos = TableMethod.randomCoordinate(flatLocalPos, vectorToBorder, borderLength/2);
        float randomRotaY = eulerAngles.y + Random.Range(-45, 45);
        TableMethod.initializeCard(cardGameObject, tableTransform, cardPlayed.Order, randomPos, new Vector3(90,randomRotaY,0));
        cardOnTable.Push(cardGameObject);
    }

    public void pickCardOnTable(){
        GameObject card = cardOnTable.Pop();
        Destroy(card);
    }

    public void drawCardInCenterDeck(){
        GameObject cardModel = centerDeck[0];
        centerDeck.RemoveAt(0);
        cardModel.SetActive(false);
    }  
}

