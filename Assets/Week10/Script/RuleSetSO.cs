using UnityEngine;

[CreateAssetMenu(fileName = "RuleSet", menuName = "Pseudoman/Rule Set")]
public class RuleSetSO : ScriptableObject
{
    public string dayName;
    [TextArea] public string ruleDescription;
    public bool checkBadgeValidity;
    public bool checkDepartment;
    public bool checkClearance;
    public bool checkEyeColor;
    public float timeLimit = 10f;
}