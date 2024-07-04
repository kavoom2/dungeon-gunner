using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeTypeListSO roomNodeTypeList;
    private RoomNodeSO currentRoomNode = null;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    // Node Layout을 정의합니다.
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    // Connection Line을 정의합니다.
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;

    // Grid Layout을 정의합니다.
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // Unity Editor의 Inspector에서 선택된 Object가 변경된 경우 이벤트 리스너를 등록합니다.
        Selection.selectionChanged += InspectorSelectionChanged;

        // Room Node의 스타일을 정의합니다.
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // Room Node의 selected 스타일을 정의합니다.
        roomNodeSelectedStyle = new GUIStyle(roomNodeStyle);
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;

        // Room Node Type 목록을 불러옵니다.
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // Unity Editor의 Inspector에서 선택된 Object가 변경된 경우 이벤트 리스너를 제거합니다.
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// Room Node Graph Scriptable Object를 더블 클릭하여 Room Node Graph Editor를 열 수 있도록 합니다.
    /// </summary>
    [UnityEditor.Callbacks.OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph =
            EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if (roomNodeGraph == null)
        {
            return false;
        }

        OpenWindow();
        currentRoomNodeGraph = roomNodeGraph;
        return true;
    }

    /// <summary>
    /// Editor Window의 GUI를 그립니다. (OnGUI 메서드는 EditorWindow의 메서드입니다.)
    /// </summary>
    private void OnGUI()
    {
        if (currentRoomNodeGraph != null)
        {
            // Background Grid를 그립니다.
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            // Connection Line을 그립니다.
            DrawDraggedLine();

            // 이벤트를 처리합니다.
            ProcessEvents(Event.current);

            // Room Node 사이의 간선을 그립니다.
            DrawRoomConnections();

            // Room Node Graph Editor를 그립니다.
            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLines = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLines = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        // Drag Threshold를 적용하여 Grid를 이동합니다.
        graphOffset += graphDrag * 0.5f;

        // Grid는 반복되는 패턴으로 그려지므로, gridSize로 나눈 나머지를 Offset으로 계산합니다. :)
        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLines; i++)
        {
            Handles.DrawLine(
                new Vector3(gridSize * i, -gridSize, 0) + gridOffset,
                new Vector3(gridSize * i, position.height + gridSize, 0) + gridOffset
            );
        }

        for (int j = 0; j < horizontalLines; j++)
        {
            Handles.DrawLine(
                new Vector3(-gridSize, gridSize * j, 0) + gridOffset,
                new Vector3(position.width + gridSize, gridSize * j, 0) + gridOffset
            );
        }

        Handles.color = Color.white;
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center,
                currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center,
                currentRoomNodeGraph.linePosition,
                Color.white,
                null,
                connectingLineWidth
            );
        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        // Graph Drag를 초기화합니다.
        graphDrag = Vector2.zero;

        // Room Node를 드래그 중인 경우, 해당 Room Node를 선택합니다.
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // Room Node Graph Editor에서 발생한 이벤트를 처리합니다.
        // Room Node Graph Event와 Room Node Event를 구분하여 처리합니다.
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.rect.Contains(currentEvent.mousePosition))
            {
                return roomNode;
            }
        }

        return null;
    }

    /// <summary>
    /// Room Node Graph Editor에서 발생한 이벤트를 처리합니다.
    /// </summary>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
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

    /// <summary>
    /// Room Node Graph Editor에서 마우스 다운 이벤트를 처리합니다.
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // 마우스 우클릭인 경우, Context Menu를 노출합니다.
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        // 마우스 좌클릭인 경우, Connection Line을 취소하거나 Room Node 선택을 취소합니다.
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu contextMenu = new();

        contextMenu.AddItem(
            new GUIContent("Create Room Node"),
            false,
            CreateRoomNode,
            mousePosition
        );
        contextMenu.AddSeparator("");
        contextMenu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        contextMenu.AddSeparator("");
        contextMenu.AddItem(
            new GUIContent("Delete Selected Room Node Connections"),
            false,
            DeleteSelectedRoomNodeConnections
        );
        contextMenu.AddItem(
            new GUIContent("Delete Selected Room Nodes"),
            false,
            DeleteSelectedRoomNodes
        );

        contextMenu.ShowAsContext();
    }

    /// <summary>
    /// 에디터 상의 모든 Room Node를 선택 해제합니다.
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// Mouse Position에 Room Node를 생성합니다.
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        // 첫 번째 Room Node를 생성하는 경우, Entrance를 자동으로 생성합니다.
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(
                new Vector2(200f, 200f),
                roomNodeTypeList.list.Find(item => item.isEntrance)
            );
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(item => item.isNone));
    }

    /// <summary>
    /// 모든 Room Node를 선택합니다.
    /// </summary>
    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }

        GUI.changed = true;
    }

    /// <summary>
    /// 선택된 Room Node의 Connection을 삭제합니다. (Connection 기준, 부모와 자식 모두 선택된 경우만 삭제합니다.)
    /// </summary>
    private void DeleteSelectedRoomNodeConnections()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(
                        roomNode.childRoomNodeIDList[i]
                    );

                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                        childRoomNode.RemoveParentRootNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        // 선택된 Room Node를 초기화합니다.
        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// 선택된 Room Node를 삭제합니다.
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        // C#에서는 Collection을 순회하면서 삭제하는 경우, 예외가 발생할 수 있습니다.
        // 따라서, 삭제할 Room Node를 Queue에 추가하고, 순회가 끝난 후 삭제합니다.
        Queue<RoomNodeSO> roomNodeDeletionQueue = new();

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                // Child Room Node의 Connection을 삭제합니다.
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRootNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // Parent Room Node의 Connection을 삭제합니다.
                foreach (string parentRoomNodeID in roomNode.parentRootNodeIDList)
                {
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        while (roomNodeDeletionQueue.Count > 0)
        {
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            // Room Node Graph에서 Room Node를 삭제합니다.
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // Editor에서 Room Node Asset을 제거하고, AssetDatabase를 저장합니다.
            DestroyImmediate(roomNodeToDelete, true);
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// Room Node Graph Editor에서 마우스 드래그 이벤트를 처리합니다.
    /// </summary>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // 마우스 우클릭인 경우, Connection Line을 그립니다.
        if (currentEvent.button == 1)
        {
            ProcessMouseRightDragEvent(currentEvent);
        }
        // 마우스 좌클릭인 경우, 그래프를 드래그합니다.
        else if (currentEvent.button == 0)
        {
            ProcessMouseLeftDragEvent(currentEvent);
        }
    }

    private void ProcessMouseRightDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void ProcessMouseLeftDragEvent(Event currentEvent)
    {
        graphDrag = currentEvent.delta;

        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(graphDrag);
        }

        GUI.changed = true;
    }

    private void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// Room Node Graph Editor에서 마우스 업 이벤트를 처리합니다.
    /// </summary>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // Room Node 위에 마우스가 위치한 경우, Connection Line을 그립니다.
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);
            if (roomNode != null)
            {
                if (
                    currentRoomNodeGraph
                        .roomNodeToDrawLineFrom
                        .AddChildRoomNodeIDToRoomNode(roomNode.id)
                )
                {
                    roomNode.AddParentRootNodeIDToRoomNode(
                        currentRoomNodeGraph.roomNodeToDrawLineFrom.id
                    );
                }
            }

            ClearLineDrag();
        }
    }

    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// Room Node 사이의 간선을 그립니다.
    /// </summary>
    private void DrawRoomConnections()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(
                            roomNode,
                            currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]
                        );
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPoint = parentRoomNode.rect.center;
        Vector2 endPoint = childRoomNode.rect.center;

        // Connection Line을 그립니다.
        Handles.DrawBezier(
            startPoint,
            endPoint,
            startPoint,
            endPoint,
            Color.white,
            null,
            connectingLineWidth
        );

        // Connection Line의 화살표를 그립니다. (부모 관계를 나타냅니다.)
        Vector2 midPoint = (startPoint + endPoint) / 2;
        Vector2 direction = (endPoint - startPoint).normalized;

        Vector2 arrowTailPoint1 =
            midPoint - new Vector2(-direction.y, direction.x) * connectingLineArrowSize;
        Vector2 arrowTailPoint2 =
            midPoint + new Vector2(-direction.y, direction.x) * connectingLineArrowSize;
        Vector2 arrowHeadPoint = midPoint + direction * connectingLineArrowSize;

        Handles.DrawBezier(
            arrowHeadPoint,
            arrowTailPoint1,
            arrowHeadPoint,
            arrowTailPoint1,
            Color.white,
            null,
            connectingLineWidth
        );
        Handles.DrawBezier(
            arrowHeadPoint,
            arrowTailPoint2,
            arrowHeadPoint,
            arrowTailPoint2,
            Color.white,
            null,
            connectingLineWidth
        );

        GUI.changed = true;
    }

    /// <summary>
    /// Mouse Position에 Room Node를 생성합니다. - Room Node Type을 지정하도록 overload합니다.
    /// </summary>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // Room Node Scriptable Object를 생성합니다.
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // Room Node를 Room Node Graph에 추가합니다.
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // Room Node를 초기화합니다.
        roomNode.Initialize(
            new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)),
            currentRoomNodeGraph,
            roomNodeType
        );

        // Room Node를 Room Node Graph Scriptable Object 파일에 추가합니다.
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        // Graph Node Dictionary를 새로고침합니다.
        currentRoomNodeGraph.OnValidate();
    }

    /// <summary>
    /// Graph Editor 창에 Room Node를 그립니다.
    /// </summary>
    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }

        GUI.changed = true;
    }

    /// <summary>
    /// Unity Inspector에서 선택된 Object가 변경된 경우 호출됩니다.
    /// 선택된 Room Node Graph가 변경된 경우, 해당 Room Node Graph를 불러옵니다.
    /// </summary>
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}
