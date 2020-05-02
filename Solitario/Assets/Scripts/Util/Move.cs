using System;
using UnityEngine;

namespace Util
{
    [Serializable]
    public class Move
    {
        public string type;
        public CardController cardController;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public bool wasCovered;
        public int deckCounter;

        public Move()
        {

        }

        public Move(string type)
        {
            this.type = type;
        }

        public Move(string type, CardController cardController, Vector3 startPosition, Vector3 endPosition, bool wasCovered, int deckCounter)
        {
            this.type = type;
            this.cardController = cardController;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.wasCovered = wasCovered;
            this.deckCounter = deckCounter;
        }
    }
}
