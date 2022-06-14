using System;
using System.Collections;
using System.Collections.Generic;

public class GameCard : Card
{
    public Area cardStatus { get; set; }

    public bool IsDisplay { get; set; }

    public GameCard(Card card){
        this.Id = card.Id;
        this.CardName = card.CardName;
        this.CardColor = card.CardColor;
        this.CardType = card.CardType;
        this.CardRarity = card.CardRarity;
        this.BluePoint = card.BluePoint;
        this.RedPoint = card.RedPoint;
        this.CardDescription = card.CardDescription;
    }
}
