using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Util;
using Random = System.Random;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private AssetReference cardReference;
    [SerializeField] private List<AssetReferenceSprite> seamsReference;
    [SerializeField] private List<AssetReferenceSprite> valuesReference;

    [SerializeField] private List<StackController> bottomStacks;
    [SerializeField] private StackController stackHearts;
    [SerializeField] private StackController stackDiamonds;
    [SerializeField] private StackController stackClubs;
    [SerializeField] private StackController stackSpades;

    [SerializeField] private Button deckButton;
    [SerializeField] private Sprite deckButtonNormalSprite;
    [SerializeField] private Sprite deckButtonEmptySprite;

    [SerializeField] private float waitTimeForServe;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;

    [SerializeField] private GameObject waitPanel;
    [SerializeField] private GameObject winPanel;

    private bool endShuffle;

    private int scoreCounter;
    private int moveCounter;

    //private Move lastMove;

    public readonly float xLocalOffset = 55f;
    public readonly float yLocalOffset = 25f;
    public readonly float zLocalOffset = 5f;

    private readonly string[] seams = { "hearts", "diamonds", "clubs", "spades" };
    private readonly string[] colors = { "red", "black" };
    private readonly int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

    public readonly string[] moveTypes = { "deck", "card" };


    private void Start()
    {
        //create deck and shuffle the element of the list
        NewTable();
    }

    private void NewTable()
    {
        StartCoroutine(WaitForDeck());
        StartCoroutine(NewCardDeck());
        StartCoroutine(ServeCard());
    }

    private void Update()
    {
        CheckWin();
    }

    private void CheckWin()
    {
        if (stackHearts.transform.childCount == 13 &&
            stackDiamonds.transform.childCount == 13 &&
            stackClubs.transform.childCount == 13 &&
            stackSpades.transform.childCount == 13)
        {
            //WIN!!
            winPanel.SetActive(true);
        }
    }

    private IEnumerator WaitForDeck()
    {
        yield return new WaitWhile(() => !endShuffle);
        waitPanel.SetActive(false);
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

                //disable card box collider
                newCardController.SetIsMovable(false);

                //add card to deck to keep track which is the next to serve
                SpawnController.Instance.cardToSpawn.Add(newCardController);
            }
        }

        //randomize the order of the list
        ShuffleDeck();
    }

    private void ShuffleDeck()
    {
        //cards = cards.OrderBy(card => Guid.NewGuid()).ToList();

        for (int i = 0; i < 3; i++)
        {
            Random rng = new Random();

            int n = SpawnController.Instance.cardToSpawn.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                CardController shuffledCard = SpawnController.Instance.cardToSpawn[k];
                SpawnController.Instance.cardToSpawn[k] = SpawnController.Instance.cardToSpawn[n];
                SpawnController.Instance.cardToSpawn[n] = shuffledCard;
            }
        }

        endShuffle = true;
    }

    private IEnumerator ServeCard()
    {
        //wait until all card GameObject are instantiated
        yield return new WaitWhile(() => SpawnController.Instance.cardToSpawn.Count < 52);

        //serve progressively the card on the table
        for (int i = 0; i < bottomStacks.Count; i++)
        {
            for (int j = 0; j < i + 1; j++)
            {
                //set the y and z offset for the current card to serve on table
                float currentYLocalOffset = yLocalOffset * j;
                float currentZLocalOffset = zLocalOffset * (j + 1);

                //get the current card to serve on table
                CardController cardController = SpawnController.Instance.cardToSpawn[j];
                GameObject cardGameObject = cardController.gameObject;

                //Remove served card form deck
                SpawnController.Instance.cardToSpawn.Remove(cardController);

                //set the new parent for the current card to serve on table
                cardGameObject.transform.SetParent(bottomStacks[i].transform);

                //calculate the offset in world coordinates
                Vector3 offsetVector = bottomStacks[i].transform.TransformVector(0, currentYLocalOffset, currentZLocalOffset);

                //set the new position of the card
                Vector3 newPosition = bottomStacks[i].transform.position - offsetVector;

                //set canvas sorting order of 
                cardController.SetOverrideCanvasSortingOrder((int)currentZLocalOffset, false);

                //coroutine to move gradually card to new position
                cardController.MoveToPosition(newPosition);

                //set flag to make card discovered if is last of the list
                if (j == i)
                {
                    cardController.SetIsCovered(false);
                    cardController.SetIsMovable(true);

                    bottomStacks[i].lastCardController = cardController;
                }

                yield return new WaitForSeconds(waitTimeForServe);

                cardController.CheckCovered();

            }
        }
    }

    public void SpawnCard()
    {
        //Move newMove;

        if (SpawnController.Instance.nextCard == -1)
        {
            SpawnController.Instance.RemoveAllChild(deckButton.transform);
            deckButton.image.sprite = deckButtonNormalSprite;

            //newMove = new Move(moveTypes[0]);
        }
        else
        {
            Transform spawnPlaceTransform = SpawnController.Instance.spawnPlaceHolder.transform;

            //set the  z offset for the current card to serve on table
            float currentZLocalOffset = zLocalOffset * spawnPlaceTransform.childCount;

            //get the current card to serve on table
            CardController cardController = SpawnController.Instance.cardToSpawn[SpawnController.Instance.nextCard];
            GameObject cardGameObject = cardController.gameObject;

            //set the new parent for the current card to serve on table
            cardGameObject.transform.SetParent(spawnPlaceTransform);

            SpawnController.Instance.AdjustChildPosition();

            //calculate the offset in world coordinates
            Vector3 offsetVector = spawnPlaceTransform.TransformVector(-SpawnController.Instance.boxCollider2D.offset.x, - SpawnController.Instance.boxCollider2D.offset.y, currentZLocalOffset);

            //set the new position of the card
            Vector3 newPosition = spawnPlaceTransform.position - offsetVector;

            //set canvas sorting order of 
            cardController.SetOverrideCanvasSortingOrder((int)currentZLocalOffset, false);

            ////set move before staring movement of card
            //newMove = new Move(moveTypes[1], cardController, cardController.transform.position, newPosition, cardController.GetIsCovered(), SpawnController.Instance.nextCard -1);

            //coroutine to move gradually card to new position
            cardController.MoveToPosition(newPosition);

            //set flag to make card discovered
            cardController.SetIsCovered(false);

        }

        UpdateMoves();

    }

    public void SetEmptyDeckButton()
    {
        deckButton.image.sprite = deckButtonEmptySprite;
    }

    public void UpdateMoves()
    {
        moveCounter++;
        movesText.text = moveCounter.ToString();

        //lastMove = move;
    }

    public void UpdateScore(int deltaScore)
    {
        scoreCounter += deltaScore;
        if (scoreCounter < 0)
        {
            scoreCounter = 0;
        }
        scoreText.text = scoreCounter.ToString();
    }

    //public void RestoreLastMove()
    //{
    //    if (lastMove.type != "")
    //    {
    //        //last move was from deck
    //        if (lastMove.type == moveTypes[0])
    //        {
    //            SpawnController.Instance.RestoreAllChild();
    //        }
    //        else
    //        {
    //            CardController cardController = lastMove.cardController;
    //            cardController.MoveToPosition(lastMove.startPosition);
    //            cardController.SetIsCovered(lastMove.wasCovered);
    //        }

    //        lastMove = null;
    //    }

    //}


}
