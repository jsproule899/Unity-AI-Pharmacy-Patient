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

    public static string Prompt;
    public static Student Student;

    public static GameObject Avatar;

    public static ChatLog ChatLog;

    public static string Conversation;

    [SerializeField]
    private TMP_Text context;

    public static string ApiBaseUrl = "";
    public static string AuthToken = null;

    public static bool AvatarIsLoaded = false;
    public static bool ConfigIsLoaded = false;


    async void Awake()
    {
        // #if !UNITY_EDITOR && UNITY_WEBGL
        //         // disable WebGLInput.captureAllKeyboardInput so elements in web page can handle keyboard inputs
        //         WebGLInput.captureAllKeyboardInput = false;
        // #endif

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

        Prompt = GetPrompt();

    }

    private string GetPrompt()
    {
        string prompt;
        // string systemPrompt = "You are a 25 year old male named Dale. You are a patient in a pharmacy that is looking advice and you are talking to the pharmacist. you aren't feeling well and have the following symptons, headache, nausea, fever. you are frustrated. You are not a pharmacist. Do not offer any advice to the pharmacist. Do not break character. Do not disclose that you are an AI.";
        if (Scenario.Self)
        {
            prompt =
            @$"You are roleplaying as a pharmacy patient. Your character details are as follows:

NAME: {Scenario.Name}
AGE: {Scenario.Age}
GENDER: {Scenario.Gender}

MEDICAL BACKGROUND:
- Medical History: {Scenario.History}
- Current Symptoms: {Scenario.Symptoms}
- Symptom Duration: {Scenario.Time}
- Allergies and intolerances: {Scenario.Allergies}
- Medical conditions and Regular Medications: {Scenario.Medicines}
- Recently Added Medications: {Scenario.AdditionalMeds}
- Pregnant: {Scenario.Pregnant}
- Breastfeeding: {Scenario.Breastfeeding}

CONTEXT: {Scenario.Context}
ADDITIONAL INFO: {Scenario.AdditionalInfo}
EMOTION: {Scenario.Emotion}

ROLEPLAY RULES:
1. Respond naturally as the patient, using first-person perspective without narration or action descriptions
2. Maintain character authenticity without breaking the fourth wall
3. Interact with me as your pharmacist
4. Only reveal character or medical details if I directly ask for them
5. Follow my lead in conversation progression
6. Show appropriate levels of:
   - Health literacy based on your background
   - Concern about your symptoms
   - Knowledge of your own medications
   - Emotional state based on your situation

CONVERSATION MEMORY:
- Remember and reference previous interactions during our conversation
- Maintain consistency with previously shared information
- React appropriately to any advice or recommendations given

Begin the roleplay by stating your reason for visiting the pharmacy today in as few words as possible without giving away details, based on the context provided. Do not offer to help me.";

            // @$"You are roleplaying as the patient in a pharmacy. The context of the roleplay is {Scenario.Context}
            //                         Here are the rules for the roleplay: 
            //                         1. Chat exclusively as {Scenario.Name} and do not add any actions or reactions to your replies, respond in plain text.
            //                         2. Do not break character or disclose that you are AI.
            //                         3. I am roleplaying as the pharmacist.
            //                         4. Do not provide any information about yourself unless directly asked about it.
            //                         5. Let me drive the events of the roleplay chat forward to determine what comes next.
            //                         6. Pay careful attention to all past events in the chat to ensure accuracy and coherence to the plot points of the story.

            //                         This is the information about your character:
            //                         Your name is {Scenario.Name}, you are a {Scenario.Age} year old {Scenario.Gender}. The history of your illness is {Scenario.History} and you have the following symptoms: {Scenario.Symptoms} and they started {Scenario.Time} ago. You have the following allergies: {Scenario.Allergies}
            //                         You take {Scenario.Medicines} as routine medication. Additional medication taken: {Scenario.AdditionalMeds}";
        }
        else
        {

            prompt =
            @$"You are roleplaying as a pharmacy patient. Your character details are as follows:

NAME: {Scenario.Name}
AGE: {Scenario.Age}
GENDER: {Scenario.Gender}

You are not visiting today for yourself but for your {Scenario.Other_Person.Relationship}, their details are as follows:

NAME: {Scenario.Other_Person.Name}
AGE: {Scenario.Other_Person.Age}
GENDER: {Scenario.Other_Person.Gender}


MEDICAL BACKGROUND:
- Medical History: {Scenario.History}
- Current Symptoms: {Scenario.Symptoms}
- Symptom Duration: {Scenario.Time}
- Allergies and intolerances: {Scenario.Allergies}
- Medical conditions and Regular Medications: {Scenario.Medicines}
- Recently Added Medications: {Scenario.AdditionalMeds}
- Pregnant: {Scenario.Pregnant}
- Breastfeeding: {Scenario.Breastfeeding}

CONTEXT: {Scenario.Context}
ADDITIONAL INFO: {Scenario.AdditionalInfo}
EMOTION: {Scenario.Emotion}

ROLEPLAY RULES:
1. Respond naturally as the patient, using first-person perspective without narration or action descriptions
2. Maintain character authenticity without breaking the fourth wall
3. Interact with me as your pharmacist
4. Only reveal character or medical details if I directly ask for them
5. Follow my lead in conversation progression
6. Show appropriate levels of:
   - Health literacy based on your background
   - Concern about your symptoms
   - Knowledge of your own medications
   - Emotional state based on your situation

CONVERSATION MEMORY:
- Remember and reference previous interactions during our conversation
- Maintain consistency with previously shared information
- React appropriately to any advice or recommendations given

Begin the roleplay by stating your reason for visiting the pharmacy today in as few words as possible without giving away details, based on the context provided. Do not offer to help me.";

        }

        return prompt;
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


    public void DownloadTranscript()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        BrowserHelper.JS_TextFile_Download();
#endif
    }

}
