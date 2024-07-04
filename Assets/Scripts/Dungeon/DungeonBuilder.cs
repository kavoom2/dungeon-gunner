using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonoBehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList = null;
    private bool dungeonBuildSuccessful = false;

    protected override void Awake()
    {
        base.Awake();

        // Room Node Type 목록을 불러옵니다.
        LoadRoomNodeTypeList();
    }

    private void OnEnable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 0f);
    }

    private void OnDisable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    /// <summary>
    /// Room Node Type 목록을 불러옵니다.
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// 현재 레벨의 Dungeon을 생성합니다.
    /// </summary>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        LoadRoomTemplatesIntoDictionary();

        // 던전 생성 변수 초기화
        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            // 임의의 Room Node Graph를 선택하여 던전을 생성합니다.
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(
                currentDungeonLevel.roomNodeGraphList
            );

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            while (
                !dungeonBuildSuccessful
                && dungeonRebuildAttemptsForNodeGraph
                    <= Settings.maxDungeonRebuildAttemptsForNodeGraph
            )
            {
                ClearDungeon();
                dungeonRebuildAttemptsForNodeGraph++;

                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }
        }

        if (dungeonBuildSuccessful)
        {
            InstantiateRoomGameObjects();
        }

        return dungeonBuildSuccessful;
    }

    /// <summary>
    /// Room Template 목록을 roomTemplateDictionary에 추가합니다.
    /// </summary>
    private void LoadRoomTemplatesIntoDictionary()
    {
        // Room Template Dictionary를 초기화합니다.
        roomTemplateDictionary.Clear();

        // Room Template List를 Dictionary에 추가합니다.
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.LogError(
                    $"Room Template 목록에 중복된 GUID의 Room Template이 있습니다. GUID: {roomTemplate.guid}"
                );
            }
        }
    }

    /// <summary>
    /// Room Node Graph를 이용하여 임의의 Room Template으로 구성된 던전을 생성합니다.
    /// </summary>
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new();

        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(
            roomNodeTypeList.list.Find((roomNodeType) => roomNodeType.isEntrance)
        );

        if (entranceNode == null)
        {
            Debug.LogError(
                $"Room Node Graph ({roomNodeGraph.name})은 Entrance Type Room Node를 가지고 있지 않습니다."
            );

            return false;
        }

        bool noRoomOverlaps = true;

        openRoomNodeQueue.Enqueue(entranceNode);
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(
            roomNodeGraph,
            openRoomNodeQueue,
            noRoomOverlaps
        );

        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }

        return false;
    }

    private bool ProcessRoomsInOpenRoomNodeQueue(
        RoomNodeGraphSO roomNodeGraph,
        Queue<RoomNodeSO> openRoomNodeQueue,
        bool noRoomOverlaps
    )
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            // Entrance Room Node인 경우
            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);
                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(roomNode.id, room);
            }
            // 이 외의 Room Node Type인 경우
            else
            {
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRootNodeIDList[0]];
                noRoomOverlaps = CanPlaceRoomWithoutOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithoutOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverlaps = true;

        while (roomOverlaps)
        {
            List<Doorway> unconnectedAvailableParentDoorwayList = GetUnconnectedAvailableDoorways(
                    parentRoom.doorwayList
                )
                .ToList();

            if (unconnectedAvailableParentDoorwayList.Count == 0)
            {
                return false;
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorwayList[
                UnityEngine.Random.Range(0, unconnectedAvailableParentDoorwayList.Count)
            ];

            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(
                roomNode,
                doorwayParent
            );

            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;
                room.isPositioned = true;
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }

        return !roomOverlaps;
    }

    /// <summary>
    /// Parent Doorway의 Orientation을 고려하여 Room Template을 반환합니다.
    /// </summary>
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(
        RoomNodeSO roomNode,
        Doorway doorwayParent
    )
    {
        RoomTemplateSO roomTemplate = null;

        // Corridor인 경우, parentDoorway의 Orientation에 따라 적절한 템플릿을 선택합니다.
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(
                        roomNodeTypeList.list.Find((roomNodeType) => roomNodeType.isCorridorNS)
                    );
                    break;
                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(
                        roomNodeTypeList.list.Find((roomNodeType) => roomNodeType.isCorridorEW)
                    );
                    break;
                case Orientation.none:
                default:
                    break;
            }
        }
        // 이 외의 경우, Room Template을 반환합니다.
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomTemplate;
    }

    /// <summary>
    /// Room을 배치하고 성공 여부를 반환합니다.
    /// </summary>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorwayList);

        if (doorway == null)
        {
            doorwayParent.isUnavailable = true;
            return false;
        }

        Vector2Int parentDoorwayPosition =
            parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        // 이미 배치된 parentRoom을 기준으로 배치해야 하므로, 1 크기만큼 위치를 재조정하여 배치합니다.
        Vector2Int adjustment = Vector2Int.zero;
        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.none:
            default:
                break;
        }

        // parentDoorwayPosition + adjustment = doorway.position - room.templateLowerBounds + room.lowerBounds 이므로
        //  - room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position
        //  - room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds
        // 으로 계산할 수 있습니다.
        room.lowerBounds =
            parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);
        if (overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;
            doorway.isConnected = true;
            doorway.isUnavailable = true;

            return true;
        }

        doorwayParent.isUnavailable = true;
        return false;
    }

    private Doorway GetOppositeDoorway(Doorway doorwayParent, List<Doorway> doorwayList)
    {
        foreach (Doorway doorwayToCheck in doorwayList)
        {
            if (
                doorwayParent.orientation == Orientation.east
                && doorwayToCheck.orientation == Orientation.west
            )
            {
                return doorwayToCheck;
            }

            if (
                doorwayParent.orientation == Orientation.west
                && doorwayToCheck.orientation == Orientation.east
            )
            {
                return doorwayToCheck;
            }

            if (
                doorwayParent.orientation == Orientation.north
                && doorwayToCheck.orientation == Orientation.south
            )
            {
                return doorwayToCheck;
            }

            if (
                doorwayParent.orientation == Orientation.south
                && doorwayToCheck.orientation == Orientation.north
            )
            {
                return doorwayToCheck;
            }
        }

        return null;
    }

    /// <summary>
    /// 겹치는 Room이 있는지 확인합니다. (lowerBounds, upperBounds로 평가합니다.)
    /// 겹치는 Room이 있으면 해당 Room을 반환합니다.
    /// </summary>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;
            if (room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }

            if (IsOverlappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        return null;
    }

    /// <summary>
    /// Room이 서로 겹치는지 확인합니다.
    /// </summary>
    private bool IsOverlappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = IsOverlappingInterval(
            room1.lowerBounds.x,
            room1.upperBounds.x,
            room2.lowerBounds.x,
            room2.upperBounds.x
        );

        bool isOverlappingY = IsOverlappingInterval(
            room1.lowerBounds.y,
            room1.upperBounds.y,
            room2.lowerBounds.y,
            room2.upperBounds.y
        );

        return isOverlappingX && isOverlappingY;
    }

    private bool IsOverlappingInterval(int min1, int max1, int min2, int max2)
    {
        if (Mathf.Max(min1, min2) <= Mathf.Min(max1, max2))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Room Node Type으로 임의의 Room Template을 반환합니다.
    /// </summary>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        if (matchingRoomTemplateList.Count == 0)
        {
            Debug.LogError($"Room Node Type ({roomNodeType.name})에 해당하는 Room Template이 없습니다.");
            return null;
        }

        return matchingRoomTemplateList[
            UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)
        ];
    }

    /// <summary>
    /// Room의 Doorway 중 연결되지 않고 사용 가능한 Doorway를 반환합니다.
    /// </summary>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> doorwayList)
    {
        foreach (Doorway doorway in doorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }

    /// <summary>
    /// Room Template과 Room Node를 이용하여 Room을 생성합니다.
    /// </summary>
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new();

        room.id = roomNode.id;
        room.templateID = roomTemplate.guid;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomNode.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorwayList = CopyDoorwayList(roomTemplate.doorwayList);

        // Entrance Room Node인 경우
        if (roomNode.parentRootNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;

            GameManager.Instance.SetCurrentRoom(room);
        }
        else
        {
            room.parentRoomID = roomNode.parentRootNodeIDList[0];
        }

        return room;
    }

    /// <summary>
    /// Room Node Graph 목록 중 랜덤하게 하나를 선택합니다.
    /// </summary>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }

        Debug.LogError("Room Node Graph 목록이 비어있습니다.");
        return null;
    }

    /// <summary>
    /// List를 Deep Copy하여 반환합니다.
    /// </summary>
    private List<string> CopyStringList(List<string> targetList)
    {
        List<string> copiedList = new();

        foreach (string item in targetList)
        {
            copiedList.Add(item);
        }

        return copiedList;
    }

    /// <summary>
    /// Room GameObject를 인스턴스화합니다.
    /// </summary>
    private void InstantiateRoomGameObjects()
    {
        foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            // Room의 위치를 계산합니다. (roomTemplateLowerBounds로 동일 좌표계에 위치하도록 합니다.)
            Vector3 roomPosition =
                new(
                    room.lowerBounds.x - room.templateLowerBounds.x,
                    room.lowerBounds.y - room.templateLowerBounds.y,
                    0f
                );

            GameObject roomGameObject = Instantiate(
                room.prefab,
                roomPosition,
                Quaternion.identity,
                transform
            );

            InstantiatedRoom instantiatedRoom =
                roomGameObject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;
            instantiatedRoom.Initialize(roomGameObject);
            room.instantiatedRoom = instantiatedRoom;
        }
    }

    private List<Doorway> CopyDoorwayList(List<Doorway> targetList)
    {
        List<Doorway> copiedList = new();

        foreach (Doorway item in targetList)
        {
            Doorway clonedItem = new();

            clonedItem.position = item.position;
            clonedItem.orientation = item.orientation;
            clonedItem.doorPrefab = item.doorPrefab;
            clonedItem.isConnected = item.isConnected;
            clonedItem.isUnavailable = item.isUnavailable;
            clonedItem.doorwayStartCopyPosition = item.doorwayStartCopyPosition;
            clonedItem.doorwayCopyTileWidth = item.doorwayCopyTileWidth;
            clonedItem.doorwayCopyTileHeight = item.doorwayCopyTileHeight;

            copiedList.Add(clonedItem);
        }

        return copiedList;
    }

    /// <summary>
    /// Room Template ID에 해당하는 Room Template을 반환합니다.
    /// </summary>
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }

        return null;
    }

    /// <summary>
    /// Room ID로 해당 Room을 반환합니다.
    /// </summary>
    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }

        return null;
    }

    /// <summary>
    /// Dungeon Room GameObject와 DungeonBuilderRoomDictionary를 초기화합니다.
    /// </summary>
    private void ClearDungeon()
    {
        foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            if (room.instantiatedRoom != null)
            {
                Destroy(room.instantiatedRoom.gameObject);
            }
        }

        dungeonBuilderRoomDictionary.Clear();
    }
}
