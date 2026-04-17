using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    public CharacterDatabase database;

    void Awake()
    {
        LoadCharacterData();
    }

    void LoadCharacterData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/characters");

        if (jsonFile == null)
        {
            Debug.LogError("Could not find characters.json in Resources/Data/");
            return;
        }

        database = JsonUtility.FromJson<CharacterDatabase>(jsonFile.text);

        if (database == null || database.characters == null || database.characters.Count == 0)
        {
            Debug.LogError("Failed to load character data.");
            return;
        }

        Debug.Log("Loaded " + database.characters.Count + " characters.");
    }

    public CharacterData GetCharacterByIndex(int index)
    {
        if (database == null || database.characters == null || database.characters.Count == 0)
            return null;

        if (index < 0 || index >= database.characters.Count)
            return null;

        return database.characters[index];
    }

    public int GetCharacterCount()
    {
        if (database == null || database.characters == null)
            return 0;

        return database.characters.Count;
    }

    public CharacterData GetRandomCharacter()
    {
        if (database == null || database.characters == null || database.characters.Count == 0)
            return null;

        int randomIndex = Random.Range(0, database.characters.Count);
        return database.characters[randomIndex];
    }
}