using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;


public class CardController : MonoBehaviour
{
    [SerializeField] private Image backCover;
    [SerializeField] private Image topSeam;
    [SerializeField] private Image mainSeam;
    [SerializeField] private Image value;

    public StackController parentStack;
    public StackController newParentStack;

    private Canvas overrideCanvas;
    private int sortingOrder;

    private void Awake()
    {
        overrideCanvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        SetCovered(false);
    }

    private void OnMouseUp()
    {
        overrideCanvas.sortingOrder = sortingOrder;
    }

    private void OnMouseDown()
    {
        sortingOrder = overrideCanvas.sortingOrder;
    }

    private void OnMouseDrag()
    {
        SetCovered(false);
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

    //private void CheckCover()
    //{
    //    if (parentStack.name != "DeckStack" &&
    //        parentStack.cardList.IndexOf(gameObject) == parentStack.cardList.Count - 1)
    //    {
    //        SetCovered(false);
    //    }
    //    else
    //    {
    //        SetCovered(true);
    //    }
    //}

    private void SetCovered(bool covered)
    {
        backCover.enabled = covered;
    }

    //public void CheckParentStack()
    //{
    //    if (parentStack != newParentStack)
    //    {
    //        Debug.Log("CHECK");
    //        SetParentStack();
    //    }
    //}

    //private void SetParentStack()
    //{
    //    parentStack.cardList.Remove(gameObject);

    //    //set the new parent stack and add the card to its list
    //    parentStack = newParentStack;
    //    parentStack.cardList.Add(gameObject);
    //}

}
