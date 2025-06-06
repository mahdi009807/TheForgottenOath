using UnityEngine;

public class ComboRecoveryBehaviour : StateMachineBehaviour
{
    [Tooltip("The name of the next attack trigger (e.g., Attack2, Attack3, Attack4)")]
    public string nextAttackTrigger;

    [Tooltip("Max time to accept the next attack input (in seconds)")]
    public float inputTimeout = 1f;

    private float timer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CombatManager.instance.EnableInput();
        timer = 0f;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;

        if (CombatManager.instance.HasPendingInput())
        {
            if (!string.IsNullOrEmpty(nextAttackTrigger))
            {
                animator.SetTrigger(nextAttackTrigger);
                CombatManager.instance.ConsumeInput();
                CombatManager.instance.DisableInput();
            }
        }
        else if (timer > inputTimeout)
        {
            // زمان ورودی تمام شد، ریست کنیم
            CombatManager.instance.attackInputCount = 0;
            CombatManager.instance.DisableInput();
        }
    }
}