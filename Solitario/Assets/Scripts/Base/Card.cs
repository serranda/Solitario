using System;

namespace Util
{
    [Serializable]
    public class Card
    {
        public int value;
        public string seam;
        public string color;

        public Card(int value, string seam, string color)
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
