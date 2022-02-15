using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MergeSort;

namespace CombinationNamespace 
{
     ////////Call these everytime//////// 
    public class Combination{
        
        List<int> cardsAlreadyinCombination = new List<int>();
        List<Card[]> Melds = new List<Card[]>();
        List<Card[]> Sequences = new List<Card[]>();
        List<Card[]> Triplets = new List<Card[]>();
        List<Card[]> Eyes = new List<Card[]>();
        public Combination(List<int> cardsAlreadyinCombination, List<Card[]> Sequences, List<Card[]> Triplets, List<Card[]> Eyes){
            
            this.cardsAlreadyinCombination = cardsAlreadyinCombination;
            this.Sequences = Sequences;
            this.Triplets = Triplets;
            this.Eyes = Eyes;
            this.Melds.AddRange(Sequences);
            this.Melds.AddRange(Triplets);
            this.Melds.AddRange(Eyes);

        }
    
        public void sayCombination(int myOrder){

            string meldsContents = "";
            foreach(Card[] meld in Melds){
                for(int i = 0; i < meld.Length; i++){
                    meldsContents += " " + meld[i].GetCardName; 
                }
            }

            Debug.Log("Player order: " + myOrder.ToString() 
                    + "  sequences: " + Sequences.Count.ToString() 
                    + " triplets: " + Triplets.Count.ToString() 
                    + " eyes: " + Eyes.Count.ToString()
                    + " melds: " + Melds.Count
                    + " contents: " + meldsContents
            );
        }

        public void remainedCards(int myOrder, List<Card> mydeck, Card card){
            List<Card> deck = mydeck;
            deck.Add(card);
            List<int> remainedCardsID = new List<int>{0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16};
            string remainedContents = "";
            
            foreach(int id in cardsAlreadyinCombination){
                remainedCardsID.Remove(id);
            }

            foreach(int id in remainedCardsID){
                if(deck[id] != null) remainedContents += " " + deck[id].GetCardName;
                else remainedContents += " NONE ";
            }

            Debug.Log("Player order: " + myOrder.ToString() + " Remain cards: " + remainedContents);
        }

        public Dictionary<int, int> meldsContained(Card card){  
            Dictionary<int, int> possibleMelds = new Dictionary<int, int>();
            foreach(Card[] sequence in Sequences){
                int index = Array.FindIndex(sequence, x => x.GetCardName == card.GetCardName);
                if(index > -1) possibleMelds.Add(0, index);
            }
    
            foreach(Card[] triplet in Triplets){
                int index = Array.FindIndex(triplet, x => x.GetCardName == card.GetCardName);
                if(index > -1) possibleMelds.Add(1, index);
            }
            if(possibleMelds.Count > 0) return possibleMelds;
            else return null;
            
        }


        public bool[] IsInSequenceOrTriplet(Card card){
            bool[] isInSequenceAndTriplet = new bool[2]{false,false};
            foreach(Card[] sequence in Sequences){
                int index = Array.FindIndex(sequence, x => x.GetCardName == card.GetCardName);
                if(index > -1){isInSequenceAndTriplet[0] = true; break;}
            }
    
            foreach(Card[] triplet in Triplets){
                int index = Array.FindIndex(triplet, x => x.GetCardName == card.GetCardName);
                if(index > -1){isInSequenceAndTriplet[1] = true; break;}
            }

            return isInSequenceAndTriplet;
        }    

        public int[] meldsInDeck{
            get{
                return new int[2]{Sequences.Count + Triplets.Count, Eyes.Count};
            }
        }

    }

    class CombinationMethod{

        public static List<Combination> CombinationCalculator(List<Card> deck, Card cardAwait){
            List<Combination> ListOfCombinations = new List<Combination>();
            List<Card> allCardsAtHand = getallCardsAtHand(deck, cardAwait);
            int count = allCardsAtHand.Count;
            for(int startIndex = 0; startIndex < count; startIndex++){   
                Combination combination = startFromSequences(allCardsAtHand, count, startIndex);
                ListOfCombinations.Add(combination);
            }
            for(int startIndex = 0; startIndex < count; startIndex++){   
                Combination combination = startFromTriplets(allCardsAtHand, count, startIndex);
                ListOfCombinations.Add(combination);
            }
            return ListOfCombinations;
        }

