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
    [SerializeField] private Image backCover;
    [SerializeField] private Image topSeam;
    [SerializeField] private Image mainSeam;
    [SerializeField] private Image value;

    private Animator animator;

    public StackController parentStack;
    public StackController newParentStack;

    private Canvas overrideCanvas;
    private int sortingOrder;

    private void Awake()
    {

    }

    private void Start()
    {
        overrideCanvas = GetComponent<Canvas>();
        animator = GetComponent<Animator>();
    }

    private void OnMouseUp()
    {
        overrideCanvas.sortingOrder = sortingOrder;

        //set the check to see if is possible to attach the card to the stack
        //bottom stack condition card.value == lastStackCard.value - 1 && card.color != lastStackCard.color
        //set attached property to the 
        //top stack card.value == lastStackCard.value + 1 && card.seam == lastStackCard.seam

    }

    private void OnMouseDown()
    {
        sortingOrder = overrideCanvas.sortingOrder;
    }

    private void OnMouseDrag()
    {
        //SetCovered(false);
        overrideCanvas.sortingOrder = 60;
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = transform.position.z;
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
}
