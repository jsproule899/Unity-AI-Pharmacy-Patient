using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;


public class Scenario
{
    public string _id { get; set; }

    public string Theme { get; set; }
    public string Context { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }
    public bool Self { get; set; }
    public OtherPerson Other_Person { get; set; }
    public bool Pregnant { get; set; }
    public bool Breastfeeding { get; set; }
    public string Medicines { get; set; }
    public string AdditionalMeds { get; set; }
    public string Time { get; set; }
    public string History { get; set; }
    public string Symptoms { get; set; }
    public string Allergies { get; set; }
    public string AdditionalInfo { get; set; }
    public string Emotion { get; set; }
    public string Outcome { get; set; }
    public string AI { get; set; }
    public string Model { get; set; }
    public string TTS { get; set; }
    public string STT { get; set; }
    public string Voice { get; set; }
    public string Avatar { get; set; }

    public static async Task<Scenario> LoadConfig(string config)
    {

        using (UnityWebRequest request = UnityWebRequest.Get(config))
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
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<Scenario>(request.downloadHandler.text);
            }
        }


    }
}
public struct OtherPerson
{
    public string Name { get; set; }
    public string Age { get; set; }
    public string Gender { get; set; }
    public string Relationship { get; set; }
}
