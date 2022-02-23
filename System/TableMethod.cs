using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
    
namespace Method
{  
    public class TableMethod
    {
        public static void initializeCard(GameObject card, Transform parent, int cardOrder, Vector3 pos, Vector3 rota){
            card.transform.SetParent(parent);
            card.GetComponent<RectTransform>().transform.SetParent(parent);
            card.GetComponent<Image>().sprite = Resources.Load<Sprite>("deckType/" + cardOrder.ToString());
            card.transform.localPosition = pos;
            card.transform.eulerAngles = rota;
        }
    
        public static Vector3 randomCoordinate(Vector3 dirVector, float maxLength, float heightOfCard){
            float randomLengthX = Mathf.Sqrt(Random.Range(0f, (float)maxLength*maxLength));
            float randomLengthZ = Mathf.Sqrt(Random.Range(0f, (float)maxLength*maxLength));
            int xDir = DecidedDir(dirVector.z);
            int zDir = DecidedDir(dirVector.x);   
            float newX = randomLengthX*zDir;
            float newZ = randomLengthZ*xDir;
            return new Vector3(newX, heightOfCard, newZ);

        }

        static int DecidedDir(float coordinate){
            int dirBeDecided;
            bool isdirDecidedByCoordinate = (coordinate != 0) ;
            if(isdirDecidedByCoordinate){
                dirBeDecided = (coordinate > 0) ? 1 : -1;
            }
            else{
                float randomfactor  = Random.Range(-1f, 1f);
                if(randomfactor == 0) dirBeDecided = 0;
                else dirBeDecided = (randomfactor > 0)? 1 :-1;
            }

            return dirBeDecided;
        }
        
        public static GameObject setMeldGameObject(Transform meldParent, Vector3 meldPosition, int meldLength, int meldOrder){
            GameObject meldGameObject = GameObject.Instantiate(Resources.Load("prefab/Meld_" + meldLength.ToString() + "Cards")) as GameObject;
            meldGameObject.transform.SetParent(meldParent);
            meldGameObject.transform.localPosition = new Vector3(meldPosition.x + meldOrder * 60, 0, meldPosition.z);
            meldGameObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
            return meldGameObject;
        }
    }

}