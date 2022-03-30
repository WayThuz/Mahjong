using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using MergeSort;
using CombinationNamespace;

public class Player : MonoBehaviour
{   
    private PlayerDeckUI myDeckUI;
    private localPlayer localplayer;
    private cameraMove myCameraMove;
    private PlayerName playerName;

    private List<Card> myDeck = new List<Card>();
    private List<Card> myDeckPlayed = new List<Card>();
    private Card newCardAwait = null;//拿到的牌
    private IEnumerator decidedMovementCoroutine;
    private IEnumerator showMeldCoroutine;

    [SerializeField] private GameObject eatButton;
    [SerializeField] private GameObject ponButton;
    [SerializeField] private GameObject kongButton;
    [SerializeField] private GameObject winButton;
    [SerializeField] private GameObject quitMovementButton;
    
    PointerEventData m_PointerEventData;
    [SerializeField] GraphicRaycaster m_Raycaster;
    [SerializeField] EventSystem m_EventSystem;
       
    [SerializeField] private int myOrder;
    [SerializeField] private int selectedCardIndex = -1;

    private List<string> flowersTable = new List<string>();
    private const int flowerOrder = 400;

    private const int initialStatus = -1;
    private const int stoppedStatus = -100;

    Regex regexMatchNumber = new Regex(@"[\d]{1,2}");//regex to find number
    List<Combination> currentCombinations = new List<Combination>();

    bool isAllComponentSet = false;
    bool isGameStart = false;
    bool isCardGiver = false;
    bool isCardGotByDrawing = false;
    //getCard()
    void OnEnable(){
        myDeckUI = transform.Find("PlayerDeck").GetComponent<PlayerDeckUI>();
        myCameraMove = GameObject.Find("CameraController").GetComponent<cameraMove>();
        playerName = this.transform.Find("PlayerName").GetComponent<PlayerName>();
        StartCoroutine(localPlayerScriptCheck());   
    }

    IEnumerator localPlayerScriptCheck(){
        localplayer = GameObject.Find("localPlayer").GetComponent<localPlayer>();
        yield return new WaitForSeconds(0.3f);

        if(!isThisPlayerScriptBelongsToLocalPlayer){
            this.enabled = false;
        }
        else{
            isAllComponentSet = true;
            myCameraMove.setPlayerPositionToCamera(this.transform);
            playerName.SetLocalPlayerName(localplayer.GetLocalPlayerName, myOrder);
        }
    }

    bool isThisPlayerScriptBelongsToLocalPlayer{
        get{
            return(localplayer.GetLocalPlayerOrder == myOrder);
        }
    }

    void LateUpdate(){ 
        if(isAllComponentSet){
            if(MahjongSys.current.SystemAllPrepared(ref isGameStart)) StartCoroutine(gameStart(myOrder));
            if(MahjongSys.current.IsCardGiver(myOrder)) turn_CardGiver();
            else if(MahjongSys.current.IsCardAwaiter(myOrder)) turn_CardAwaiter();             
        }
    }

    #region AllPlayer movement
    IEnumerator gameStart(int playerOrder){ 
        myDeck = MahjongSys.current.DealCards(myOrder); 
        buttonTakeRest();
        yield return new WaitForSeconds(myOrder*0.5f);
        replaceCards();
    }

    void turn_CardGiver(){
        cardGiverStartTurn();                    
        if(!needToReplaceCards){
            if(nextMovement == initialStatus && !isMovementChecked) movementCheck(myDeck, newCardAwait);
            if(Input.GetKeyDown(KeyCode.Mouse0)) pickAndPlayCard(); 
        }
        else replaceCards();             
    }
    
    void cardGiverStartTurn(){ 
        if(newCardAwait == null && !isCardGiver){
            nextMovement = initialStatus;
            isCardGiver = true;
            getCard();
        }     
    } 

