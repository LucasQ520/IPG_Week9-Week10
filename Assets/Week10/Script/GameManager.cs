using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public VisitorJsonLoader jsonLoader;
    public GameUI ui;
    public RuleSetSO[] days;

    [Header("3D NPC Setup")]
    public GameObject npcPrefab;
    public Transform npcSpawnPoint;
    public Transform npcStopPoint;
    public Transform allowExitPoint;
    public Transform denyExitPoint;

    private List<VisitorData> visitorPool = new List<VisitorData>();
    private VisitorData currentVisitor;
    private NPCVisitor currentNPC;

    private int currentDayIndex;
    private int visitorIndex;
    private int score;
    private int strikes;

    private float timer;
    private bool roundActive;
    private bool gameEnded;

    private const int maxStrikes = 3;
    private const int visitorsPerDay = 3;

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (!roundActive || gameEnded)
        {
            return;
        }

        timer -= Time.deltaTime;
        ui.timerText.text = "Time: " + timer.ToString("F1");

        if (timer <= 0f)
        {
            timer = 0f;
            EvaluateDecision(false, true);
        }
    }

    public void StartGame()
    {
        Debug.Log("StartGame called.");

        score = 0;
        strikes = 0;
        visitorIndex = 0;
        currentDayIndex = 0;
        roundActive = false;
        gameEnded = false;

        VisitorData[] loadedVisitors = jsonLoader.LoadVisitors();

        visitorPool.Clear();

        if (loadedVisitors != null)
        {
            visitorPool.AddRange(loadedVisitors);
            Debug.Log("Loaded visitors: " + visitorPool.Count);
        }
        else
        {
            Debug.LogError("Loaded visitors is null.");
        }

        ShuffleVisitors();

        ui.restartButton.gameObject.SetActive(false);
        ui.resultText.text = "";

        UpdateScoreUI();
        LoadNextVisitor();
    }

    void ShuffleVisitors()
    {
        for (int i = 0; i < visitorPool.Count; i++)
        {
            int randomIndex = Random.Range(i, visitorPool.Count);
            VisitorData temp = visitorPool[i];
            visitorPool[i] = visitorPool[randomIndex];
            visitorPool[randomIndex] = temp;
        }
    }

    void LoadNextVisitor()
    {
        Debug.Log("LoadNextVisitor called.");

        if (strikes >= maxStrikes)
        {
            EndGame("Too many mistakes. The building has been infiltrated.");
            return;
        }

        if (currentDayIndex >= days.Length)
        {
            EndGame("You survived all inspection days. Infiltration delayed.");
            return;
        }

        int totalVisitorsNeeded = (currentDayIndex + 1) * visitorsPerDay;
        if (visitorIndex >= totalVisitorsNeeded)
        {
            currentDayIndex++;

            if (currentDayIndex >= days.Length)
            {
                EndGame("You survived all inspection days. Infiltration delayed.");
                return;
            }
        }

        if (visitorIndex >= visitorPool.Count)
        {
            Debug.LogError("No visitors available in visitorPool.");
            EndGame("No more visitor data available.");
            return;
        }

        currentVisitor = visitorPool[visitorIndex];
        visitorIndex++;

        Debug.Log("Current visitor: " + currentVisitor.name);

        RuleSetSO rules = days[currentDayIndex];
        timer = rules.timeLimit;
        roundActive = true;

        DisplayRules(rules);
        DisplayVisitor(currentVisitor);
        ui.resultText.text = "Inspect the visitor.";

        SpawnNPC(currentVisitor);
    }

    void SpawnNPC(VisitorData visitor)
    {
        if (currentNPC != null)
        {
            Destroy(currentNPC.gameObject);
        }

        if (npcPrefab == null)
        {
            Debug.LogWarning("NPC Prefab is not assigned.");
            return;
        }

        if (npcSpawnPoint == null)
        {
            Debug.LogWarning("NPC Spawn Point is not assigned.");
            return;
        }

        if (npcStopPoint == null)
        {
            Debug.LogWarning("NPC Stop Point is not assigned.");
            return;
        }

        GameObject npcObject = Instantiate(npcPrefab, npcSpawnPoint.position, Quaternion.identity);
        currentNPC = npcObject.GetComponent<NPCVisitor>();

        if (currentNPC == null)
        {
            Debug.LogWarning("Spawned NPC does not have NPCVisitor script.");
            return;
        }

        currentNPC.Setup(visitor, npcStopPoint.position);
    }

    void DisplayRules(RuleSetSO rules)
    {
        Debug.Log("Displaying rules for: " + rules.dayName);

        ui.dayText.text = "Day: " + rules.dayName;
        ui.rulesText.text = rules.ruleDescription;
    }

    void DisplayVisitor(VisitorData visitor)
    {
        Debug.Log("Displaying visitor UI for: " + visitor.name);

        ui.nameText.text = "Name: " + visitor.name;
        ui.badgeText.text = "Badge ID: " + visitor.badgeId;
        ui.departmentText.text = "Department: " + visitor.department;
        ui.clearanceText.text = "Clearance: " + visitor.clearance;
        ui.eyeColorText.text = "Eye Color: " + visitor.eyeColor;

        Sprite portrait = Resources.Load<Sprite>(visitor.portraitResource);

        if (portrait != null)
        {
            ui.portraitImage.sprite = portrait;
        }
        else
        {
            Debug.LogWarning("Portrait not found at: " + visitor.portraitResource);
        }
    }

    public void AllowVisitor()
    {
        if (!roundActive || gameEnded)
        {
            return;
        }

        EvaluateDecision(true, false);
    }

    public void DenyVisitor()
    {
        if (!roundActive || gameEnded)
        {
            return;
        }

        EvaluateDecision(false, false);
    }

    void EvaluateDecision(bool playerAllowed, bool timeout)
    {
        roundActive = false;

        bool shouldAllow = PassesCurrentRules(currentVisitor, days[currentDayIndex]);
        bool correct = playerAllowed == shouldAllow;

        if (timeout)
        {
            correct = false;
        }

        if (correct)
        {
            score++;
            ui.resultText.text = "Correct.";
        }
        else
        {
            strikes++;

            if (timeout)
            {
                ui.resultText.text = "Too slow. Strike received.";
            }
            else
            {
                ui.resultText.text = "Incorrect decision. Strike received.";
            }
        }

        UpdateScoreUI();

        if (currentNPC != null)
        {
            if (playerAllowed && allowExitPoint != null)
            {
                currentNPC.WalkTo(allowExitPoint.position);
            }
            else if (!playerAllowed && denyExitPoint != null)
            {
                currentNPC.WalkTo(denyExitPoint.position);
            }
        }

        StartCoroutine(NextVisitorDelay());
    }

    bool PassesCurrentRules(VisitorData visitor, RuleSetSO rules)
    {
        if (rules.checkBadgeValidity && !visitor.badgeIsValid)
        {
            return false;
        }

        if (rules.checkDepartment && !visitor.departmentAllowed)
        {
            return false;
        }

        if (rules.checkClearance && !visitor.clearanceAllowed)
        {
            return false;
        }

        if (rules.checkEyeColor && !visitor.eyeColorMatches)
        {
            return false;
        }

        if (visitor.isPseudoman && rules.checkEyeColor)
        {
            return false;
        }

        return true;
    }

    IEnumerator NextVisitorDelay()
    {
        yield return new WaitForSeconds(2f);
        LoadNextVisitor();
    }

    void UpdateScoreUI()
    {
        ui.scoreText.text = "Score: " + score;
        ui.strikesText.text = "Strikes: " + strikes + "/" + maxStrikes;
    }

    void EndGame(string message)
    {
        gameEnded = true;
        roundActive = false;
        ui.resultText.text = message;
        ui.restartButton.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}