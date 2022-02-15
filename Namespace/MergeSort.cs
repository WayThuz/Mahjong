using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MergeSort
{       
    class myMergeSort{

        public static void MergeSort(ref List<int> intList, int startIndex, int endIndex){
            if(startIndex < endIndex){
                int midIndex = (int)((startIndex + endIndex) / 2);
                MergeSort(ref intList, startIndex, midIndex);
                MergeSort(ref intList, midIndex+1, endIndex);            
                Merge(ref intList, startIndex, midIndex, endIndex);
            }
        }

        static void Merge(ref List<int> intList, int startIndex, int midIndex, int endIndex){
            if(endIndex - startIndex == 1){
                int leftInt = intList[startIndex];
                int rightInt = intList[endIndex];
                if(leftInt > rightInt){
                    intList[startIndex] = rightInt;
                    intList[endIndex] = leftInt;
                }
            }
            else{
                List<int> leftList = intList.GetRange(startIndex, midIndex-startIndex+1);
                List<int> rightList = intList.GetRange(midIndex+1, endIndex-midIndex);
                int a = startIndex*10000+midIndex*1000+endIndex*100;
                for(int i = 0; i < leftList.Count; i++){      
                    bool isInsert = false;
                    if(isInsert) continue;
                    for(int j = 0; j < rightList.Count; j++){
                        if(leftList[i] <= rightList[j] && !isInsert){
                            rightList.Insert(j, leftList[i]);
                            isInsert = true;
                        }
                        else if(j == rightList.Count-1  && !isInsert){//&& leftList[i] >= rightList[j]
                            rightList.Insert(j+1, leftList[i]);
                            isInsert = true;
                        }
                    }
                }
                for(int i = 0; i < rightList.Count; i++){
                    intList[startIndex+i] = rightList[i];
                }
            }
        }
    }
}   
