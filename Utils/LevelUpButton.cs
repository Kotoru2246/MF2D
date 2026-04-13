using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelUpButton : MonoBehaviour
{
    public TMP_Text weaponName;
    public TMP_Text weaponDescription;
    public Image weaponIcon;
    private Button button;
    private Animator animator;

    private Weapon assignedWeapon;

    private void Awake()
    {
        button = GetComponent<Button>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // When the GameObject is enabled, trigger the appropriate animation
        if (assignedWeapon != null)
        {
            // Check if this is a new weapon
            bool isNewWeapon = !PlayerController.Instance.availableWeapons.Contains(assignedWeapon);
            
            if (isNewWeapon)
            {
                if (animator != null)
                {
                    animator.Play("Normal");
                }
            }
            else
            {
                int nextLevel = assignedWeapon.weaponLevel + 1;
                if (nextLevel < assignedWeapon.stats.Count)
                {
                    if (animator != null)
                    {
                        animator.Play("Normal");
                    }
                }
                else
                {
                    if (animator != null)
                    {
                        animator.Play("Disabled");
                    }
                }
            }
        }
    }

    public void ActivateButton(Weapon weapon)
    {
        // Ensure button reference is set
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        weaponName.text = weapon.name;
        
        // Fallback upgrades (GetCoin, Heal) — always available, no levels
        if (weapon.isFallbackUpgrade)
        {
            weaponDescription.text = weapon.stats[0].description;
            if (button != null) button.interactable = true;
            if (gameObject.activeInHierarchy && animator != null)
                animator.Play("Normal");
        }
        // Check if this is a new weapon (not yet in availableWeapons)
        else if (!PlayerController.Instance.availableWeapons.Contains(weapon))
        {
            // New weapon - show first level description
            weaponDescription.text = weapon.stats[0].description + " (New!)";
            if (button != null)
            {
                button.interactable = true;
            }
            
            // Trigger normal/enabled state animation if already active
            if (gameObject.activeInHierarchy && animator != null)
            {
                animator.Play("Normal");
            }
        }
        else
        {
            // Existing weapon - check if there's a next level available
            int nextLevel = weapon.weaponLevel + 1;
            if (nextLevel < weapon.stats.Count)
            {
                weaponDescription.text = weapon.stats[nextLevel].description;
                if (button != null)
                {
                    button.interactable = true;
                }
                
                // Trigger normal/enabled state animation if already active
                if (gameObject.activeInHierarchy && animator != null)
                {
                    animator.Play("Normal");
                }
            }
            else
            {
                // Weapon is at max level
                if (weapon.weaponLevel < weapon.stats.Count)
                {
                    weaponDescription.text = weapon.stats[weapon.weaponLevel].description + " (Max Level)";
                }
                else
                {
                    weaponDescription.text = "Max Level";
                }
                
                if (button != null)
                {
                    button.interactable = false;
                }
                
                // Trigger disabled state animation if already active
                if (gameObject.activeInHierarchy && animator != null)
                {
                    animator.Play("Disabled");
                }
            }
        }
        
        weaponIcon.sprite = weapon.weaponImage;
        assignedWeapon = weapon;
    }

    public void SelectUpgrade()
    {
        if (assignedWeapon.isFallbackUpgrade)
        {
            // Fallback upgrade — just apply the effect, no activation or tracking
            assignedWeapon.LevelUp();
        }
        else if (!PlayerController.Instance.availableWeapons.Contains(assignedWeapon))
        {
            // New weapon/upgrade - add it to available weapons and activate it
            assignedWeapon.gameObject.SetActive(true);
            PlayerController.Instance.availableWeapons.Add(assignedWeapon);
            
            // Set as active weapon if it's the first one
            if (PlayerController.Instance.activeWeapon == null)
            {
                PlayerController.Instance.activeWeapon = assignedWeapon;
            }
        }
        else
        {
            // Existing weapon/upgrade - level it up
            assignedWeapon.LevelUp();
        }
        
        UIController.Instance.LevelUpPanelClose();
    }
}
