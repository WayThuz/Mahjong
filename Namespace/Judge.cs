using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CombinationNamespace;

namespace Judge
{
    public class PlayerDecider{
        const int numberOfPlayers = 4;
        const int win = 2;
        const int stop = -100;

        public static int getPlayerByCheckMovement(int[] playerMovement, int previousPlayer, out bool isWinner){
            int winnerPlayer = getWinner(playerMovement, previousPlayer);
            if(winnerPlayer != -1){
                isWinner = true;
                return winnerPlayer;
            }
            else{
                int nextPlayer = getNextPlayer(playerMovement, previousPlayer);
                isWinner = false;
                return nextPlayer;
            }
        }

        static int getWinner(int[] playerMovement, int previousPlayer){
            int winnerPlayer = -1;
            for(int i  = 0; i < numberOfPlayers; i++){
                int playerOrder = (previousPlayer + i)%numberOfPlayers;
                if(playerMovement[playerOrder] == win){
                    winnerPlayer = playerOrder;
                    return winnerPlayer;
                }
            }
            return winnerPlayer;
        }   

        static int getNextPlayer(int[] playerMovement, int previousPlayer){
            int nextPlayer = previousPlayer;
            for (int playerCompared = 0; playerCompared < playerMovement.Length; playerCompared++){
                nextPlayer = comparePlayersIndex(playerMovement, nextPlayer, playerCompared, previousPlayer);
            }
            if (playerMovement[nextPlayer] < 0) nextPlayer = (previousPlayer + 1) % numberOfPlayers;
            return nextPlayer;
        }   

        static int comparePlayersIndex(int[] playerMovement, int playerIndex, int playerCompared, int previousPlayer){
            if (playerMovement[playerIndex] == playerMovement[playerCompared]){
                if (playerCompared - previousPlayer > 0 && previousPlayer - playerIndex > 0) return playerCompared;
                else if (playerCompared < playerIndex) return playerCompared;
            }
            else if (playerMovement[playerCompared] > playerMovement[playerIndex]) return playerCompared;
            return playerIndex;
        }

        public static bool hasPlayerPrior(int[] playerMovement, int playerOrder, int movement){
            if(movement == stop){
                throw new ArgumentException("movement = -100, which is the value should not call this function");
            }
            for(int i = 0; i < playerMovement.Length; i++){
                if(i == playerOrder) continue;
                if(playerMovement[i] > movement || playerMovement[i] == win){
                    return true;
                }
            }
            return false;
        }

        public static bool allFinished(int[] playerMovement, bool[] turnFinishedCheck){
            foreach (int movement in playerMovement){ if(movement == -1) return false; }
            foreach(bool isFinished in turnFinishedCheck){ if(!isFinished) return false; }
            return true;
        }

        
    }

    public class PlayerIdentifier{
        const int numberOfPlayers = 4;

        public static bool isCardGiver(int playerIndex, int currentTurnGiverIndex){
            return ((playerIndex + 4) % 4 == currentTurnGiverIndex);
        }

        public static bool isCardAwaiter(int playerIndex, int currentTurnGiverIndex){
            return (playerIndex != currentTurnGiverIndex);
        }
    }

    public class DeckIdentifier{
        const int numberOfPlayers = 4;
        const int eat = 0;
        const int pon = 1;

        public static bool canEat(int playerOrder, int currentTurnGiverIndex, List<Combination> combine, Card card){
            bool deckCanEat = deckCanDoMovement(playerOrder, combine, card, eat);
            return (deckCanEat && (playerOrder == (currentTurnGiverIndex + 1) % numberOfPlayers));
        }

        public static bool canPon(int playerOrder, int currentTurnGiverIndex, List<Combination> combine, Card card){
            bool deckCanPon = deckCanDoMovement(playerOrder, combine, card, pon);
            return (deckCanPon && (playerOrder != currentTurnGiverIndex));
        }

        public static bool canWin(int playerOrder, List<Combination> combine, int meldOnBroadCount, out int numberOfWinningCombine){
            numberOfWinningCombine = 0;
            bool winningCheck = false;
            foreach (Combination combination in combine){
                if (combination.meldsInDeck[0] + meldOnBroadCount == 5 && combination.meldsInDeck[1] == 1){
                    winningCheck = true;
                    numberOfWinningCombine++;
                }
            }
            return winningCheck;
        }

        static bool deckCanDoMovement(int playerOrder, List<Combination> combine, Card card, int movement){
            foreach(Combination combination in combine){ 
                bool[] SequenceOrTriplet = combination.IsInSequenceOrTriplet(card);
                if(SequenceOrTriplet[movement]) return true;    
            } 
            return false;
        } 
    }
}
