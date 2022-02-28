using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class droppingCard : MonoBehaviour
{
    [SerializeField] private Image cardImage; 
    [SerializeField] private Rigidbody modelRigid;

    void Awake(){
        if(cardImage == null) cardImage = GetComponent<Image>();
        GetRandomImage();
    }

    void GetRandomImage(){
        string randomImageName = "1";
        int randomCard = Random.Range(1, 42);
        if(randomCard < 8) randomImageName = randomCard.ToString();
        else if(randomCard < 17) randomImageName = (100 + randomCard - 7).ToString();
        else if(randomCard < 26) randomImageName = (200 + randomCard - 16).ToString();
        else if(randomCard < 35) randomImageName = (300 + randomCard - 25).ToString();
        else randomImageName = (400 + randomCard - 34).ToString();
        cardImage.sprite = Resources.Load<Sprite>("deckType/" + randomImageName);
    }

    public void StartFalling(){
        modelRigid.useGravity = true;
    }
}
