using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemachineTarget : MonoBehaviour
{
    private CinemachineTargetGroup cinemachineTargetGroup;

    #region Tooltip
    [Tooltip("마우스 커서의 위치를 따라다니는 Target Transform입니다.")]
    #endregion
    [SerializeField]
    private Transform cursorTarget;

    private void Awake()
    {
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
    }

    private void Start()
    {
        SetCinemachineTargetGroup();
    }

    private void Update()
    {
        cursorTarget.position = HelperUtilities.GetMouseWorldPosition();
    }

    private void SetCinemachineTargetGroup()
    {
        // CinemachineTargetGroup의 Target을 생성합니다. (플레이어)
        CinemachineTargetGroup.Target cinemachineGroupTarget_player =
            new()
            {
                target = GameManager.Instance.GetPlayer().transform,
                weight = 1f,
                radius = 2.5f
            };

        // CinemachineTargetGroup의 Target을 생성합니다. (마우스 커서)
        CinemachineTargetGroup.Target cinemachineGroupTarget_cursor =
            new()
            {
                target = cursorTarget,
                weight = 0.5f,
                radius = 1f
            };

        CinemachineTargetGroup.Target[] cinemachineTargetArray = new CinemachineTargetGroup.Target[]
        {
            cinemachineGroupTarget_player,
            cinemachineGroupTarget_cursor
        };
        cinemachineTargetGroup.m_Targets = cinemachineTargetArray;
    }
}
