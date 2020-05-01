using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Animator), 
    typeof(Canvas), 
    typeof(BoxCollider2D))]
public class CardController : MonoBehaviour
{
    private readonly int sortingOrderOnClick = 100;

    [SerializeField] private Image topSeam;
    [SerializeField] private Image mainSeam;
    [SerializeField] private Image value;
    [SerializeField] private float movingSpeed;

    private Animator animator;

    private Canvas overrideCanvas;
    public int sortingOrder;

    private BoxCollider2D boxCollider;

    private Vector3 startingPosition;

    public StackController parentStack;

    public bool fromSpawn;

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
    }

    private void OnMouseUp()
    {
        Vector3 newPosition = startingPosition;
        int newSortingOrder = sortingOrder;
        //set transform of parenStack
        Transform newParentTransform = parentStack.transform;
        string newParentType = newParentTransform.parent.name;

        Debug.Log(newParentType);

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
            }
        }
        //the stack has child
        else
        {
            Debug.LogFormat("PARENT {0} NOT EMPTY", parentStack.name);

            //get index of previous card from parenStack cardList(the last card is the this one, so we need to take the card before in the list)
            int parentStackLastIndex = parentStack.transform.childCount - 1;

            CardController lastCardController = parentStack.transform.GetChild(parentStackLastIndex).GetComponent<CardController>();
            Card lastCard = lastCardController.card;

            Debug.LogFormat("LAST CARD: {0} ", lastCard);
            Debug.LogFormat("THIS CARD: {0} ", card);

            if (newParentType == "BottomStacks" && card.value == lastCard.value - 1 && card.color != lastCard.color)
            {
                //the card actually has as  parent the last card of the stack
                newParentTransform = lastCardController.transform;

                Debug.Log(newParentTransform.GetChild(newParentTransform.childCount - 1).GetComponent<CardController>());

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
                    newSortingOrder = lastCardController.overrideCanvas.sortingOrder + (int) currentZLocalOffset;
                }
            }
            else if (newParentType == "FinalStacks" && card.value == lastCard.value + 1 && card.seam == lastCard.seam)
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
            }

            lastCardController.CheckCoverAndTouch();
        }

        MoveToPosition(newPosition, newSortingOrder);
    }

    private void OnMouseDown()
    {
        //save the sorting order to restore it later
        sortingOrder = overrideCanvas.sortingOrder;

        startingPosition = transform.position;

    }

    private void OnMouseDrag()
    {
        //Set sorting order to prevent render issues
        overrideCanvas.sortingOrder = sortingOrderOnClick;

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

    public void CheckCoverAndTouch()
    {
        if (transform.GetSiblingIndex() == transform.parent.childCount)
        {
            SetIsCovered(false);
            SetIsCovered(true);
        }
    }

    public void SetIsCovered(bool covered)
    {
        animator.SetBool("covered", covered);
    }

    public void SetIsMovable(bool clickable)
    {
        boxCollider.enabled = clickable;
    }

    public void MoveToPosition(Vector3 newPosition, int sortOrder)
    {
        StartCoroutine(MoveToPositionCoroutine(newPosition, sortOrder));
    }

    private IEnumerator MoveToPositionCoroutine(Vector3 newPosition, int sortOrder)
    {        
        //restore the correct sorting order
        overrideCanvas.sortingOrder = sortingOrder = sortOrder;

        float startTime = 0;

        float distance = Vector3.Distance(transform.position, newPosition);

        float totalTime = distance / movingSpeed;

        while (startTime < totalTime)
        {
            transform.position = Vector3.Lerp(transform.position, newPosition, startTime);

            if (transform.position == newPosition)
            {
                break;
                Debug.Log("TEST1");
            }

            startTime += Time.deltaTime;

            yield return null;
        }
        Debug.Log("TEST2");



    }
}
