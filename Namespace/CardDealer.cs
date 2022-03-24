using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardDealer{
    public class Dealer{
        const int number_TotalDeals = 4;
        const int cards_EachDeal = 4;
        public static Card[] generateDeck(){
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

        public static List<Card> shuffle(Card[] deck){
            List<int> assignList = new List<int>();
            List<Card> newShuffledDeck = new List<Card>();
            for (int i = 0; i < 144; i++){
                assignList.Add(i);
            }
            for (int i = 0; i < 144; i++){
                int randomIndex = UnityEngine.Random.Range(0, assignList.Count);
                newShuffledDeck.Add(deck[assignList[randomIndex]]);
                assignList.RemoveAt(randomIndex);
            }

            return newShuffledDeck;
        }
        
        public static List<Card> assignDeck(int[] types, int[] numbers){
            if(types.Length != numbers.Length || types.Length != 144){
                throw new ArgumentException("Length of the variable types does not equals to the variable numbers");
            }
            List<Card> deck = new List<Card>();
            for(int i = 0; i < 144; i++){
                Card card = new Card(types[i], numbers[i]);
                deck.Add(card);
            }

            return deck;
        }

        public static List<Card> dealCards(int playerOrder, List<Card> shuffledDeck){
            List<Card> deck = new List<Card>();
            for (int i = 0; i < number_TotalDeals; i++){
                int cardOnTop = playerOrder * cards_EachDeal + i * 16;
                deck.Add(shuffledDeck[cardOnTop]);
                deck.Add(shuffledDeck[cardOnTop + 1]);
                deck.Add(shuffledDeck[cardOnTop + 2]);
                deck.Add(shuffledDeck[cardOnTop + 3]);
            }
            return deck;
        }

        public static int[] type_Cards(List<Card> cards){
            int[] types = new int[144];
            for(int i = 0; i < 144; i++){
                types[i] = cards[i].Type;
            }
            return types;
        }

        public static int[] ID_Cards(List<Card> cards){
            int[] numbers = new int[144];
            for(int i = 0; i < 144; i++){
                numbers[i] = cards[i].Number;
            }
            return numbers;
        }
    }
}
