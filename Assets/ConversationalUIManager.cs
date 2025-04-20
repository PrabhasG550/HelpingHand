using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConversationUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject conversationUI;
    public GameObject joystickUI;
    public GameObject playerChatPanel;

    [Header("Input & Chat")]
    public TMP_InputField inputField;
    public TextMeshProUGUI npcChatText;
    public ScrollRect npcScrollRect;

    [Header("Buttons")]
    public Button sendButton;
    public Button exitButton;

    [Header("Output")]
    public TextMeshProUGUI lastMessageText;

    void Start()
    {
        playerChatPanel.SetActive(true); // always visible during convo
        conversationUI.SetActive(false);

        sendButton.onClick.AddListener(SendMessageToNPC);
        exitButton.onClick.AddListener(ExitConversation);
    }

    public void EnterConversationMode()
    {
        Debug.Log("EnterConversationMode triggered by: " + gameObject.name);

        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().isFrozen = true;

        conversationUI.SetActive(true);
        joystickUI.SetActive(false);

        playerChatPanel.SetActive(true); // Make sure this isn't missing!

        inputField.text = "";
        inputField.ActivateInputField();
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
    }

    public void SendMessageToNPC()
    {
        string userMessage = inputField.text;
        if (string.IsNullOrWhiteSpace(userMessage)) return;

        // Just update the little preview bubble
        lastMessageText.text += "\nYou: " + userMessage;

        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void ExitConversation()
    {
        conversationUI.SetActive(false);
        joystickUI.SetActive(true);
        playerChatPanel.SetActive(false);
        inputField.DeactivateInputField();

        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().isFrozen = false;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void AppendNPCMessage(string message)
    {
        npcChatText.text += "\nCloud: " + message;
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        npcScrollRect.verticalNormalizedPosition = 0f;
    }
}
