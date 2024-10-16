using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Quit : MonoBehaviour
{
    [SerializeField] private Button Exitbutton;


    void Start()
    {
        Exitbutton.onClick.AddListener(ExitUnity);


    }
    void ExitUnity()
    {


#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE)
    Application.Quit();
    Debug.Log("Exiting Unity...");
#elif (UNITY_WEBGL)
    Application.Quit();
    Debug.Log("Exiting Unity...");   
    Application.ExternalEval("document.location.reload(true)");
#endif



    }
}
