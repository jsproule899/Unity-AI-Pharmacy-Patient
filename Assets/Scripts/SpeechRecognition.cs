using UnityEngine;
using System.IO;
using OpenAI;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;
using WebGLAudioData;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;


public class SpeechRecognition : MonoBehaviour
{
    public UIController UI;
    [SerializeField] private AIChat aIChat;
    private AudioClip clip;
    private byte[] bytes;
    private bool isRecording = false;
    private AudioSource audioSource;
    private float[] clipSampleData = new float[8192];
    private float thresholdDb = -80f;
    private bool isSetThresholdRunning = false;
    private float[] speechHistory = new float[3];
    private int speechIndex = 0;
    private bool isSpeaking = false;
    private bool hasSpoke = false;
    private bool isListening = false;
    private bool thresholdSet = false;
    private bool isPushingToTalk = false;
    private bool isThinking = false;
    private bool isThinkingCoroutineRunning = false;
    private Coroutine thinkingCoroutine;
    private Coroutine holdDetectionCoroutine;
    private float holdThreshold = 0.5f; // Time in seconds to consider a hold

    private SpeechDetector speechDetector;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        speechDetector = GetComponent<SpeechDetector>();
    }
    void Start()
    {

        //Workaround for Safari Microphone bug
        Microphone.GetPosition(null);
        // StartCoroutine(SafariMicrophoneInitialise());

    }

    private void Update()
    {
        if (!thresholdSet && !isSetThresholdRunning)
        {
            isSetThresholdRunning = true;
            StartCoroutine(SetSpeechThreshold());
        }

        if (isThinking && !isThinkingCoroutineRunning)
        {
            thinkingCoroutine = StartCoroutine(TranscriptionPendingText(UI.userMessage, " \u25CF \u25CF \u25CF ", 0.15f));
        }

        // Check for the push-to-talk key (Space key)
        if (Input.GetKeyDown(KeyCode.Space) && UI.recordButton.gameObject.activeSelf && UI.recordButton.interactable)
        {
            StartPushToTalk();
        }

        if (Input.GetKeyUp(KeyCode.Space) && UI.recordButton.gameObject.activeSelf && UI.recordButton.interactable)
        {
            StopPushToTalk();
        }

        if (!isRecording && !hasSpoke && isListening && UI.recordButton.interactable)
        {
            StartRecording();
        }

        if ((isRecording && hasSpoke && !isSpeaking && !isPushingToTalk) || (isRecording && clip != null && Microphone.GetPosition(null) >= clip.samples))
        {
            StopRecording();
        }

    }


    public void OnRecordButtonPointerDown()
    {
        // Start checking for hold
        if(!UI.recordButton.interactable) return;
        holdDetectionCoroutine = StartCoroutine(HoldDetection());
        
    }

    private IEnumerator HoldDetection()
    {
        yield return new WaitForSeconds(holdThreshold);

        // If this coroutine completes, it's a hold: start push-to-talk
        StartPushToTalk();
        holdDetectionCoroutine = null;
    }

    public void OnRecordButtonPointerUp()
    {
         if(!UI.recordButton.interactable) return;
        if (holdDetectionCoroutine != null)
        {
            // Hold detection was not completed: treat as click
            StopCoroutine(holdDetectionCoroutine);
            holdDetectionCoroutine = null;
            ToggleListening();
        }
        else
        {
            // Hold was detected: stop push-to-talk
            StopPushToTalk();
        }
    }

    private void StartPushToTalk()
    {
        isPushingToTalk = true;
        ToggleListening(); // Start listening when the key is pressed
    }

    private void StopPushToTalk()
    {
        isPushingToTalk = false;
        // Stop listening when the key is released
        if (isListening)
        {
            hasSpoke = true;
            isSpeaking = false;
            ToggleListening();
        }
    }

    //workaround for safari webgl microphone bug
    private IEnumerator SafariMicrophoneInitialise()
    {
        clip = Microphone.Start(null, false, 5, 44100);
        yield return new WaitForSeconds(1);
        Microphone.End(null);
        isRecording = false;
    }

    IEnumerator TranscriptionPendingText(TextMeshProUGUI output, string newText, float typingSpeed)
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

    private float LinearToDecibel(float linear)
    {
        float dB;
        if (linear != 0)
            dB = 20.0f * Mathf.Log10(linear);
        else
            dB = -144.0f;
        return dB;
    }

    private float DecibelToLinear(float dB)
    {
        float linear = Mathf.Pow(10.0f, dB / 20.0f);
        return linear;
    }

    private void ToggleListening()
    {
        if (isListening)
        {
            CancelInvoke("ListeningForSpeech");
        }
        else
        {
            InvokeRepeating("ListeningForSpeech", 0.5f, 0.5f);
        }
        isListening = !isListening;
        toggleRecording();
    }

    private IEnumerator SetSpeechThreshold()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        
    List<float> maxVolumeReadings = new List<float>();
    int numReadings = 3;
    float delayBetweenReadings = 0.5f;

    for (int i = 0; i < numReadings; i++)
    {
        SpeechDetector.checkForSpeech(250);  
        maxVolumeReadings.Add(speechDetector.maxVolume);

        
        yield return new WaitForSeconds(delayBetweenReadings);
    }

    
    float averageMaxVolume = maxVolumeReadings.Average();
        if(averageMaxVolume > -144.0f)
        {
            thresholdDb = averageMaxVolume + 20; 
            Debug.Log($"Threshold is now set to : {thresholdDb}");
            thresholdSet = true;
            SpeechDetector.StopListening();
        }else{
        Debug.Log("Average maxVolume is too low to set threshold.");
        }
        isSetThresholdRunning = false;
