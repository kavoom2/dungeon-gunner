using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RoomNodeGraph",
    menuName = "Scriptable Objects/Dungeon/Room Node Graph"
)]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector]
    public RoomNodeTypeListSO roomNodeTypeList;

    [HideInInspector]
    public List<RoomNodeSO> roomNodeList = new();

    [HideInInspector]
    public Dictionary<string, RoomNodeSO> roomNodeDictionary = new();

    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    /// <summary>
    /// Room Node Dictionary를 불러옵니다.
    /// </summary>
    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();

        foreach (RoomNodeSO roomNode in roomNodeList)
        {
            roomNodeDictionary.Add(roomNode.id, roomNode);
        }
    }

    /// <summary>
    /// RoomNodeTypeSO에 해당하는 RoomNodeSO를 반환합니다.
    /// </summary>
    public RoomNodeSO GetRoomNode(RoomNodeTypeSO roomNodeType)
    {
        foreach (RoomNodeSO roomNode in roomNodeList)
        {
            if (roomNode.roomNodeType == roomNodeType)
            {
                return roomNode;
            }
        }

        return null;
    }

    public RoomNodeSO GetRoomNode(string roomNodeID)
    {
        if (roomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode))
        {
            return roomNode;
        }

        return null;
    }

    /// <summary>
    /// 부모 RoomNodeSO에 연결된 자식 RoomNodeSO들을 반환합니다.
    /// </summary>
    public IEnumerable<RoomNodeSO> GetChildRoomNodes(RoomNodeSO parentRoomNode)
    {
        foreach (string childNodeID in parentRoomNode.childRoomNodeIDList)
        {
            yield return GetRoomNode(childNodeID);
        }
    }

    #region Editor Only
#if UNITY_EDITOR
    [HideInInspector]
    public RoomNodeSO roomNodeToDrawLineFrom = null;

    [HideInInspector]
    public Vector2 linePosition;

    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO roomNode, Vector2 position)
    {
        roomNodeToDrawLineFrom = roomNode;
        linePosition = position;
    }
#endif
    #endregion Editor Only
}
