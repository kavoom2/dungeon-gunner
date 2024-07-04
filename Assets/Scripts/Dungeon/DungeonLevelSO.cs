using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    #region Header
    [Space(10)]
    [Header("Level 기본 정보")]
    #endregion
    #region Tooltip
    [Tooltip("레벨의 이름을 지정합니다.")]
    #endregion
    public string levelName;

    #region Header
    [Space(10)]
    [Header("Level의 Room Template 목록")]
    #endregion
    #region Tooltip
    [Tooltip(
        "이 레벨의 던전을 생성하기 위해 사용하는 Room Template 목록입니다. Room Node Graph 상에 있는 모든 Room Node의 Room Node Type에 대응하는 Room Template이 하나 이상 존재해야 합니다."
    )]
    #endregion
    public List<RoomTemplateSO> roomTemplateList;

    #region Header
    [Header("Level의 Room Node Graph 목록")]
    #endregion
    #region Tooltip
    [Tooltip("이 레벨의 던전을 생성하기 위해 사용하는 Room Node Graph 목록입니다.")]
    #endregion
    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        HelperUtilities.ValidateCheckEnumerableValues(
            this,
            nameof(roomTemplateList),
            roomNodeGraphList
        );
        HelperUtilities.ValidateCheckEnumerableValues(
            this,
            nameof(roomNodeGraphList),
            roomNodeGraphList
        );

        // Room Template에 특정 Node Type이 누락되었는지 확인합니다. (Room Node Graph에 있는지 확인하지 않습니다.)
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (roomTemplate == null)
            {
                return;
            }

            if (roomTemplate.roomNodeType.isCorridorEW)
            {
                isEWCorridor = true;
            }

            if (roomTemplate.roomNodeType.isCorridorNS)
            {
                isNSCorridor = true;
            }

            if (roomTemplate.roomNodeType.isEntrance)
            {
                isEntrance = true;
            }
        }

        if (isEWCorridor == false)
        {
            Debug.Log($"{this.name}에 E/W 방향의 Room Node Type이 없습니다.");
        }

        if (isNSCorridor == false)
        {
            Debug.Log($"{this.name}에 N/S 방향의 Room Node Type이 없습니다.");
        }

        if (isEntrance == false)
        {
            Debug.Log($"{this.name}에 Entrance Room Node Type이 없습니다.");
        }

        // Room Node Graph의 유효성을 확인합니다.
        foreach (RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
            {
                return;
            }

            foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
            {
                if (roomNode == null)
                {
                    continue;
                }

                // Corridor와 Entrance는 이미 확인하였으므로 생략합니다.
                if (
                    roomNode.roomNodeType.isEntrance
                    || roomNode.roomNodeType.isCorridorEW
                    || roomNode.roomNodeType.isCorridorNS
                    || roomNode.roomNodeType.isCorridor
                    || roomNode.roomNodeType.isNone
                )
                {
                    continue;
                }

                // Room Node Type에 대응하는 Room Template이 있는지 확인합니다.
                bool isRoomNodeTypeFound = false;
                foreach (RoomTemplateSO roomTemplate in roomTemplateList)
                {
                    if (roomTemplate.roomNodeType == roomNode.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }

                if (isRoomNodeTypeFound == false)
                {
                    Debug.Log(
                        $"{this.name}에 {roomNode.roomNodeType.name} Room Node Type에 대응하는 Room Template이 없습니다."
                    );
                }
            }
        }
    }
#endif
    #endregion
}
