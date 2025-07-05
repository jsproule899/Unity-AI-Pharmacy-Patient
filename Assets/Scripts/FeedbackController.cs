using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FeedbackController : MonoBehaviour
{

    public UIController UI;
    private bool isGenerating;
    private bool isGeneratingCoroutineRunning;

    private Coroutine generatingCoroutine;
    private Scenario scenario;
    // Start is called before the first frame update

    void Start()
    {
        scenario = Config.Scenario;
        GenerateFeedback();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGeneratingCoroutineRunning && UI.feedbackScrollbar.IsActive())
        {     
            UI.feedbackScrollbar.value = 0;
        }

        if (isGenerating && !isGeneratingCoroutineRunning)
        {
            generatingCoroutine = StartCoroutine(TypeText(UI.AIFeedback, " \u25CF \u25CF \u25CF ", 0.15f));
        }

    }

    private async void GenerateFeedback()
    {
        isGenerating = true;
        AIChatRespone response = await AIChatRequest(Config.ApiBaseUrl + "/api/aichat/" + scenario.AI.ToLower(), new CreateChatCompletionRequest
        {
            Model = scenario.Model,
            Messages = new(){new ChatMessage(){
                Role = "system",
                Content = $"give the student pharmacist some brief feedback on this interaction, aligning with UK pharmacy practices. Similar to how a lecturer might give feedback. Do not offer to elaborate. Provide the feedback only and don't introduce the task."

            }, new ChatMessage(){
                Role = "user",
                Content = $"Scenario: {scenario.Context} \n{Config.Conversation}"
            }}
        });


        if (response?.Message != null)
        {
            isGenerating = false;
            StopCoroutine(generatingCoroutine);
            isGeneratingCoroutineRunning = false;
            UI.AIFeedback.text = "";
            StartCoroutine(TypeText(UI.AIFeedback, response.Message, 0.01f));
        }
        else if (response?.Error != null)
        {
            isGenerating = false;
            StopCoroutine(generatingCoroutine);
            isGeneratingCoroutineRunning = false;
            UI.AIFeedback.text = "";
            UI.AIFeedback.text = "Error:" + response.Error.Message;
        }
        else
        {
            isGenerating = false;
            StopCoroutine(generatingCoroutine);
            isGeneratingCoroutineRunning = false;
            UI.AIFeedback.text = "";
            UI.AIFeedback.text = "Error:" + "API Proxy Error";
        }
    }

    IEnumerator TypeText(TextMeshProUGUI output, string newText, float typingSpeed)
    {
        output.text = "";
        isGeneratingCoroutineRunning = true;
        foreach (char letter in newText)
        {
            output.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }
        isGeneratingCoroutineRunning = false;
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


