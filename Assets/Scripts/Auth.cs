using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Auth : MonoBehaviour
{
    public UIController UI;
    public bool isAuth = false;
    public string email;
    public string studentNum;
    public string authToken = null;



    public async Task LoginAsync()
    {
        UI.startButtonText.text = "Loading...";
        Credentials credentials = new(UI.studentNumInput.text, UI.passwordInput.text);
        authToken = await LoginRequest(credentials);
        this.studentNum = credentials.identifier;
        isAuth = authToken != null;
        UI.startButtonText.text = (!isAuth) ? "Try Again" : "Start";
    }



    private async Task<string> LoginRequest(Credentials credentials)
    {

        string body = JsonConvert.SerializeObject(credentials);


        using (UnityWebRequest request = UnityWebRequest.Post($"{Config.ApiBaseUrl}/api/auth/login", body, "application/json"))
        {

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                ResponseError errMessage = JsonConvert.DeserializeObject<ResponseError>(request.downloadHandler.text);
                StartCoroutine(UI.ErrorToast(errMessage.message, 5));
                return null;
            }
            else
            {
                return request.downloadHandler.text;
            }
        }
    }

    public void SetAuth(string token)
    {
        if (token.Length > 0)
        {
            authToken = token;
            Config.AuthToken = authToken;
            this.isAuth = true;
            Debug.Log($"Auth set");
        }

    }

    public void SetEmail(string email)
    {
        this.email = email;
    }


    public void SetStudentNum(string studentNum)
    {
        this.studentNum = studentNum;
    }

}



struct Credentials
{
    [JsonProperty(PropertyName = "identifier")]
    public string identifier;
    [JsonProperty(PropertyName = "password")]
    public string password;

    public Credentials(string studentNumInput, string passwordInput) : this()
    {
        identifier = studentNumInput;
        password = passwordInput;
    }
}

struct ResponseError
{
    [JsonProperty(PropertyName = "message")]
    public string message;

}