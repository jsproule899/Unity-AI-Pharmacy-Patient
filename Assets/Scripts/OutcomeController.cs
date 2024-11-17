using System;
using System.Collections;
using System.Collections.Generic;
using JSBrowserUtilities;
using ReadyPlayerMe.Core.WebView;
using TMPro;
using UnityEngine;

public class OutcomeController : MonoBehaviour
{
    public ModalSystem modal;
    public GameObject OutcomeResult;

    public string outcome { get; set; }



    public void HandleSubmit()
    {

        ShowOutcomeResult();
        saveToLog();
        #if UNITY_WEBGL
        string timestamp = DateTime.Now.ToString("yyyyddMM_HHmmss");
        BrowserHelper.JS_TextFile_CreateBlob($"{Config.Student.Id}_chatlog_{timestamp}");
        StartCoroutine(DelayedRedirect(2));
        #endif
    }


    public void ShowOutcomeResult()
    {
        OutcomeResult.gameObject.SetActive(true);

        if (String.Equals(outcome, Config.Scenario.Outcome, StringComparison.OrdinalIgnoreCase))
        {
            OutcomeResult.transform.Find("Correct").gameObject.SetActive(true);
        }
        else
        {
            OutcomeResult.transform.Find("Incorrect").gameObject.SetActive(true);
        }

    }

    public void saveToLog()
    {
        TMP_InputField[] inputs = modal.GetInputs();

        switch (inputs.Length)
        {
            case 1:
                Config.ChatLog.WriteOutcomeToChatLog(outcome, inputs[0].text);
                break;
            case 2:
                Config.ChatLog.WriteOutcomeToChatLog(outcome, inputs[0].text, inputs[1].text);
                break;
            case 3:
                Config.ChatLog.WriteOutcomeToChatLog(outcome, inputs[0].text, inputs[1].text, inputs[2].text);
                break;
            default:
                Config.ChatLog.WriteOutcomeToChatLog(outcome);
                break;
        }


    }

    public IEnumerator DelayedRedirect(float delayInSeconds)
    {
        yield return new WaitForSeconds(delayInSeconds);

        string url = BrowserHelper.JS_GetBaseUrl();
        BrowserHelper.JS_Redirect(url + "/scenario");
    }
}
