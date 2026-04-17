using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;

    public ChatMessage(string role, string content)
    {
        this.role = role;
        this.content = content;
    }
}

[Serializable]
public class ChatCompletionRequest
{
    public string model;
    public List<ChatMessage> messages;
    public float temperature;
    public int max_tokens;
}

[Serializable]
public class ChatCompletionResponse
{
    public Choice[] choices;
    public ApiError error;
}

[Serializable]
public class Choice
{
    public Message message;
}

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class ApiError
{
    public string message;
    public string type;
    public string code;
}

public class OpenAIManager : MonoBehaviour
{
    [Header("OpenAI Settings")]
    [SerializeField] private string apiKey = "";
    [SerializeField] private string modelName = "gpt-4o-mini";
    [SerializeField] private float temperature = 0.8f;
    [SerializeField] private int maxTokens = 150;

    public IEnumerator GenerateStory(CharacterData character, Action<string> callback)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            callback?.Invoke("Error");
            yield break;
        }

        string url = "https://api.openai.com/v1/chat/completions";

        string prompt =
            "Write a short fantasy reincarnation backstory in 3 sentences for this character.\n" +
            "Name: " + character.name + "\n" +
            "Origin: " + character.origin + "\n" +
            "World: " + character.world + "\n" +
            "Class: " + character.classType + "\n" +
            "Rarity: " + character.rarity + "\n" +
            "HP: " + character.hp + "\n" +
            "MP: " + character.mp + "\n" +
            "Attack: " + character.attack + "\n" +
            "Special Ability: " + character.specialAbility + "\n" +
            "Make it dramatic but easy to read.";

        ChatCompletionRequest requestData = new ChatCompletionRequest
        {
            model = modelName,
            messages = new List<ChatMessage>
            {
                new ChatMessage("user", prompt)
            },
            temperature = temperature,
            max_tokens = maxTokens
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey.Trim());

            yield return request.SendWebRequest();

            string rawResponse = request.downloadHandler != null ? request.downloadHandler.text : "";
            Debug.Log("OpenAI raw response: " + rawResponse);
            Debug.Log("HTTP status: " + request.responseCode);

            ChatCompletionResponse responseData = null;
            if (!string.IsNullOrEmpty(rawResponse))
            {
                try
                {
                    responseData = JsonUtility.FromJson<ChatCompletionResponse>(rawResponse);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Could not parse response JSON: " + e.Message);
                }
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                string detailedMessage = "Error: request failed.";

                if (responseData != null && responseData.error != null && !string.IsNullOrEmpty(responseData.error.message))
                {
                    detailedMessage = "Error: " + responseData.error.message;
                }
                else if (!string.IsNullOrEmpty(request.error))
                {
                    detailedMessage = "Error: " + request.error;
                }

                Debug.LogError(detailedMessage);
                callback?.Invoke(detailedMessage);
                yield break;
            }

            if (responseData != null &&
                responseData.choices != null &&
                responseData.choices.Length > 0 &&
                responseData.choices[0].message != null &&
                !string.IsNullOrEmpty(responseData.choices[0].message.content))
            {
                callback?.Invoke(responseData.choices[0].message.content.Trim());
            }
            else if (responseData != null && responseData.error != null && !string.IsNullOrEmpty(responseData.error.message))
            {
                callback?.Invoke("Error: " + responseData.error.message);
            }
            else
            {
                callback?.Invoke("Error: story response was empty.");
            }
        }
    }
}