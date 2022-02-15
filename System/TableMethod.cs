using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TableMethod : MonoBehaviour
{
    public static void initializeCard(GameObject card, Transform parent, int cardOrder, Vector3 pos, Vector3 rota){
        card.transform.SetParent(parent);
        card.GetComponent<RectTransform>().transform.SetParent(parent);
        card.GetComponent<Image>().sprite = Resources.Load<Sprite>("deckType/" + cardOrder.ToString());
        card.transform.localPosition = pos;
        card.transform.eulerAngles = rota;
    }

    public static Vector3 randomCoordinate(Vector2 localPosition, Vector2 vectorToBorder, float radius){
        float randomPosX;
        float randomPosZ;
        float CompVectorClose2Border;
        float length;
        float limit;
        if(Mathf.Abs(vectorToBorder.x) <= 0.01f){
            randomPosZ = Random.Range(-vectorToBorder.y, vectorToBorder.y);     
            CompVectorClose2Border = localPosition.x;
            length = Mathf.Sqrt(radius*radius - randomPosZ*randomPosZ);
            limit = (localPosition.x >= 0) ? localPosition.x - 100f - length : localPosition.x + 100f + length;
            randomPosZ = randomPosZ + localPosition.y;
            randomPosX = Random.Range(CompVectorClose2Border, limit);
        }
        else{
            randomPosX = Random.Range(-vectorToBorder.x, vectorToBorder.x); 
            if(vectorToBorder.y <= 0.01f){
                CompVectorClose2Border = localPosition.y;
            }    
            else{
                CompVectorClose2Border = localPosition.y + (randomPosX/vectorToBorder.x)*vectorToBorder.y;
            }
            length = Mathf.Sqrt(radius*radius - randomPosX*randomPosX);
            limit = (localPosition.y >= 0) ? localPosition.y - 100f - length : localPosition.y + 100f + length;
            randomPosX = randomPosX + localPosition.x;
            randomPosZ = Random.Range(CompVectorClose2Border, limit);
        }
        if(float.IsNaN(randomPosX)) randomPosX = 50;
        if(float.IsNaN(randomPosZ)) randomPosZ = 50;
        return new Vector3(randomPosX, 20, randomPosZ);
    }

    public static Vector2 VectorToBorder(Vector2 localPosition, float radius){
        if(Mathf.Abs(localPosition.y) <= 0.01f) return new Vector2(0, radius);
        if(Mathf.Abs(localPosition.x) <= 0.01f) return new Vector2(radius, 0);
        float slopeOfGradient = -localPosition.x/localPosition.y;
        float ratioToBorder = 0f;
        //Theoritically, radius/Sqrt(1 + slopeOfGradient*slopeOfGradient)
        if(slopeOfGradient <= 1f){
            ratioToBorder = radius/(1+slopeOfGradient/2); 
        }
        else{
            ratioToBorder = radius/(0.3f + slopeOfGradient);
        }
        return new Vector2(ratioToBorder, ratioToBorder*slopeOfGradient);
    }
}
