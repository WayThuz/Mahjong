using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

using CombinationNamespace;

public class MahjongSys : MonoBehaviourPunCallbacks{
    public static MahjongSys current;
    private PhotonView photonview;
    [SerializeField] private GameObject[] playerObjs = new GameObject[4];
    private Card currentCardPlayed = null;
    private Card cardGot = null;
    private List<Card> shuffledDeck = new List<Card>();
    private IEnumerator decidedNextPlayerCoroutine;

    private bool isShuffled = false;
    private int nextCardIndexBeingDrew = 64;//0~63 + 1
    private int currentTurnGiverIndex = -1;

    const int numberOfPlayers = 4;
    const int number_TotalDeals = 4;
    const int cards_EachDeal = 4;

    //-1 for initial, 0 for eat, 1 for pon, 2 for win  -100 for stopping
    private int[] playerMovement = new int[numberOfPlayers] { -1, -1, -1, -1 };
    private bool[] turnFinishedCheck = new bool[numberOfPlayers] { false, false, false, false };
    private bool isCardGotInDeck = false;
    
#region MethodSystemWouldCall
    void Awake(){
        if (current == null) current = this;
        photonview = GetComponent<PhotonView>();
    }

    public void OnPlayerAllPrepared(){
        PlayerAwake();
        InitializeCenterDeck();
    }

    #region PlayerAwake
    void PlayerAwake(){
        photonview.RPC("playerGameObjectEnabled", RpcTarget.AllBuffered);
    }    
    [PunRPC]
    void playerGameObjectEnabled(){
        foreach(GameObject player in playerObjs){
            player.SetActive(true);
        }
    }
    #endregion
    
    #region Initialized
    void InitializeCenterDeck(){
        Card[] initialDeck = GenerateDeck();
        StartCoroutine(shuffledCards(initialDeck, () => SystemActivated()));
    }
    
    Card[] GenerateDeck(){
        Card[] deck = new Card[144];
        for (int j = 0; j < 5; j++){
            int cardAmounts; int orderStarting; int circleInType;
            switch (j){
                case 0://字 
                    cardAmounts = 28; orderStarting = 108; circleInType = 7;
                    break;
                case 4://花
                    cardAmounts = 8; orderStarting = 136; circleInType = 100;//big enough to prevent from repeated;
                    break;

                default://條筒萬
                    cardAmounts = 36; orderStarting = 36 * (j - 1); circleInType = 9;
                    break;
            }
            for (int i = 0; i < cardAmounts; i++){
                Card newCard = new Card(j, i % circleInType + 1);
                deck[i + orderStarting] = newCard;
            }
        }
        return deck;
    }

    IEnumerator shuffledCards(Card[] initialDeck, Action method){
        FirstShuffle(initialDeck);
        yield return new WaitForSeconds(0.5f);
        NextShuffle();
        yield return new WaitForSeconds(0.5f);
        NextShuffle();
        yield return new WaitForSeconds(0.5f);
        isShuffled = true;
        method();
    }

    void FirstShuffle(Card[] deck){
        List<int> assignList = new List<int>();
        for (int i = 0; i < 144; i++){
            assignList.Add(i);
        }
        for (int i = 0; i < 144; i++){
            int randomIndex = UnityEngine.Random.Range(0, assignList.Count);
            shuffledDeck.Add(deck[assignList[randomIndex]]);
            assignList.RemoveAt(randomIndex);
        }
    }

    void NextShuffle(){
        List<int> assignList = new List<int>();
        List<Card> newShuffledDeck = new List<Card>();
        for (int i = 0; i < 144; i++){
            assignList.Add(i);
        }
        for (int i = 0; i < 144; i++){
            int randomIndex = UnityEngine.Random.Range(0, assignList.Count);
            newShuffledDeck.Add(shuffledDeck[assignList[randomIndex]]);
            assignList.RemoveAt(randomIndex);
        }
        shuffledDeck = newShuffledDeck;
    }
    #endregion

