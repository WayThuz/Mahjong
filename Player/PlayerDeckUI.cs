using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

using CombinationNamespace;
public class PlayerDeckUI : MonoBehaviourPunCallbacks{
    private PhotonView photonview;
    [SerializeField] private Image cardAwaitSprite;
    [SerializeField] private List<GameObject> cardModels = new List<GameObject>();
    [SerializeField] private List<Image> deckSprites = new List<Image>();
    [SerializeField] private Text flowersText;

    [Range(0, 50), SerializeField] private float lengthBetweenNeighbourCards;

    [SerializeField] private Vector2 hintPosition;
    [SerializeField] private Transform meldParent;
    [SerializeField] private Vector3 meldPosition = new Vector3(-240, -100, 50);
    [SerializeField] private Vector2 sizeDelta;

    [SerializeField] private int myOrder;
    private int meldOnBroadCount = 0;
    private GameObject canvas;
    List<meldHint> allMeldHints = new List<meldHint>();

    private int[] meld = null;
    private bool lock_DestroyAllHint = false;

    void Start(){
        canvas = this.gameObject;
        photonview = GetComponent<PhotonView>();
        SetCardPosition(lengthBetweenNeighbourCards);
    }

    public void ShowCardName(List<Card> deck, Card newCardAwait){
        if (newCardAwait != null)
        {
            getCardImage(cardAwaitSprite, newCardAwait.Order.ToString());
        }
        else
        {
            getCardImage(cardAwaitSprite, "null");
        }
        for (int i = 0; i < deck.Count; i++)
        {
            getCardImage(deckSprites[i], deck[i].Order.ToString());
        }
    }

    public void showMeldToBroad(ref List<Card> deck, Card cardAwait){
        bool isTriplet = (meld[0] == meld[1] && meld[1] == meld[2]);
        meldOnBroadCount++;
        if (isTriplet && meld.Length == 3) tripletToBroad(deck, cardAwait);
        if (isTriplet && meld.Length == 4) kongToBroad(deck, cardAwait);
        else sequenceToBroad(deck, cardAwait);
        rearrangeDeckspriteName();
        meld = null;
    }

    void sequenceToBroad(List<Card> deck, Card cardAwait){
        int[] cardOrderInMeld = new int[3];
        for (int i = 0; i < meld.Length; i++){
            if (cardAwait.Order == meld[i]){
                cardOrderInMeld[i] = cardAwait.Order;
                getCardImage(cardAwaitSprite, null);
                continue;
            }
            for (int j = deck.Count - 1; j > -1; j--){
                if (deck[j].Order != meld[i]) continue;
                cardOrderInMeld[i] = deck[j].Order;
                removeCardFromDeck(deck, j);
                break;
            }
        }

        photonview.RPC("putCardToBroad", RpcTarget.AllBuffered, cardOrderInMeld);
    }

    void tripletToBroad(List<Card> deck, Card cardAwait){
        int[] cardOrderInMeld = new int[3];
        int count_CardAssigned = 0;
        if (cardAwait != null)
        {
            if (cardAwait.Order == meld[0]){
                cardOrderInMeld[count_CardAssigned] = cardAwait.Order;
                getCardImage(cardAwaitSprite, null);
                count_CardAssigned++;
            }
        }
        for (int j = deck.Count - 1; j > -1; j--){
            if (deck[j].Order != meld[0]) continue;
            cardOrderInMeld[count_CardAssigned] = deck[j].Order;
            removeCardFromDeck(deck, j);
            count_CardAssigned++;
            if (count_CardAssigned == meld.Length) break;
        }

        photonview.RPC("putCardToBroad", RpcTarget.AllBuffered, cardOrderInMeld);
    }

    void kongToBroad(List<Card> deck, Card cardAwait){
        int[] cardOrderInMeld = new int[4];
        int count_CardAssigned = 0;
        if (cardAwait != null){
            if (cardAwait.Order == meld[0]){
                cardOrderInMeld[count_CardAssigned] = cardAwait.Order;
                getCardImage(cardAwaitSprite, null);
                count_CardAssigned++;
            }
        }
        for (int j = deck.Count - 1; j > -1; j--){
            if (deck[j].Order != meld[0]) continue;
            cardOrderInMeld[count_CardAssigned] = deck[j].Order;        
            removeCardFromDeck(deck, j);
            count_CardAssigned++;
            if (count_CardAssigned == meld.Length) break;
        }

        photonview.RPC("putCardToBroad", RpcTarget.AllBuffered, cardOrderInMeld);
    }

    void putCardToBroad(int[] cardOrderInMeld){
        GameObject meldGameObject = setMeldGameObject();
        for(int i  = 0; i < cardOrderInMeld.Length; i++){
            Image cardSpriteInMeld = meldGameObject.transform.GetChild(i).GetComponent<Image>();
            getCardImage(cardSpriteInMeld, cardOrderInMeld[i].ToString());
        }
       
    }

    void getCardImage(Image cardImage, string cardID){
        var cardSprite = Resources.Load<Sprite>("deckType/" + cardID);
        if (cardSprite != null) cardImage.sprite = cardSprite;
    }

    GameObject setMeldGameObject(){
        GameObject meldGameObject = GameObject.Instantiate(Resources.Load("prefab/Meld_" + meld.Length.ToString() + "Cards")) as GameObject;
        meldGameObject.transform.SetParent(meldParent);
        meldGameObject.transform.localPosition = new Vector3(meldPosition.x + meldOnBroadCount * 60, 0, meldPosition.z);
        meldGameObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
        return meldGameObject;
    }

