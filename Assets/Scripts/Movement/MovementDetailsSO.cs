using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MovementDetails_",
    menuName = "Scriptable Objects/Movement/MovementDetails"
)]
public class MovementDetailsSO : ScriptableObject
{
    #region Header
    [Space(10)]
    [Header("Movement 상세 정보")]
    #endregion
    #region Tooltip
    [Tooltip("최소 이동 속도입니다. GetMoveSpeed() 함수는 최소 - 최대 사이의 임의의 값을 반환합니다.")]
    #endregion
    public float minMoveSpeed = 8f;

    #region Tooltip
    [Tooltip("최대 이동 속도입니다. GetMoveSpeed() 함수는 최소 - 최대 사이의 임의의 값을 반환합니다.")]
    #endregion
    public float maxMoveSpeed = 8f;

    #region Tooltip
    [Tooltip("구르기 속도입니다.")]
    #endregion
    public float rollSpeed;

    #region Tooltip
    [Tooltip("구르기 이동 거리입니다.")]
    #endregion
    public float rollDistance;

    #region Tooltip
    [Tooltip("구르기 쿨다운 시간입니다.")]
    #endregion
    public float rollCooldownTime;

    /// <summary>
    /// 최소 - 최대 사이의 임의의 이동 속도 값을 반환합니다.
    /// </summary>
    public float GetMoveSpeed()
    {
        if (minMoveSpeed == maxMoveSpeed)
        {
            return minMoveSpeed;
        }

        return Random.Range(minMoveSpeed, maxMoveSpeed);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 이동 속도 유효성 검사
        HelperUtilities.ValidateCheckPositiveRange(
            this,
            nameof(minMoveSpeed),
            minMoveSpeed,
            nameof(maxMoveSpeed),
            maxMoveSpeed,
            false
        );

        // 구르기 유효성 검사
        if (rollSpeed != 0 || rollDistance != 0 || rollCooldownTime != 0)
        {
            HelperUtilities.ValidateCheckPositiveValue(
                this,
                nameof(rollDistance),
                rollDistance,
                false
            );
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollSpeed), rollSpeed, false);
            HelperUtilities.ValidateCheckPositiveValue(
                this,
                nameof(rollCooldownTime),
                rollCooldownTime,
                false
            );
        }
    }
#endif
    #endregion
}
