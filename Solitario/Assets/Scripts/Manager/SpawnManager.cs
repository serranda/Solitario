using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controller;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Manager
{
    public class SpawnManager : Singleton<SpawnManager>
    {
        [SerializeField] private float waitTimeForServe;

        [SerializeField] private Sprite deckButtonNormalSprite;
        [SerializeField] private Sprite deckButtonEmptySprite;

        public GameObject spawnedCard;
        public Button deckButton;

        public int lastCardSpawned;
        public List<CardController> deck;

        public readonly float xLocalOffset = 55f;
        public readonly float yLocalOffset = 25f;
        public readonly float zLocalOffset = 5f;

        public IEnumerator ServeCard()
        {
            //close waiting panel
            AlertManager.Instance.CloseAlertPanel();

            //serve progressively the card on the table
            for (int i = 0; i < GameManager.Instance.bottomStacks.Count; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    //set the y and z offset for the current card to serve on table
                    float currentYLocalOffset = yLocalOffset * j;
                    float currentZLocalOffset = zLocalOffset * (j + 1);

                    lastCardSpawned++;

                    //get the current card to serve on table
                    CardController cardController = deck[lastCardSpawned];
                    GameObject cardGameObject = cardController.gameObject;

                    cardController.served = true;

                    //set the new parent for the current card to serve on table
                    cardGameObject.transform.SetParent(GameManager.Instance.bottomStacks[i].transform);

                    //calculate the offset in world coordinates
                    Vector3 offsetVector = GameManager.Instance.bottomStacks[i].transform.TransformVector(0, currentYLocalOffset, currentZLocalOffset);

                    //set the new position of the card
                    Vector3 newPosition = GameManager.Instance.bottomStacks[i].transform.position - offsetVector;

                    //set canvas sorting order of 
                    cardController.SetOverrideCanvasSortingOrder((int)currentZLocalOffset, false);


                    //coroutine to move gradually card to new position
                    cardController.MoveToPosition(newPosition);

                    //set flag to make card discovered if is last of the list
                    if (j == i)
                    {
                        cardController.SetIsCovered(false);
                        cardController.SetIsMovable(true);

                        GameManager.Instance.bottomStacks[i].lastCardController = cardController;
                    }

                    yield return new WaitForSeconds(waitTimeForServe);

                    cardController.CheckCovered();

                }
            }

            StartCoroutine(GameManager.Instance.CheckWin());
        }

        public void SpawnCard()
        {
            if (lastCardSpawned == -1)
            {
                ResetSpawnedChildren(deckButton.transform);
                deckButton.image.sprite = deckButtonNormalSprite;

                lastCardSpawned++;
                return;

                //newMove = new Move(MoveTypes[0]);
            }

            //increment next card index
            lastCardSpawned++;


            //if next card already been served, keep increment next card index
            while (deck[lastCardSpawned].served)
            {
                lastCardSpawned++;
            }

            Transform spawnedCardTransform = spawnedCard.transform;

            //get the current card to serve on table
            CardController cardController = deck[lastCardSpawned];
            GameObject cardGameObject = cardController.gameObject;

            //set the new parent for the current card to serve on table
            cardGameObject.transform.SetParent(spawnedCardTransform);

            SetSpawnedChildren();

            //set flag to make card discovered
            cardController.SetIsCovered(false);


            //if all next card already served, reached end of spawnable card
            //set next card to -1 and set button to empty sprite
            IsDeckFinished();
        }

        public void SetSpawnedChildren()
        {
            //get spawned card list
            List<CardController> cardControllers = spawnedCard.GetComponentsInChildren<CardController>().ToList();

            //for each card spawned, check its index relative to the previous list
            //if it's between the last 3 card, adjust the position
            foreach (CardController cardController in cardControllers)
            {

                int cardPositionFromEnd = cardControllers.Count - (cardControllers.IndexOf(cardController) + 1);

                if (cardPositionFromEnd == 0 || cardPositionFromEnd == 1 || cardPositionFromEnd == 2)
                {
                    //set the  z offset for the current card to serve on table
                    float currentZLocalOffset = zLocalOffset * (cardControllers.IndexOf(cardController) + 1);

                    Vector3 offsetVector = spawnedCard.transform.TransformVector(xLocalOffset * cardPositionFromEnd, 0, currentZLocalOffset);
                    Vector3 newPosition = spawnedCard.transform.position - offsetVector;

                    //set canvas sorting order of 
                    cardController.SetOverrideCanvasSortingOrder((int)currentZLocalOffset, false);

                    cardController.MoveToPosition(newPosition);
                }

                //disable control for all card except last
                cardController.SetIsMovable(cardPositionFromEnd == 0);
            }

        }

        private void IsDeckFinished()
        {
            if (lastCardSpawned == deck.Count - 1)
            {
                lastCardSpawned = -1;
                deckButton.image.sprite = deckButtonEmptySprite;
            }
        }

        public void ResetSpawnedChildren(Transform newParent)
        {
            StartCoroutine(ResetSpawnedChildrenRoutine(newParent));
        }

        private IEnumerator ResetSpawnedChildrenRoutine(Transform newParent)
        {
            List<CardController> cardControllers = spawnedCard.GetComponentsInChildren<CardController>().ToList();

            foreach (CardController cardController in cardControllers)
            {
                GameObject cardGameObject = cardController.gameObject;

                //set the new parent for the current card to serve on table
                cardGameObject.transform.SetParent(newParent, false);

                //set the new position of the card
                Vector3 newPosition = newParent.position;

                //set flag to make card discovered if is last of the list
                cardController.SetIsCovered(true);
                cardController.SetIsMovable(false);

                //set canvas sorting order of 
                cardController.SetOverrideCanvasSortingOrder(0, false);

                //coroutine to move gradually card to new position
                cardController.MoveToPosition(newPosition);

                yield return new WaitForEndOfFrame();
            }

            lastCardSpawned = 0;

            //update score
            GameManager.Instance.UpdateScore(-100);
        }
    }
}