    #region SystemActivated
    void SystemActivated(){
        if(isShuffled){
            assignShuffleDeckToOtherClient();
            photonview.RPC("settingsForNextTurn", RpcTarget.AllBuffered, 0, true);
        }
    }
    void assignShuffleDeckToOtherClient(){   
        int[] types = new int[144];
        int[] numbers = new int[144];
        for(int i = 0; i < 144; i++){
            types[i] = shuffledDeck[i].Type;
            numbers[i] = shuffledDeck[i].Number;
        }
        photonview.RPC("assignDeck", RpcTarget.OthersBuffered, types, numbers);
    }

    [PunRPC]
    void assignDeck(int[] types, int[] numbers){
        if(!isShuffled){
            this.shuffledDeck = new List<Card>();
            for (int i = 0; i < 144; i++){
                Card card = new Card(types[i], numbers[i]);
                shuffledDeck.Add(card);
            }
            isShuffled = true;
        }
    }
    #endregion  

    #region LoadTurn
    public void LoadNextTurn(int previousPlayer, int cardPlayedType, int cardPlayedNumber){
        makeLocalPlayerCanDoNothing();
        photonview.RPC("synchronizeVariables", RpcTarget.AllBuffered, cardPlayedType, cardPlayedNumber);
        if(PhotonNetwork.IsMasterClient) MasterSysLoadNextTurn(previousPlayer);
        else photonview.RPC("MasterSysLoadNextTurn", RpcTarget.MasterClient, previousPlayer);
    }

    void makeLocalPlayerCanDoNothing(){
        currentTurnGiverIndex = -1;//let previous player(i.e the localPlayer call this function) cannot become giver/awaiter before the giver being decided
    }

    [PunRPC]
    void synchronizeVariables(int cardPlayedType, int cardPlayedNumber){
        setCurrentCardPlayed(cardPlayedType, cardPlayedNumber);
    }

    void setCurrentCardPlayed(int cardPlayedType, int cardPlayedNumber) {
        Card cardPlayed = new Card(cardPlayedType, cardPlayedNumber);
        CurrentCardPlayed = cardPlayed;
    }

    [PunRPC]
    void MasterSysLoadNextTurn(int previousPlayer){
        decidedNextPlayerCoroutine = decidedNextPlayer(previousPlayer);
        StartCoroutine(decidedNextPlayerCoroutine);
    }

    IEnumerator decidedNextPlayer(int previousPlayer){  
        for (int i = 0; i < 20; i++){
            yield return new WaitForSeconds(1f);
            if (allFinished()){
                nextPlayerAction(previousPlayer);
                yield break;
            }
        } 
        nextPlayerAction(previousPlayer);
    }

    bool allFinished(){
        bool isMoved = true;
        foreach (int movement in playerMovement){
            if (movement == -1){
                isMoved = false;
                Debug.Log("isMoved = false");
            }
        }
        return (isMoved && turnFinishedCheck[0] && turnFinishedCheck[1] && turnFinishedCheck[2] && turnFinishedCheck[3]);
    }

    void nextPlayerAction(int previousPlayer){
        bool isWinner = false;
        int nextPlayer = getPlayerByCheckMovement(previousPlayer, out isWinner);
        if(isWinner) PlayerWins(nextPlayer);
        else StartCoroutine(turnPreparation(nextPlayer));
    }


    int getPlayerByCheckMovement(int previousPlayer, out bool isWinner){
        int winnerPlayer = getWinner(previousPlayer);
        if(winnerPlayer != -1){
            isWinner = true;
            return winnerPlayer;
        }
        else{
            int nextPlayer = getNextPlayer(previousPlayer);
            isWinner = false;
            return nextPlayer;
        }
    }

    int getWinner(int previousPlayer){
        int winnerPlayer = -1;
        for(int i  = 0; i < 4; i++){
            int playerOrder = (previousPlayer + i)%4;
            if(playerMovement[playerOrder] == 2){
                winnerPlayer = playerOrder;
                return winnerPlayer;
            }
        }
        return winnerPlayer;
    }

