using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

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
}
