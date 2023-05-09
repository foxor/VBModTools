using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StayOnScreen : MonoBehaviour
{
    public enum HorizontalOrientation {
        Left,
        Right,
    }
    public HorizontalOrientation StartingOrientation;

    private RectTransform rt;
    private RectTransform parentRect;
    private HorizontalOrientation ho;
    private Vector3[] corners = new Vector3[4];
    public void Awake() {
        rt = transform as RectTransform;
        parentRect = transform.parent as RectTransform;

        if (parentRect == null) {
            enabled = false;
            return;
        }

        // Start in the default orientation
        SetHorizontalOrientation(StartingOrientation);
    }
    private void SetHorizontalOrientation(HorizontalOrientation newHo) {
        var xPivot = newHo == HorizontalOrientation.Right ? 0 : 1;
        var parentWidth = parentRect.sizeDelta.x;
        var xPos = newHo == HorizontalOrientation.Right ? parentWidth / 2f : -parentWidth / 2f;
        rt.pivot = new Vector2(xPivot, 0.5f);
        rt.localPosition = new Vector3(xPos, rt.localPosition.y, 0f);
        ho = newHo;
    }
    private void LayoutHorizontal() {
        if (ho == HorizontalOrientation.Right) {
            var overRHS = corners[2].x > Screen.width;
            if (overRHS) {
                SetHorizontalOrientation(HorizontalOrientation.Left);
            }
        }
        else if (ho == HorizontalOrientation.Left) {
            var overLHS = corners[1].x < 0;
            if (overLHS) {
                SetHorizontalOrientation(HorizontalOrientation.Right);
            }
        }
    }
    private void LayoutVertical() {
        var bottomOverlap = -corners[0].y;
        var topOverlap = corners[1].y - Screen.height;
        if (bottomOverlap > 0) {
            rt.localPosition += new Vector3(0f, bottomOverlap);
        }
        else if (topOverlap > 0) {
            rt.localPosition += new Vector3(0f, -topOverlap);
        }
    }
    public void LateUpdate() {
        // Measure where we are
        rt.GetWorldCorners(corners);

        LayoutHorizontal();
        LayoutVertical();
    }
}