    int getNextPlayer(int previousPlayer){
        int nextPlayer = previousPlayer;
        for (int playerCompared = 0; playerCompared < playerMovement.Length; playerCompared++){
            nextPlayer = comparePlayersIndex(nextPlayer, playerCompared, previousPlayer);
        }
        if (playerMovement[nextPlayer] < 0) nextPlayer = (previousPlayer + 1) % numberOfPlayers;
        return nextPlayer;
    }

    int comparePlayersIndex(int playerIndex, int playerCompared, int previousPlayer){
        if (playerMovement[playerIndex] == playerMovement[playerCompared]){
            if (playerCompared - previousPlayer > 0 && previousPlayer - playerIndex > 0) return playerCompared;
            else if (playerCompared < playerIndex) return playerCompared;
        }
        else if (playerMovement[playerCompared] > playerMovement[playerIndex]) return playerCompared;
        return playerIndex;
    }

    public void PlayerWins(int winnerOrder){
       photonview.RPC("loadWinningScene", RpcTarget.AllBuffered, winnerOrder);
    }

    [PunRPC]
    void loadWinningScene(int winnerOrder){
        PlayerPrefs.SetInt("winnerPlayerIndex", winnerOrder);
        SceneManager.LoadScene(2);
    }

    IEnumerator turnPreparation(int playerIndex){
        StopCoroutine(decidedNextPlayerCoroutine);
        bool isAllPlayerPass = (playerMovement[playerIndex] < 0);
        yield return new WaitForSeconds(0.5f);
        photonview.RPC("settingsForNextTurn", RpcTarget.AllBuffered, playerIndex, isAllPlayerPass);
    }
    #endregion

    #region SettingsForNextTurn
    [PunRPC]
    void settingsForNextTurn(int playerIndex, bool isAllPlayerPass){    
        setCardGot(isAllPlayerPass); 
        CurrentCardPlayed = null;
        resetMovement(PhotonNetwork.IsMasterClient);
        resetFinishedCheck();
        if(playerIndex != -1) currentTurnGiverIndex = playerIndex;   
    }

    void setCardGot(bool isAllPlayerPass){   
        if(isAllPlayerPass){
            Debug.Log("CardGot = " + nextCardIndexBeingDrew.ToString());
            cardGot = shuffledDeck[nextCardIndexBeingDrew];
            nextCardIndexBeingDrew++;
            isCardGotInDeck = true;
        }
        else{
            cardGot = CurrentCardPlayed;
            isCardGotInDeck = false;
        }
    }

    

    void resetMovement(bool isMasterClient){
        for (int i = 0; i < numberOfPlayers; i++){
            setPlayerMovement(i, -1, isMasterClient);
        }
    }

    void resetFinishedCheck(){
        for (int i = 0; i < numberOfPlayers; i++){
            turnFinishedCheck[i] = false;
        }
    }
    #endregion

    #region Others
    public void storeWinningDeck(List<Card> deck, List<int[]> melds, int meldCount){
        PlayerPrefs.SetInt("winnerDeckCount", deck.Count);
        PlayerPrefs.SetInt("winnerMeldsCount", meldCount);
        for(int i = 0; i < deck.Count; i++){
            PlayerPrefs.SetString("winnerDeck_" + i, deck[i].Order.ToString());
        }

        if(melds.Count != meldCount){
            Debug.LogWarning("melds.Count does not equals to the variable meldCount");
        }

        for(int i = 0; i < meldCount; i++){
            PlayerPrefs.SetInt("winnerMeldsCount_" + i, i);
            for(int j = 0; j < melds[i].Length; j++){
                PlayerPrefs.SetString("winnerMelds_" + i + "_" + j, melds[i][j].ToString());
            }
        }
    }
    #endregion
#endregion

#region MethodPlayerWouldCall
    public List<Card> DealCards(int playerOrder){
        List<Card> myDeck = new List<Card>();
        for (int i = 0; i < number_TotalDeals; i++){
            int cardOnTop = playerOrder * cards_EachDeal + i * 16;
            myDeck.Add(shuffledDeck[cardOnTop]);
            myDeck.Add(shuffledDeck[cardOnTop + 1]);
            myDeck.Add(shuffledDeck[cardOnTop + 2]);
            myDeck.Add(shuffledDeck[cardOnTop + 3]);
        }
        return myDeck;
    }

