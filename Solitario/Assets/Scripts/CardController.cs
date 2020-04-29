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
    [SerializeField] private Image topSeam;
    [SerializeField] private Image mainSeam;
    [SerializeField] private Image value;
    [SerializeField] private float movingTime;

    private Animator animator;

    private Canvas overrideCanvas;
    private int sortingOrder;

    private Vector3 startingPosition;

    public StackController parentStack;

    public Card card;

    private void Start()
    {
        overrideCanvas = GetComponent<Canvas>();
        animator = GetComponent<Animator>();
    }

    private void OnMouseUp()
    {
        if (parentStack.cardList.IndexOf(this) != parentStack.cardList.Count - 1) return;

        Vector3 newPosition = Vector3.positiveInfinity;

        //TODO HANDLE CASE 0 ELEMENT IN THE STACK

        //get last index of parenStack cardList
        int parentStackLastIndex = parentStack.cardList.Count - 1;

        
        //get last card from parentStack
        Card lastCard = parentStack.cardList[parentStackLastIndex].card;

        //set transform of parenStack
        Transform parentTransform = parentStack.transform;

        string parentType = parentTransform.parent.name;

        if (parentType == "BottomStacks" && card.value == lastCard.value - 1 && card.color != lastCard.color)
        {
            //card can be moved to the current bottom stacks, set parent
            gameObject.transform.SetParent(parentTransform);

            //set the y and z offset for the current card to serve on table
            float currentYLocalOffset = GameManager.yLocalOffset * parentStackLastIndex;
            float currentZLocalOffset = GameManager.zLocalOffset * parentStackLastIndex;

            //calculate the offset in world coordinates
            Vector3 offsetVector = parentTransform.TransformVector(0, currentYLocalOffset, currentZLocalOffset);

            //set the new position of the card
            newPosition = parentTransform.position - offsetVector;
        }
        else if(parentType == "FinalStacks" && card.value == lastCard.value + 1 && card.seam == lastCard.seam)
        {
            //card can be moved to the final stacks, set parent and newPosition
            gameObject.transform.SetParent(parentTransform);

            //set the new position of the card
            newPosition = parentTransform.position;
        }
        else
        {
            //card can't move, set as newPosition the previous one
            newPosition = startingPosition;
        }

        if (newPosition != Vector3.positiveInfinity)
        {
            //move the card to the newPosition
            MoveToPosition(newPosition);
        }
        else
        {
            //card hasn't been moved, but need to set the correct sorting order
            overrideCanvas.sortingOrder = sortingOrder;
        }
    }

    private void OnMouseDown()
    {
        if (parentStack.cardList.IndexOf(this) != parentStack.cardList.Count - 1) return;
        sortingOrder = overrideCanvas.sortingOrder;

        startingPosition = transform.position;

    }

    private void OnMouseDrag()
    {
        if (parentStack.cardList.IndexOf(this) != parentStack.cardList.Count - 1) return;

        //Set sorting order to prevent render issues
        overrideCanvas.sortingOrder = 60;

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

    private void CheckCover()
    {
        if (parentStack.name != "DeckStack" &&
            parentStack.cardList.IndexOf(this) == parentStack.cardList.Count - 1)
        {
            SetCovered(false);
        }
        else
        {
            SetCovered(true);
        }
    }

    public void SetCovered(bool covered)
    {
        //backCover.enabled = covered;
        animator.SetBool("covered", covered);

    }

    public void MoveToPosition(Vector3 newPosition)
    {
        StartCoroutine(MoveToPositionCoroutine(newPosition));
    }

    private IEnumerator MoveToPositionCoroutine(Vector3 newPosition)
    {
        //move the card to the new position
        float elapsedTime = 0f;

        while (elapsedTime < movingTime)
        {
            transform.position = Vector3.Lerp(transform.position, newPosition, elapsedTime);
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        //set the correct sorting order
        overrideCanvas.sortingOrder = sortingOrder;

    }
}
