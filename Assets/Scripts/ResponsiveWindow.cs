using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResponsiveWindow : MonoBehaviour
{
    private LayoutElement layoutElement;
    [SerializeField] float widthRatio;
    [SerializeField] float heightRatio;

    void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
    }

    void Update()
    {
        layoutElement.preferredWidth = GameObject.Find("UI").transform.GetComponent<RectTransform>().rect.width * widthRatio;
        layoutElement.preferredHeight = GameObject.Find("UI").transform.GetComponent<RectTransform>().rect.height * heightRatio;
    }
}