        static List<Card> getallCardsAtHand(List<Card> deck, Card cardAwait){
            List<Card> allCardsAtHand = new List<Card>();
            foreach(Card card in deck){
                allCardsAtHand.Add(card);
            }
            int count = deck.Count;
            if(cardAwait != null){ 
                insertCardAwaitByOrder(allCardsAtHand, cardAwait, count);
            }
            return allCardsAtHand;
        }

        static void insertCardAwaitByOrder(List<Card> allCardsAtHand, Card cardAwait, int count){
            if(cardAwait.Order <= allCardsAtHand[0].Order){
                allCardsAtHand.Insert(0, cardAwait);
            } 
            else if(allCardsAtHand[count-1].Order <= cardAwait.Order){
                allCardsAtHand.Add(cardAwait);
            }
            else{
                for(int i = 0; i < count-1; i++){
                    if(allCardsAtHand[i].Order <= cardAwait.Order && cardAwait.Order <= allCardsAtHand[i+1].Order){
                        allCardsAtHand.Insert(i+1, cardAwait);
                        break;
                    }   
                }
            }
        }
        static Combination startFromSequences(List<Card> allCardsAtHand, int count, int startIndex){
            List<int> cardsAlreadyinCombination = new List<int>();
            List<Card[]> Sequences = findSequences(allCardsAtHand, cardsAlreadyinCombination, count, startIndex);
            List<Card[]> Triplets = findTriplets(allCardsAtHand, cardsAlreadyinCombination, count);
            List<Card[]> Eyes = findEyes(allCardsAtHand, cardsAlreadyinCombination, count); 
            return new Combination(cardsAlreadyinCombination, Sequences, Triplets, Eyes);       
        }

        static Combination startFromTriplets(List<Card> allCardsAtHand, int count, int startIndex){
            List<int> cardsAlreadyinCombination = new List<int>();
            List<Card[]> Triplets = findTriplets(allCardsAtHand, cardsAlreadyinCombination, count);
            List<Card[]> Sequences = findSequences(allCardsAtHand, cardsAlreadyinCombination, count, startIndex);
            List<Card[]> Eyes = findEyes(allCardsAtHand, cardsAlreadyinCombination, count);      
            return new Combination(cardsAlreadyinCombination, Sequences, Triplets, Eyes);         
        }

        static List<Card[]> findSequences(List<Card> allCardsAtHand,List<int> cardsAlreadyinCombination, int count, int startIndex){
            List<Card[]> Sequences = new List<Card[]>();
            for(int i = startIndex; i < startIndex+count; i++){ 
                int thisCard = i%count;
                if(cardsAlreadyinCombination.Contains(thisCard)) continue;
                Card[] sequence = sequenceContainCard(allCardsAtHand, cardsAlreadyinCombination, thisCard, count);
                if(sequence != null) Sequences.Add(sequence);
            }
            return Sequences;
        }

        static Card[] sequenceContainCard(List<Card> allCardsAtHand, List<int> cardsAlreadyinCombination, int thisCard, int count){            
            int orderofSelectedCard = allCardsAtHand[thisCard].Order; 
            if(orderofSelectedCard < 100 || orderofSelectedCard > 400) return null;
            int rightCard = -1;
            int leftCard = -1;

            for(int j = 0; j < count; j++){
                if(j == thisCard || cardsAlreadyinCombination.Contains(j)) continue;
                if(allCardsAtHand[j].Order == orderofSelectedCard+1){ rightCard = j;  break; }
            }

            for(int j = 0; j < count; j++){
                if(j == thisCard || cardsAlreadyinCombination.Contains(j)) continue;
                if(allCardsAtHand[j].Order == orderofSelectedCard-1){ leftCard = j;  break; }
            }

            if(leftCard != -1 && rightCard != -1){
                cardsAlreadyinCombination.AddRange(new List<int>{leftCard, thisCard, rightCard});
                Card[] sequence = new Card[3]{allCardsAtHand[leftCard], allCardsAtHand[thisCard], allCardsAtHand[rightCard]};
                return sequence;
            }
            else return null;
        }