    #region PlayerFinishTurn
    public void finishedTurn(int playerOrder){
        if(!turnFinishedCheck[playerOrder]){
            photonview.RPC("receivedPlayerFinishedTurn", RpcTarget.AllBuffered, playerOrder);
        }
    }

    [PunRPC]
    void receivedPlayerFinishedTurn(int playerOrder){
        if(!turnFinishedCheck[playerOrder]) turnFinishedCheck[playerOrder] = true;
    }
    #endregion

    public void setPlayerMovement(int playerOrder, int nextMovement, bool isReset){
        photonview.RPC("receivedPlayerSetMovement", RpcTarget.AllBuffered, playerOrder, nextMovement, isReset);
    }

    [PunRPC]
    void receivedPlayerSetMovement(int playerOrder, int nextMovement, bool isReset){
        if(isReset && nextMovement == -1) playerMovement[playerOrder] = nextMovement;
        else if(!isReset && nextMovement != -1) playerMovement[playerOrder] = nextMovement;
    }

    public bool HasMovementPriorThanMine(int playerOrder, int movement){
        if(movement == -100){
            throw new ArgumentException("movement = -100, which is the value should not call this function");
        }
        for(int i = 0; i < playerMovement.Length; i++){
            if(i == playerOrder) continue;
            if(playerMovement[i] > movement || playerMovement[i] == 2){
                return true;
            }
        }
        return false;
    }

    public bool WinningCheck(int playerOrder, List<Combination> combinationList, int meldOnBroadCount, out int numberOfWinningCombination){
        numberOfWinningCombination = 0;
        bool winningCheck = false;
        foreach (Combination combination in combinationList){
            if (combination.meldsInDeck[0] + meldOnBroadCount == 5 && combination.meldsInDeck[1] == 1){
                winningCheck = true;
                numberOfWinningCombination++;
            }
        }
        return winningCheck;
    }

    public bool IsCardGiver(int playerIndex){
        return ((playerIndex + 4) % 4 == currentTurnGiverIndex);
    }

    public bool IsCardAwaiter(int playerIndex){
        return (playerIndex != currentTurnGiverIndex);
    }

    public bool playerCanEat(bool deckCanEat, int playerOrder){
        return (deckCanEat && (playerOrder == (currentTurnGiverIndex + 1) % numberOfPlayers));
    }

    public bool playerCanPon(bool deckCanPon, int playerOrder){
        return (deckCanPon && (playerOrder != currentTurnGiverIndex));
    }

    public bool SystemAllPrepared(ref bool isGameStart)
    {
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

    public Card DrawCardInDeck(){
        photonview.RPC("receivedDrawCard", RpcTarget.MasterClient, PlayerPrefs.GetInt("PlayerID"));
        if(!PhotonNetwork.IsMasterClient) nextCardIndexBeingDrew++;
        return shuffledDeck[nextCardIndexBeingDrew];
    }

    [PunRPC]
    void receivedDrawCard(int name){
        if(PhotonNetwork.IsMasterClient){
            nextCardIndexBeingDrew++;
            photonview.RPC("setNextCardBeingDrew", RpcTarget.OthersBuffered, nextCardIndexBeingDrew);
        }
    }

    [PunRPC]
    void setNextCardBeingDrew(int newIndex){
        nextCardIndexBeingDrew = newIndex;
    }

    public Card CurrentCardPlayed{
        get{
            return currentCardPlayed;
        }
        private set{
            currentCardPlayed = value;
        }
    }

#endregion

    
}