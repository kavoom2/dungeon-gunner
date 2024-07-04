using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class Door : MonoBehaviour
{
    #region
    [Space(10)]
    [Header("Object 레퍼런스")]
    #endregion
    #region
    [Tooltip("Door Collider GameObject의 BoxCollider2D 컴포넌트입니다.")]
    #endregion
    [SerializeField]
    private BoxCollider2D doorCollider;

    [HideInInspector]
    public bool isBossRoomDoor = false;

    private BoxCollider2D doorTrigger;
    private bool isOpen = false;
    private bool previousIsOpen = false;
    private Animator animator;

    private void Awake()
    {
        // Door Collider는 초기에 비활성화 상태로 시작합니다.
        doorCollider.enabled = false;

        animator = GetComponent<Animator>();
        doorTrigger = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (
            collision.CompareTag(Settings.playerTag)
            || collision.CompareTag(Settings.playerWeaponTag)
        )
        {
            OpenDoor();
        }
    }

    private void OnEnable()
    {
        // 부모 GameObject가 비활성화 상태에서 활성화 상태로 전환될 때, Animator의 이전 상태를 유지합니다.
        animator.SetBool(Settings.open, isOpen);
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            previousIsOpen = true;
            doorCollider.enabled = false;
            doorTrigger.enabled = false;

            animator.SetBool(Settings.open, true);
        }
    }

    public void LockDoor()
    {
        isOpen = false;
        doorCollider.enabled = true;
        doorTrigger.enabled = false;

        animator.SetBool(Settings.open, false);
    }

    public void UnlockDoor()
    {
        doorCollider.enabled = false;
        doorTrigger.enabled = true;

        if (previousIsOpen)
        {
            isOpen = false;
            OpenDoor();
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorCollider), doorCollider);
    }
#endif
    #endregion
}