    void getCard(){
        Card cardGot;
        isCardGotByDrawing = MahjongSys.current.CardGiverDrawCard(out cardGot);   
        if(!isCardGotByDrawing){ 
            MahjongTable.current.pickCardOnTable();        
        }
        else{
            MahjongTable.current.playerDrawCardInCenterDeck();
            myDeckUI.setMeld = null;
            myDeckUI.changeCardAwaitStatus(true);
            newCardAwait = cardGot; 
        }
        showMeldCoroutine = showMeldToBroad(cardGot);
        StartCoroutine(showMeldCoroutine);    
        resortedAndShowDeck();
    }

    IEnumerator showMeldToBroad(Card cardGot){      
        for(int i = 0; i < 200; i++){
            yield return new WaitForSeconds(0.1f);
            if(myDeckUI.isMeldAssigned){
                myDeckUI.showMeldToBroad(ref myDeck, cardGot);            
                yield return new WaitForSeconds(0.15f);
                yield return StartCoroutine(checkConcealedKong(cardGot)); 
                yield break;
            }
        }      
    }

    IEnumerator checkConcealedKong(Card cardGot){
        List<int> cardsInKong = CombinationMethod.deckHasKong(myDeck, cardGot);
        if(cardsInKong.Count > 0){
            myDeckUI.AssignKongs(cardsInKong);
        }
        for(int i = 0; i < 200; i++){
            yield return new WaitForSeconds(0.1f);
            if(myDeckUI.isMeldAssigned){
                myDeckUI.showMeldToBroad(ref myDeck, cardGot);
                yield break;
            }
        }

    }

    void pickAndPlayCard(){
        int indexOfCardClicked = IndexOfCardClicked();
        if(selectedCardIndex != indexOfCardClicked){
            selectedCardIndex = indexOfCardClicked;
        }
        else if(selectedCardIndex != initialStatus){       
            Card cardPlayed = CardPlayed(selectedCardIndex); 
            setCardPlayed(cardPlayed);          
            removeCardPlayedFromDeck();
            myDeckUI.changeCardAwaitStatus(false);
            CardGiverTurnFinished(cardPlayed);
        }
    } 

    void setCardPlayed(Card cardPlayed){
        if(cardPlayed != null){
            myDeckPlayed.Add(cardPlayed);
            MahjongTable.current.setCardPlayed(cardPlayed.Order, transform.position, transform.eulerAngles);
        }
    }

    void removeCardPlayedFromDeck(){
        if(!isCardGotByDrawing) myDeckUI.extractCard(myDeck, selectedCardIndex);
        else replaceCardPlayed(selectedCardIndex);                              
        selectedCardIndex = -1;
        resortedAndShowDeck();
    }

    void replaceCardPlayed(int cardIndex){
        if(cardIndex != -10){
            myDeck.RemoveAt(cardIndex);
            if(newCardAwait != null){
                Card newCardJoin = newCardAwait;
                myDeck.Add(newCardJoin); 
                newCardAwait = null;
            }  
        } 
        else newCardAwait = null;
    }

    void CardGiverTurnFinished(Card cardPlayed){
        buttonTakeRest();
        stopAllActions();  
        if(nextMovement == initialStatus) passThisTurn();   
        MahjongSys.current.finishedTurn(myOrder);
        StartCoroutine(turnEnd(cardPlayed));
    }

    void stopAllActions(){
        if(decidedMovementCoroutine != null){
            StopCoroutine(decidedMovementCoroutine);
            decidedMovementCoroutine = null;
        }    
        if(showMeldCoroutine != null){
            StopCoroutine(showMeldCoroutine);
            showMeldCoroutine = null;
        }
        isMovementChecked = false;
        if(newCardAwait != null) newCardAwait = null;
    }

    IEnumerator turnEnd(Card cardPlayed){
        LoadNextTurn(cardPlayed.Type, cardPlayed.Number);
        yield return new WaitForSeconds(0.3f);
        isCardGiver = false;
    } 

   
    void LoadNextTurn(int cardPlayedType, int cardPlayedNumber){
        MahjongSys.current.LoadNextTurn(myOrder, cardPlayedType, cardPlayedNumber);
    }

