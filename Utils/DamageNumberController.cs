using UnityEngine;

public class DamageNumberController : MonoBehaviour
{
    public static DamageNumberController Instance;
    public DamageNumber prefab;
    [SerializeField] private Color defaultDamageColor = Color.white;

    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(this);

        }
        else { 
            Instance = this; 
        }
        
    }

    public void CreateNumber(float value, Vector3 location) {
        DamageNumber damageNumber = Instantiate(prefab, location, transform.rotation, transform);
        damageNumber.SetValue(Mathf.RoundToInt(value));
        damageNumber.SetColor(defaultDamageColor);
    }

    public void CreateText(string value, Vector3 location, Color color)
    {
        DamageNumber damageNumber = Instantiate(prefab, location, transform.rotation, transform);
        damageNumber.SetText(value);
        damageNumber.SetColor(color);
    }
}
