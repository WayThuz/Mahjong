using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

using CombinationNamespace;
using CardDealer;
using Judge;

public class MahjongSys : MonoBehaviourPunCallbacks{
    public static MahjongSys current;
    private PhotonView photonview;
    [SerializeField] private GameObject[] playerObjs = new GameObject[4];
    public Card CurrentCardPlayed{ get; private set; }
    private Card cardGot = null;
    private List<Card> shuffledDeck = new List<Card>();
    private IEnumerator decidedNextPlayerCoroutine;

    private bool isShuffled = false;
    private int nextCardIndexBeingDrew = 64;//0~63 + 1
    private int currentTurnGiverIndex = -1;

    const int numberOfPlayers = 4;

    //-1 for initial, 0 for eat, 1 for pon, 2 for win  -100 for stopping
    private int[] playerMovement = new int[numberOfPlayers] { -1, -1, -1, -1 };
    private bool[] turnFinishedCheck = new bool[numberOfPlayers] { false, false, false, false };
    private bool isCardGotInDeck = false;
    
#region MethodSystemWouldCall
    void Awake(){
        if (current == null) current = this;
        photonview = GetComponent<PhotonView>();
    }

    void Update(){
        if(nextCardIndexBeingDrew >= 128){
            if(PhotonNetwork.IsMasterClient){             
                restartScene();
            }
        }
    }

    void restartScene(){
        string restartMessage = "流局";
        PlayerPrefs.SetString("restartMessage", restartMessage);
        photonview.RPC("loadRestartScene", RpcTarget.AllBuffered);        
    }

    [PunRPC]
    void loadRestartScene(){
        SceneManager.LoadScene("RestartScene");
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
        Card[] initialDeck = Dealer.generateDeck();
        StartCoroutine(shuffleCenterDeck(initialDeck, () => SystemActivated()));
    }

    IEnumerator shuffleCenterDeck(Card[] initialDeck, Action method){
        shuffledDeck = Dealer.shuffle(initialDeck);
        yield return new WaitForSeconds(0.5f);
        shuffledDeck = Dealer.shuffle(shuffledDeck.ToArray());
        yield return new WaitForSeconds(0.5f);
        shuffledDeck = Dealer.shuffle(shuffledDeck.ToArray());
        yield return new WaitForSeconds(0.5f);
        isShuffled = true;
        method();
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
        int[] types = Dealer.type_Cards(shuffledDeck);
        int[] numbers = Dealer.ID_Cards(shuffledDeck);
        photonview.RPC("assignDeck", RpcTarget.OthersBuffered, types, numbers);
    }

    [PunRPC]
    void assignDeck(int[] types, int[] numbers){
        if(!isShuffled){
            this.shuffledDeck = Dealer.assignDeck(types, numbers);       
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
        CurrentCardPlayed = new Card(cardPlayedType, cardPlayedNumber);
    }

    [PunRPC]
    void MasterSysLoadNextTurn(int previousPlayer){
        decidedNextPlayerCoroutine = decidedNextPlayer(previousPlayer);
        StartCoroutine(decidedNextPlayerCoroutine);
    }

    IEnumerator decidedNextPlayer(int previousPlayer){  
        for (int i = 0; i < 20; i++){
            yield return new WaitForSeconds(1f);
            if(PlayerDecider.allFinished(playerMovement, turnFinishedCheck)){
                nextPlayerAction(previousPlayer);
                yield break;
            }
        } 
        nextPlayerAction(previousPlayer);
    }

    void nextPlayerAction(int previousPlayer){
        bool isWinner = false;
        int nextPlayer = PlayerDecider.getPlayerByCheckMovement(playerMovement, previousPlayer, out isWinner);
        if(isWinner) PlayerWins(nextPlayer);
        else StartCoroutine(turnPreparation(nextPlayer));
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
        return Dealer.dealCards(playerOrder, shuffledDeck);
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

    public bool HasMovementPriorThanMine(int myOrder, int myMovement){
        return PlayerDecider.hasPlayerPrior(playerMovement, myOrder, myMovement);
    }

    public bool IsCardGiver(int playerOrder){
        return PlayerIdentifier.isCardGiver(playerOrder, currentTurnGiverIndex);
    }

    public bool IsCardAwaiter(int playerOrder){
        return PlayerIdentifier.isCardAwaiter(playerOrder, currentTurnGiverIndex);
    }

    public bool[] playerCanDoMovement(int playerOrder, List<Card> deck, Card card, int meldCount){
        bool[] enabledMovement = new bool[4]{false, false, false, false};
        List<Combination> combine = CombinationMethod.CombinationCalculator(deck, card);
        int eat = 0; int pon = 1; int win = 2; int kong = 3;
        if(playerCanEat(playerOrder, combine, card)) enabledMovement[eat] = true;
        if(playerCanPon(playerOrder, combine, card)) enabledMovement[pon] = true;
        if(playerCanWin(playerOrder, combine, meldCount)) enabledMovement[win] = true;
        if(playerCanKong(playerOrder, deck, card)) enabledMovement[kong] = true; 
        return enabledMovement;
    }

    bool playerCanEat(int playerOrder, List<Combination> combine, Card card){
        if(IsCardGiver(playerOrder)) return false;
        return DeckIdentifier.canEat(playerOrder, currentTurnGiverIndex, combine, card);
    }

    bool playerCanPon(int playerOrder, List<Combination> combine, Card card){
        if(IsCardGiver(playerOrder)) return false;
        return DeckIdentifier.canPon(playerOrder, currentTurnGiverIndex, combine, card);
    }

    bool playerCanKong(int playerOrder, List<Card> deck, Card card){
        List<int> cardsInKong = CombinationMethod.deckHasKong(deck, card);
         if(cardsInKong.Contains(card.Order) && IsCardGiver(playerOrder - 1)){
            return true;//明槓, cannot do this if cardGiver is your previous player
        }
        return false;
    }

    bool playerCanWin(int playerOrder, List<Combination> combine, int meldOnBroadCount){
        int numberOfWinningCombination = 0;
        return DeckIdentifier.canWin(playerOrder, combine, meldOnBroadCount, out numberOfWinningCombination);
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

#endregion
}