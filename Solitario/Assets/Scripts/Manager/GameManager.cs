using System;
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

        public List<StackController> bottomStacks;
        [SerializeField] private StackController stackHearts;
        [SerializeField] private StackController stackDiamonds;
        [SerializeField] private StackController stackClubs;
        [SerializeField] private StackController stackSpades;

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI movesText;

        private int scoreCounter;
        private int moveCounter;

        //private Move lastMove;

        public GameState currentState;

        public enum Seams { Hearts, Diamonds, Clubs, Spades }
        public enum Colors { Red, Black }
        public enum Values { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
        public enum GameState { Menu, Playing, Pause, Win }
        public enum MoveTypes { Deck, Card }

        private void Awake()
        {
            currentState = GameState.Menu;
        }

        private void Start()
        {
            //create deck and shuffle the element of the list
            StartCoroutine(NewCardDeck());
        }

        private IEnumerator NewCardDeck()
        {
            yield return new WaitWhile(() => currentState == GameState.Menu);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    //create a new card
                    Card newCard = new Card((Values) Enum.GetValues(typeof(Values)).GetValue(j), 
                        (Seams)Enum.GetValues(typeof(Seams)).GetValue(i), 
                        (Colors)Enum.GetValues(typeof(Colors)).GetValue(i / 2));

                    //get the index of the sprites
                    int seamIndex = Array.IndexOf(Enum.GetValues(typeof(Seams)), newCard.seam);
                    int valueIndex = Array.IndexOf(Enum.GetValues(typeof(Values)), newCard.value);

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
                    newCardGameObject.name = newCard.value.ToString() + newCard.seam.ToString();

                    //get gameObject CardController component and set new card sprites
                    CardController newCardController = newCardGameObject.GetComponent<CardController>();
                    newCardController.InitializeSprites(seamSprite, valueSprite);

                    //set crad on newCardController
                    newCardController.card = newCard;

                    //disable card box collider
                    newCardController.SetIsMovable(false);

                    //add card to deck to keep track which is the next to serve
                    SpawnManager.Instance.deck.Add(newCardController);
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

                int n = SpawnManager.Instance.deck.Count;

                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    CardController shuffledCard = SpawnManager.Instance.deck[k];
                    SpawnManager.Instance.deck[k] = SpawnManager.Instance.deck[n];
                    SpawnManager.Instance.deck[n] = shuffledCard;
                }
            }

            StartCoroutine(SpawnManager.Instance.ServeCard());
        }

        public IEnumerator CheckWin()
        {
            while (currentState == GameState.Playing)
            {
                if (stackHearts.transform.childCount == 13 &&
                    stackDiamonds.transform.childCount == 13 &&
                    stackClubs.transform.childCount == 13 &&
                    stackSpades.transform.childCount == 13)
                {
                    //WIN!!
                    AlertManager.Instance.SpawnWinPanel();

                    currentState = GameState.Win;

                    yield break;
                }

                yield return new WaitWhile(() => currentState != GameState.Playing);
            }
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

        //public void UpdateMoves()
        //{
        //    moveCounter++;
        //    movesText.text = moveCounter.ToString();
        //    //lastMove = move;
        //}

        //public void RestoreLastMove()
        //{
        //    if (lastMove.type != "")
        //    {
        //        //last move was from deck
        //        if (lastMove.type == MoveTypes[0])
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

