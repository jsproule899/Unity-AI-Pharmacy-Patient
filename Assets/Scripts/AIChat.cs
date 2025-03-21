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
        ChatMessage systemMessage = createSystemMessage(Config.Prompt);
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

    public void SaveToLog()
    {
        foreach (ChatMessage message in chatHistory)
        {
            switch (message.Role)
            {
                case "assistant":
                    Config.Conversation += "Patient: ";
                    Config.Conversation += message.Content + "\n";
                    break;
                case "user":
                    Config.Conversation += "Pharmacist: ";
                    Config.Conversation += message.Content + "\n";
                    break;
            }
        }

        Config.ChatLog.WriteMessagesToChatLog();
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