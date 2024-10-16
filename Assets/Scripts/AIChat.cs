using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OpenAI;
using System;
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

    public float typingSpeed = 0.1f; // Speed of typing in seconds



    private List<ChatMessage> chatHistory = new List<ChatMessage>();
    // Start is called before the first frame update
    void Start()
    {
        sendButton = GameObject.Find("Send Button").GetComponent<Button>();
        recordButton = GameObject.Find("Record Button").GetComponent<Button>();
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
    public async void SendChat()
    {
        if (input.text == null || input.text.Length == 0)
        {
            Console.WriteLine("Input box empty");
            return;
        }

        sendButton.interactable = false;
        recordButton.interactable = false;
        output.text = "";
        ChatMessage userMessage = createUserMessage(input.text);
        chatHistory.Add(userMessage);
        message.text = "Pharmacist: " + input.text;
        input.text = "";


        var response = await OpenAIChatRequest("http://localhost:3030/api/aichat/openai", new CreateChatCompletionRequest
        {
            Model = "gpt-4o",
            Messages = chatHistory
        });
        if (response.Error == null)
        {
            await chatToVoice.TextToSpeech(response.Choices[0].Message.Content.Replace("\n", ""));
            StartCoroutine(TypeText(output, "Patient: " + response.Choices[0].Message.Content));
            var assistantMessage = createAssistantMessage(response.Choices[0].Message.Content);
            chatHistory.Add(assistantMessage);
        }
        else
        {
            output.text = "Error:" + response.Error.Message;
            chatToVoice.ToggleButtonsOnError();

        }
    }

    public async void SendChatFromVoice(string voiceMessage)
    {

        if (voiceMessage == null || voiceMessage.Length == 0)
        {
            Console.WriteLine("Voice message was silent");
            return;
        }

        sendButton.interactable = false;
        recordButton.interactable = false;

        output.text = "";
        ChatMessage userMessage = createUserMessage(voiceMessage);
        chatHistory.Add(userMessage);
        message.text = "Pharmacist: " + voiceMessage;

        // var response = await openai.CreateChatCompletion(new CreateChatCompletionRequest
        // {
        //     Model = "gpt-4o",
        //     Messages = chatHistory
        // });
        var response = await OpenAIChatRequest("http://localhost:3030/api/aichat/openai", new CreateChatCompletionRequest
        {
            Model = "gpt-4o",
            Messages = chatHistory
        });

        if (response.Error == null)
        {
            await chatToVoice.TextToSpeech(response.Choices[0].Message.Content.Replace("\n", ""));
            StartCoroutine(TypeText(output, "Patient: " + response.Choices[0].Message.Content));
            var assistantMessage = createAssistantMessage(response.Choices[0].Message.Content);
            chatHistory.Add(assistantMessage);
        }
        else
        {
            output.text = "Error:" + response.Error.Message;
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

    private async Task<CreateChatCompletionResponse> OpenAIChatRequest(string uri, CreateChatCompletionRequest chatRequest)
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
                return new CreateChatCompletionResponse();
            }
            else
            {
                return JsonConvert.DeserializeObject<CreateChatCompletionResponse>(request.downloadHandler.text);
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
