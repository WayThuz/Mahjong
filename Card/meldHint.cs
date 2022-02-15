using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class meldHint 
{   private int meldHintOrder = -1;
    private int[] cardsInMeld;
    private List<GameObject> hintComponents = new List<GameObject>();
    private GameObject parentGameObject;
    private Button hintButton;
    private string buttonGameObjectName = "buttonGameObject";

    public meldHint(GameObject parentGameObject, int[] cardsInMeld, int meldHintOrder){
        this.parentGameObject = parentGameObject;
        this.cardsInMeld = cardsInMeld;
        this.meldHintOrder = meldHintOrder;
    }

    public void CreateMeldImage(Vector2 sizeDelta, float hintPositionX, float hintPositionY){
        setButton(new Vector2(hintPositionX*meldHintOrder*5, hintPositionY));
        if(cardsInMeld.Length > 0){
            for(int i = 0; i < cardsInMeld.Length; i++){
                Sprite cardSprite = findSpriteInResouces("deckType/",  cardsInMeld[i].ToString());
                if(cardSprite == null) continue;
                SetImage(cardSprite, new Vector2(hintPositionX*(i - 1 + meldHintOrder*5), hintPositionY), sizeDelta);           
            }
        }
    }

    public void DestroyAllHints(){
        parentGameObject.GetComponent<PlayerDeckUI>().DestroyAllHints();
    }

    public void setMeld(){
        parentGameObject.GetComponent<PlayerDeckUI>().setMeld = cardsInMeld;
    } 
    
    public void DestroyThisHints(){
        if(hintComponents.Count > 0){
            foreach (GameObject component in hintComponents){
                GameObject.Destroy(component);
            }
            hintComponents = new List<GameObject>();
        }
    }

    public void setPlayerMovement(){
        int order = parentGameObject.GetComponent<PlayerDeckUI>().MyOrder;
        bool isKong = (cardsInMeld.Length == 4);
        if(!isKong) MahjongSys.current.setPlayerMovement(order, 0);
    }

    Sprite findSpriteInResouces(string path, string cardImageName){
        Sprite cardSprite = Resources.Load<Sprite>(path + cardImageName);
        return cardSprite;
    }

    void setButton(Vector2 position){
        GameObject buttonGameObject = GameObject.Instantiate(Resources.Load("prefab/meldHintButton", typeof(GameObject))) as GameObject;
        RectTransform rectTr = buttonGameObject.GetComponent<RectTransform>();
        Button button = buttonGameObject.GetComponent<Button>();
        setParent(parentGameObject, buttonGameObject, rectTr);
        setButtonRect(rectTr, position);
        setButtonListener(button);
        buttonGameObject.name = buttonGameObjectName;
        hintComponents.Add(buttonGameObject);
        hintButton = button;         
    }

    void SetImage(Sprite sprite, Vector2 ImagePosition, Vector2 sizeDelta){
        GameObject imageGameObject = new GameObject();
        RectTransform rectTr = imageGameObject.AddComponent<RectTransform>();
        Image cardImage = imageGameObject.AddComponent<Image>();
        setParent(parentGameObject, imageGameObject, rectTr);   
        setImageRect(rectTr, ImagePosition, sizeDelta);    
        cardImage.sprite = sprite;
        hintComponents.Add(imageGameObject);
    }

    void setButtonRect(RectTransform rectTr, Vector2 position){
        rectTr.transform.localPosition = Vector3.zero;
        rectTr.transform.localRotation = Quaternion.identity;
        rectTr.anchoredPosition = position;
    }

    void setImageRect(RectTransform rectTr, Vector2 position, Vector2 sizeDelta){
        rectTr.transform.localPosition = Vector3.zero;
        rectTr.transform.localRotation = Quaternion.identity;
        rectTr.localScale = Vector3.one;
        rectTr.anchoredPosition = position;
        rectTr.sizeDelta = sizeDelta;
    }

    void setButtonListener(Button button){
        button.onClick.AddListener(setMeld);
        button.onClick.AddListener(DestroyAllHints);
        button.onClick.AddListener(setPlayerMovement);
    }

    void setParent(GameObject parent, GameObject child, RectTransform rectTr){
        rectTr.transform.SetParent(parent.transform);
        child.transform.SetParent(parent.transform);
    }

}