        static List<Card[]> findTriplets(List<Card> allCardsAtHand, List<int> cardsAlreadyinCombination, int count){
            List<Card[]> Triplets = new List<Card[]>();
            for(int j = 1; j < count-1; j++){               
                if(cardsAlreadyinCombination.Contains(j) || cardsAlreadyinCombination.Contains(j+1) || cardsAlreadyinCombination.Contains(j-1)) continue;           
                if(allCardsAtHand[j].Order == allCardsAtHand[j+1].Order && allCardsAtHand[j].Order == allCardsAtHand[j-1].Order){                   
                    cardsAlreadyinCombination.AddRange(new List<int>{j,j+1,j-1});
                    Card[] triplet = new Card[3]{allCardsAtHand[j-1], allCardsAtHand[j], allCardsAtHand[j+1]};
                    Triplets.Add(triplet);
                }
            }
            return Triplets;
        }

        static List<Card[]> findEyes(List<Card> allCardsAtHand, List<int> cardsAlreadyinCombination, int count){
            List<Card[]> Eyes = new List<Card[]>();
            for(int j = 0; j < count-1; j++){
                if(cardsAlreadyinCombination.Contains(j) || cardsAlreadyinCombination.Contains(j+1)) continue;
                if(allCardsAtHand[j].Order != allCardsAtHand[j+1].Order) continue;      
                cardsAlreadyinCombination.AddRange(new List<int>{j,j+1});
                Card[] eyes = new Card[2]{allCardsAtHand[j], allCardsAtHand[j+1]};
                Eyes.Add(eyes);
            }
            return Eyes;
        }

        public static List<int> deckHasKong(List<Card> deck, Card card){
            List<int> cardsInKong = new List<int>();
            for(int i = 0; i < deck.Count-2; i++){     
                if(deck[i].Order != deck[i+1].Order || deck[i].Order != deck[i+2].Order) continue;
                if(i+3 == deck.Count){
                    if(deck[i].Order != card.Order) continue;
                }
                else{
                   if(deck[i].Order != deck[i+3].Order && deck[i].Order != card.Order) continue;
                }
                cardsInKong.Add(deck[i].Order);
            }
            return cardsInKong;
        }

        public static bool[] CardAwaitIndexInMelds(List<Card> deck, int cardAwaitOrder){
            const int left = 0; const int right = 1;
            bool[] cardAwaitIndexInMelds = new bool[3]{false,false,false};
            bool[] hasCardOnSide = new bool[2]{false,false};//left & right 
            bool[] hasCardOnTwoSidesAway = new bool[2]{false,false};
            foreach(Card card in deck){
                if(card.Order == cardAwaitOrder + 1) hasCardOnSide[right] = true;
                else if(card.Order == cardAwaitOrder - 1) hasCardOnSide[left] = true;
                else if(card.Order == cardAwaitOrder - 2) hasCardOnTwoSidesAway[left] = true;
                else if(card.Order == cardAwaitOrder + 2) hasCardOnTwoSidesAway[right] = true;
            }

            cardAwaitIndexInMelds[0] = hasCardOnSide[right] && hasCardOnTwoSidesAway[right];  
            cardAwaitIndexInMelds[1] = hasCardOnSide[left] && hasCardOnSide[right];
            cardAwaitIndexInMelds[2] = hasCardOnSide[left] && hasCardOnTwoSidesAway[left];

            return cardAwaitIndexInMelds;
        }

    }
}
