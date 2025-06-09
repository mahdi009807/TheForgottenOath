using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private float _maxHealth = 100;
    private float _currentHealth;
    [SerializeField] private Image _healthBarFill;

    void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void UpdateHealth(float amount) 
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth); // Ensure health stays within bounds
        UpdateHealthBar();
    }

    private void UpdateHealthBar() 
    {
        float targetFillAmount = _currentHealth / _maxHealth;
        _healthBarFill.fillAmount = targetFillAmount;
    }
} 