using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlaceController : Singleton<SpawnPlaceController>
{
    
    public List<CardController> spawnedCard;

    public StackController spawnPlaceHolder;

    public BoxCollider2D boxCollider2D;

    public void AdjustChildPosition()
    {
        foreach (CardController cardController in spawnedCard)
        {

            int cardIndexPosition = spawnedCard.Count - spawnedCard.IndexOf(cardController);

            if (cardIndexPosition == 2 || cardIndexPosition == 3)
            {
                Vector3 offsetVector = spawnPlaceHolder.transform.TransformVector(-(boxCollider2D.offset.x - GameManager.instance.xLocalOffset * (cardIndexPosition-1)), -boxCollider2D.offset.y, 0);
                Vector3 newPosition = spawnPlaceHolder.transform.position - offsetVector;

                cardController.MoveToPosition(newPosition, cardController.sortingOrder);
            }
        }
    }

    public void RemoveAllChild(Transform newParent)
    {
        StartCoroutine(RemoveAllChildCoroutine(newParent));
    }

    private IEnumerator RemoveAllChildCoroutine(Transform newParent)
    {
        foreach (CardController cardController in spawnedCard)
        {
            GameObject cardGameObject = cardController.gameObject;

            //set the new parent for the current card to serve on table
            cardGameObject.transform.SetParent(newParent, false);

            //set the new position of the card
            Vector3 newPosition = newParent.position;

            //set flag to make card discovered if is last of the list
            cardController.SetIsCovered(true);
            cardController.SetIsMovable(false);

            //coroutine to move gradually card to new position
            cardController.MoveToPosition(newPosition, 0);

            yield return new WaitForFixedUpdate();
        }

        spawnedCard.Clear();
    }
}
