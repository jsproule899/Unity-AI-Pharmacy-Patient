using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class messageSystem : MonoBehaviour
{
    public GameObject userMessageBox;
    public TextMeshProUGUI userMessage;
    public GameObject aiMessageBox;
    public TextMeshProUGUI aiMessage;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (userMessage.text == null || userMessage.text.Length < 1)
        {
            userMessageBox.gameObject.SetActive(false);
        }
        else
        {
            userMessageBox.gameObject.SetActive(true);
        }

        if (aiMessage.text == null || aiMessage.text.Length < 1)
        {
            aiMessageBox.gameObject.SetActive(false);
        }
        else
        {
            aiMessageBox.gameObject.SetActive(true);
        }


    }
}
