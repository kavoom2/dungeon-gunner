using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RoomNodeType_",
    menuName = "Scriptable Objects/Dungeon/Room Node Type"
)]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    #region Header
    [Header("Editor UI에서 선택 가능한 타입이면 체크해 주세요.")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;

    #region Header
    [Header("Corridor 타입인지 여부 (NS 또는 EW 중 하나를 체크해야 합니다.)")]
    #endregion Header
    public bool isCorridor;

    #region Header
    [Header("CorridorNS 타입인지 여부")]
    #endregion Header
    public bool isCorridorNS;

    #region Header
    [Header("CorridorEW 타입인지 여부")]
    #endregion Header
    public bool isCorridorEW;

    #region Header
    [Header("Entrance 타입인지 여부")]
    #endregion Header
    public bool isEntrance;

    #region Header
    [Header("Boss Room 타입인지 여부")]
    #endregion Header
    public bool isBossRoom;

    #region Header
    [Header("None(Unassigned) 타입인지 여부")]
    #endregion Header
    public bool isNone;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}
