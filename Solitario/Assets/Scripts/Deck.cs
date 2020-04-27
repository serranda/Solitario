using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class Deck
{
    public List<Card> cards = new List<Card>();

    public Deck()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                Card newCard = new Card(CardArrays.values[j],CardArrays.seams[i], CardArrays.colors[i/2]);

                cards.Add(newCard);
            }
        }
    }

    public void ShuffleDeck()
    {
        //cards = cards.OrderBy(card => Guid.NewGuid()).ToList();

        Random rng = new Random();

        int n = cards.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card shuffledCard = cards[k];
            cards[k] = cards[n];
            cards[n] = shuffledCard;
        }
    }
}
