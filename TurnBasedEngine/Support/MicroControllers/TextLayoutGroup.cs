using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextLayoutGroup : MonoBehaviour, ILayoutGroup, ILayoutElement {

    [System.NonSerialized]
    private RectTransform m_Rect;
    protected RectTransform rectTransform {
        get {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }
    protected List<float> childWidths = new List<float>();
    protected List<float> childHeights = new List<float>();
    protected List<int> childRowCounts = new List<int>();
    protected List<float> rowHeights = new List<float>();
    protected List<float> rowWidths = new List<float>();
    public float minWidth { get; protected set; }
    public float preferredWidth { get; protected set; }
    public float flexibleWidth => 1f;
    public float minHeight { get; protected set; }
    public float preferredHeight { get; protected set; }
    public float flexibleHeight => 0f;
    int ILayoutElement.layoutPriority => 0;
    public float margin = 0f;
    public float border = 0f;
    public bool center = false;

    void ILayoutElement.CalculateLayoutInputHorizontal() {
        minWidth = 0f;
        preferredWidth = 0f;
        childWidths.Clear();
        for (int i = 0; i < rectTransform.childCount; i++) {
            var rect = rectTransform.GetChild(i) as RectTransform;
            var width = LayoutUtility.GetPreferredWidth(rect);
            minWidth = Mathf.Max(minWidth, width);
            preferredWidth += width;
            childWidths.Add(width);
        }
        minWidth += border * 2f;
        preferredWidth += border * 2f;
    }

    void ILayoutController.SetLayoutHorizontal() {
        var totalWidth = rectTransform.rect.size[0];
        var availableWidth = totalWidth - border * 2f;
        var cursor = 0f;
        childRowCounts.Clear();
        rowWidths.Clear();
        int rowStartIndex = 0;
        for (var i = 0; i < rectTransform.childCount; i++) {
            var width = childWidths[i];
            cursor += width;
            // Time to wrap
            if (cursor > availableWidth) {
                rowWidths.Add(cursor - width);
                childRowCounts.Add(i - rowStartIndex);
                rowStartIndex = i;
                cursor = width;
            }
            cursor += margin;
        }
        rowWidths.Add(cursor - margin);
        childRowCounts.Add(rectTransform.childCount - rowStartIndex);

        var rowIndex = 0;
        var leftOfCursor = border;
        if (center) {
            leftOfCursor += (availableWidth - rowWidths[rowIndex]) / 2f;
        }

        cursor = 0f;
        for (var i = 0; i < rectTransform.childCount; i++) {
            var width = childWidths[i];
            cursor += width;
            if (cursor > availableWidth) {
                cursor = width;
                rowIndex += 1;
                if (center) {
                    leftOfCursor = border;
                    leftOfCursor += (availableWidth - rowWidths[rowIndex]) / 2f;
                }
            }
            var pos = leftOfCursor + cursor - width;
            cursor += margin;

            var rect = rectTransform.GetChild(i) as RectTransform;
            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.up;
            rect.pivot = Vector2.one * .5f;

            Vector2 sizeDelta = rect.sizeDelta;
            sizeDelta[0] = width;
            rect.sizeDelta = sizeDelta;

            Vector2 anchoredPosition = rect.anchoredPosition;
            anchoredPosition[0] = pos + width * rect.pivot[0];
            rect.anchoredPosition = anchoredPosition;
        }
    }

    void ILayoutElement.CalculateLayoutInputVertical() {
        if (rectTransform.childCount != childRowCounts.Count) {
            (this as ILayoutController).SetLayoutHorizontal();
        }
        preferredHeight = 0f;
        minHeight = 0f;
        var rowStartIndex = 0;
        var rowIndex = 0;
        var rowPreferredHeight = 0f;
        childHeights.Clear();
        rowHeights.Clear();
        for (int i = 0; i < rectTransform.childCount; i++) {
            var rect = rectTransform.GetChild(i) as RectTransform;
            var rowCount = i - rowStartIndex;
            if (rowCount >= childRowCounts[rowIndex]) {
                preferredHeight += rowPreferredHeight;
                minHeight = Mathf.Max(minHeight, rowPreferredHeight);
                rowHeights.Add(rowPreferredHeight);
                rowIndex++;
                rowPreferredHeight = 0f;
                rowStartIndex = i;
            }
            var height = LayoutUtility.GetPreferredHeight(rect);
            rowPreferredHeight = Mathf.Max(rowPreferredHeight, height);
            childHeights.Add(height);
        }
        // Update for final row
        preferredHeight += rowPreferredHeight;
        minHeight = Mathf.Max(minHeight, rowPreferredHeight);
        rowHeights.Add(rowPreferredHeight);

        minHeight += border * 2f;
        preferredHeight += border * 2f;
    }

    void ILayoutController.SetLayoutVertical() {
        var totalHeight = rectTransform.rect.size[1];
        var rowStartIndex = 0;
        var rowIndex = 0;
        var rowHeight = border;
        var extraHeight = totalHeight - (preferredHeight - border * 2f);
        if (center && extraHeight > 0) {
            rowHeight = extraHeight / 2f;
        }
        for (int i = 0; i < rectTransform.childCount; i++) {
            var rect = rectTransform.GetChild(i) as RectTransform;
            var rowCount = i - rowStartIndex;
            if (rowCount >= childRowCounts[rowIndex]) {
                rowHeight += rowHeights[rowIndex];
                rowIndex++;
                rowStartIndex = i;
            }
            var height = childHeights[i];
            var midAlignMargin = (rowHeights[rowIndex] - height) / 2f;

            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.up;
            rect.pivot = Vector2.one * .5f;

            Vector2 sizeDelta = rect.sizeDelta;
            sizeDelta[1] = height;
            rect.sizeDelta = sizeDelta;

            Vector2 anchoredPosition = rect.anchoredPosition;
            anchoredPosition[1] = -rowHeight - midAlignMargin - height * rect.pivot[0];
            rect.anchoredPosition = anchoredPosition;
        }
        childHeights.Clear();
        childWidths.Clear();
        childRowCounts.Clear();
        rowHeights.Clear();
    }
}