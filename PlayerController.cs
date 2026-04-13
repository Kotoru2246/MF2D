using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool facingLeft = true;
    public Vector3 playerMoveDirection;
    public float playerCurrentHealth;
    public float playerMaxHealth;
    public int experience;
    public int currentLevel;
    public int maxLevel;
    public List<int> playerLevels;
    public List<Weapon> allWeapons; // All possible weapons
    public List<Weapon> availableWeapons; // Currently unlocked weapons
    public Weapon activeWeapon;
    
    [Header("Base Stats (Chỉ số gốc)")]
    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 5f;

    [Header("Derived Stats (Cập nhật từ shop)")]
    public float damageMultiplier = 1f;
    public float cooldownReduction = 0f;
    public float expGainMultiplier = 1f;
    [SerializeField] private float healthRegenRate = 0f;
    [SerializeField] private float armorValue = 0f;

    [HideInInspector] public float levelUpDamageBonus = 0f;
    [HideInInspector] public float levelUpSpeedBonus = 0f;

    [Header("Upgrades")]
    public List<Weapon> allUpgrades;
    public List<Weapon> fallbackUpgrades;

    private ItemsPickUp itemsPickUp;

    private bool isImmune;
    [SerializeField] private float immuneDuration;
    [SerializeField] private float immunityTimer;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    
    public void Start()
    {
        for (int i = playerLevels.Count; i < maxLevel; i++)
        {
            playerLevels.Add(Mathf.CeilToInt(playerLevels[playerLevels.Count - 1] * 1.1f + 15));
        }
        
        playerCurrentHealth = playerMaxHealth;
        UIController.Instance.UpdatePlayerHealthSlider();
        UIController.Instance.UpdatePlayerExperienceSlider();
        
        itemsPickUp = GetComponentInChildren<ItemsPickUp>();

        // Get all weapons and disable them initially
        allWeapons = new List<Weapon>(GetComponentsInChildren<Weapon>());
        
        // Remove upgrades and fallbacks from the weapons list
        allWeapons.RemoveAll(w => allUpgrades.Contains(w) || fallbackUpgrades.Contains(w));
        
        availableWeapons = new List<Weapon>();
        
        foreach (Weapon weapon in allWeapons)
        {
            weapon.gameObject.SetActive(false);

            // SpiningWeapon manages its own position — skip the y offset
            if (weapon is SpiningWeapon) continue;

            Vector3 weaponPos = weapon.transform.localPosition;
            weaponPos.y = 0.6f;
            weapon.transform.localPosition = weaponPos;
        }
        
        // Disable upgrades and fallbacks initially
        foreach (Weapon upgrade in allUpgrades)
            upgrade.gameObject.SetActive(false);
        foreach (Weapon fallback in fallbackUpgrades)
            fallback.gameObject.SetActive(false);
        
        // Show initial weapon selection using level up panel
        ShowInitialWeaponSelection();
        // New Update player data
        UpdateStats();
    }
    
    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        playerMoveDirection = new Vector3(inputX, inputY).normalized;

        animator.SetFloat("moveX", inputX);
        animator.SetFloat("moveY", inputY);

        if (playerMoveDirection == Vector3.zero)
        {
            animator.SetBool("moving", false);
        }
        else
        {
            animator.SetBool("moving", true);
        }
        
        if (inputX < 0 && !facingLeft)
        {
            FlipCharacter();
        }
        else if (inputX > 0 && facingLeft)
        {
            FlipCharacter();
        }
        
        if (immunityTimer > 0)
        {
            immunityTimer -= Time.deltaTime;
        }
        else
        {
            isImmune = false;
        }

        if (healthRegenRate > 0 && playerCurrentHealth < playerMaxHealth)
        {
            playerCurrentHealth = Mathf.Min(playerCurrentHealth + healthRegenRate * Time.deltaTime, playerMaxHealth);
            UIController.Instance.UpdatePlayerHealthSlider();
        }
    }
    
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(playerMoveDirection.x * moveSpeed, playerMoveDirection.y * moveSpeed);
    }
    
    void FlipCharacter()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;
        facingLeft = !facingLeft;
        
        // Counter-flip all weapons to keep them upright
        foreach (Weapon weapon in availableWeapons)
        {
            if (weapon != null)
            {
                Vector3 weaponScale = weapon.transform.localScale;
                weaponScale.x *= -1;
                weapon.transform.localScale = weaponScale;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isImmune)
        {
            isImmune = true;
            immunityTimer = immuneDuration;
            float damageReduction = armorValue / (armorValue + 100f);
            playerCurrentHealth -= damage * (1f - damageReduction);
            UIController.Instance.UpdatePlayerHealthSlider();
            
            if (playerCurrentHealth <= 0)
            {
                gameObject.SetActive(false);
                GameManager.Instance.GameOver();
            }
        }
    }
    
    public void GetExperience(int experienceToGet)
    {
        experience += Mathf.RoundToInt(experienceToGet * expGainMultiplier);
        UIController.Instance.UpdatePlayerExperienceSlider();
        
        if (experience >= playerLevels[currentLevel - 1])
        {
            LevelUp();
        }
    }
    
    // update data from shopitem
    public void UpdateStats()
    {
        if (ShopManager.Instance == null) return;
        playerMaxHealth = baseMaxHealth + ShopManager.Instance.GetTotalBonus(StatType.MaxHealth);
        moveSpeed = baseMoveSpeed + ShopManager.Instance.GetTotalBonus(StatType.MoveSpeed) + levelUpSpeedBonus;
        damageMultiplier = 1f + ShopManager.Instance.GetTotalBonus(StatType.DamageMultiplier) + levelUpDamageBonus;
        cooldownReduction = Mathf.Clamp(ShopManager.Instance.GetTotalBonus(StatType.CooldownReduction), 0f, 0.75f);
        expGainMultiplier = 1f + ShopManager.Instance.GetTotalBonus(StatType.ExpGain);
        healthRegenRate = ShopManager.Instance.GetTotalBonus(StatType.HealthRegen);
        armorValue = ShopManager.Instance.GetTotalBonus(StatType.Armor);
        if (itemsPickUp != null)
        {
            itemsPickUp.SetPickupRadius(ShopManager.Instance.GetTotalBonus(StatType.MagnetRadius));
        }
        Debug.Log($"<color=cyan>Đã cập nhật chỉ số! Tốc độ chạy mới: {moveSpeed}</color>");
    }

    private void ShowInitialWeaponSelection()
    {
        // Only show pure weapons (not upgrades or fallbacks) for the first pick
        List<Weapon> weaponsOnly = allWeapons.FindAll(w => !w.isUpgrade && !w.isFallbackUpgrade);
        List<Weapon> randomWeapons = GetRandomWeapons(weaponsOnly, 3);
        
        int buttonsToShow = Mathf.Min(UIController.Instance.levelUpButtons.Length, randomWeapons.Count);
        
        for (int i = 0; i < buttonsToShow; i++)
        {
            UIController.Instance.levelUpButtons[i].gameObject.SetActive(true);
            UIController.Instance.levelUpButtons[i].ActivateButton(randomWeapons[i]);
        }
        
        for (int i = buttonsToShow; i < UIController.Instance.levelUpButtons.Length; i++)
        {
            UIController.Instance.levelUpButtons[i].gameObject.SetActive(false);
        }
        
        UIController.Instance.LevelUpPanelOpen();
    }

    public void LevelUp()
    {
        experience -= playerLevels[currentLevel - 1];
        currentLevel++;
        UIController.Instance.UpdatePlayerExperienceSlider();
        
        // Build pool of valid weapons and upgrades
        List<Weapon> weaponPool = new List<Weapon>();
        
        // Add unlocked weapons/upgrades that aren't at max level
        foreach (Weapon weapon in availableWeapons)
        {
            if (weapon.weaponLevel < weapon.stats.Count - 1)
            {
                weaponPool.Add(weapon);
            }
        }
        
        // Add new weapons that haven't been unlocked yet
        foreach (Weapon weapon in allWeapons)
        {
            if (!availableWeapons.Contains(weapon))
            {
                weaponPool.Add(weapon);
            }
        }
        
        // Add new upgrades that haven't been unlocked yet
        foreach (Weapon upgrade in allUpgrades)
        {
            if (!availableWeapons.Contains(upgrade))
            {
                weaponPool.Add(upgrade);
            }
        }
        
        // If nothing left to upgrade, use fallback options (GetCoin, Heal)
        if (weaponPool.Count == 0)
        {
            weaponPool.AddRange(fallbackUpgrades);
        }
        
        // Get 3 random options from the pool
        List<Weapon> randomWeapons = GetRandomWeapons(weaponPool, 3);
        
        int buttonsToShow = Mathf.Min(UIController.Instance.levelUpButtons.Length, randomWeapons.Count);
        
        for (int i = 0; i < buttonsToShow; i++)
        {
            UIController.Instance.levelUpButtons[i].gameObject.SetActive(true);
            UIController.Instance.levelUpButtons[i].ActivateButton(randomWeapons[i]);
        }
        
        for (int i = buttonsToShow; i < UIController.Instance.levelUpButtons.Length; i++)
        {
            UIController.Instance.levelUpButtons[i].gameObject.SetActive(false);
        }
        
        UIController.Instance.LevelUpPanelOpen();
    }

    // Helper method to get random weapons from a list
    private List<Weapon> GetRandomWeapons(List<Weapon> sourceList, int count)
    {
        List<Weapon> result = new List<Weapon>();
        List<Weapon> tempList = new List<Weapon>(sourceList);
        
        int numToSelect = Mathf.Min(count, tempList.Count);
        
        for (int i = 0; i < numToSelect; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            result.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }
        
        return result;
    }
}
