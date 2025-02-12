using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OpenAI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using System;

public class AIChat : MonoBehaviour
{
    public UIController UI;
    [SerializeField] private ChatToVoice chatToVoice;
    public float typingSpeed = 0.05f; // Speed of typing in seconds

    public bool isThinking = false;
    public bool isThinkingCoroutineRunning = false;

    private Coroutine thinkingCoroutine;
    private List<ChatMessage> chatHistory = new List<ChatMessage>();
    private Scenario scenario;

    // Start is called before the first frame update
    void Start()
    {


        scenario = Config.Scenario;


        string systemPrompt;
        // string systemPrompt = "You are a 25 year old male named Dale. You are a patient in a pharmacy that is looking advice and you are talking to the pharmacist. you aren't feeling well and have the following symptons, headache, nausea, fever. you are frustrated. You are not a pharmacist. Do not offer any advice to the pharmacist. Do not break character. Do not disclose that you are an AI.";
        if (scenario.Self)
        {
            systemPrompt =
            @$"You are roleplaying as a pharmacy patient. Your character details are as follows:

NAME: {scenario.Name}
AGE: {scenario.Age}
GENDER: {scenario.Gender}

MEDICAL BACKGROUND:
- Medical History: {scenario.History}
- Current Symptoms: {scenario.Symptoms}
- Symptom Duration: {scenario.Time}
- Allergies and intolerances: {scenario.Allergies}
- Medical conditions and Regular Medications: {scenario.Medicines}
- Recently Added Medications: {scenario.AdditionalMeds}
- Pregnant: {scenario.Pregnant}
- Breastfeeding: {scenario.Breastfeeding}

CONTEXT: {scenario.Context}
ADDITIONAL INFO: {scenario.AdditionalInfo}
EMOTION: {scenario.Emotion}

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

            // @$"You are roleplaying as the patient in a pharmacy. The context of the roleplay is {scenario.Context}
            //                         Here are the rules for the roleplay: 
            //                         1. Chat exclusively as {scenario.Name} and do not add any actions or reactions to your replies, respond in plain text.
            //                         2. Do not break character or disclose that you are AI.
            //                         3. I am roleplaying as the pharmacist.
            //                         4. Do not provide any information about yourself unless directly asked about it.
            //                         5. Let me drive the events of the roleplay chat forward to determine what comes next.
            //                         6. Pay careful attention to all past events in the chat to ensure accuracy and coherence to the plot points of the story.

            //                         This is the information about your character:
            //                         Your name is {scenario.Name}, you are a {scenario.Age} year old {scenario.Gender}. The history of your illness is {scenario.History} and you have the following symptoms: {scenario.Symptoms} and they started {scenario.Time} ago. You have the following allergies: {scenario.Allergies}
            //                         You take {scenario.Medicines} as routine medication. Additional medication taken: {scenario.AdditionalMeds}";
        }
        else
        {

            systemPrompt =
            @$"You are roleplaying as a pharmacy patient. Your character details are as follows:

NAME: {scenario.Name}
AGE: {scenario.Age}
GENDER: {scenario.Gender}

You are not visiting today for yourself but for your {scenario.Other_Person.Relationship}, their details are as follows:

NAME: {scenario.Other_Person.Name}
AGE: {scenario.Other_Person.Age}
GENDER: {scenario.Other_Person.Gender}


MEDICAL BACKGROUND:
- Medical History: {scenario.History}
- Current Symptoms: {scenario.Symptoms}
- Symptom Duration: {scenario.Time}
- Allergies and intolerances: {scenario.Allergies}
- Medical conditions and Regular Medications: {scenario.Medicines}
- Recently Added Medications: {scenario.AdditionalMeds}
- Pregnant: {scenario.Pregnant}
- Breastfeeding: {scenario.Breastfeeding}

CONTEXT: {scenario.Context}
ADDITIONAL INFO: {scenario.AdditionalInfo}
EMOTION: {scenario.Emotion}

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



        ChatMessage systemMessage = createSystemMessage(systemPrompt);
        chatHistory.Add(systemMessage);


    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && UI.sendButton.interactable)
        {
            SendChat();
        }

        if (isThinking && !isThinkingCoroutineRunning)
        {
            thinkingCoroutine = StartCoroutine(TypeText(UI.AIMessage, " \u25CF \u25CF \u25CF ", 0.15f));
        }
    }
    public async void SendChat(string voiceMessage = "")
    {
        string messageToSend = voiceMessage.Trim() ?? UI.KeyboardInput.text;
        if (voiceMessage.Length == 0) messageToSend = UI.KeyboardInput.text;

        if (messageToSend == null || messageToSend.Length == 0) return;

        UI.sendButton.interactable = false;
        UI.recordButton.interactable = false;
        UI.AIMessage.text = "";

        ChatMessage userMessage = createUserMessage(messageToSend);
        chatHistory.Add(userMessage);
        UI.userMessage.text = userMessage.Content;

        UI.KeyboardInput.text = "";


        isThinking = true;
        AIChatRespone response = await AIChatRequest(Config.ApiBaseUrl + "/api/aichat/" + scenario.AI.ToLower(), new CreateChatCompletionRequest
        {
            Model = scenario.Model,
            Messages = chatHistory
        });


        if (response?.Message != null)
        {
            await chatToVoice.TextToSpeech(response.Message);
            isThinking = false;
            StopCoroutine(thinkingCoroutine);
            isThinkingCoroutineRunning = false;
            UI.AIMessage.text = "";
            StartCoroutine(TypeText(UI.AIMessage, response.Message, typingSpeed));
            var assistantMessage = createAssistantMessage(response.Message);
            chatHistory.Add(assistantMessage);

        }
        else if (response?.Error != null)
        {
            isThinking = false;
            StopCoroutine(thinkingCoroutine);
            isThinkingCoroutineRunning = false;
            UI.AIMessage.text = "";
            UI.AIMessage.text = "Error:" + response.Error.Message;
            UI.ToggleButtonsOnError();


        }
        else
        {
            isThinking = false;
            StopCoroutine(thinkingCoroutine);
            isThinkingCoroutineRunning = false;
            UI.AIMessage.text = "";
            UI.AIMessage.text = "Error:" + "API Proxy Error";
            UI.ToggleButtonsOnError();

        }


    }

