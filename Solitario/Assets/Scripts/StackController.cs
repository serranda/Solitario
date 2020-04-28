using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackController : MonoBehaviour
{
    public List<GameObject> cardList;

    private BoxCollider2D boxCollider2D;


    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        if (transform.parent.name == "BottomStacks")
        {
            float newHeight = Screen.height - Math.Abs(transform.parent.GetComponent<RectTransform>().anchoredPosition.y);
            float newOffsetY = (newHeight - boxCollider2D.size.y) / 2;

            Vector2 newSize = new Vector2(boxCollider2D.size.x, newHeight);
            Vector2 newOffset = new Vector2(boxCollider2D.offset.x, -newOffsetY);

            boxCollider2D.size = newSize;
            boxCollider2D.offset = newOffset;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CardController cardController = other.GetComponent<CardController>();

        if (cardController)
        {
            //Set the new parent stack
            cardList.Add(cardController.gameObject);
            cardController.parentStack = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CardController cardController = other.GetComponent<CardController>();

        if (cardController)
        {
            //Set the new parent stack
            cardList.Remove(cardController.gameObject);
        }
    }
}
