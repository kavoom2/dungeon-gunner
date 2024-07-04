using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class DungeonTemplateSO : ScriptableObject
{
    [HideInInspector]
    public string guid;

    #region Header
    [Space(10)]
    [Header("Room Prefab")]
    #endregion
    #region Tooltip
    [Tooltip("Room과 Environment에 관한 TileMaps를 가지고 있는 Room Prefab입니다.")]
    #endregion
    public GameObject prefab;

    [HideInInspector] // Prefab 변경 여부로 GUID를 재발급하기 위해 사용합니다.
    public GameObject previousPrefab;

    #region Header
    [Space(10)]
    [Header("Room 구성 요소")]
    #endregion
    #region Tooltip
    [Tooltip(
        "RoomNodeTypeSO(RoomNodeType Scriptable Object)입니다. Room Node Graph에서 사용되는 Room Node에 해당하는 Room Type입니다. (단, Corridor는 방향에 따라 CorridorNS 또는 CorridorEW로 구분됩니다.)"
    )]
    #endregion
    public RoomNodeTypeSO roomNodeType;

    #region Tooltip
    [Tooltip("lowerBounds는 Room 영역 외부 테두리의 왼쪽 하단 모서리에 해당합니다. (Note: World 좌표계가 아닌, Local 좌표계입니다.)")]
    #endregion
    public Vector2Int lowerBounds;

    #region Tooltip
    [Tooltip("lowerBounds는 Room 영역 외부 테두리의 우측 상단 모서리에 해당합니다. (Note: World 좌표계가 아닌, Local 좌표계입니다.)")]
    #endregion
    public Vector2Int upperBounds;

    #region Tooltip
    [Tooltip("최대 4개의 방향으로 연결할 수 있는 Doorway List입니다.")]
    #endregion
    [SerializeField]
    private List<Doorway> doorwayList;

    #region Tooltip
    [Tooltip("Enemy 또는 Chest가 생성될 수 있는 타일맵 좌표입니다.")]
    #endregion
    public Vector2Int[] spawnPositionArray;

    /// <summary>
    /// Room Template의 Doorway List를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);
        HelperUtilities.ValidateCheckEnumerableValues(
            this,
            nameof(spawnPositionArray),
            spawnPositionArray
        );
    }
#endif
    #endregion
}
