using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;
using System.Collections;

public class BlenderBotHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public TMP_Text outputText;
    public ScrollRect scrollRect;

    [Header("API Settings")]
    [TextArea]
    public string apiKey; // Paste your Hugging Face API key here
    private string endpoint = "https://api-inference.huggingface.co/models/facebook/blenderbot-400M-distill";


    private bool isWaiting = false;

    void Update()
    {
        if (inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            SendMessageToBot();
        }
    }

    public void SendMessageToBot()
    {
        if (isWaiting) return;

        string message = inputField.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        outputText.text += $"<color=blue>You: {message}</color>\nBot is typing...\n";
        inputField.text = "";
        inputField.interactable = false;
        isWaiting = true;

        StartCoroutine(SendBlenderBotRequest(message));
    }

    private IEnumerator SendBlenderBotRequest(string message)
    {
        string json = $"{{\"inputs\": \"{message}\"}}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        string response = request.downloadHandler.text;

        if (request.result == UnityWebRequest.Result.Success)
        {
            string reply = ExtractReply(response);
            outputText.text = outputText.text.Replace("Bot is typing...\n", "");
            outputText.text += $"<color=black>Bot: {reply}</color>\n\n";
        }
        else
        {
            outputText.text = outputText.text.Replace("Bot is typing...\n", "");
            outputText.text += $"<color=red>Error: {request.error}</color>\n";
        }

        isWaiting = false;
        inputField.interactable = true;
        inputField.ActivateInputField();
    }

    private string ExtractReply(string json)
    {
        try
        {
            string wrapped = "{\"array\":" + json + "}";
            var parsed = JsonUtility.FromJson<BotResponseWrapper>(wrapped);
            return parsed.array[0].generated_text;
        }
        catch
        {
            return "Could not understand response.";
        }
    }

    [Serializable]
    private class BotResponse
    {
        public string generated_text;
    }

    [Serializable]
    private class BotResponseWrapper
    {
        public BotResponse[] array;
    }
}
