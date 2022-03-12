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
        StartCoroutine(localPlayerScriptCheck());   
    }

    IEnumerator localPlayerScriptCheck(){
        localplayer = GameObject.Find("localPlayer").GetComponent<localPlayer>();
        yield return new WaitForSeconds(0.2f);

        if(!isThisPlayerScriptBelongsToLocalPlayer){
            this.enabled = false;
        }
        else{
            isAllComponentSet = true;
            myCameraMove.setPlayerPositionToCamera(this.transform);
        }
    }

    bool isThisPlayerScriptBelongsToLocalPlayer{
        get{
            return(localplayer.GetLocalPlayerOrder == myOrder);
        }
    }

    void LateUpdate(){ 
        if(isAllComponentSet){
            if(MahjongSys.current.SystemAllPrepared(ref isGameStart)) gameStart();
            if(MahjongSys.current.IsCardGiver(myOrder)) turn_CardGiver();
            else if(MahjongSys.current.IsCardAwaiter(myOrder)) turn_CardAwaiter();             
        }
    }

    #region AllPlayer movement
    void gameStart(){ 
        myDeck = MahjongSys.current.DealCards(myOrder); 
        replaceCards();
        buttonTakeRest();
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
        bool isMeldAssigned = myDeckUI.isMeldAssigned;
        if(isMeldAssigned){
            myDeckUI.showMeldToBroad(ref myDeck, cardGot);
            yield break;  
        }
        else{
            for(int i = 0; i < 200; i++){
                yield return new WaitForSeconds(0.1f);
                if(isMeldAssigned){
                    myDeckUI.showMeldToBroad(ref myDeck, cardGot);
                    yield break;  
                }
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
                myDeck[i] = MahjongSys.current.DrawCardInDeck;
            }
            else if(newCardAwait != null && newCardAwait.Order >= flowerOrder){
                cardReplaced = newCardAwait;
                newCardAwait = MahjongSys.current.DrawCardInDeck;
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
    //0 for only eat, 1 for only pon, 2 for win 
    void movementCheck(List<Card> deck, Card card){  
        int eat, pon, kong, win; 
        eat = pon = kong = win = 0;      
        isMovementChecked = true;  
        if(card == null){
            decidedMovementCoroutine = decidedMovement(eat, pon, kong, win);
            StartCoroutine(decidedMovementCoroutine);
        } 
        else{
            List<Combination> combine_DeckAndCard = CombinationMethod.CombinationCalculator(deck, card);
            bool[] checkTable = IsInMelds(combine_DeckAndCard, card);  
            if(checkTable[2]) kong = 1; 
            if(isWin(combine_DeckAndCard)) win = 1;
            if(!MahjongSys.current.IsCardGiver(myOrder)){
                if(MahjongSys.current.playerCanEat(checkTable[0], myOrder)) eat = 1;
                if(MahjongSys.current.playerCanPon(checkTable[1], myOrder)) pon = 1;
            }
            decidedMovementCoroutine = decidedMovement(eat, pon, kong, win);
            StartCoroutine(decidedMovementCoroutine);
        }      
    }
    
    IEnumerator decidedMovement(int eat, int pon, int kong, int win){
        isMovementChecked = true;
        movementButtonSetActive(eat, pon, kong, win);      
        if(eat + pon + kong + win == 0){
            passThisTurn();
            yield break;
        }  
        for(int i = 0; i < 200; i++){
            yield return new WaitForSeconds(0.1f);//count
            if(nextMovement != initialStatus) yield break;    
        }
        passThisTurn();
    }
    
    void movementButtonSetActive(int eat, int pon, int kong, int win){
        if(MahjongSys.current.IsCardGiver(myOrder)){
            if(kong == 1) kongButton.SetActive(true); if(win == 1) winButton.SetActive(true);
            if(kong + win > 0) quitMovementButton.SetActive(true);
        }
        else{
            if(eat == 1) eatButton.SetActive(true); if(pon == 1) ponButton.SetActive(true); 
            if(win == 1) winButton.SetActive(true); if(kong == 1) kongButton.SetActive(true); 
            if(eat + pon + kong + win > 0) quitMovementButton.SetActive(true);
        }       
    }
    
    void passThisTurn(){
        nextMovement = -100; // if no decided;
        MahjongSys.current.setPlayerMovement(myOrder, nextMovement);
    }

    //0 for eat, 1 for pun, 2 for win, -100 for stop 
    public void pushMovementButton(int movement){
        if(decidedMovementCoroutine != null){
            StopCoroutine(decidedMovementCoroutine);
            decidedMovementCoroutine = null;
        }  
        nextMovement = movement;
        MahjongSys.current.setPlayerMovement(myOrder, nextMovement);
        nextMovement = stoppedStatus;    
        if(movement == 0 || movement == 1 || movement == 3){
            displayMeld(movement);        
            StartCoroutine(meldHintLifeTimeCountdown(movement)); 
        }
        else if(MahjongSys.current.IsCardGiver(myOrder) && movement == 2) MahjongSys.current.PlayerWins(myOrder);//自摸

        buttonTakeRest();
    }    

    void displayMeld(int movement){ 
        Card currentCardPlayed = MahjongSys.current.CurrentCardPlayed; 
        if(currentCardPlayed != null) myDeckUI.AssignMeld(movement, myDeck, currentCardPlayed.Order);   
        else if(movement == 3){
            List<int> cardsInKong = CombinationMethod.deckHasKong(myDeck, newCardAwait);
            myDeckUI.AssignMultipleKongs(myDeck, cardsInKong);
        } 
    }

    IEnumerator meldHintLifeTimeCountdown(int movement){
        for(int i = 0; i < 100; i++){
            yield return new WaitForSeconds(0.05f);
            if(MahjongSys.current.HasMovementPriorThanMine(myOrder, movement)) myDeckUI.DestroyAllHints();
        }
        myDeckUI.DestroyAllHints();
    } 
    
    void buttonTakeRest(){
        eatButton.SetActive(false); ponButton.SetActive(false);
        kongButton.SetActive(false); winButton.SetActive(false);
        quitMovementButton.SetActive(false);
    }

    bool[] IsInMelds(List<Combination> combine_DeckAndCard, Card card){
        bool[] tableOfMelds = new bool[3]{false,false,false};
        foreach(Combination combination in combine_DeckAndCard){ 
            bool[] SequenceOrTriplet = combination.IsInSequenceOrTriplet(card);
            if(SequenceOrTriplet[0]) tableOfMelds[0] = true;
            if(SequenceOrTriplet[1]) tableOfMelds[1] = true;          
        }
        List<int> cardsInKong = CombinationMethod.deckHasKong(myDeck, card);
        if(cardsInKong.Contains(card.Order) && !MahjongSys.current.IsCardGiver(myOrder - 1)) tableOfMelds[2] = true;//明槓
        else if(cardsInKong.Count > 0 && MahjongSys.current.IsCardGiver(myOrder)) tableOfMelds[2] = true;//暗槓
        return tableOfMelds;
    }

    bool isWin(List<Combination> combinations){
        int numberOfWinningCombination = 0;
        if(MahjongSys.current.WinningCheck(myOrder, combinations, myDeckUI.MeldOnBroadCount, out numberOfWinningCombination)){
            Debug.Log("Player order: " + myOrder.ToString() + " wins !!! Number of winning combinations: " + numberOfWinningCombination.ToString());
            return true;
        }
        return false;
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
