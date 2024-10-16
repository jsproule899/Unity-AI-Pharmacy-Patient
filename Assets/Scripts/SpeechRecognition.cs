using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using OpenAI;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class SpeechRecognition : MonoBehaviour
{
    
    private Button recordButton;
    private Button sendButton;
    private TextMeshProUGUI recordButtonText;
    [SerializeField] private AIChat aIChat;
    private AudioClip clip;
    private byte[] bytes;
    private bool recording;
    
    void Start()
    {
        recordButton = GameObject.Find("Record Button").GetComponent<Button>();
        sendButton = GameObject.Find("Send Button").GetComponent<Button>();
        recordButton.onClick.AddListener(toggleRecording);
        recordButtonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (recording && Microphone.GetPosition("") >= clip.samples)
        {
            StopRecording();
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
        else if(colour.Equals("white"))
        {
            recordButton.image.color = new Color32(255, 255, 255, 255);
        }

    }
    private void StartRecording()
    {
        setButtonColor("red");
        recordButtonText.text = "Recording...";
        clip = Microphone.Start(null, false, 240, 44100);
        recording = true;

    }

    private async void StopRecording()
    {
        recordButtonText.text = "Start Recording";
        setButtonColor("white");
        recordButton.interactable = false;
        sendButton.interactable = false;

        int position = Microphone.GetPosition("");
        Microphone.End(null);

        float[] samples = new float[position * clip.channels];

        clip.GetData(samples, 0);

        bytes = await EncodeAsWAV(samples, clip.frequency, clip.channels);
        
        var res = await OpenAITranscriptionRequest(bytes);
        aIChat.SendChatFromVoice(res.Text);
        recording = false;

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
