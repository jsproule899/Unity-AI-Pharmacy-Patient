using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using TMPro;
using ReadyPlayerMe.Core.Editor;

public class Config : MonoBehaviour
{

    public static Scenario scenario;

    [SerializeField]
    public static GameObject Avatar;

    [SerializeField]
    private TMP_Text context;


    public bool avatarLoaded = false;

    // Start is called before the first frame update
    async void Start()
    {
        Avatar = GameObject.FindWithTag("Avatar");
        scenario = await Scenario.LoadConfig(Application.streamingAssetsPath + "/Scenario_Config.json");
        context.text = scenario.Context;
        
        // avatarLoaded = LoadAvatar();


    }

    // Update is called once per frame
    void Update()
    {

    }

    private bool LoadAvatar()
    {

        if (scenario.Gender == "Female")
        {
            
            
            


        }
        else
        {
            Avatar = Instantiate(Resources.Load("Male_Avatar")) as GameObject;
        }

        return true;

    }


}
