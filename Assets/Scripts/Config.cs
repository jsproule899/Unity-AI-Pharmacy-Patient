using UnityEngine;
using TMPro;
using JSBrowserUtilities;
using System.Linq;
using Unity.VisualScripting;
using System;
using System.Threading.Tasks;


public class Config : MonoBehaviour
{

    public static Scenario Scenario;
    public static Student Student;

    public static GameObject Avatar;

    public static ChatLog ChatLog;

    [SerializeField]
    private TMP_Text context;

    public static string ApiBaseUrl = "";
    public static string AuthToken = null;

    public static bool AvatarIsLoaded = false;
    public static bool ConfigIsLoaded = false;


    async void Awake()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        // disable WebGLInput.captureAllKeyboardInput so elements in web page can handle keyboard inputs
        WebGLInput.captureAllKeyboardInput = false;
#endif

        if (!ConfigIsLoaded)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string[] urlPath = BrowserHelper.JS_GetUrlPath().Split("/");
            string scenarioId = urlPath.Last();
#else
            string scenarioId = "";
#endif


            Avatar = GameObject.FindWithTag("Avatar");
            // Scenario = await Scenario.LoadConfig(Application.streamingAssetsPath + $"/Scenario_Config{scenarioId}.json");
            // Wait for the ApiBaseUrl to be set before continuing
            while (string.IsNullOrEmpty(ApiBaseUrl))
            {
                await Task.Yield(); // This will yield control and check again during the next frame
            }

            Debug.Log("API BASE URL SET BEFORE FETCH");
            Scenario = await Scenario.LoadConfig(ApiBaseUrl + $"/api/scenario/{scenarioId}");

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

        Avatar = Instantiate(Resources.Load(Scenario.Avatar), oldAvatar.transform.position, oldAvatar.transform.rotation, oldAvatar.transform.parent) as GameObject;

        Destroy(oldAvatar);
        AvatarIsLoaded = true;
        return true;

    }

    public void SetApiBaseUrl(string apiBaseUrl)
    {
        ApiBaseUrl = apiBaseUrl;
        Debug.Log($"API Base URL set to {ApiBaseUrl}");

    }

    public void SetAuthToken(string token)
    {
        if(token.Length>0)
        AuthToken = token;
        Debug.Log($"AuthToken set");
    }

}
