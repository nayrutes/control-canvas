using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISizeCopy : MonoBehaviour
{
    public RectTransform source;
    public RectTransform target;
    
    void Start()
    {
        if (source == null)
        {
            source = GetComponent<RectTransform>();
        }
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }
    }
    // Update is called once per frame
    [ExecuteAlways]
    void Update()
    {
        target.pivot = source.pivot;
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, source.rect.width);
        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, source.rect.height);
    }
}
