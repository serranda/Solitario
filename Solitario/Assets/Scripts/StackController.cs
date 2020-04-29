using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackController : MonoBehaviour
{
    public List<CardController> cardList;

    private BoxCollider2D boxCollider2D;

    private void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CardController cardController = other.GetComponent<CardController>();

        if (cardController)
        {
            //Set the new parent stack
            cardList.Add(cardController);
            cardController.parentStack = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CardController cardController = other.GetComponent<CardController>();

        if (cardController)
        {
            //Set the new parent stack
            cardList.Remove(cardController);
        }
    }
}
