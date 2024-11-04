using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{

    private TMP_InputField studentNumInput;
    private TMP_InputField justificationInput;

    private TMP_InputField KeyboardInput;
    private Button startButton;
    private Button treatButton;
    private Button referButton;

    private Button switchInputButton;
     private TextMeshProUGUI switchInputButtonText; 
    private Button recordButton;

    private Button sendButton;


    void Start()
    {

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:
                studentNumInput = GameObject.Find("Student Num Input").GetComponent<TMP_InputField>();
                startButton = GameObject.Find("Start Button").GetComponent<Button>();
                startButton.onClick.AddListener(() =>
                {
                    Config.Student = new Student(studentNumInput.text);
                    Config.ChatLog = new ChatLog($"{Config.Student.Id}_{Config.Scenario.Id}");
                    Config.ChatLog.WriteConfigToChatLog();


                });

                studentNumInput.Select();

                studentNumInput.onSubmit.AddListener((text) =>
                {
                    if (startButton.interactable) startButton.onClick.Invoke();
                });
                break;
            case 1:
                switchInputButton = GameObject.Find("Toggle Input").GetComponent<Button>();
                switchInputButtonText = switchInputButton.GetComponentInChildren<TextMeshProUGUI>();
                recordButton = GameObject.Find("Record Button").GetComponent<Button>();
                KeyboardInput = GameObject.Find("User Input").GetComponent<TMP_InputField>();
                sendButton = GameObject.Find("Send Button").GetComponent<Button>();
                SwitchInput();
                break;
            case 2:
                justificationInput = GameObject.Find("Justification Input").GetComponent<TMP_InputField>();
                treatButton = GameObject.Find("Treat Button").GetComponent<Button>();
                referButton = GameObject.Find("Refer Button").GetComponent<Button>();
                justificationInput.Select();
                treatButton.onClick.AddListener(() =>
                {
                    Config.ChatLog.WriteOutcomeToChatLog("Treat", justificationInput.text);

                });

                referButton.onClick.AddListener(() =>
                {
                    Config.ChatLog.WriteOutcomeToChatLog("Refer", justificationInput.text);
                });
                break;
        }

    }


    void Update()
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:
                ValidateStudentNumInput();
                break;
            case 1:
                break;
            case 2:
                break;

        }


    }

    private void ValidateStudentNumInput()
    {
        if (studentNumInput.text != null && studentNumInput.text.Length > 6)
        {
            startButton.interactable = true;
        }
        else
        {
            startButton.interactable = false;
        }
    }

    public void SwitchInput()
    {
        if (KeyboardInput.gameObject.activeSelf && sendButton.gameObject.activeSelf)
        {
            KeyboardInput.gameObject.SetActive(false);
            sendButton.gameObject.SetActive(false);
            recordButton.gameObject.SetActive(true);
            switchInputButton.Select();
            switchInputButtonText.text = "Keyboard";
        }
        else if (recordButton.gameObject.activeSelf)
        {
             KeyboardInput.gameObject.SetActive(true);
            sendButton.gameObject.SetActive(true);
            recordButton.gameObject.SetActive(false);
            KeyboardInput.Select();
            switchInputButtonText.text = "Microphone";
        }

    }

}
