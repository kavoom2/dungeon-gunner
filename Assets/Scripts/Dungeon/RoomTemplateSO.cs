using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector]
    public string guid;

    #region Header
    [Space(10)]
    [Header("Room Prefab")]
    #endregion
    #region Tooltip
    [Tooltip("Room의 Game Object Prefab입니다. (방의 모든 TileMap과 Environment Game Objects을 담고 있습니다.)")]
    #endregion
    public GameObject prefab;

    [HideInInspector]
    // this is used to regenerate the guid if the so is copied and the prefab is changed
    public GameObject previousPrefab;

    #region Header
    [Space(10)]
    [Header("Room 구성 요소")]
    #endregion
    #region Tooltip
    [Tooltip(
        "Room Type Node Scriptable Object입니다. Room Node Type은 Room Node Graph에서 사용되는 Room Node에 해당합니다. 통로의 경우 예외입니다. Room Node Graph에는 '통로'라는 하나의 통로 유형만 있습니다. 방 템플릿의 경우 2개의 통로 노드 유형이 있습니다 -- CorridorNS & CorridorEW."
    )]
    #endregion
    public RoomNodeTypeSO roomNodeType;

    #region Tooltip
    [Tooltip(
        "방 타일맵을 완전히 둘러싼 직사각형을 상상해보세요. 방 타일맵의 하단 왼쪽 모서리를 완전히 둘러싸는 직사각형의 하단 왼쪽 모서리를 나타냅니다. 이는 타일맵의 그리드 위치를 사용하여 방의 타일맵에서 결정해야 합니다. (해당 좌표 브러시 포인터를 사용하여 해당 좌표의 타일맵 그리드 위치를 가져옵니다(참고: 이는 월드 위치가 아닌 로컬 타일맵 위치입니다."
    )]
    #endregion
    public Vector2Int lowerBounds;

    #region Tooltip
    [Tooltip(
        "방 타일맵을 완전히 둘러싼 직사각형을 상상해보세요. 방 타일맵의 하단 왼쪽 모서리를 완전히 둘러싸는 직사각형의 오른쪽 상단 모서리를 나타냅니다. 이는 타일맵의 그리드 위치를 사용하여 방의 타일맵에서 결정해야 합니다. (해당 좌표 브러시 포인터를 사용하여 해당 좌표의 타일맵 그리드 위치를 가져옵니다(참고: 이는 월드 위치가 아닌 로컬 타일맵 위치입니다."
    )]
    #endregion
    public Vector2Int upperBounds;

    #region Tooltip
    [Tooltip("방에는 최대 네 개의 문이 있어야 합니다. 각각의 방위에 하나씩이어야 하며, 가운데 타일 위치가 문 위치 'position'이어야 합니다.")]
    #endregion
    [SerializeField]
    public List<Doorway> doorwayList;

    #region Tooltip
    [Tooltip("방에 대한 각 가능한 스폰 위치(적 및 상자에 사용)를 타일맵 좌표로 이 배열에 추가해야 합니다.")]
    #endregion
    public Vector2Int[] spawnPositionArray;

    /// <summary>
    /// 이 방의 Entrance 목록을 반환합니다.
    /// </summary>
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation

#if UNITY_EDITOR
    // Validate SO fields
    private void OnValidate()
    {
        // Set unique GUID if empty or the prefab changes
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        // Check spawn positions populated
        HelperUtilities.ValidateCheckEnumerableValues(
            this,
            nameof(spawnPositionArray),
            spawnPositionArray
        );
    }

#endif

    #endregion Validation
}
