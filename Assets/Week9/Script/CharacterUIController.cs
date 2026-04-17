using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterUIController : MonoBehaviour
{
    public CharacterLoader loader;
    public OpenAIManager openAI;

    [Header("UI Text")]
    public TMP_Text nameText;
    public TMP_Text originText;
    public TMP_Text worldText;
    public TMP_Text classText;
    public TMP_Text rarityText;
    public TMP_Text hpText;
    public TMP_Text mpText;
    public TMP_Text attackText;
    public TMP_Text abilityText;
    public TMP_Text indexText;
    public TMP_Text storyText;

    [Header("UI Image")]
    public Image characterImage;

    [Header("3D Display")]
    public Renderer summonCapsuleRenderer;

    private int currentIndex = 0;

    void Start()
    {
        if (loader != null && loader.GetCharacterCount() > 0)
        {
            ShowCharacter(currentIndex);
            storyText.text = "Click Generate Story to create a backstory.";
        }
        else
        {
            Debug.LogWarning("Error.");
        }
    }

    public void ShowCharacter(int index)
    {
        CharacterData character = loader.GetCharacterByIndex(index);

        if (character == null)
            return;

        currentIndex = index;

        nameText.text = character.name;
        originText.text = "Origin: " + character.origin;
        worldText.text = "World: " + character.world;
        classText.text = "Class: " + character.classType;
        rarityText.text = "Rarity: " + character.rarity;
        hpText.text = "HP: " + character.hp;
        mpText.text = "MP: " + character.mp;
        attackText.text = "ATK: " + character.attack;
        abilityText.text = "Ability: " + character.specialAbility;
        indexText.text = "Character " + (currentIndex + 1) + " / " + loader.GetCharacterCount();

        LoadCharacterImage(character.image);
        UpdateRarityColor(character.rarity);
        UpdateCapsuleColor(character.rarity);

        if (storyText != null)
            storyText.text = "Click Generate Story to create a backstory.";
    }

    void LoadCharacterImage(string imageName)
    {
        Debug.Log("Trying to load image: Images/" + imageName);

        Sprite loadedSprite = Resources.Load<Sprite>("Images/" + imageName);

        if (loadedSprite != null)
        {
            Debug.Log("Loaded sprite successfully: " + imageName);
            characterImage.sprite = loadedSprite;
            characterImage.color = Color.white;
        }
        else
        {
            Debug.LogError("Image not found in Resources/Images/: " + imageName);
            characterImage.sprite = null;
            characterImage.color = Color.red;
        }
    }

    void UpdateRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "Common":
                rarityText.color = Color.white;
                break;
            case "Rare":
                rarityText.color = Color.cyan;
                break;
            case "Epic":
                rarityText.color = new Color(0.7f, 0.3f, 1f);
                break;
            case "Legendary":
                rarityText.color = Color.yellow;
                break;
            default:
                rarityText.color = Color.white;
                break;
        }
    }

    void UpdateCapsuleColor(string rarity)
    {
        if (summonCapsuleRenderer == null)
        {
            Debug.LogWarning("Not assigned.");
            return;
        }

        Color capsuleColor = Color.white;

        switch (rarity)
        {
            case "Common":
                capsuleColor = Color.white;
                break;
            case "Rare":
                capsuleColor = Color.cyan;
                break;
            case "Epic":
                capsuleColor = new Color(0.7f, 0.3f, 1f);
                break;
            case "Legendary":
                capsuleColor = Color.yellow;
                break;
            default:
                capsuleColor = Color.white;
                break;
        }

        summonCapsuleRenderer.material.color = capsuleColor;
    }

    public void NextCharacter()
    {
        int count = loader.GetCharacterCount();
        if (count == 0) return;

        currentIndex++;
        if (currentIndex >= count)
            currentIndex = 0;

        ShowCharacter(currentIndex);
    }

    public void PreviousCharacter()
    {
        int count = loader.GetCharacterCount();
        if (count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = count - 1;

        ShowCharacter(currentIndex);
    }

    public void RandomSummon()
    {
        CharacterData randomCharacter = loader.GetRandomCharacter();
        if (randomCharacter != null)
        {
            ShowCharacter(randomCharacter.id);
        }
    }

    public void GenerateStory()
    {
        CharacterData character = loader.GetCharacterByIndex(currentIndex);

        if (character == null)
        {
            if (storyText != null)
                storyText.text = "Error.";
            return;
        }

        if (openAI == null)
        {
            if (storyText != null)
                storyText.text = "Error.";
            return;
        }

        if (storyText != null)
            storyText.text = "Generating story...";

        StartCoroutine(openAI.GenerateStory(character, (result) =>
        {
            if (storyText != null)
                storyText.text = result;
        }));
    }
}