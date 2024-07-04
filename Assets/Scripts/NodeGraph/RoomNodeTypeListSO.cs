using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RoomNodeTypeList",
    menuName = "Scriptable Objects/Dungeon/Room Node Type List"
)]
public class RoomNodeTypeListSO : ScriptableObject
{
    #region Header ROOM NODE TYPE LIST
    [Space(10)]
    [Header("Room Node Type 리스트")]
    #endregion Header
    #region Tooltip
    [Tooltip(
        "Game 내에서 사용되는 모든 RoomNodeTypeSO 리스트입니다. Enum 타입보다 정보를 효과적으로 사용하기 위해 클래스로 구성된 Type입니다."
    )]
    #endregion Tooltip

    public List<RoomNodeTypeSO> list = new();

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
#endif
    #endregion Validation
}
