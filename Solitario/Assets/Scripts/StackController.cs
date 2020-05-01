using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        CardController cardController = other.GetComponent<CardController>();

        if (!cardController) return;

        //Set the new parent stack
        cardController.parentStack = this;
    }
}
