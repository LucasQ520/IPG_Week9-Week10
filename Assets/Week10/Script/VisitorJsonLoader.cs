using UnityEngine;

public class VisitorJsonLoader : MonoBehaviour
{
    public VisitorData[] LoadVisitors()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/visitors");

        if (jsonFile == null)
        {
            Debug.LogError("visitors.json not found in Resources/Data/");
            return null;
        }

        VisitorDatabase database = JsonUtility.FromJson<VisitorDatabase>(jsonFile.text);

        if (database == null || database.visitors == null)
        {
            Debug.LogError("Failed to parse visitor JSON.");
            return null;
        }

        return database.visitors;
    }
}