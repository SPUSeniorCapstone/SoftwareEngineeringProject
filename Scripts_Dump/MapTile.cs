using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MapTile
{
    public bool isPassable;
    public bool isAdjacentToUnpassable;
    public bool isPlaceable;
    public float movementBuff;
    public float height;
    public float weight;
    public Color mapColor;
}
