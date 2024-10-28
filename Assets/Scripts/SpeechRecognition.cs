using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using OpenAI;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;


public class SpeechRecognition : MonoBehaviour
{

    private Button recordButton;
    private Button sendButton;
    private TextMeshProUGUI recordButtonText;
    [SerializeField] private AIChat aIChat;
    private AudioClip clip;
    private byte[] bytes;
    private bool recording;
    private AudioSource audioSource;
    private float[] clipSampleData = new float[512];
    private float thresholdDb = -100;
    private bool isSpeaking = false;
    private bool spoke = false;
    private bool isListening = false;


    void Start()
    {
        recordButton = GameObject.Find("Record Button").GetComponent<Button>();
        sendButton = GameObject.Find("Send Button").GetComponent<Button>();
        recordButton.onClick.AddListener(ToggleListening);
        recordButtonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
        InvokeRepeating("ListeningForSpeech",0.1f, 0.5f);


    }

    private void Update()
    {
        if (recording && Microphone.GetPosition("") >= clip.samples)
        {
            StopRecording();
        }
        



        if (recording && spoke && !isSpeaking)
        {
            StopRecording();
        }

        if (!recording && !spoke && isListening && recordButton.interactable)
        {
            StartRecording();
        }


    }

    // private IEnumerator SetThresholdDb()
    // {
    //     yield return new WaitForSeconds(1);
    //     audioSource.GetSpectrumData(clipSampleData, 0, FFTWindow.Rectangular);
    //     float currentAverageVolumeDb = LinearToDecibel(clipSampleData.Average());
    //     thresholdDb = currentAverageVolumeDb;
    //     Debug.Log("DB Threshold set: " + thresholdDb);

    // }

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
        isListening = !isListening;
        toggleRecording();
    }

    private void ListeningForSpeech()
    {

       if (!isListening) return;

        audioSource.GetSpectrumData(clipSampleData, 0, FFTWindow.Rectangular);
        float currentAverageVolumeDb = LinearToDecibel(clipSampleData.Average());
        if (currentAverageVolumeDb > thresholdDb)
        {
            Debug.Log("Speaking");
            isSpeaking = true;
            spoke = true;
        }
        else
        {
            // Debug.Log(currentAverageVolumeDb);
            Debug.Log("Silent");
            isSpeaking = false;

        }
    }


    private void toggleRecording()
    {
        if (recording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    private void setButtonColor(string colour)
    {

        if (colour.Equals("red"))
        {
            recordButton.image.color = new Color32(255, 100, 100, 255);
        }
        else if (colour.Equals("white"))
        {
            recordButton.image.color = new Color32(255, 255, 255, 255);
        }

    }
    private void StartRecording()
    {

        setButtonColor("red");
        recordButtonText.text = "Listening...";
        clip = Microphone.Start(null, false, 240, 44100);
        audioSource.clip = clip;
        audioSource.Play();
        isListening = true;
        recording = true;

    }

    private async void StopRecording()
    {
        recordButtonText.text = "Speak";
        setButtonColor("white");
       

        int position = Microphone.GetPosition("");
        Microphone.End(null);
        recording = false;
        if (spoke)
        {
            float[] samples = new float[position * clip.channels];
            clip.GetData(samples, 0);
            bytes = await EncodeAsWAV(samples, clip.frequency, clip.channels);
            var res = await OpenAITranscriptionRequest(bytes);
            aIChat.SendChat(res.Text);
        }
        spoke = false;
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


        using (UnityWebRequest request = UnityWebRequest.Put("http://localhost:3030/api/stt/huggingface", audio))
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
                return response = new CreateAudioResponse();
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
