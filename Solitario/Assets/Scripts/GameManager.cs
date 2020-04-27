using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject cardReference;
    [SerializeField] private List<Sprite> seamsReference;
    [SerializeField] private List<Sprite> valuesReference;

    [SerializeField] private Deck deck;

    [SerializeField] private StackController deckStack;
    [SerializeField] private StackController spawnStack;
    [SerializeField] private List<StackController> bottomStacks;
    [SerializeField] private List<StackController> finalStacks;

    private float yOffset = 45f;
    private float zOffset = 5f;
    
    private void Start()
    {
        //create deck and shuffle the element of the list
        deck = new Deck();
        NewTable();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewTable();
        }
    }

    private void NewTable()
    {
        deck.ShuffleDeck();

        SpawnAllCard();

        ServeCard();
    }

    private void SpawnAllCard()
    {

        for (int i = 0; i < deck.cards.Count; i++)
        {
            Card card = deck.cards[i];

            int seamIndex = Array.IndexOf(CardArrays.seams, card.seam);
            int valueIndex = Array.IndexOf(CardArrays.values, card.value);

            //get sprite reference from card properties
            Sprite seamSprite = seamsReference[seamIndex];
            Sprite valueSprite = valuesReference[valueIndex];

            GameObject newCardGameObject = Instantiate(cardReference, deckStack.transform, false);

            newCardGameObject.name = card.value + card.seam;

            deckStack.cardList.Add(newCardGameObject);

            //get cardController from newCard and set the sprites
            CardController newCardController = newCardGameObject.GetComponent<CardController>();

            newCardController.InitializeSprites(seamSprite, valueSprite);
            newCardController.SetParentStack(deckStack);
        }

    }

    private void ServeCard()
    {
        for (int i = 0; i < bottomStacks.Count; i++)
        {
            for (int j = i; j < bottomStacks.Count; j++)
            {
                float currentYOffset = yOffset * i;
                float currentZOffset = zOffset * i;

                GameObject cardGameObject = deckStack.cardList[j];
                deckStack.cardList.RemoveAt(j);

                bottomStacks[j].cardList.Add(cardGameObject);

                cardGameObject.transform.SetParent(bottomStacks[j].transform,false);
                Vector3 newPosition = cardGameObject.transform.localPosition - new Vector3(0, currentYOffset, currentZOffset);
                cardGameObject.transform.localPosition = newPosition;

                cardGameObject.GetComponent<Canvas>().sortingOrder = (int) currentZOffset;
                cardGameObject.GetComponent<CardController>().SetParentStack(bottomStacks[j]);

            }
        }
    }
}
