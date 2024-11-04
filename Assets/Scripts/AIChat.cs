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

    private Scenario scenario;

    // Start is called before the first frame update
    void Start()
    {
        sendButton = GameObject.Find("Send Button").GetComponent<Button>();
        recordButton = GameObject.Find("Record Button").GetComponent<Button>();
        apiOption = GameObject.Find("AI Dropdown").GetComponent<TMP_Dropdown>();
        scenario = Config.Scenario;
        apiOption.captionText.text = scenario.AI;
        string path = Application.persistentDataPath + "/chatlog.txt";

        // string systemPrompt = "You are a 25 year old male named Dale. You are a patient in a pharmacy that is looking advice and you are talking to the pharmacist. you aren't feeling well and have the following symptons, headache, nausea, fever. you are frustrated. You are not a pharmacist. Do not offer any advice to the pharmacist. Do not break character. Do not disclose that you are an AI.";
        string systemPrompt = @$"You are roleplaying as the patient in a pharmacy. The context of the roleplay is {scenario.Context}
                                    Here are the rules for the roleplay: 
                                    1. Chat exclusively as {scenario.Name} and do not add any actions or reactions to your replies, respond in plain text.
                                    2. Do not break character or disclose that you are AI.
                                    3. I am roleplaying as the pharmacist.
                                    4. Do not provide any information about yourself unless directly asked about it.
                                    5. Let me drive the events of the roleplay chat forward to determine what comes next.
                                    6. Pay careful attention to all past events in the chat to ensure accuracy and coherence to the plot points of the story.
                                            
                                    This is the information about your character:
                                    Your name is {scenario.Name}, you are a {scenario.Age} year old {scenario.Gender}. The history of your illness is {scenario.History} and you have the following symptoms: {scenario.Symptoms} and they started {scenario.Time} ago. You have the following allergies: {scenario.Allergies}
                                    You take {scenario.Medicines} as routine medication. Additional medication taken: {scenario.AdditionalMeds}";


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
    public async void SendChat(string voiceMessage = "")
    {
        string messageToSend = voiceMessage ?? input.text;
        if (voiceMessage.Length == 0) messageToSend = input.text;

        if (messageToSend == null || messageToSend.Length == 0) return;

        sendButton.interactable = false;
        recordButton.interactable = false;
        output.text = "";
        ChatMessage userMessage = createUserMessage(messageToSend);
        chatHistory.Add(userMessage);
        message.text = "Pharmacist: " + userMessage.Content;
        
        input.text = "";
            


        AIChatRespone response = null;

        if (apiOption.captionText.text.Equals("OpenAI"))
        {
            response = await AIChatRequest("http://localhost:3030/api/aichat/openai", new CreateChatCompletionRequest
            {
                Model = scenario.Model,
                Messages = chatHistory
            });

        }
        else if (apiOption.captionText.text.Equals("Claude"))
        {
            response = await AIChatRequest("http://localhost:3030/api/aichat/anthropic", new CreateChatCompletionRequest
            {
                Model = scenario.Model,
                Messages = chatHistory
            });

        }
        input.Select();

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
            output.text = "Error:" + "API Proxy Error";
            chatToVoice.ToggleButtonsOnError();
            
        }

    }

    public void saveToLog(){
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