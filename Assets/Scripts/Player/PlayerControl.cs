using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region Tooltip
    [Tooltip("이동 속도 등 플레이어의 Movement 상세 정보 Scriptable Object입니다.")]
    #endregion
    [SerializeField]
    private MovementDetailsSO movementDetails;

    #region Tooltip
    [Tooltip("Player의 Weapon Shoot Position GameObject입니다.")]
    #endregion
    [SerializeField]
    private Transform weaponShootPosition;

    private Player player;
    private float moveSpeed;
    private Coroutine playerRollCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate;
    private bool isPlayerRolling;
    private float playerRollCooldownTimer = 0f;

    private void Awake()
    {
        player = GetComponent<Player>();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();

        SetPlayerAnimationSpeed();
    }

    private void SetPlayerAnimationSpeed()
    {
        player.animator.speed = moveSpeed / Settings.baseSpeedForPlayerAnimation;
    }

    private void Update()
    {
        if (isPlayerRolling)
        {
            return;
        }

        // 플레이어의 이동 입력을 처리합니다.
        MovementInput();

        // 플레이어의 무기 입력을 처리합니다.
        WeaponInput();

        // 구르기 쿨다운 타이머를 업데이트합니다.
        PlayerRollCooldownTimer();
    }

    private void MovementInput()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");
        bool spaceKeyDown = Input.GetKeyDown(KeyCode.Space);

        Vector2 direction = new Vector2(horizontalMovement, verticalMovement).normalized;
        if (direction != Vector2.zero)
        {
            if (!spaceKeyDown)
            {
                player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
            }
            else if (playerRollCooldownTimer <= 0f)
            {
                PlayerRoll((Vector3)direction);
            }
        }
        else
        {
            player.idleEvent.CallIdleEvent();
        }
    }

    private void PlayerRoll(Vector3 direction)
    {
        playerRollCoroutine = StartCoroutine(PlayerRollRoutine(direction));
    }

    private IEnumerator PlayerRollRoutine(Vector3 direction)
    {
        float minDistance = 0.2f;
        isPlayerRolling = true;
        Vector3 targetPosition =
            player.transform.position + (Vector3)direction * movementDetails.rollDistance;

        while (Vector3.Distance(player.transform.position, targetPosition) > minDistance)
        {
            player
                .movementToPositionEvent
                .CallMovementToPositionEvent(
                    targetPosition,
                    player.transform.position,
                    movementDetails.rollSpeed,
                    direction,
                    isPlayerRolling
                );

            // 다음 FixedUpdate까지 대기합니다.
            yield return waitForFixedUpdate;
        }

        isPlayerRolling = false;
        playerRollCooldownTimer = movementDetails.rollCooldownTime;
        player.transform.position = targetPosition;
    }

    private void PlayerRollCooldownTimer()
    {
        if (playerRollCooldownTimer >= 0f)
        {
            playerRollCooldownTimer -= Time.deltaTime;
        }
    }

    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees,
            playerAngleDegrees;
        AimDirection playerAimDirection;

        // Aim Weapon Input을 처리합니다.
        AimWeaponInput(
            out weaponDirection,
            out weaponAngleDegrees,
            out playerAngleDegrees,
            out playerAimDirection
        );

        player
            .aimWeaponEvent
            .CallAimWeaponEvent(
                playerAimDirection,
                playerAngleDegrees,
                weaponAngleDegrees,
                weaponDirection
            );
    }

    private void AimWeaponInput(
        out Vector3 weaponDirection,
        out float weaponAngleDegrees,
        out float playerAngleDegrees,
        out AimDirection playerAimDirection
    )
    {
        // 마우스의 World Position을 계산합니다.
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        // Weapon 방향의 Normalized Vector를 계산합니다.
        weaponDirection = (mouseWorldPosition - weaponShootPosition.position).normalized;

        // 플ㄹ레이어의 방향을 계산합니다.
        Vector3 playerDirection = (mouseWorldPosition - transform.position).normalized;

        // Weapon의 각도를 계산합니다.
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // Player의 각도를 계산합니다.
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        // Player의 Aim 방향을 계산합니다. (Sprite, Animation, Positioning 등에 사용됩니다.)
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StopPlayerRollRoutine();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        StopPlayerRollRoutine();
    }

    private void StopPlayerRollRoutine()
    {
        if (playerRollCoroutine != null)
        {
            StopCoroutine(playerRollCoroutine);
            isPlayerRolling = false;
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion
}
