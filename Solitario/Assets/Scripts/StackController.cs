using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class StackController : MonoBehaviour
{
    public CardController lastCardController;

    private void Update()
    {
        List<CardController> cardChildren = GetComponentsInChildren<CardController>().ToList();
        foreach (CardController cardController in cardChildren)
        {
            if (cardChildren.IndexOf(cardController) == cardChildren.Count - 1)
            {
                lastCardController = cardController;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CardController cardController = other.GetComponent<CardController>();

        if (!cardController) return;

        //Set the new parent stack
        cardController.parentStack = this;
    }
}