    Card CardPlayed(int cardIndex){
        Card cardPlayed = null;
        if(cardIndex != -10){ //cardAwait 以外的 card index 都不為-10  
            cardPlayed = myDeck[cardIndex];               
        }
        else if(newCardAwait != null){          
            cardPlayed = newCardAwait;      
        }
        return cardPlayed;
    }   

    void turn_CardAwaiter(){   
        if(MahjongSys.current.CurrentCardPlayed != null) move_CardOnBroad(MahjongSys.current.CurrentCardPlayed);                            
        else nextMovement = initialStatus;            
    }

    void move_CardOnBroad(Card cardOnBroad){   
        if(nextMovement == initialStatus && !isMovementChecked) movementCheck(myDeck, cardOnBroad);
        if(nextMovement == stoppedStatus){
            isMovementChecked = false;
            MahjongSys.current.finishedTurn(myOrder);//the movement might still waiting for decision
        }
        else if(nextMovement != initialStatus) MahjongSys.current.finishedTurn(myOrder);
    } 

    #endregion
    
    #region card replace
    void replaceCards(){
        replaceFlowers();
        myDeckUI.showCardReplaced(flowersTable);
        resortedAndShowDeck();
    }

    void replaceFlowers(){
        for(int i = 0; i < myDeck.Count+1; i++){
            Card cardReplaced = null;
            if(i < myDeck.Count && myDeck[i].Order >= flowerOrder){
                cardReplaced = myDeck[i];
                myDeck[i] = MahjongSys.current.DrawCardInDeck();
            }
            else if(newCardAwait != null && newCardAwait.Order >= flowerOrder){
                cardReplaced = newCardAwait;
                newCardAwait = MahjongSys.current.DrawCardInDeck();
            }           
            if(cardReplaced == null) continue;
            myDeckPlayed.Add(cardReplaced);
            flowersTable.Add(cardReplaced.GetCardName);
        } 
    }

    bool needToReplaceCards{         
        get{
            foreach(Card card in myDeck){
                if(card.Order >= flowerOrder) return true;
            }     
            if(newCardAwait != null && newCardAwait.Order >= flowerOrder) return true;   
            return false;
        }
    }
    #endregion

    #region deckCheck, resorted and show deck
    void resortedAndShowDeck(){
        resortDeckOrder();
        myDeckUI.cardImageUpdate(myDeck, newCardAwait);
    }

    private int nextMovement = initialStatus;
    private bool isMovementChecked = false;
    const int eat = 0; const int pon = 1; const int win = 2; const int kong = 3; 
    void movementCheck(List<Card> deck, Card card){  
        bool[] playerCanDoMovement = new bool[4]{false, false, false, false};  
        isMovementChecked = true;  
        if(card == null){
            decidedMovementCoroutine = decidedMovement(playerCanDoMovement);
            StartCoroutine(decidedMovementCoroutine);
        } 
        else{      
            playerCanDoMovement = MahjongSys.current.playerCanDoMovement(myOrder, deck, card, myDeckUI.MeldOnBroadCount);
            decidedMovementCoroutine = decidedMovement(playerCanDoMovement);
            StartCoroutine(decidedMovementCoroutine);
        }      
    }
    
    IEnumerator decidedMovement(bool[] playerCanDoMovement){
        isMovementChecked = true;

        if(!playerCanDoMovement[eat] && !playerCanDoMovement[pon] && !playerCanDoMovement[kong] && !playerCanDoMovement[win]){
            passThisTurn();
            yield break;
        }  
        movementButtonSetActive(playerCanDoMovement);      
        for(int i = 0; i < 200; i++){
            yield return new WaitForSeconds(0.1f);//count
            if(nextMovement != initialStatus) yield break;    
        }
        passThisTurn();
    }
    
