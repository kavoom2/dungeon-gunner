using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerDetails_",
    menuName = "Scriptable Objects/Player/Player Details"
)]
public class PlayerDetailsSO : ScriptableObject
{
    #region Header
    [Space(10)]
    [Header("Player Base 상세")]
    #endregion
    #region Tooltip
    [Tooltip("플레이어 캐릭터의 이름")]
    #endregion
    public string playerCharacterName;

    #region Tooltip
    [Tooltip("플레이어 캐릭터 Prefab")]
    #endregion
    public GameObject playerPrefab;

    #region Tooltip
    [Tooltip("Runtime Animator Controller")]
    #endregion
    public RuntimeAnimatorController runtimeAnimatorController;

    #region Header
    [Space(10)]
    [Header("플레이어 시작 시 소지 체력")]
    #endregion
    public int playerHealthAmount;

    #region Header
    [Space(10)]
    [Header("기타 플레이어 상세")]
    #endregion
    #region Tooltip
    [Tooltip("플레이어의 미니맵 아이콘 Sprite")]
    #endregion
    public Sprite playerMinimapIcon;

    #region Tooltip
    [Tooltip("플레이어의 손 Sprite")]
    #endregion
    public Sprite playerHandSprite;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(
            this,
            nameof(playerCharacterName),
            playerCharacterName
        );
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerPrefab), playerPrefab);
        HelperUtilities.ValidateCheckPositiveValue(
            this,
            nameof(playerHealthAmount),
            playerHealthAmount,
            false
        );
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerMinimapIcon), playerMinimapIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerHandSprite), playerHandSprite);
        HelperUtilities.ValidateCheckNullValue(
            this,
            nameof(runtimeAnimatorController),
            runtimeAnimatorController
        );
    }
#endif
    #endregion
}
