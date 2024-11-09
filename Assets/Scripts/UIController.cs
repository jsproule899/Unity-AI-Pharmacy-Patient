using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{

    public TMP_InputField studentNumInput;
    public TMP_InputField justificationInput;

    public TMP_InputField KeyboardInput;
    public TextMeshProUGUI userMessage;
    public TextMeshProUGUI AIMessage;
    public Button startButton;
    public Button treatButton;
    public Button referButton;

    public Button switchInputButton;
    public TextMeshProUGUI switchInputButtonText;
    public Button recordButton;
    public TextMeshProUGUI recordButtonText;
    public Button sendButton;


    void Awake()
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
                recordButtonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
                KeyboardInput = GameObject.Find("User Input").GetComponent<TMP_InputField>();
                sendButton = GameObject.Find("Send Button").GetComponent<Button>();
                userMessage = GameObject.Find("User Message").GetComponent<TextMeshProUGUI>();
                AIMessage = GameObject.Find("AI Message").GetComponent<TextMeshProUGUI>();
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

     public void ToggleButtonsOnError()
    {
        recordButton.interactable = !recordButton.interactable;
        sendButton.interactable = !sendButton.interactable;
    }

        public void setButtonColor(Button button, string colour)
    {

        if (colour.Equals("red"))
        {
            button.image.color = new Color32(255, 100, 100, 255);
        }
        else if (colour.Equals("white"))
        {
            button.image.color = new Color32(255, 255, 255, 255);
        }

    }

}
