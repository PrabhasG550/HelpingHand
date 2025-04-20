// NPCConversationManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class NPCConversationManager : MonoBehaviour
{
    public GameObject movementControls;
    public GameObject chatUI;
    public TMP_InputField userInputField;
    public TextMeshProUGUI chatLogText;
    public CloudPromptGenerator cloudPromptGenerator;

    [Header("Stage Prompt Storage")]
    public NPCPromptData npcPromptStage1;
    public NPCPromptData npcPromptStage2;
    public NPCPromptData npcPromptStage3;

    [Header("NPC Stage Assignment")]
    public int npcStageNumber = 1; // 1 = Bridge, 2 = Well, 3 = Mountains

    private string npcSystemPrompt;
    private string openAI_API_Key;
    private string apiUrl = "https://api.openai.com/v1/chat/completions";
    private string conversationHistory = "";
    private List<string> introResponses = new List<string>();
    private int introQuestionsAnswered = 0;
    private bool cloudIntroComplete = false;
    private string playerName = "";
    private int currentPlayerStage = 1;

    [System.Serializable]
    public class Message { public string role; public string content; }
    [System.Serializable]
    public class ChatRequest { public string model = "gpt-4o"; public Message[] messages; }
    [System.Serializable]
    public class ChatChoice { public Message message; }
    [System.Serializable]
    public class ChatResponse { public ChatChoice[] choices; }
    [System.Serializable]
    public class APIKeyData { public string openai_api_key; }

    void Start()
    {
        LoadAPIKey();
        EnterConversationMode();

        if (npcStageNumber == 1 && !cloudIntroComplete)
        {
            Debug.Log("Cloud: Hello! My name is Cloud.\nYou’ve entered a quiet world where your journey will help others—and in doing so, help yourself.\nLet’s begin. What name would you like to go by?");
        }
        else
        {
            AssignPromptByStage();

            if (npcStageNumber > currentPlayerStage)
            {
                Debug.Log("NPC: Come talk to me once you've talked to someone else.");
                return;
            }

            Debug.Log("NPC: Hello.");
        }
    }

    void LoadAPIKey()
    {
        TextAsset configFile = Resources.Load<TextAsset>("config");
        if (configFile != null)
        {
            APIKeyData keyData = JsonUtility.FromJson<APIKeyData>(configFile.text);
            openAI_API_Key = keyData.openai_api_key;
        }
        else
        {
            Debug.LogError("API key config file not found!");
        }
    }

    public void OnSendButton()
    {
        string userMessage = userInputField.text;
        if (string.IsNullOrWhiteSpace(userMessage)) return;

        Debug.Log("You: " + userMessage);
        userInputField.text = "";

        if (!cloudIntroComplete && npcStageNumber == 1)
        {
            HandleCloudIntro(userMessage);
        }
        else if (npcStageNumber > currentPlayerStage)
        {
            Debug.Log("NPC: Come talk to me once you've talked to someone else.");
        }
        else
        {
            StartCoroutine(SendMessageToGPT(userMessage));
        }
    }

    void HandleCloudIntro(string userMessage)
    {
        introResponses.Add(userMessage);
        introQuestionsAnswered++;

        if (introQuestionsAnswered == 1) playerName = userMessage;

        if (introQuestionsAnswered == 1)
            Debug.Log("Cloud: How have you been feeling lately?");
        else if (introQuestionsAnswered == 2)
            Debug.Log("Cloud: What’s one thing on your mind—like anxiety, stress, loneliness, or something else—you’d like to focus on today?");
        else if (introQuestionsAnswered == 3)
        {
            cloudIntroComplete = true;
            string combinedInput = string.Join(" ", introResponses);
            StartCoroutine(cloudPromptGenerator.GenerateNPCPrompts(combinedInput, OnPromptsGenerated));
        }
    }

    void OnPromptsGenerated(List<string> npcPrompts)
    {
        if (npcPrompts == null || npcPrompts.Count < 3)
        {
            Debug.LogError("Failed to generate NPC prompts.");
            return;
        }

        npcPromptStage1.promptText = npcPrompts[0];
        npcPromptStage2.promptText = npcPrompts[1];
        npcPromptStage3.promptText = npcPrompts[2];

        npcSystemPrompt = npcPromptStage1.promptText;

        Debug.Log("Cloud: Thank you, " + playerName + ". I understand you a little better now. When you’re ready, head toward the place where the bridge meets the breeze.\n\nI'll be here when you return.");
    }

    void AssignPromptByStage()
    {
        if (npcStageNumber == 1) npcSystemPrompt = npcPromptStage1.promptText;
        else if (npcStageNumber == 2) npcSystemPrompt = npcPromptStage2.promptText;
        else if (npcStageNumber == 3) npcSystemPrompt = npcPromptStage3.promptText;
    }

    IEnumerator SendMessageToGPT(string userMessage)
    {
        var messages = new Message[] {
            new Message { role = "system", content = npcSystemPrompt },
            new Message { role = "user", content = userMessage }
        };

        ChatRequest requestData = new ChatRequest { messages = messages };
        string jsonData = JsonUtility.ToJson(requestData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + openAI_API_Key);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
            yield break;
        }

        ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
        string aiReply = response.choices[0].message.content;

        Debug.Log("NPC: " + aiReply);

        if (aiReply.Contains("Thank you for helping me") && aiReply.Contains("The way to the next civilian"))
        {
            currentPlayerStage++;
            yield return new WaitForSeconds(2f);
            ExitConversationMode();
        }
    }

    void EnterConversationMode()
    {
        movementControls.SetActive(false);
        chatUI.SetActive(true);
    }

    void ExitConversationMode()
    {
        movementControls.SetActive(true);
        chatUI.SetActive(false);
        SaveChatHistory();
    }

    void SaveChatHistory()
    {
        string filename = "ChatLog_" + gameObject.name + ".txt";
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + filename, conversationHistory);
    }
}
