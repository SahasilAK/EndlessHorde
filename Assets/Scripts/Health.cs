using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    public int Current { get; private set; }
    public int Max => maxHealth;
    public bool IsDead { get; private set; }
    public event Action<Health> Died;
    public event Action<Health> Changed;

    private void Awake()
    {
        Current = maxHealth;
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = Mathf.Max(1, value);
        Current = maxHealth;
        IsDead = false;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        Current = Mathf.Max(0, Current - amount);
        Changed?.Invoke(this);
        if (Current == 0)
        {
            IsDead = true;
            Died?.Invoke(this);
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        Current = Mathf.Min(Max, Current + amount);
        Changed?.Invoke(this);
    }
}