    public void removeCardFromDeck(List<Card> deck, int cardIndex){
        deckSprites[cardIndex].name = "null";
        deckSprites[cardIndex].gameObject.SetActive(false);
        cardModels[cardIndex].gameObject.SetActive(false);
        deckSprites.RemoveAt(cardIndex);
        cardModels.RemoveAt(cardIndex);
        deck.RemoveAt(cardIndex);
        photonview.RPC("SetCardPosition", RpcTarget.AllBuffered, lengthBetweenNeighbourCards);
    }

    [PunRPC]
    void SetCardPosition(float length){
        float centerCardIndex = ((float)cardModels.Count + 1) / 2f;
        for (int i = 0; i < cardModels.Count; i++){
            float posX = lengthBetweenNeighbourCards * (i + 1 - centerCardIndex);
            cardModels[i].transform.localPosition = new Vector3(posX, 0, 15);
            deckSprites[i].transform.localPosition = new Vector3(posX, 0, 0);
        }
    }

    public void rearrangeDeckspriteName(){
        for (int k = 0; k < deckSprites.Count; k++){
            deckSprites[k].name = "Card(" + k.ToString() + ")";
        }
    }

    public void DestroyAllHints(){
        if (!lock_DestroyAllHint){
            lock_DestroyAllHint = true;
            foreach (meldHint hint in allMeldHints){
                if (!isMeldAssigned) hint.setMeld();
                hint.DestroyThisHints();
            }
            allMeldHints = new List<meldHint>();
            lock_DestroyAllHint = false;
        }
    }

    public void AssignMeld(int movement, List<Card> deck, int cardAwaitOrder){
        if (movement == 0){
            DisplayAndCreateSequenceHints(deck, cardAwaitOrder);
        }
        else if (movement == 1){
            meld = new int[3] { cardAwaitOrder, cardAwaitOrder, cardAwaitOrder };
        }
        else if (movement == 3){
            meld = new int[4] { cardAwaitOrder, cardAwaitOrder, cardAwaitOrder, cardAwaitOrder };//not cardAwaitOrder
        }
        else meld = null;
    }

    public void AssignMultipleKongs(List<Card> deck, List<int> cardsCanBeKong){
        DisplayAndCreateKongHints(cardsCanBeKong);
    }

    void DisplayAndCreateSequenceHints(List<Card> deck, int cardAwaitOrder){
        allMeldHints = new List<meldHint>();
        bool[] cardAwaitIndexInMelds = CombinationMethod.CardAwaitIndexInMelds(deck, cardAwaitOrder);
        CreateSequenceHints(allMeldHints, cardAwaitIndexInMelds, cardAwaitOrder);
        DisplayAllHints(allMeldHints);
    }

    void CreateSequenceHints(List<meldHint> allMeldHints, bool[] cardAwaitIndexInMelds, int cardAwaitOrder){
        int meldHintOrder = 0;
        if (cardAwaitIndexInMelds[0]){
            int[] cardsInMeld = new int[3] { cardAwaitOrder + 1, cardAwaitOrder, cardAwaitOrder + 2 };
            addHint(allMeldHints, cardsInMeld, ref meldHintOrder);
        }
        if (cardAwaitIndexInMelds[1]){
            int[] cardsInMeld = new int[3] { cardAwaitOrder - 1, cardAwaitOrder, cardAwaitOrder + 1 };
            addHint(allMeldHints, cardsInMeld, ref meldHintOrder);
        }
        if (cardAwaitIndexInMelds[2]){
            int[] cardsInMeld = new int[3] { cardAwaitOrder - 2, cardAwaitOrder, cardAwaitOrder - 1 };
            addHint(allMeldHints, cardsInMeld, ref meldHintOrder);
        }
    }

    void DisplayAndCreateKongHints(List<int> cardsCanBeKong){
        allMeldHints = new List<meldHint>();
        int meldHintOrder = 0;
        foreach (int card in cardsCanBeKong){
            int[] cardsInMeld = new int[4] { card, card, card, card };
            addHint(allMeldHints, cardsInMeld, ref meldHintOrder);
        }
        DisplayAllHints(allMeldHints);
    }

    void DisplayAllHints(List<meldHint> allMeldHints){
        foreach (meldHint hint in allMeldHints){
            hint.CreateMeldImage(sizeDelta, hintPosition.x, hintPosition.y);
        }
    }

    void addHint(List<meldHint> allMeldHints, int[] cardsInMeld, ref int meldHintOrder){
        meldHint hint = new meldHint(canvas, cardsInMeld, meldHintOrder);
        allMeldHints.Add(hint);
        meldHintOrder++;
    }

    public void showCardReplaced(List<string> flowersTable){
        string flowerstext = "";
        foreach (string flower in flowersTable){
            flowerstext += flower + "  ";
        }
        flowersText.text = flowerstext;
    }

    public int[] setMeld{
        set{
            meld = value;
        }
    }

    public int MyOrder{
        get{
            return myOrder;
        }
    }

    public int MeldOnBroadCount{
        get{
            return meldOnBroadCount;
        }
    }

    public bool isMeldAssigned{
        get{
            return (meld != null);
        }
    }

}
