using UnityEngine;
using System.IO;
using OpenAI;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;
using WebGLAudioData;

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
    private bool isSpeaking = false;
    private bool hasSpoke = false;
    private bool isListening = false;
    private bool thresholdSet = false;
    private bool isPushingToTalk = false;

    private SpeechDetector speechDetector;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        speechDetector = GetComponent<SpeechDetector>();
    }
    void Start()
    {
        UI.recordButton.onClick.AddListener(ToggleListening);

    }

    private void Update()
    {
        if (!thresholdSet)
        {
            SetSpeechThreshold();
        }

        // Check for the push-to-talk key (Space key)
        if (Input.GetKeyDown(KeyCode.Space) && UI.recordButton.gameObject.activeSelf && UI.recordButton.interactable)
        {
            isPushingToTalk = true;
            ToggleListening(); // Start listening when the key is pressed
        }

        if (Input.GetKeyUp(KeyCode.Space) && UI.recordButton.gameObject.activeSelf && UI.recordButton.interactable)
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

        if (!isRecording && !hasSpoke && isListening && UI.recordButton.interactable)
        {
            StartRecording();
        }

        if ((isRecording && hasSpoke && !isSpeaking && !isPushingToTalk) || (isRecording && Microphone.GetPosition("") >= clip.samples))
        {

            StopRecording();
        }

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

    private void SetSpeechThreshold()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SpeechDetector.checkForSpeech(250);
        if(speechDetector.maxVolume > -144.0f)
        {
            thresholdDb = speechDetector.maxVolume + 30; 
            Debug.Log($"Threshold is now set to : {thresholdDb}");
            thresholdSet = true;
            SpeechDetector.StopListening();
        }
#else
        thresholdDb = -80;
        Debug.Log($"Threshold is now set to : {thresholdDb}");
        thresholdSet = true;
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
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
            SpeechDetector.checkForSpeech(250);
            currentVolumeDb = speechDetector.maxVolume;
#endif

        // Debug.Log($"From unity: {currentVolumeDb}");
        if (currentVolumeDb > thresholdDb && isRecording)
        {
            isSpeaking = true;
            hasSpoke = true;
        }
        else
        {
            isSpeaking = false;
        }
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
        isRecording = true;
        audioSource.clip = clip;
        audioSource.Play();
    }

    private async void StopRecording()
    {
        UI.recordButton.animator.SetBool("isRecording", false);


        int position = Microphone.GetPosition("");
        Microphone.End(null);
        isRecording = false;

        if (hasSpoke)
        {
            UI.sendButton.interactable = false;
            UI.recordButton.interactable = false;
            float[] samples = new float[position * clip.channels];
            clip.GetData(samples, 0);
            hasSpoke = false;
            bytes = await EncodeAsWAV(samples, clip.frequency, clip.channels);
            var res = await OpenAITranscriptionRequest(bytes);
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


        using (UnityWebRequest request = UnityWebRequest.Put(Config.ApiBaseUrl+"/api/stt/huggingface", audio))
        {
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
