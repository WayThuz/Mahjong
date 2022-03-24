using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CombinationNamespace;

namespace Judge
{
    public class PlayerDecider{
        const int numberOfPlayers = 4;

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
            for(int i  = 0; i < 4; i++){
                int playerOrder = (previousPlayer + i)%4;
                if(playerMovement[playerOrder] == 2){
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

        public static bool allFinished(int[] playerMovement, bool[] turnFinishedCheck){
            foreach (int movement in playerMovement){ if(movement == -1) return false; }
            foreach(bool isFinished in turnFinishedCheck){ if(!isFinished) return false; }
            return true;
        }

        public static bool isWin(int playerOrder, List<Combination> combinationList, int meldOnBroadCount, out int numberOfWinningCombination){
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
    }

    public class PlayerIdentifier{
        const int numberOfPlayers = 4;

        public static bool isCardGiver(int playerIndex, int currentTurnGiverIndex){
            return ((playerIndex + 4) % 4 == currentTurnGiverIndex);
        }

        public static bool isCardAwaiter(int playerIndex, int currentTurnGiverIndex){
            return (playerIndex != currentTurnGiverIndex);
        }

        public static bool canEat(bool deckCanEat, int playerOrder, int currentTurnGiverIndex){
            return (deckCanEat && (playerOrder == (currentTurnGiverIndex + 1) % numberOfPlayers));
        }

        public static bool canPon(bool deckCanPon, int playerOrder, int currentTurnGiverIndex){
            return (deckCanPon && (playerOrder != currentTurnGiverIndex));
        }
    }
}
