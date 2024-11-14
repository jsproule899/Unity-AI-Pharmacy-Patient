using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
    public TextMeshProUGUI text;

    public RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void SetText(string content)
    {
        text.text = content;
    }

    private void Update()
    {
        Vector2 position = Input.mousePosition;

        float pivotX = position.x / Screen.width;
        float pivotY = position.y / Screen.height;
        

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        position.y = position.y + 20;
        transform.position = position;

        
    }
}