    void movementButtonSetActive(bool[] playerCanDoMovement){
        
        if(MahjongSys.current.IsCardGiver(myOrder)){
            if(playerCanDoMovement[kong]) kongButton.SetActive(true); 
            if(playerCanDoMovement[win]) winButton.SetActive(true);
            if(playerCanDoMovement[kong] || playerCanDoMovement[win]){
                quitMovementButton.SetActive(true);
            }
        }
        else{
            if(playerCanDoMovement[eat]) eatButton.SetActive(true); 
            if(playerCanDoMovement[pon]) ponButton.SetActive(true); 
            if(playerCanDoMovement[win]) winButton.SetActive(true); 
            if(playerCanDoMovement[kong]) kongButton.SetActive(true); 
            quitMovementButton.SetActive(true);
        }       
    }
    
    void passThisTurn(){
        nextMovement = -100; // if no decided;
        MahjongSys.current.setPlayerMovement(myOrder, -100, false);
    }

    public void pushMovementButton(int movement){
        if(decidedMovementCoroutine != null){
            StopCoroutine(decidedMovementCoroutine);
            decidedMovementCoroutine = null;
        }  
        nextMovement = movement;
        MahjongSys.current.setPlayerMovement(myOrder, nextMovement, false);
        nextMovement = stoppedStatus;    
        if(movement == eat || movement == pon || movement == kong){
            displayMeld(movement);        
            if(movement == eat) StartCoroutine(meldHintLifeTimeCountdown(movement)); 
        }
        else if(movement == win){
            MahjongSys.current.storeWinningDeck(myDeck, myDeckUI.MeldOnBroad, myDeckUI.MeldOnBroadCount);
            if(MahjongSys.current.IsCardGiver(myOrder)) MahjongSys.current.PlayerWins(myOrder);//自摸
        }

        buttonTakeRest();
    }    

    void displayMeld(int movement){ 
        Card currentCardPlayed = MahjongSys.current.CurrentCardPlayed; 
        if(currentCardPlayed != null){
            myDeckUI.AssignMeld(movement, myDeck, currentCardPlayed.Order);   
        }
    }

    IEnumerator meldHintLifeTimeCountdown(int movement){
        for(int i = 0; i < 100; i++){
            yield return new WaitForSeconds(0.05f);
            if(MahjongSys.current.HasMovementPriorThanMine(myOrder, movement)){
                myDeckUI.DestroyAllHints();
                yield break;
            }
        }
        myDeckUI.DestroyAllHints();
    } 
    
    void buttonTakeRest(){
        eatButton.SetActive(false); ponButton.SetActive(false);
        kongButton.SetActive(false); winButton.SetActive(false);
        quitMovementButton.SetActive(false);
    }


    void resortDeckOrder(){
        List<int> orderList = new List<int>();
        List<Card> myDeck_new = new List<Card>();
        foreach (Card card in myDeck){
            int order = card.Order;
            orderList.Add(order);          
        }
        myMergeSort.MergeSort(ref orderList, 0, myDeck.Count-1);
        
        for(int i = 0; i < orderList.Count; i++){
            for(int j = 0; j < myDeck.Count; j++){
                if(myDeck[j].Order == orderList[i]){
                    myDeck_new.Add(myDeck[j]);
                    myDeck.RemoveAt(j);
                    break;  
                }                             
            }
        }      
        myDeck = myDeck_new;
    }
    
    int IndexOfCardClicked(){
        int indexOfCardClicked = -1;
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;
        List<RaycastResult> selectedResults = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, selectedResults);
        foreach(RaycastResult result in selectedResults){
            indexOfCardClicked = findSelectCardIndex(result);     
        }
        return indexOfCardClicked;
    }

    int findSelectCardIndex(RaycastResult result){
        int cardIndex = -1;
        string name = result.gameObject.name;
        if(name != "CardAwait"){
            MatchCollection testResult = regexMatchNumber.Matches(name);
            foreach(Match i in testResult){
                int number = int.Parse(i.Value);
                if(cardIndex == -1) cardIndex = number;
            }
        }
        else cardIndex = -10;
        return cardIndex;
    }
    #endregion
}
