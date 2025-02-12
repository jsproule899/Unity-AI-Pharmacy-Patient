using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SpriteChanger : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites;

    private int index = 0;


    public void ChangeSprite(int index)
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[index];
    }

    public void ChangeSprite()
    {
        index = index < sprites.Length - 1 ? index+1 : 0;
        gameObject.GetComponent<Image>().sprite = sprites[index];
    }


}
