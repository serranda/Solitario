﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Controller;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Util;
using Random = System.Random;

namespace Manager
{
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

        [SerializeField] private float waitTimeForServe;

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI movesText;

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
            AlertManager.Instance.SpawnAlertPanel("in attesa che il mazzo venga mischiato...", false, null, null);
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
            }
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
                    GameObject newCardGameObject = Instantiate(newCardPrefab, SpawnManager.Instance.deckButton.transform, false);

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
                    SpawnManager.Instance.cardToSpawn.Add(newCardController);
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

                int n = SpawnManager.Instance.cardToSpawn.Count;

                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    CardController shuffledCard = SpawnManager.Instance.cardToSpawn[k];
                    SpawnManager.Instance.cardToSpawn[k] = SpawnManager.Instance.cardToSpawn[n];
                    SpawnManager.Instance.cardToSpawn[n] = shuffledCard;
                }
            }
        }

        private IEnumerator ServeCard()
        {
            //wait until all card GameObject are instantiated
            yield return new WaitWhile(() => SpawnManager.Instance.cardToSpawn.Count < 52);

            AlertManager.Instance.GetActiveAlertController().DestroyPanel();

            //serve progressively the card on the table
            for (int i = 0; i < bottomStacks.Count; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    //set the y and z offset for the current card to serve on table
                    float currentYLocalOffset = yLocalOffset * j;
                    float currentZLocalOffset = zLocalOffset * (j + 1);

                    //get the current card to serve on table
                    CardController cardController = SpawnManager.Instance.cardToSpawn[j];
                    GameObject cardGameObject = cardController.gameObject;

                    //Remove served card form deck
                    SpawnManager.Instance.cardToSpawn.Remove(cardController);

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
        //            SpawnManager.Instance.RestoreAllChild();
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
}

