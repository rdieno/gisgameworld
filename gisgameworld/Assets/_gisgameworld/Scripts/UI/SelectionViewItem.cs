using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionViewItem : MonoBehaviour
{
    public Location location;
    public Text label;
    public float lat;
    public float lon;
    public Button button;
    public bool currentLocation;
}
