using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectMatch : MonoBehaviour {
    public RectTransform ToMatch;
    protected void LateUpdate() {
        var myRect = transform as RectTransform;
        myRect.position = ToMatch.position;
        myRect.sizeDelta = ToMatch.sizeDelta;
        myRect.pivot = ToMatch.pivot;
    }
}