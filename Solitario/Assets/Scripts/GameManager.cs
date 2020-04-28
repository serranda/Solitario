using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private AssetReference cardReference;
    [SerializeField] private List<AssetReferenceSprite> seamsReference;
    [SerializeField] private List<AssetReferenceSprite> valuesReference;

    [SerializeField] private StackController deckStack;
    [SerializeField] private StackController spawnStack;
    [SerializeField] private List<StackController> bottomStacks;
    [SerializeField] private List<StackController> finalStacks;

    private float yLocalOffset = 45f;
    private float zLocalOffset = 5f;

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
        if (Input.GetKeyDown(KeyCode.F))
        {
            deckStack.cardList[0].SetCovered(false);
        }
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
                GameObject newCardGameObject = Instantiate(newCardPrefab, deckStack.transform, false);

                //change GameObject name
                newCardGameObject.name = newCard.value + newCard.seam;

                //get gameObject CardController component and set new card sprites
                CardController newCardController = newCardGameObject.GetComponent<CardController>();
                newCardController.InitializeSprites(seamSprite, valueSprite);

                //TODO TEMP IF COLLIDER FIXED
                deckStack.cardList.Add(newCardController);
            }
        }

        //randomize the order of the list
        ShuffleDeck();

    }

    private void ShuffleDeck()
    {
        //cards = cards.OrderBy(card => Guid.NewGuid()).ToList();

        Random rng = new Random();

        int n = deckStack.cardList.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            CardController shuffledCard = deckStack.cardList[k];
            deckStack.cardList[k] = deckStack.cardList[n];
            deckStack.cardList[n] = shuffledCard;
        }
    }

    private IEnumerator ServeCard()
    {
        //wait until all card GameObject are instantiated
        yield return new WaitWhile(() => deckStack.cardList.Count < 52);

        //serve progressively the card on the table
        for (int i = 0; i < bottomStacks.Count; i++)
        {
            for (int j = 0; j < i + 1; j++)
            {
                //set the y and z offset for the current card to serve on table
                float currentYLocalOffset = yLocalOffset * j;
                float currentZLocalOffset = zLocalOffset * j;

                //get the current card to serve on table
                CardController cardController = deckStack.cardList[j];
                GameObject cardGameObject = cardController.gameObject;
                
                //TODO TEMP IF COLLIDER FIXED
                deckStack.cardList.Remove(cardController);

                //set the new parent for the current card to serve on table
                cardGameObject.transform.SetParent(bottomStacks[i].transform);
                
                //set the sorting order to render properly the card
                cardGameObject.GetComponent<Canvas>().sortingOrder = (int)currentZLocalOffset;

                //calculate the offset in world coordinates
                Vector3 offsetVector = bottomStacks[i].transform.TransformVector(0, currentYLocalOffset, currentZLocalOffset);

                //set the new position of the card
                Vector3 newPosition = bottomStacks[i].transform.position - offsetVector;

                //TODO REPLACE WITH ANIMATION
                //move the card to the new position
                float elapsedTime = 0f;

                while (elapsedTime < 0.2)
                {
                    cardGameObject.transform.position = Vector3.Lerp(cardGameObject.transform.position, newPosition, elapsedTime);
                    elapsedTime += Time.deltaTime;

                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }
}
