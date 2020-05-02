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
            int cardPositionFromEnd = spawnedCard.Count - spawnedCard.IndexOf(cardController);

            //adjust only last 3 card of the stack
            if (cardPositionFromEnd == 1 || cardPositionFromEnd == 2 || cardPositionFromEnd == 3)
            {
                Vector3 offsetVector = spawnPlaceHolder.transform.TransformVector(-boxCollider2D.offset.x + GameManager.instance.xLocalOffset * (cardPositionFromEnd - 1), -boxCollider2D.offset.y, 0);
                Vector3 newPosition = spawnPlaceHolder.transform.position - offsetVector;


                //set canvas sorting order of 
                cardController.SetOverrideCanvasSortingOrder((int)GameManager.Instance.zLocalOffset * spawnedCard.IndexOf(cardController), false);

                cardController.MoveToPosition(newPosition);
            }

            //disable control for all card except last
            cardController.SetIsMovable(cardPositionFromEnd == 1);
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

            RectTransform newParentRectTransform = (RectTransform) newParent;

            //calculate the offset in world coordinates
            Vector3 offsetVector = newParent.TransformVector(newParentRectTransform.rect.width / 2, newParentRectTransform.rect.height / 2, 0);

            //set the new position of the card
            Vector3 newPosition = newParent.position - offsetVector;

            //set flag to make card discovered if is last of the list
            cardController.SetIsCovered(true);
            cardController.SetIsMovable(false);

            //set canvas sorting order of 
            cardController.SetOverrideCanvasSortingOrder(0, false);

            //coroutine to move gradually card to new position
            cardController.MoveToPosition(newPosition);

            yield return new WaitForEndOfFrame();
        }

        spawnedCard.Clear();
    }
}
