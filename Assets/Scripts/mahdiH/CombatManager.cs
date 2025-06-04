using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    public int attackInputCount = 0;
    public bool canReceiveInput = true;

    private void Awake()
    {
        instance = this;
        canReceiveInput = true;
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && canReceiveInput)
        {
            attackInputCount++;
            canReceiveInput = false; // فقط در Recovery باز می‌شود
        }
    }

    public bool HasPendingInput()
    {
        return attackInputCount > 0;
    }

    public void ConsumeInput()
    {
        if (attackInputCount > 0)
            attackInputCount--;
    }

    public void EnableInput()
    {
        canReceiveInput = true;
    }

    public void DisableInput()
    {
        canReceiveInput = false;
    }
}