using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;


public class CardController : MonoBehaviour
{
    private readonly int sortingOrderOnClick = 100;

    [SerializeField] private Image topSeam;
    [SerializeField] private Image mainSeam;
    [SerializeField] private Image value;
    [SerializeField] private float movingSpeed;

    private Animator animator;

    private Canvas overrideCanvas;
    private int sortingOrder;

    private Vector3 startingPosition;

    private bool isCovered;

    public StackController parentStack;

    public Card card;

    private void OnEnable()
    {
        overrideCanvas = GetComponent<Canvas>();
        animator = GetComponent<Animator>();
    }

    private void OnMouseUp()
    {
        Vector3 newPosition = startingPosition;
        int newSortingOrder = sortingOrder;
        //set transform of parenStack
        Transform parentTransform = parentStack.transform;
        string parentType = parentTransform.parent.name;

        Debug.Log(parentType);

        //the stack has no child
        if (parentStack.transform.childCount == 0)
        {
            if (parentType == "BottomStacks" && card.value == 13)
            {
                //card is a king and can be move in the empty stack
                gameObject.transform.SetParent(parentTransform);

                //set the y and z offset for the current card to serve on table
                float currentZLocalOffset = GameManager.zLocalOffset;

                //calculate the offset in world coordinates
                Vector3 offsetVector = parentTransform.TransformVector(0, 0, currentZLocalOffset);

                //set the new position of the card
                newPosition = parentTransform.position - offsetVector;

                //set new sorting order of the card
                newSortingOrder = (int)currentZLocalOffset;
            }
            else if (parentType == "FinalStacks" && card.value == 1 &&
                     parentStack.name.IndexOf(card.seam, StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                //card can be moved to the final stacks, set parent and newPosition
                gameObject.transform.SetParent(parentTransform);

                //set the z offset for the current card to serve on table
                float currentZLocalOffset = GameManager.zLocalOffset;

                //calculate the offset in world coordinates
                Vector3 offsetVector = parentTransform.TransformVector(0, 0, currentZLocalOffset);

                //set the new position of the card
                newPosition = parentTransform.position - offsetVector;


                //set new sorting order of the card
                newSortingOrder = (int)currentZLocalOffset;
            }
        }
        //the stack has child
        else
        {
            Debug.LogFormat("PARENT NOT EMPTY {0}", parentStack.transform.childCount);

            //get index of previous card from parenStack cardList(the last card is the this one, so we need to take the card before in the list)
            int parentStackLastIndex = parentStack.transform.childCount - 1;

            CardController lastCardController = parentStack.transform.GetChild(parentStackLastIndex).GetComponent<CardController>();
            Card lastCard = lastCardController.card;

            Debug.LogFormat("LAST CARD: {0} ", lastCard);
            Debug.LogFormat("THIS CARD: {0} ", card);

            if (parentType == "BottomStacks" && card.value == lastCard.value - 1 && card.color != lastCard.color)
            {
                //card can be moved to the current bottom stacks, set parent
                gameObject.transform.SetParent(parentTransform);

                //set the y and z offset for the current card to serve on table
                float currentYLocalOffset = GameManager.yLocalOffset * (parentStackLastIndex + 1);
                float currentZLocalOffset = GameManager.zLocalOffset * (parentStackLastIndex + 2);

                //calculate the offset in world coordinates
                Vector3 offsetVector = parentTransform.TransformVector(0, currentYLocalOffset, currentZLocalOffset);

                //set the new position of the card
                newPosition = parentTransform.position - offsetVector;

                //set new sorting order of the card
                newSortingOrder = (int)currentZLocalOffset;
            }
            else if (parentType == "FinalStacks" && card.value == lastCard.value + 1 && card.seam == lastCard.seam)
            {
                //card can be moved to the final stacks, set parent and newPosition
                gameObject.transform.SetParent(parentTransform);

                //set the z offset for the current card to serve on table
                float currentZLocalOffset = GameManager.zLocalOffset * (parentStackLastIndex + 2);

                //calculate the offset in world coordinates
                Vector3 offsetVector = parentTransform.TransformVector(0, 0, currentZLocalOffset);

                //set the new position of the card
                newPosition = parentTransform.position - offsetVector;

                //set new sorting order of the card
                newSortingOrder = (int)currentZLocalOffset;
            }
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

    public void SetIsCovered(bool covered)
    {
        //backCover.enabled = covered;
        animator.SetBool("covered", covered);
    }

    public void MoveToPosition(Vector3 newPosition, int sortOrder)
    {
        StartCoroutine(MoveToPositionCoroutine(newPosition, sortOrder));
    }

    private IEnumerator MoveToPositionCoroutine(Vector3 newPosition, int sortOrder)
    {

        while (transform.position != newPosition)
        {
            transform.position = Vector3.Lerp(transform.position, newPosition, movingSpeed * Time.deltaTime);

            //if (transform.position == newPosition)
            //{
            //    yield break;
            //}

            yield return null;
        }

        //restore the correct sorting order
        overrideCanvas.sortingOrder = sortOrder;
    }
}
