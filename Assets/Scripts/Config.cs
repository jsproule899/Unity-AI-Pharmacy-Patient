using UnityEngine;
using TMPro;
using JSBrowserUtilities;
using System.Linq;


public class Config : MonoBehaviour
{

    public static Scenario Scenario;
    public static Student Student;

    public static GameObject Avatar;

    public static ChatLog ChatLog;

    [SerializeField]
    private TMP_Text context;


    public static bool AvatarIsLoaded = false;
    public static bool ConfigIsLoaded = false;


    async void Awake()
    {
        if (!ConfigIsLoaded)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            string[] urlPath = BrowserHelper.JS_GetUrlPath().Split("/");
            string scenarioId = urlPath.Last();
            #else
            string scenarioId = "1";
            #endif

            Avatar = GameObject.FindWithTag("Avatar");
            Scenario = await Scenario.LoadConfig(Application.streamingAssetsPath + $"/Scenario_Config{scenarioId}.json");
            context = GameObject.Find("Context").GetComponent<TMP_Text>();
            context.text = Scenario.Context;
            ConfigIsLoaded = true;
        }


        


    }

    // Update is called once per frame
    void Update()
    {

    }

    public static bool LoadAvatar()
    {
        GameObject oldAvatar = GameObject.FindWithTag("Avatar");

        if (Scenario.Gender == "Female")
        {
            Avatar = Instantiate(Resources.Load("Female_Avatar"), oldAvatar.transform.position, oldAvatar.transform.rotation, oldAvatar.transform.parent) as GameObject;

        }
        else
        {
            Avatar = Instantiate(Resources.Load("Male_Avatar"), oldAvatar.transform.position, oldAvatar.transform.rotation, oldAvatar.transform.parent) as GameObject;
        }
        Destroy(oldAvatar);
        AvatarIsLoaded = true;
        return true;

    }


}
