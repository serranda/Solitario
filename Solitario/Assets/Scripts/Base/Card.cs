using System;
using Manager;

namespace Util
{
    [Serializable]
    public class Card
    {
        public GameManager.Values value;
        public GameManager.Seams seam;
        public GameManager.Colors color;

        public Card(GameManager.Values value, GameManager.Seams seam, GameManager.Colors color)
        {
            this.value = value;
            this.seam = seam;
            this.color = color;
        }

        public override string ToString()
        {
            return $"{nameof(value)}: {value}, {nameof(seam)}: {seam}, {nameof(color)}: {color}";
        }
    }
}
