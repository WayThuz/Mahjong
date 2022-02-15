using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{   
    
    private int type;// 0：字  1：萬  2：條  3：筒  4：花
    private int number;
    private string name;
    readonly string[] honorArray = new string[7]{"東", "南", "西", "北","中", "發", "白"};
    readonly string[] flowerArray = new string[8]{"梅", "蘭", "菊", "竹", "春", "夏", "秋", "冬"};
    public Card(int type, int number){
        this.type = type;
        this.number = number;
        this.name = PrintCardName();
    }
    private string PrintCardName(){
        string typeString = "";
        switch(type){
            case 0:
                typeString = honorArray[number-1];
                break;

            case 1:
                typeString = number.ToString() + "\n萬";
                break;

            case 2:
                typeString = number.ToString() + "\n條";
                break;

            case 3:
                typeString = number.ToString() + "\n筒";
                break;

            case 4:
                typeString = flowerArray[number-1];  
                break;
                
            default:
                typeString = "沒指派類型";
                break;
        }

        return typeString;
    }

    public int Order{
        get{
            return type*100 + number;
        }
    }

    public int Type{
        get{
            return type;
        }
    }

    public int Number{
        get{
            return number;
        }
    }

    public string GetCardName{
        get{
            return name;
        }
    }
}
