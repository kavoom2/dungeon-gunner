using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    private static GameResources _instance;

    public static GameResources Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameResources>("GameResources");
            }

            return _instance;
        }
    }

    #region Header
    [Space(10)]
    [Header("던전")]
    #endregion
    #region Tooltip
    [Tooltip("던전을 생성할 때 사용되는 모든 RoomNodeTypeSO를 관리하는 리스트입니다.")]
    #endregion
    public RoomNodeTypeListSO roomNodeTypeList;

    #region Header
    [Space(10)]
    [Header("플레이어")]
    #endregion
    #region Tooltip
    [Tooltip("현재 선택한 플레이어의 기본 상세를 관리하는 ScriptableObject입니다.")]
    #endregion
    public CurrentPlayerSO currentPlayer;

    #region Header
    [Space(10)]
    [Header("머터리얼")]
    #endregion
    public Material dimmedMaterial;

    public Material litMaterial;

    public Shader variableLitShader;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeTypeList), roomNodeTypeList);
        HelperUtilities.ValidateCheckNullValue(this, nameof(currentPlayer), currentPlayer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(dimmedMaterial), dimmedMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(litMaterial), litMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(variableLitShader), variableLitShader);
    }
#endif
    #endregion
}
