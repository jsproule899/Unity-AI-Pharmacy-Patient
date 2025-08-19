using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class UIController : MonoBehaviour
{
    private EventSystem system;
    private Auth auth;
    public InputField studentNumInput;
    public InputField passwordInput;
    public Button togglePassword;
    public TextMeshProUGUI errMessageText;
    public GameObject errMessage;
    public InputField justificationInput;
    public InputField KeyboardInput;
    public TextMeshProUGUI userMessage;
    public TextMeshProUGUI AIMessage;
    public TextMeshProUGUI AIFeedback;
    public Scrollbar feedbackScrollbar;
    public Button startButton;
    public TextMeshProUGUI startButtonText;
    public Button treatButton;
    public Button referButton;
    public Button switchInputButton;
    public Button recordButton;
    public Button sendButton;
    public GameObject outcomeModal;
    public GameObject issueModal;

    void Awake()
    {
        system = EventSystem.current;


        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:
                auth = GameObject.Find("Authentication").GetComponent<Auth>();
                studentNumInput = GameObject.Find("Student Num Input").GetComponent<InputField>();
                passwordInput = GameObject.Find("Password").GetComponent<InputField>();
                togglePassword = GameObject.Find("Show Password").GetComponentInChildren<Button>();
                errMessageText = GameObject.Find("Error Message").GetComponentInChildren<TextMeshProUGUI>();
                errMessage = GameObject.Find("Error Message");
                errMessage.SetActive(false);
                startButton = GameObject.Find("Start Button").GetComponent<Button>();
                startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();



                studentNumInput.Select();
                studentNumInput.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret
                system.SetSelectedGameObject(studentNumInput.gameObject, new BaseEventData(system));
                break;
            case 1:
                switchInputButton = GameObject.Find("Toggle Input").GetComponent<Button>();
                recordButton = GameObject.Find("Record Button").GetComponent<Button>();
                KeyboardInput = GameObject.Find("User Input").GetComponent<InputField>();
                sendButton = GameObject.Find("Send Button").GetComponent<Button>();
                userMessage = GameObject.Find("User Message").GetComponent<TextMeshProUGUI>();
                AIMessage = GameObject.Find("AI Message").GetComponent<TextMeshProUGUI>();
                outcomeModal = GameObject.Find("Outcome Modal");
                issueModal = GameObject.Find("Issue Modal");
                SwitchInput();
                break;
            case 2:
                AIFeedback = GameObject.Find("FeedbackTMP").GetComponent<TextMeshProUGUI>();
                feedbackScrollbar = GameObject.Find("Feedback").GetComponent<ScrollRect>().verticalScrollbar;
                break;

        }

    }

    void Start()
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:


                startButton.onClick.AddListener(async () =>
                                {

                                    if (!auth.isAuth)
                                    {
                                        await auth.LoginAsync();
                                    }
                                    if (auth.isAuth)
                                    {
                                        Config.Student = new Student(auth.studentNum);
                                        Config.ChatLog = new ChatLog($"{Config.Student.Id}_{Config.Scenario._id}");
                                        Config.ChatLog.WriteConfigToChatLog();
                                        GameObject.Find("SceneController").GetComponent<SceneController>().LoadNextScene();
                                    }



                                });

                passwordInput.onSubmit.AddListener((text) =>
                {
                    if (startButton.interactable) startButton.onClick.Invoke();
                });

                if (auth.isAuth)
                {
                    studentNumInput.gameObject.SetActive(false);
                    passwordInput.gameObject.SetActive(false);
                }

                studentNumInput.Select();
                studentNumInput.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret
                system.SetSelectedGameObject(studentNumInput.gameObject, new BaseEventData(system));
                break;
            case 1:
                if (!Config.Scenario.Anonymize) { GameObject.Find("Anon Modal").GetComponent<ModalSystem>().Hide(); }
                break;
        }
    }


    void Update()
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:
                if (!auth.isAuth)
                {
                    startButton.interactable = ValidateStudentNumInput() && ValidatePasswordInput();
                }
                else
                {
                    startButton.interactable = true;
                    studentNumInput.gameObject.SetActive(false);
                    passwordInput.gameObject.SetActive(false);
                }
                break;
            case 1:
                break;
            case 2:
                break;

        }

        TabInputs();


    }

    private bool ValidateStudentNumInput()
    {
        return studentNumInput.text != null && studentNumInput.text.Length > 6;
    }

    private bool ValidatePasswordInput()
    {
        return passwordInput.text != null && passwordInput.text.Length > 6;
    }

    public void SwitchInput()
    {
        if (KeyboardInput.gameObject.activeSelf && sendButton.gameObject.activeSelf)
        {
            KeyboardInput.gameObject.SetActive(false);
            KeyboardInput.text = "";
            sendButton.gameObject.SetActive(false);
            recordButton.gameObject.SetActive(true);
            switchInputButton.transform.GetChild(0).gameObject.SetActive(true);
            switchInputButton.transform.GetChild(1).gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);

        }
        else if (recordButton.gameObject.activeSelf)
        {
            KeyboardInput.gameObject.SetActive(true);

            sendButton.gameObject.SetActive(true);
            recordButton.gameObject.SetActive(false);
            switchInputButton.transform.GetChild(0).gameObject.SetActive(false);
            switchInputButton.transform.GetChild(1).gameObject.SetActive(true);
            KeyboardInput.Select();

        }

    }

    public void ToggleButtonsOnError()
    {
        recordButton.interactable = !recordButton.interactable;
        sendButton.interactable = !sendButton.interactable;
    }

    public void ShowError(string message)
    {

        errMessageText.text = message;
        errMessage.SetActive(true);

    }

    public IEnumerator ErrorToast(string message, int timeInSeconds)
    {

        errMessageText.text = message;
        errMessage.SetActive(true);

        yield return new WaitForSeconds(timeInSeconds);

        errMessage.SetActive(false);
    }

    public void SetButtonColor(Button button, string colour)
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

    public async void TogglePasswordAsync()
    {
        passwordInput.inputType = (passwordInput.inputType == InputField.InputType.Password) ? InputField.InputType.Standard : InputField.InputType.Password;
        passwordInput.OnPointerClick(new PointerEventData(system));

        await Task.Delay(250);
        passwordInput.ActivateInputField();
        passwordInput.MoveTextEnd(false);



    }




    private void TabInputs()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

            if (next != null)
            {

                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null)
                    inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret

                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }
            else
            {
                Selectable prev = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
                InputField inputfield = prev.GetComponent<InputField>();
                if (inputfield != null)
                    inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret

                system.SetSelectedGameObject(prev.gameObject, new BaseEventData(system));
            }


        }
    }

}
