using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OpenAI;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;



public class AIChat : MonoBehaviour
{

    [SerializeField] private TMP_InputField input;
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private TextMeshProUGUI output;
    [SerializeField] private ChatToVoice chatToVoice;
    Button sendButton;
    Button recordButton;
    TMP_Dropdown apiOption;
    public float typingSpeed = 0.1f; // Speed of typing in seconds
    private List<ChatMessage> chatHistory = new List<ChatMessage>();

    // Start is called before the first frame update
    void Start()
    {
        sendButton = GameObject.Find("Send Button").GetComponent<Button>();
        recordButton = GameObject.Find("Record Button").GetComponent<Button>();
        apiOption = GameObject.Find("AI Dropdown").GetComponent<TMP_Dropdown>();
        string systemPrompt = "You are a 25 year old male named Dale. You are a patient in a pharmacy that is looking advice and you are talking to the pharmacist. you aren't feeling well and have the following symptons, headache, nausea, fever. you are frustrated. You are not a pharmacist. Do not offer any advice to the pharmacist. Do not break character. Do not disclose that you are an AI.";
        ChatMessage systemMessage = createSystemMessage(systemPrompt);
        chatHistory.Add(systemMessage);


    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && sendButton.interactable)
        {
            SendChat();
        }

    }
    public async void SendChat(string voiceMessage = null)
    {
        string messageToSend = voiceMessage ?? input.text;

        if (messageToSend == null || messageToSend.Length == 0) return;

        sendButton.interactable = false;
        recordButton.interactable = false;
        output.text = "";
        ChatMessage userMessage = createUserMessage(messageToSend);
        chatHistory.Add(userMessage);
        message.text = "Pharmacist: " + userMessage.Content;
        input.text = "";


        AIChatRespone response = null;

        if (apiOption.captionText.text.Equals("OpenAI GPT-4"))
        {
            response = await AIChatRequest("http://localhost:3030/api/aichat/openai", new CreateChatCompletionRequest
            {
                Model = "gpt-4o",
                Messages = chatHistory
            });

        }
        else if (apiOption.captionText.text.Equals("Claude 3.5"))
        {
            response = await AIChatRequest("http://localhost:3030/api/aichat/anthropic", new CreateChatCompletionRequest
            {
                Model = "claude-3-haiku-20240307",
                Messages = chatHistory
            });

        }

        if (response?.Message != null)
        {
            await chatToVoice.TextToSpeech(response.Message);
            StartCoroutine(TypeText(output, "Patient: " + response.Message));
            var assistantMessage = createAssistantMessage(response.Message);
            chatHistory.Add(assistantMessage);
        }
        else if (response?.Error != null)
        {
            output.text = "Error:" + response.Error.Message;
            chatToVoice.ToggleButtonsOnError();

        }
        else
        {
            output.text = "Error:" + "Error with response from API Proxy";
            chatToVoice.ToggleButtonsOnError();
        }
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

    IEnumerator TypeText(TextMeshProUGUI output, string newText)
    {
        foreach (char letter in newText)
        {
            output.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

    }

    private async Task<AIChatRespone> AIChatRequest(string uri, CreateChatCompletionRequest chatRequest)
    {

        string body = JsonConvert.SerializeObject(chatRequest, jsonSerializerSettings);

        using (UnityWebRequest request = UnityWebRequest.Post(uri, body, "application/json"))
        {
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