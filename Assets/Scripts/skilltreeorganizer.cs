using UnityEngine;

using System.Collections.Generic;

public class SkillTreeOrganizer : MonoBehaviour
{
    [Header("Skill References")]
    [SerializeField] private SkillImplementations skillImplementations;

    private void Start()
    {
        InitializeSkillTree();
    }

    private void InitializeSkillTree()
    {
        // Set up skill prerequisites
        // Example: bulletSpeedSkill requires speedBoostSkill
        skillImplementations.GetBulletSpeed().SetPrerequisites(new Skill[] { skillImplementations.GetSpeedBoost() });
        // Example: fireRateSkill requires bulletSpeedSkill
        skillImplementations.GetFireRate().SetPrerequisites(new Skill[] { skillImplementations.GetBulletSpeed() });
        // Health skill has no prerequisites in this example
        skillImplementations.GetHealthIncrease().SetPrerequisites(new Skill[] { });

        // Update all skill UI elements
        Skill[] allSkills = GetComponentsInChildren<Skill>();
        foreach (Skill skill in allSkills)
        {
            //skill.UpdateSkillUI(); // Uncomment this line if you need to update the UI here as well
        }
    }
}

