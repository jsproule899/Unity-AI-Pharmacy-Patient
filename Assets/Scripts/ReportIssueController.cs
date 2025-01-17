using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSBrowserUtilities;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReportIssueController : MonoBehaviour
{

    public ModalSystem modal;
    private static string url = $"{Config.ApiBaseUrl}/api/issue";



    public void Update()
    {
        string inputText = modal.GetInputs()[0].text;
        if (inputText == null || inputText.Length == 0)
        {
            modal.RecursiveFindChild(modal.transform, "Confirm Button").gameObject.GetComponent<Button>().interactable = false;

        }
        else
        {
            modal.RecursiveFindChild(modal.transform, "Confirm Button").gameObject.GetComponent<Button>().interactable = true;
        }
    }


    public static void HandleSubmit(ModalSystem modal)
    {
        ReportedIssue issue = new ReportedIssue();
#if UNITY_WEBGL && !UNITY_EDITOR
        string[] urlPath = BrowserHelper.JS_GetUrlPath().Split("/");
        issue.ScenarioId = urlPath.Last();
#endif
        int i = modal.GetDropdowns()[0].value;
        issue.Category = modal.GetDropdowns()[0].options[i].text;
        issue.Details = modal.GetInputs()[0].text;
        _ = PostIssue(issue);
    }
    private static async Task PostIssue(ReportedIssue issue)
    {


        string body = JsonConvert.SerializeObject(issue);

        using (UnityWebRequest request = UnityWebRequest.Post(url, body, "application/json"))
        {
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);

            }
            Debug.Log("Issue logged");

        }
    }

}

public struct ReportedIssue
{
    [JsonProperty(PropertyName = "Scenario")]
    public string ScenarioId { get; set; }
    [JsonProperty(PropertyName = "Category")]
    public string Category { get; set; }
    [JsonProperty(PropertyName = "Details")]
    public string Details { get; set; }
}
