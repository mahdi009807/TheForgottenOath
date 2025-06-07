using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;

    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    [ContextMenu("Open")]
    public void Open()
    {
         animator.SetTrigger("Open");
    }
}
