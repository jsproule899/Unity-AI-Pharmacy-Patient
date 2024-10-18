using UnityEngine;
using UnityEngine.Networking;
using ReadyPlayerMe.Core;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

public class ChatToVoice : MonoBehaviour
{
    [SerializeField] GameObject avatar;
    Button recordButton;
    Button sendButton;
    TMP_Dropdown voiceAPIOption;

    AudioSource avatarAudio;
    VoiceHandler avatarVoiceHandler;
    private bool hasPlayed = false;
    void Start()
    {
        recordButton = GameObject.Find("Record Button").GetComponent<Button>();
        sendButton = GameObject.Find("Send Button").GetComponent<Button>();
        voiceAPIOption = GameObject.Find("AI Voice Dropdown").GetComponent<TMP_Dropdown>();
        avatarAudio = avatar.GetComponent<AudioSource>();
        avatarVoiceHandler = avatar.GetComponent<VoiceHandler>();

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
            recordButton.interactable = false;
            sendButton.interactable = false;
            hasPlayed = true;
        }
        else if (hasPlayed && !avatarAudio.isPlaying)
        {
            recordButton.interactable = true;
            sendButton.interactable = true;
            hasPlayed = false;
        }

    }

    public void ToggleButtonsOnError()
    {
        recordButton.interactable = !recordButton.interactable;
        sendButton.interactable = !sendButton.interactable;
    }

    public async Task<bool> TextToSpeech(string text)
    {
        string audioURI = null;
        AudioClip responseAudio = null;

        if (voiceAPIOption.captionText.text.Equals("Eleven Labs"))
        {
            string voice = "IKne3meq5aSn9XLyUdCD";
            string body = $@"{{""text"": ""{text}"", ""voice"": ""{voice}""}}";
            responseAudio = await VoicePostRequest("http://localhost:3030/api/tts/elevenlabs/", body);

        }
        else if (voiceAPIOption.captionText.text.Equals("Unreal Speech"))
        {
            string body = @$"{{""Text"": ""{text}"", ""VoiceId"": ""Dan""}}";
            responseAudio = await VoicePostRequest("http://localhost:3030/api/tts/unrealspeech/stream", body);
            // audioURI =  await UnrealSpeechUriRequest("http://localhost:3030/api/tts/unrealspeech/speech", body);
        }

        if (audioURI != null)
        {
            responseAudio = await LoadAudio(audioURI);
        }

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
            request.downloadHandler = new DownloadHandlerAudioClip(uri, AudioType.MPEG);

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                ToggleButtonsOnError();
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
            request.downloadHandler = new DownloadHandlerAudioClip(uri, AudioType.MPEG);

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                ToggleButtonsOnError();
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
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Audio error:" + request.error);
                ToggleButtonsOnError();
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
