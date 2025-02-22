using UnityEngine;
using UnityEngine.Networking;
using ReadyPlayerMe.Core;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

public class ChatToVoice : MonoBehaviour
{
    public UIController UI;
    AudioSource avatarAudio;
    VoiceHandler avatarVoiceHandler;
    private bool hasPlayed = false;

    private Scenario scenario;
    void Start()
    {

        avatarAudio = Config.Avatar.GetComponent<AudioSource>();
        avatarVoiceHandler = Config.Avatar.GetComponent<VoiceHandler>();
        scenario = Config.Scenario;

    }

    // Update is called once per frame
    void Update()
    {
        ToggleButtonsProcessingMessage();

    }

    private void ToggleButtonsProcessingMessage()
    {
        if (avatarAudio.isPlaying)
        {
            Config.Avatar.GetComponent<Animator>().SetBool("isTalking", true);
            UI.recordButton.interactable = false;
            UI.sendButton.interactable = false;
            hasPlayed = true;
        }
        else if (hasPlayed && !avatarAudio.isPlaying)
        {
            Config.Avatar.GetComponent<Animator>().SetBool("isTalking", false);
            UI.recordButton.interactable = true;
            UI.sendButton.interactable = true;
            hasPlayed = false;

        }

    }


    public async Task<bool> TextToSpeech(string text)
    {
        string audioURI = null;
        AudioClip responseAudio = null;

        string apiRoute = "/api/tts/" + scenario.TTS.Replace(" ", "").ToLower();
        VoiceRequest body = new VoiceRequest { Text = text, Voice = scenario.Voice, Mode="stream"};
        responseAudio = await VoicePostRequest(Config.ApiBaseUrl + apiRoute, JsonConvert.SerializeObject(body));

        if (audioURI != null)
        {
            responseAudio = await LoadAudio(audioURI);
        }

        if (UI.KeyboardInput.gameObject.activeSelf)
            UI.KeyboardInput.Select();

        return PlayAvatarVoiceClip(responseAudio);

    }

    private bool PlayAvatarVoiceClip(AudioClip voiceClip)
    {
        if (voiceClip != null)
        {
            avatarAudio.clip = voiceClip;
            avatarVoiceHandler.AudioClip = voiceClip;
            avatarVoiceHandler.PlayCurrentAudioClip();
            return true;
        }
        return false;
    }



    private async Task<AudioClip> VoicePostRequest(string uri, string body)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(uri, body, "application/json"))
        {
            request.SetRequestHeader("authorization", $"Bearer {Config.AuthToken}");
            request.downloadHandler = new DownloadHandlerAudioClip(uri, AudioType.MPEG);

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                UI.ToggleButtonsOnError();
                return null;
            }
            else
            {
                return ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;

            }
        }
    }

    private async Task<string> UnrealSpeechUriRequest(string uri, string body)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(uri, body, "application/json"))
        {
            request.SetRequestHeader("authorization", $"Bearer {Config.AuthToken}");
            request.downloadHandler = new DownloadHandlerAudioClip(uri, AudioType.MPEG);

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                UI.ToggleButtonsOnError();
                return null;
            }
            else
            {
                string json = request.downloadHandler.text;
                UnrealSpeechResponse res = JsonUtility.FromJson<UnrealSpeechResponse>(json);
                return res.OutputUri;

            }
        }
    }


    public async Task<AudioClip> LoadAudio(string url)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            request.SetRequestHeader("authorization", $"Bearer {Config.AuthToken}");
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Audio error:" + request.error);
                UI.ToggleButtonsOnError();
                return null;
            }
            else
            {
                return ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
            }
        }
    }
}


public struct UnrealSpeechResponse
{
    public string OutputUri;
}


public struct VoiceRequest
{
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("voice")]
    public string Voice { get; set; }

    [JsonProperty("mode")]
    public string Mode { get; set; }


}
