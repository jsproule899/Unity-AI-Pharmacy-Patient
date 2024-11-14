using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResponsiveMessage : MonoBehaviour
{
    public TextMeshProUGUI message;

    private LayoutElement layoutElement;

    float standardPreferredWidth;


    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();

        standardPreferredWidth = layoutElement.preferredWidth;
    }

    private void Update()
    {

        float responsivePreferedWidth = standardPreferredWidth * transform.parent.parent.GetComponent<RectTransform>().rect.width / 1920f;
        layoutElement.preferredWidth = responsivePreferedWidth > 500 ? responsivePreferedWidth : 500;
        layoutElement.enabled = message.text.Length *11.3 > layoutElement.preferredWidth;



    }
}
