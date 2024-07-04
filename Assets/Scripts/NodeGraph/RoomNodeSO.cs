using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector]
    public string id;

    [HideInInspector]
    public List<string> parentRootNodeIDList = new();

    [HideInInspector]
    public List<string> childRoomNodeIDList = new();

    [HideInInspector]
    public RoomNodeGraphSO roomNodeGraph;

    public RoomNodeTypeSO roomNodeType;

    [HideInInspector]
    public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Only
#if UNITY_EDITOR
    [HideInInspector]
    public Rect rect;

    [HideInInspector]
    public bool isLeftClickDragging = false;

    [HideInInspector]
    public bool isSelected = false;

    /// <summary>
    /// Room Node를 초기화합니다.
    /// </summary>
    public void Initialize(Rect rect, RoomNodeGraphSO roomNodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "Room Node";
        this.roomNodeGraph = roomNodeGraph;
        this.roomNodeType = roomNodeType;

        // Room Node Type 목록을 불러옵니다.
        this.roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle roomNodeStyle)
    {
        // Node Box를 그립니다.
        GUILayout.BeginArea(rect, roomNodeStyle);

        // Popup Selection Change가 발생하는지 확인합니다.
        EditorGUI.BeginChangeCheck();

        // Room Node가 Entrance 타입이거나 부모 노드가 있는 경우 레이블을 노출하며,
        // 그렇지 않으면 Select 팝업을 노출합니다.
        if (parentRootNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            int selectedIndex = roomNodeTypeList.list.FindIndex(item => item == roomNodeType);
            int nextSelectedIndex = EditorGUILayout.Popup(
                "",
                selectedIndex,
                GetRoomNodeTypesToDisplay()
            );

            roomNodeType = roomNodeTypeList.list[nextSelectedIndex];

            // Room Type이 변경되었을 때 유효하지 않는 연결인 경우 연결을 제거합니다.
            if (
                (
                    roomNodeTypeList.list[selectedIndex].isCorridor
                    && !roomNodeTypeList.list[nextSelectedIndex].isCorridor
                )
                || (
                    !roomNodeTypeList.list[selectedIndex].isCorridor
                    && roomNodeTypeList.list[nextSelectedIndex].isCorridor
                )
                || (
                    !roomNodeTypeList.list[selectedIndex].isBossRoom
                    && roomNodeTypeList.list[nextSelectedIndex].isBossRoom
                )
            )
            {
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(
                            childRoomNodeIDList[i]
                        );

                        if (childRoomNode != null)
                        {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                            childRoomNode.RemoveParentRootNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Room Node Type 목록 중에서 Room Node Graph Editor에 표시할 Room Node Type 목록을 반환합니다.
    /// </summary>
    private string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomNodeTypeNameList = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomNodeTypeNameList[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomNodeTypeNameList;
    }

    /// <summary>
    /// Room Node의 UI 이벤트를 처리합니다.
    /// </summary>
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // Left Click
        if (currentEvent.button == 0)
        {
            ProcessMouseLeftDownEvent(currentEvent);
        }
        // Right Click
        else if (currentEvent.button == 1)
        {
            ProcessMouseRightDownEvent(currentEvent);
        }
    }

    private void ProcessMouseLeftDownEvent(Event currentEvent)
    {
        // UI 상에서 Node를 선택합니다.
        Selection.activeObject = this;
        isSelected = !isSelected;
    }

    private void ProcessMouseRightDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessMouseLeftClickUpEvent(currentEvent);
        }
    }

    private void ProcessMouseLeftClickUpEvent(Event currentEvent)
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Child Room Node ID를 Room Node에 추가합니다.
    /// </summary>
    public bool AddChildRoomNodeIDToRoomNode(string childRoomNodeID)
    {
        if (IsChildRoomValid(childRoomNodeID))
        {
            childRoomNodeIDList.Add(childRoomNodeID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Child Room Node가 추가할 수 있는 Node인지 확인합니다.
    /// </summary>
    public bool IsChildRoomValid(string childRoomNodeID)
    {
        bool isBossRoomNodeAlreadyConnected = false;
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRootNodeIDList.Count > 0)
            {
                isBossRoomNodeAlreadyConnected = true;
                break;
            }
        }

        // Child Room Node가 Boss Room이고 이미 연결된 Boss Room이 있는 경우, 추가하지 않습니다.
        if (
            roomNodeGraph.GetRoomNode(childRoomNodeID).roomNodeType.isBossRoom
            && isBossRoomNodeAlreadyConnected
        )
        {
            return false;
        }

        // Child Room Node가 None인 경우 추가하지 않습니다.
        if (roomNodeGraph.GetRoomNode(childRoomNodeID).roomNodeType.isNone)
        {
            return false;
        }

        // Child Room Node로 이미 추가된 경우, 추가하지 않습니다.
        if (childRoomNodeIDList.Contains(childRoomNodeID))
        {
            return false;
        }

        // 자기 자신을 Child Room Node로 추가하지 않습니다.
        if (childRoomNodeID == id)
        {
            return false;
        }

        // Parent Root Node로 이미 추가된 경우, 추가하지 않습니다.
        if (parentRootNodeIDList.Contains(childRoomNodeID))
        {
            return false;
        }

        // Parent Room Node는 하나만 가질 수 있습니다.
        if (roomNodeGraph.GetRoomNode(childRoomNodeID).parentRootNodeIDList.Count > 0)
        {
            return false;
        }

        // 복도와 복도를 연결할 수 없습니다.
        if (
            roomNodeGraph.GetRoomNode(childRoomNodeID).roomNodeType.isCorridor
            && roomNodeType.isCorridor
        )
        {
            return false;
        }

        // 하나의 방에 복도는 {maxChildCorridors}개까지만 연결할 수 있습니다.
        if (
            roomNodeGraph.GetRoomNode(childRoomNodeID).roomNodeType.isCorridor
            && childRoomNodeIDList.Count >= Settings.maxChildCorridors
        )
        {
            return false;
        }

        // Entrance는 최상위 노드이므로 Child Room Node로 추가할 수 없습니다.
        if (roomNodeGraph.GetRoomNode(childRoomNodeID).roomNodeType.isEntrance)
        {
            return false;
        }

        // 복도는 두 개의 방에 연결되며, 하나의 부모 노드와 하나의 자식 노드만 가질 수 있습니다.
        if (
            !roomNodeGraph.GetRoomNode(childRoomNodeID).roomNodeType.isCorridor
            && childRoomNodeIDList.Count > 0
        )
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Parent Root Node ID를 Room Node에 추가합니다.
    /// </summary>
    public bool AddParentRootNodeIDToRoomNode(string parentRootNodeID)
    {
        parentRootNodeIDList.Add(parentRootNodeID);
        return true;
    }

    /// <summary>
    /// Child Room Node ID를 Room Node에서 제거합니다.
    /// </summary>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childRoomNodeID)
    {
        if (childRoomNodeIDList.Contains(childRoomNodeID))
        {
            childRoomNodeIDList.Remove(childRoomNodeID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parent Root Node ID를 Room Node에서 제거합니다.
    /// </summary>
    public bool RemoveParentRootNodeIDFromRoomNode(string parentRootNodeID)
    {
        if (parentRootNodeIDList.Contains(parentRootNodeID))
        {
            parentRootNodeIDList.Remove(parentRootNodeID);
            return true;
        }

        return false;
    }
#endif
    #endregion Editor Only
}
