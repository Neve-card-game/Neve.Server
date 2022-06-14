using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Card
{

    public int? BluePoint;
    public int? RedPoint;
    public string? Id;
    public string? CardName;
    public string? CardColor;
    public string? CardRarity;
    public string? CardType;
    public string? CardDescription;

    public Card(){}
    public Card(string id, string cardName, string cardColor, string cardType, string cardRarity, int bluePoint, int redPoint, string cardDescription)
    {
        Id = id;
        CardName = cardName;
        CardColor = cardColor;
        CardType = cardType;
        CardRarity = cardRarity;
        BluePoint = bluePoint;
        RedPoint = redPoint;
        CardDescription = cardDescription;
    }
}