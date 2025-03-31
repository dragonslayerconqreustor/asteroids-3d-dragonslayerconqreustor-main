using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Base abstract Skill class that all specific skills will inherit from
public abstract class Skill : MonoBehaviour
{
    [Header("Skill Properties")]
    [SerializeField] public int skillCost = 25;
    public int SkillCost => skillCost;
    [SerializeField] protected string skillName;
    [SerializeField] protected string description;
    [SerializeField] protected int cost;
    [SerializeField] protected int maxLevel = 3;
    [SerializeField] protected Image skillIcon;
    [SerializeField] protected TextMeshProUGUI levelText;
    [SerializeField] protected Button skillButton;

    protected int currentLevel = 0;
    protected bool isUnlocked = false;
    protected Skill[] prerequisites;

    protected virtual void Start()
    {
        // Set up button click listener
        if (skillButton != null)
        {
            skillButton.onClick.AddListener(TryUpgradeSkill);
        }

        // Update UI
        UpdateSkillUI();
    }

    public virtual void TryUpgradeSkill()
    {
        // Check if max level reached
        if (currentLevel >= maxLevel)
        {
            Debug.Log($"Skill {skillName} is already at max level.");
            return;
        }

        // Check prerequisites
        if (!CheckPrerequisites())
        {
            Debug.Log($"Prerequisites for {skillName} not met.");
            return;
        }

        // Try to spend fragments
        if (SkillTreeManager.Instance.SpendFragments(cost))
        {
            currentLevel++;
            ApplySkillEffect();
            UpdateSkillUI();

            if (currentLevel == 1)
            {
                isUnlocked = true;
            }
        }
        else
        {
            Debug.Log("Not enough asteroid fragments!");
        }
    }

    protected virtual bool CheckPrerequisites()
    {
        if (prerequisites == null || prerequisites.Length == 0)
        {
            return true;
        }

        foreach (Skill prerequisite in prerequisites)
        {
            if (!prerequisite.isUnlocked)
            {
                return false;
            }
        }

        return true;
    }

    protected virtual void UpdateSkillUI()
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {currentLevel}/{maxLevel}";
        }

        // Visual feedback for locked/unlocked status
        if (skillButton != null)
        {
            // Update button interactability based on prerequisites and max level
            skillButton.interactable = (currentLevel < maxLevel) && CheckPrerequisites();

            // You could also change the button color based on unlock status
        }
    }

    // Abstract method to be implemented by each specific skill
    protected abstract void ApplySkillEffect();

    // Getters
    public int GetCurrentLevel() { return currentLevel; }
    public bool IsUnlocked() { return isUnlocked; }
    public string GetSkillName() { return skillName; }
    public string GetDescription() { return description; }
    public int GetCost() { return cost; }

    public void SetPrerequisites(Skill[] skills)
    {
        prerequisites = skills;
    }



 
}