#else
        thresholdDb = -80;
        Debug.Log($"Threshold is now set to : {thresholdDb}");
        thresholdSet = true;
        isSetThresholdRunning = false;
        yield break;
#endif

    }


    private void ListeningForSpeech()
    {
        if (!isListening)
        {

#if UNITY_WEBGL && !UNITY_EDITOR
            SpeechDetector.StopListening();
#endif
            return;
        }
        float currentVolumeDb;
#if UNITY_EDITOR
        audioSource.GetSpectrumData(clipSampleData, 0, FFTWindow.Rectangular);
        currentVolumeDb = LinearToDecibel(clipSampleData.Average());
        setSpeechHistory(currentVolumeDb);
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
            SpeechDetector.checkForSpeech(250);
            setSpeechHistory(speechDetector.maxVolume);

            //currentVolumeDb = speechDetector.maxVolume;
#endif
        float averageSpeechHistory = speechHistory.Where(x => x != 0).Average();
        Debug.Log($@"Average volume: {averageSpeechHistory}");
        if (averageSpeechHistory > thresholdDb && isRecording)
        {
            isSpeaking = true;
            hasSpoke = true;
        }
        else
        {
            isSpeaking = false;
        }
    }

    private void setSpeechHistory(float volume)
    {
        speechHistory[speechIndex] = volume;
        speechIndex = (speechIndex + 1) % speechHistory.Length;
    }


    private void toggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }


    private void StartRecording()
    {
        if (isRecording) return;
        UI.recordButton.animator.SetBool("isRecording", true);

        clip = Microphone.Start(null, false, 240, 44100);
        audioSource.clip = clip;
        audioSource.Play();
        isRecording = true;
    }

    private async void StopRecording()
    {
        UI.recordButton.animator.SetBool("isRecording", false);


        int position = Microphone.GetPosition("");
        Microphone.End(null);
        isRecording = false;

        if (hasSpoke && clip != null)
        {
            UI.sendButton.interactable = false;
            UI.recordButton.interactable = false;
            isThinking = true;
            float[] samples = new float[position * clip.channels];
            clip.GetData(samples, 0);
            hasSpoke = false;
            bytes = await EncodeAsWAV(samples, clip.frequency, clip.channels);
            var res = await OpenAITranscriptionRequest(bytes);
            isThinking = false;
            StopCoroutine(thinkingCoroutine);
            isThinkingCoroutineRunning = false;
            if (res.Error != null)
            {

                UI.AIMessage.text = "Cannot connect to transcription API";
                UI.ToggleButtonsOnError();
                return;
            }
            aIChat.SendChat(res.Text);
        }
    }


    private Task<byte[]> EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * channels * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }

            return Task.FromResult(memoryStream.ToArray());
        }
    }

    private async Task<CreateAudioResponse> OpenAITranscriptionRequest(byte[] audio)
    {
        CreateAudioResponse response;


        using (UnityWebRequest request = UnityWebRequest.Put($"{Config.ApiBaseUrl}/api/stt/{Config.Scenario.STT ?? "openai"}", audio))
        {
            request.SetRequestHeader("authorization", $"Bearer {Config.AuthToken}");
            request.method = "POST";
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                response = new CreateAudioResponse();
                response.Error = new ApiError();
                response.Error.Message = request.error;
                return response;
            }
            else
            {
                response = JsonConvert.DeserializeObject<CreateAudioResponse>(request.downloadHandler.text);
                return response;
            }
        }
    }
}

public struct CreateAudioResponse : IResponse
{
    public ApiError Error { get; set; }

    public string Warning { get; set; }

    public string Text { get; set; }
}