    public void saveToLog()
    {
        Config.ChatLog.WriteMessagesToChatLog(chatHistory);
    }



    ChatMessage createUserMessage(string content)
    {
        return new ChatMessage()
        {
            Role = "user",
            Content = content
        };

    }

    ChatMessage createAssistantMessage(string content)
    {
        return new ChatMessage()
        {
            Role = "assistant",
            Content = content
        };

    }

    ChatMessage createSystemMessage(string content)
    {
        return new ChatMessage()
        {
            Role = "system",
            Content = content
        };

    }

    IEnumerator TypeText(TextMeshProUGUI output, string newText, float typingSpeed)
    {
        output.text = "";
        isThinkingCoroutineRunning = true;
        foreach (char letter in newText)
        {
            output.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }
        isThinkingCoroutineRunning = false;
    }

    private async Task<AIChatRespone> AIChatRequest(string uri, CreateChatCompletionRequest chatRequest)
    {

        string body = JsonConvert.SerializeObject(chatRequest, jsonSerializerSettings);

        using (UnityWebRequest request = UnityWebRequest.Post(uri, body, "application/json"))
        {
            request.SetRequestHeader("authorization", $"Bearer {Config.AuthToken}");
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                return new AIChatRespone();
            }
            else
            {
                return JsonConvert.DeserializeObject<AIChatRespone>(request.downloadHandler.text);
            }
        }
    }

    private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CustomNamingStrategy()
        },
        Culture = CultureInfo.InvariantCulture
    };


}

class AIChatRespone
{
    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; }
    [JsonProperty(PropertyName = "error")]
    public ApiError Error { get; set; }
}