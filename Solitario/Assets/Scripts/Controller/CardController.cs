using System;
using System.Collections;
using Manager;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Vector3 = UnityEngine.Vector3;


namespace Controller
{
    [RequireComponent(typeof(Animator),
    typeof(Canvas),
    typeof(BoxCollider2D))]
    public class CardController : MonoBehaviour
    {
        private readonly int sortingOrderOnClick = 500;

        [SerializeField] private Image topSeam;
        [SerializeField] private Image mainSeam;
        [SerializeField] private Image value;
        [SerializeField] private float movingSpeed;

        private BoxCollider2D boxCollider;

        private bool isCovered;
        private bool isMoving;

        private Vector3 startingPosition;

        private Animator animator;

        private Canvas overrideCanvas;
        private int sortingOrder;

        public StackController parentStack;

        public Card card;

        private void OnEnable()
        {
            //get animator component
            animator = GetComponent<Animator>();

            //get canvas component
            overrideCanvas = GetComponent<Canvas>();

            //save the sorting order to restore it later
            sortingOrder = overrideCanvas.sortingOrder;

            //get collider component
            boxCollider = GetComponent<BoxCollider2D>();

            isCovered = true;
        }

        private void OnMouseUp()
        {
            Vector3 newPosition = startingPosition;
            int newSortingOrder = sortingOrder;
            //set transform of parenStack
            Transform newParentTransform = parentStack.transform;
            string newParentType = newParentTransform.parent.name;

            int score = 0;

            //Debug.Log(newParentType);

            //the stack has no child
            if (parentStack.transform.childCount == 0)
            {
                if (newParentType == "BottomStacks" && card.value == 13)
                {
                    //card is a king and can be move in the empty stack
                    gameObject.transform.SetParent(newParentTransform);

                    //set the y and z offset for the current card to serve on table
                    float currentZLocalOffset = GameManager.Instance.zLocalOffset;

                    //calculate the offset in world coordinates
                    Vector3 offsetVector = newParentTransform.TransformVector(0, 0, currentZLocalOffset);

                    //set the new position of the card
                    newPosition = newParentTransform.position - offsetVector;

                    //set new sorting order of the card
                    newSortingOrder = (int)currentZLocalOffset;

                    //set score for current move
                    score = 5;
                }
                else if (newParentType == "FinalStacks" && card.value == 1 &&
                         parentStack.name.IndexOf(card.seam, StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    //card can be moved to the final stacks, set parent and newPosition
                    gameObject.transform.SetParent(newParentTransform);

                    //set the z offset for the current card to serve on table
                    float currentZLocalOffset = GameManager.Instance.zLocalOffset;

                    //calculate the offset in world coordinates
                    Vector3 offsetVector = newParentTransform.TransformVector(0, 0, currentZLocalOffset);

                    //set the new position of the card
                    newPosition = newParentTransform.position - offsetVector;

                    //set new sorting order of the card
                    newSortingOrder = (int)currentZLocalOffset;

                    //disable box collider (card no more movable)
                    SetIsMovable(false);

                    //set score for current move
                    score = 15;

                }
            }
            //the stack has child
            else
            {
                //get index of previous card from parenStack cardList(the last card is the this one, so we need to take the card before in the list)
                int parentStackLastIndex = parentStack.transform.childCount - 1;

                CardController lastCardController = parentStack.lastCardController;
                Card lastCard = lastCardController.card;

                //Debug.LogFormat("LAST CARD: {0} ", lastCard);
                //Debug.LogFormat("THIS CARD: {0} ", card);

                if (newParentType == "BottomStacks" && card.value == lastCard.value - 1 && card.color != lastCard.color)
                {
                    //the card actually has as  parent the last card of the stack
                    newParentTransform = lastCardController.transform;

                    //CHECK IF THE CARD HAS ALREADY A CHILD CARD, IF YES DON'T MOVE THE CARD
                    if (!newParentTransform.GetChild(newParentTransform.childCount - 1).GetComponent<CardController>())
                    {
                        //card can be moved to the current bottom stacks, set parent 
                        gameObject.transform.SetParent(newParentTransform);

                        //set the y and z offset for the current card to serve on table
                        float currentYLocalOffset = GameManager.Instance.yLocalOffset * 2;
                        float currentZLocalOffset = GameManager.Instance.zLocalOffset;

                        //calculate the offset in world coordinates
                        Vector3 offsetVector = newParentTransform.TransformVector(0, currentYLocalOffset, currentZLocalOffset);

                        //set the new position of the card
                        newPosition = newParentTransform.position - offsetVector;

                        //set new sorting order of the card
                        newSortingOrder = lastCardController.overrideCanvas.sortingOrder + (int)currentZLocalOffset;

                        //set score for current move
                        score = 5;
                    }
                }
                else if (newParentType == "FinalStacks" &&
                         card.value == lastCard.value + 1 &&
                         card.seam == lastCard.seam &&
                         transform.GetComponentsInChildren<CardController>().Length <= 1)
                {
                    //card can be moved to the final stacks, set parent and newPosition
                    gameObject.transform.SetParent(newParentTransform);

                    //set the z offset for the current card to serve on table
                    float currentZLocalOffset = GameManager.Instance.zLocalOffset * (parentStackLastIndex + 2);

                    //calculate the offset in world coordinates
                    Vector3 offsetVector = newParentTransform.TransformVector(0, 0, currentZLocalOffset);

                    //set the new position of the card
                    newPosition = newParentTransform.position - offsetVector;

                    //set new sorting order of the card
                    newSortingOrder = (int)currentZLocalOffset;

                    //disable box collider (card no more movable)
                    SetIsMovable(false);

                    //set score for current move
                    score = 15;
                }
            }

            //card has been moved
            if (newPosition != startingPosition)
            {
                //check if card was in spawn list, if yes remove from list and adjust other cards
                if (SpawnManager.Instance.cardToSpawn.Contains(this))
                {
                    SpawnManager.Instance.cardToSpawn.Remove(this);
                    if (SpawnManager.Instance.nextCard != -1)
                    {
                        SpawnManager.Instance.nextCard -= 2;
                    }
                    SpawnManager.Instance.AdjustChildPosition();
                }

                ////set move before staring movement of card
                //Move newMove = new Move(GameManager.Instance.moveTypes[1], this, transform.position, newPosition, GetIsCovered(), SpawnManager.Instance.nextCard);


                //update move (ONLY IF CARD HAS BEEN MOVE TO A NEW POSITION)
                GameManager.Instance.UpdateMoves();
            }
            else
            {
                if (SpawnManager.Instance.cardToSpawn.Contains(this))
                    newSortingOrder = sortingOrderOnClick;
            }

            SetOverrideCanvasSortingOrder(newSortingOrder, false);

            //get the care controller child count (it return also itself so need to subtract 1 to ghet the actual child number)
            if (transform.GetComponentsInChildren<CardController>().Length > 1)
            {
                foreach (CardController cardController in transform.GetComponentsInChildren<CardController>())
                {
                    if (cardController != this)
                    {
                        cardController.SetOverrideCanvasSortingOrder(cardController.GetOverrideCanvasSortingOrder() - sortingOrderOnClick + newSortingOrder - sortingOrder, false);
                    }
                }
            }

            MoveToPosition(newPosition);

            //update score
            GameManager.Instance.UpdateScore(score);

        }

        private void OnMouseDown()
        {
            //Set sorting order to prevent render issues
            SetOverrideCanvasSortingOrder(sortingOrderOnClick, true);

            startingPosition = transform.position;

            //get the care controller child count (it return also itself so need to subtract 1 to ghet the actual child number)
            if (transform.GetComponentsInChildren<CardController>().Length > 1)
            {
                foreach (CardController cardController in transform.GetComponentsInChildren<CardController>())
                {
                    if (cardController != this)
                    {
                        cardController.SetOverrideCanvasSortingOrder(cardController.GetOverrideCanvasSortingOrder() + sortingOrderOnClick, true);
                    }
                }
            }
        }

        private void OnMouseDrag()
        {
            //get newPosition from mouse
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //set correct z value for new position
            newPosition.z = transform.position.z;

            //set new position to gameObject
            transform.position = newPosition;
        }

        public void InitializeSprites(Sprite seamSprite, Sprite valueSprite)
        {
            //set sean sprite
            topSeam.sprite = seamSprite;
            mainSeam.sprite = seamSprite;

            //set value sprite and color 
            value.sprite = valueSprite;
            value.color = seamSprite.name == "diamonds" || seamSprite.name == "hearts" ? Color.red : Color.black;
        }

        public void SetIsCovered(bool covered)
        {
            isCovered = covered;
            animator.SetBool("covered", covered);
        }

        public bool GetIsCovered()
        {
            return isCovered;
        }

        public void SetIsMovable(bool clickable)
        {
            boxCollider.enabled = clickable;
        }

        public void SetOverrideCanvasSortingOrder(int newSortingOrder, bool forLater)
        {
            //save the sorting order to restore it later
            if (forLater)
                sortingOrder = overrideCanvas.sortingOrder;

            overrideCanvas.sortingOrder = newSortingOrder;
        }

        public int GetOverrideCanvasSortingOrder()
        {
            return overrideCanvas.sortingOrder;
        }

        public void CheckCovered()
        {
            StartCoroutine(CheckCoveredRoutine());
        }

        private IEnumerator CheckCoveredRoutine()
        {
            while (isCovered)
            {
                yield return new WaitWhile(() => isMoving);
                yield return new WaitWhile(() => transform.parent.GetComponent<StackController>().lastCardController != this);

                SetIsCovered(false);
                SetIsMovable(true);

                yield break;
            }
        }

        public void MoveToPosition(Vector3 newPosition)
        {
            isMoving = true;
            StartCoroutine(MoveToPositionRoutine(newPosition));
        }

        private IEnumerator MoveToPositionRoutine(Vector3 newPosition)
        {

            float startTime = 0;

            float distance = Vector3.Distance(transform.position, newPosition);

            float totalTime = distance / movingSpeed;

            while (startTime < totalTime)
            {
                transform.position = Vector3.Lerp(transform.position, newPosition, startTime);

                if (transform.position == newPosition)
                {
                    isMoving = false;

                    yield break;
                }

                startTime += Time.deltaTime;

                yield return null;
            }

            isMoving = false;
        }
    }
}

