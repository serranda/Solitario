using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Manager
{
    public class SpawnManager : Singleton<SpawnManager>
    {
        [SerializeField] private StackController spawnedStackController;
        [SerializeField] private BoxCollider2D spawnedCollider2D;

        [SerializeField] private Sprite deckButtonNormalSprite;
        [SerializeField] private Sprite deckButtonEmptySprite;

        public Button deckButton;
        public GameObject spawnedCard;

        public int nextCard;
        public List<CardController> cardToSpawn;


        public void SpawnCard()
        {
            //Move newMove;

            if (nextCard == -1)
            {
                RemoveAllChild(deckButton.transform);
                deckButton.image.sprite = deckButtonNormalSprite;

                //newMove = new Move(MoveTypes[0]);
            }
            else
            {
                Transform spawnPlaceTransform = spawnedStackController.transform;

                //set the  z offset for the current card to serve on table
                float currentZLocalOffset = GameManager.Instance.zLocalOffset * spawnPlaceTransform.childCount;

                //get the current card to serve on table
                CardController cardController = cardToSpawn[nextCard];
                GameObject cardGameObject = cardController.gameObject;

                //set the new parent for the current card to serve on table
                cardGameObject.transform.SetParent(spawnPlaceTransform);

                AdjustChildPosition();

                //calculate the offset in world coordinates
                Vector3 offsetVector = spawnPlaceTransform.TransformVector(-spawnedCollider2D.offset.x, -spawnedCollider2D.offset.y, currentZLocalOffset);

                //set the new position of the card
                Vector3 newPosition = spawnPlaceTransform.position - offsetVector;

                //set canvas sorting order of 
                cardController.SetOverrideCanvasSortingOrder((int)currentZLocalOffset, false);

                ////set move before staring movement of card
                //newMove = new Move(MoveTypes[1], cardController, cardController.transform.position, newPosition, cardController.GetIsCovered(), SpawnManager.Instance.nextCard -1);

                //coroutine to move gradually card to new position
                cardController.MoveToPosition(newPosition);

                //set flag to make card discovered
                cardController.SetIsCovered(false);

            }
            ////Update moves
            //GameManager.Instance.UpdateMoves();

        }

        public void SetEmptyDeckButton()
        {
            deckButton.image.sprite = deckButtonEmptySprite;
        }

        public void AdjustChildPosition()
        {
            foreach (CardController cardController in cardToSpawn)
            {
                int cardPositionFromEnd = nextCard - cardToSpawn.IndexOf(cardController);

                //adjust only last 3 card of the stack
                if (cardPositionFromEnd == 0 || cardPositionFromEnd == 1 || cardPositionFromEnd == 2)
                {
                    Vector3 offsetVector = spawnedStackController.transform.TransformVector(-spawnedCollider2D.offset.x + GameManager.instance.xLocalOffset * cardPositionFromEnd, -spawnedCollider2D.offset.y, 0);
                    Vector3 newPosition = spawnedStackController.transform.position - offsetVector;


                    //set canvas sorting order of 
                    cardController.SetOverrideCanvasSortingOrder((int)GameManager.Instance.zLocalOffset * cardToSpawn.IndexOf(cardController), false);

                    cardController.MoveToPosition(newPosition);
                }

                //disable control for all card except last
                cardController.SetIsMovable(cardPositionFromEnd == 0);
            }

            //end of the deck, no need to update next card
            if (nextCard != -1)
            {
                nextCard++;
            }

            if (nextCard == cardToSpawn.Count)
            {
                nextCard = -1;

                SetEmptyDeckButton();
            }

        }

        public void RemoveAllChild(Transform newParent)
        {
            StartCoroutine(RemoveAllChildRoutine(newParent));
        }

        private IEnumerator RemoveAllChildRoutine(Transform newParent)
        {
            foreach (CardController cardController in cardToSpawn)
            {
                GameObject cardGameObject = cardController.gameObject;

                //set the new parent for the current card to serve on table
                cardGameObject.transform.SetParent(newParent, false);

                RectTransform newParentRectTransform = (RectTransform)newParent;

                //calculate the offset in world coordinates
                Vector3 offsetVector = newParent.TransformVector(newParentRectTransform.rect.width / 2, newParentRectTransform.rect.height / 2, 0);

                //set the new position of the card
                Vector3 newPosition = newParent.position - offsetVector;

                //set flag to make card discovered if is last of the list
                cardController.SetIsCovered(true);
                cardController.SetIsMovable(false);

                //set canvas sorting order of 
                cardController.SetOverrideCanvasSortingOrder(0, false);

                //coroutine to move gradually card to new position
                cardController.MoveToPosition(newPosition);

                yield return new WaitForEndOfFrame();
            }

            nextCard = 0;

            //update score
            GameManager.Instance.UpdateScore(-100);
        }

        public void RestoreAllChild()
        {
            StartCoroutine(RestoreAllChildRoutine());
        }

        private IEnumerator RestoreAllChildRoutine()
        {
            foreach (CardController cardController in cardToSpawn)
            {
                Transform spawnPlaceTransform = spawnedStackController.transform;

                //set the  z offset for the current card to serve on table
                float currentZLocalOffset = GameManager.Instance.zLocalOffset * spawnPlaceTransform.childCount;

                //get the current card to serve on table
                GameObject cardGameObject = cardController.gameObject;

                //set the new parent for the current card to serve on table
                cardGameObject.transform.SetParent(spawnPlaceTransform);

                AdjustChildPosition();

                //calculate the offset in world coordinates
                Vector3 offsetVector = spawnPlaceTransform.TransformVector(-spawnedCollider2D.offset.x, -spawnedCollider2D.offset.y, currentZLocalOffset);

                //set the new position of the card
                Vector3 newPosition = spawnPlaceTransform.position - offsetVector;

                //set canvas sorting order of 
                cardController.SetOverrideCanvasSortingOrder((int)currentZLocalOffset, false);

                //coroutine to move gradually card to new position
                cardController.MoveToPosition(newPosition);

                //set flag to make card discovered
                cardController.SetIsCovered(false);

                yield return new WaitForEndOfFrame();

            }

            nextCard = -1;

            //set sprite for empty deck
            SetEmptyDeckButton();

            //update score
            GameManager.Instance.UpdateScore(100);
        }
    }
}

