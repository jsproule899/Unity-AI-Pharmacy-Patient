using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


public class Scenario
{
    public string Context;
    public string Name;
    public int Age;
    public string Gender;
    public string Medicines;
    public string AdditionalMeds;
    public string History;
    public string Symptoms;
    public string Allergies;
    public string Time;
    
    public static async Task<Scenario> LoadConfig(string config)
    {

        using (UnityWebRequest request = UnityWebRequest.Get(config))
        {
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
                return JsonUtility.FromJson<Scenario>(request.downloadHandler.text);
            }
        }


    }
}

