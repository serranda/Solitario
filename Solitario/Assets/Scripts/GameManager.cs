using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private AssetReference cardReference;
    [SerializeField] private List<AssetReferenceSprite> seamsReference;
    [SerializeField] private List<AssetReferenceSprite> valuesReference;

    [SerializeField] private List<StackController> bottomStacks;
    [SerializeField] private List<StackController> finalStacks;

    [SerializeField] private List<CardController> deck;

    [SerializeField] private Button deckButton;

    [SerializeField] private float waitTimeForServe;

    public static readonly float yLocalOffset = 45f;
    public static readonly float zLocalOffset = 5f;

    private static readonly string[] seams = { "hearts", "diamonds", "clubs", "spades" };
    private static readonly string[] colors = { "red", "black" };
    private static readonly int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

    private void Start()
    {
        //create deck and shuffle the element of the list
        NewTable();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    deckStack.cardList[0].SetCovered(false);
        //}
    }

    private void NewTable()
    {
        StartCoroutine(NewCardDeck());
        StartCoroutine(ServeCard());

    }

    private IEnumerator NewCardDeck()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                //create a new card
                Card newCard = new Card(values[j], seams[i], colors[i / 2]);

                //get the index of the sprites
                int seamIndex = Array.IndexOf(seams, newCard.seam);
                int valueIndex = Array.IndexOf(values, newCard.value);

                //load new card seam's sprite
                AsyncOperationHandle<Sprite> handleSprite = seamsReference[seamIndex].LoadAssetAsync<Sprite>();
                yield return handleSprite;
                Sprite seamSprite = handleSprite.Result;

                //load new card value's sprite
                handleSprite = valuesReference[valueIndex].LoadAssetAsync<Sprite>();
                yield return handleSprite;
                Sprite valueSprite = handleSprite.Result;

                //load new card prefab
                AsyncOperationHandle<GameObject> handleGameObject = cardReference.LoadAssetAsync<GameObject>();
                yield return handleGameObject;
                GameObject newCardPrefab = handleGameObject.Result;

                //instantiate new card GameObject
                GameObject newCardGameObject = Instantiate(newCardPrefab, deckButton.transform, false);

                //change GameObject name
                newCardGameObject.name = newCard.value + newCard.seam;

                //get gameObject CardController component and set new card sprites
                CardController newCardController = newCardGameObject.GetComponent<CardController>();
                newCardController.InitializeSprites(seamSprite, valueSprite);

                //set crad on newCardController
                newCardController.card = newCard;

                //add card to deck to keep track which is the next to serve
                deck.Add(newCardController);
            }
        }

        //randomize the order of the list
        ShuffleDeck();

    }

    private void ShuffleDeck()
    {
        //cards = cards.OrderBy(card => Guid.NewGuid()).ToList();

        Random rng = new Random();

        int n = deck.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            CardController shuffledCard = deck[k];
            deck[k] = deck[n];
            deck[n] = shuffledCard;
        }
    }

    private IEnumerator ServeCard()
    {
        //wait until all card GameObject are instantiated
        yield return new WaitWhile(() => deck.Count < 52);

        //serve progressively the card on the table
        for (int i = 0; i < bottomStacks.Count; i++)
        {
            for (int j = 0; j < i + 1; j++)
            {
                //set the y and z offset for the current card to serve on table
                float currentYLocalOffset = yLocalOffset * j;
                float currentZLocalOffset = zLocalOffset * (j + 1);

                //get the current card to serve on table
                CardController cardController = deck[j];
                GameObject cardGameObject = cardController.gameObject;

                //Remove served card form deck
                deck.Remove(cardController);

                //set the new parent for the current card to serve on table
                cardGameObject.transform.SetParent(bottomStacks[i].transform);

                //calculate the offset in world coordinates
                Vector3 offsetVector = bottomStacks[i].transform.TransformVector(0, currentYLocalOffset, currentZLocalOffset);

                //set the new position of the card
                Vector3 newPosition = bottomStacks[i].transform.position - offsetVector;

                //coroutine to move gradually card to new position
                cardController.MoveToPosition(newPosition, (int)currentZLocalOffset);

                //set flag to make card discovered if is last of the list
                if (j == i)
                {
                    cardController.SetIsCovered(false);
                }

                yield return new WaitForSeconds(waitTimeForServe);
            }
        }
    }
}
