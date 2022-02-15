using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

using CombinationNamespace;

public class MahjongSys : MonoBehaviourPunCallbacks
{   
    public static MahjongSys current;
    private PhotonView photonview;
    
    private Card[] deck = new Card[144];
    private Card currentCardPlayed = null;
    private Card cardGot = null;
    private List<Card> shuffledDeck = new List<Card>();
    private IEnumerator decidedNextPlayerCoroutine;

    private bool isShuffled = false;
    private bool isAllPlayerPass = true;
    private int cardPlayerIndex = -1;
    private int nextCardIndex = -1;
    private int currentTurnPlayerIndex = -1;
    private int localPlayerOrder = -1;
    
    const int numberOfPlayers = 4;
    const int number_TotalDeals = 4;
    const int cards_EachDeal = 4;

    //-1 for initial, 0 for eat, 1 for pon, 2 for win  -100 for stopping
    private int[] playerMovement = new int[numberOfPlayers]{-1,-1,-1,-1};
    private bool[] turnFinishedCheck = new bool[numberOfPlayers]{false,false,false,false};

    void Awake(){
        if(current == null) current = this;    
        photonview = GetComponent<PhotonView>();
        if(PhotonNetwork.IsMasterClient){
            photonview.RPC("InitializeCenterDeck", PhotonNetwork.LocalPlayer);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player other){
        if(PhotonNetwork.IsMasterClient){
            assignShuffleDeckToEachClient();
        }
    }

    [PunRPC]
    void InitializeCenterDeck(){
        GenerateCards();       
        StartCoroutine(shuffledCards());
    }

    void GenerateCards(){       
        for(int j = 0; j < 5; j++){          
            int cardAmounts; int orderStarting; int circleInType;
            switch(j){
                case 0://字 
                    cardAmounts = 28; orderStarting = 108; circleInType = 7;
                    break;
                case 4://花
                    cardAmounts = 8; orderStarting = 136; circleInType = 100;//big enough to prevent from repeated;
                    break;

                default://條筒萬
                    cardAmounts = 36; orderStarting = 36*(j-1); circleInType = 9;
                    break; 
            }
            for(int i = 0; i < cardAmounts; i++){
                Card newCard = new Card(j, i%circleInType + 1);
                deck[i + orderStarting] = newCard;
            }            
        }
    }
    
    IEnumerator shuffledCards(){
        FirstShuffle();
        yield return new WaitForSeconds(0.5f);
        NextShuffle();
        yield return new WaitForSeconds(0.5f);
        NextShuffle();
        yield return new WaitForSeconds(0.5f);
        isShuffled = true;
        yield return new WaitForSeconds(0.5f);
        SystemActivated();
    }

    void FirstShuffle(){
        List<int> assignList = new List<int>();
        for(int i = 0; i < 144; i++){
            assignList.Add(i);
        }    
        for(int i = 0; i < 144; i++){
            int randomIndex = Random.Range(0, assignList.Count);
            shuffledDeck.Add(deck[assignList[randomIndex]]);
            assignList.RemoveAt(randomIndex);
        }  
    }

    void NextShuffle(){
        List<int> assignList = new List<int>();
        List<Card> newShuffledDeck = new List<Card>();
        for(int i = 0; i < 144; i++){
            assignList.Add(i);
        }    
        for(int i = 0; i < 144; i++){
            int randomIndex = Random.Range(0, assignList.Count);
            newShuffledDeck.Add(shuffledDeck[assignList[randomIndex]]);
            assignList.RemoveAt(randomIndex);
        }  
        shuffledDeck = newShuffledDeck;
    }

    public List<Card> DealCards(int playerOrder){
        List<Card> myDeck = new List<Card>();
        for(int i = 0; i < number_TotalDeals; i++){
            int cardOnTop = playerOrder*cards_EachDeal + i*16;
            myDeck.Add(shuffledDeck[cardOnTop]);
            myDeck.Add(shuffledDeck[cardOnTop + 1]);
            myDeck.Add(shuffledDeck[cardOnTop + 2]);
            myDeck.Add(shuffledDeck[cardOnTop + 3]);
            if(nextCardIndex < cardOnTop + 3) nextCardIndex = cardOnTop + 3;
        }
        return myDeck;
    }
  
    void SystemActivated(){
        Debug.Log("SystemActivated");
        assignShuffleDeckToEachClient();
        settingsForNextTurn();      
        currentTurnPlayerIndex = 0;
    }

    void assignShuffleDeckToEachClient(){
        List<int> types = new List<int>();
        List<int> numbers = new List<int>();
        foreach(Card card in shuffledDeck){
            types.Add(card.Type);
            numbers.Add(card.Number);
        }
        photonview.RPC("assignDeck", RpcTarget.All, types.ToArray(), numbers.ToArray());
    }

    [PunRPC]
    void assignDeck(int[] types, int[] numbers){
        if(!isShuffled){
            isShuffled = true;
            this.shuffledDeck = new List<Card>();
            for(int i = 0; i < types.Length; i++){
                Card card = new Card(types[i], numbers[i]);
                shuffledDeck.Add(card);
            }
        }
    }
    
    public IEnumerator LoadNextTurn(int previousPlayer, Card cardPlayed){
        currentTurnPlayerIndex = -1;
        CurrentCardPlayed = cardPlayed;
        decidedNextPlayerCoroutine = decidedNextPlayer(previousPlayer);
        yield return StartCoroutine(decidedNextPlayerCoroutine);     
    }

    IEnumerator decidedNextPlayer(int previousPlayer){
        int nextPlayer = -1;
        if(previousPlayer == -1){  
            nextPlayer = 0;
            yield return StartCoroutine(turnPreparation(nextPlayer)); 
            yield break;
        }

        for(int i = 0; i < 21; i++){
            yield return new WaitForSeconds(1f);   
            if(!allFinished) continue;            
            nextPlayer = getNextPlayer(previousPlayer);
            yield return StartCoroutine(turnPreparation(nextPlayer));
            yield break;    
        }
        nextPlayer = getNextPlayer(previousPlayer);   
        yield return StartCoroutine(turnPreparation(nextPlayer)); 
    }
    
    IEnumerator turnPreparation(int playerIndex){  
        StopCoroutine(decidedNextPlayerCoroutine);
        if(playerMovement[playerIndex] < 0) isAllPlayerPass = true;       
        else isAllPlayerPass = false;  
        settingsForNextTurn();
        yield return new WaitForSeconds(0.2f);
        currentTurnPlayerIndex = playerIndex;
    }

    void settingsForNextTurn(){
        setCardGot();
        resetMovement();
        resetFinishedCheck(); 
    }
    
    bool isCardGotInDeck = false;
    void setCardGot(){
        if(isAllPlayerPass){
            cardGot = DrawCardInDeck;  
            isCardGotInDeck = true;
        }
        else{
            cardGot = currentCardPlayed;
            isCardGotInDeck = false;
        }
        CurrentCardPlayed = null; 
    }

    void resetMovement(){
        for(int i = 0; i < numberOfPlayers; i++){
            setPlayerMovement(i, -1);
        }
    }

    void resetFinishedCheck(){
        for(int i = 0; i < numberOfPlayers; i++){
            turnFinishedCheck[i] = false;
        }
    }

    int getNextPlayer(int previousPlayer){
        int nextPlayer = previousPlayer;
        for(int playerCompared = 0; playerCompared < playerMovement.Length; playerCompared++){ 
            nextPlayer = comparePlayersIndex(nextPlayer, playerCompared, previousPlayer); 
        }
        if(playerMovement[nextPlayer] < 0) nextPlayer = (previousPlayer+1) % numberOfPlayers;
        return nextPlayer;
    }

    int comparePlayersIndex(int playerIndex, int playerCompared, int previousPlayer){
        if(playerMovement[playerIndex] == playerMovement[playerCompared]){
            if(playerCompared - previousPlayer > 0 && previousPlayer - playerIndex > 0) return playerCompared;
            else if(playerCompared < playerIndex) return playerCompared;  
        }
        else if(playerMovement[playerCompared] > playerMovement[playerIndex]) return playerCompared;
        return playerIndex;       
    }

   
    public void finishedTurn(int playerOrder){
        if(IsCardGiver(playerOrder)) cardGiverFinishedTurn(playerOrder);
        else cardAwaiterFinishedTurn(playerOrder);
    }

    void cardGiverFinishedTurn(int playerOrder){
        if(!turnFinishedCheck[playerOrder]) turnFinishedCheck[playerOrder] = true;
        cardPlayerIndex = playerOrder;
    }

    void cardAwaiterFinishedTurn(int playerOrder){
        if(!turnFinishedCheck[playerOrder]) turnFinishedCheck[playerOrder] = true;
    }

    public void setPlayerMovement(int playerOrder, int nextMovement){
        if(playerOrder < numberOfPlayers) playerMovement[playerOrder] = nextMovement;      
    }

    public bool WinningCheck(int playerOrder, List<Combination> combinationList, int meldOnBroadCount,out int numberOfWinningCombination){  
        numberOfWinningCombination = 0;
        bool winningCheck = false;
        foreach(Combination combination in combinationList){
            //Debug.Log("Player " + playerOrder.ToString() + ": (" + combination.meldsInDeck[0].ToString() + ", " + meldOnBroadCount.ToString() + ")");
            if(combination.meldsInDeck[0] + meldOnBroadCount == 5 && combination.meldsInDeck[1] == 1){ 
                winningCheck = true; 
                numberOfWinningCombination++; 
            }
        }
        return winningCheck;     
    }


    public bool IsCardGiver(int playerOrder){
        return ((playerOrder + 4)%4 == currentTurnPlayerIndex);
    }

    public bool IsCardAwaiter(int playerOrder){
        return (playerOrder != currentTurnPlayerIndex);
    }

    public bool playerCanEat(bool deckCanEat, int playerOrder){
        return (deckCanEat && (playerOrder == (cardPlayerIndex+1) % numberOfPlayers));
    }

    public bool playerCanPon(bool deckCanPon, int playerOrder){
        return (deckCanPon && (playerOrder != cardPlayerIndex));
    }

    public bool SystemAllPrepared(ref bool isGameStart){
        if(!isGameStart && isShuffled){
            isGameStart = true;
            return true;
        }
        else return false;
    }

    public bool CardGiverDrawCard(out Card CardGot){
        CardGot = cardGot;
        return isCardGotInDeck;
    }

    public Card DrawCardInDeck{
        get{
            nextCardIndex++;
            return shuffledDeck[nextCardIndex];
        }
    }

    public Card CurrentCardPlayed{
        get{
            return currentCardPlayed;
        }
        private set{
            currentCardPlayed = value;
        }
    }

    public int LocalPlayerOrder{
        get{
            localPlayerOrder++;
            return localPlayerOrder;
        }
    }

    bool allFinished{
        get{
            bool isMoved = true;
            foreach(int movement in playerMovement){
                if(movement == -1) isMoved = false;
            }
            return (isMoved && turnFinishedCheck[0] && turnFinishedCheck[1] && turnFinishedCheck[2] && turnFinishedCheck[3]);
        }
    }

    int[] eachTypeCount = new int[5]{8,10,10,10,9};
    public void showWholeDeck(List<Card> deck){
        List<int> deckType = new List<int>();
        int[] amountEachType = new int[42];
        for(int i = 0; i < 42; i++){ 
            amountEachType[i] = 0; 
        }

        for(int i = 0; i < 5; i++){
            for(int j = 1; j < eachTypeCount[i]; j++){ deckType.Add(i*100+j); }
        }

        foreach (Card card in deck){
            int order =  card.Order;
            for(int i = 0; i < deckType.Count; i++){
                if(deckType[i] == order) amountEachType[i]++;     
            }
        }

        for(int i = 0; i < 42; i++){
            Debug.Log("id: " + i.ToString() + " amounts: " + amountEachType[i].ToString());
        }       
    }
}
