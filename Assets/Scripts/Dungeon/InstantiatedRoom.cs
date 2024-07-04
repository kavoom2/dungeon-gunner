using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector]
    public Room room;

    [HideInInspector]
    public Grid grid;

    [HideInInspector]
    public Tilemap groundTilemap;

    [HideInInspector]
    public Tilemap decoration1Tilemap;

    [HideInInspector]
    public Tilemap decoration2Tilemap;

    [HideInInspector]
    public Tilemap frontTilemap;

    [HideInInspector]
    public Tilemap collisionTilemap;

    [HideInInspector]
    public Tilemap minimapTilemap;

    [HideInInspector]
    public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        roomColliderBounds = boxCollider.bounds;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (
            collision.CompareTag(Settings.playerTag)
            && room != GameManager.Instance.GetCurrentRoom()
        )
        {
            room.isPreviouslyVisited = true;
            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }

    public void Initialize(GameObject roomGameObject)
    {
        PopulateTilemapMemberVariables(roomGameObject);

        BlockOffUnusedDoorways();

        AddDoorsToRooms();

        DisableCollisionTilemapRenderer();
    }

    /// <summary>
    /// Instantiated Room의 Tilemap 멤버 변수들을 초기화합니다.
    /// </summary>
    private void PopulateTilemapMemberVariables(GameObject roomGameObject)
    {
        grid = roomGameObject.GetComponentInChildren<Grid>();
        Tilemap[] tilemaps = roomGameObject.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            switch (tilemap.gameObject.tag)
            {
                case "groundTilemap":
                {
                    groundTilemap = tilemap;
                    break;
                }
                case "decoration1Tilemap":
                {
                    decoration1Tilemap = tilemap;
                    break;
                }
                case "decoration2Tilemap":
                {
                    decoration2Tilemap = tilemap;
                    break;
                }
                case "frontTilemap":
                {
                    frontTilemap = tilemap;
                    break;
                }
                case "collisionTilemap":
                {
                    collisionTilemap = tilemap;
                    break;
                }
                case "minimapTilemap":
                {
                    minimapTilemap = tilemap;
                    break;
                }
                default:
                {
                    Debug.LogWarning(
                        $"Invalid tilemap tag in {gameObject.name}: " + tilemap.gameObject.tag
                    );
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 사용되지 않는 출입구를 막습니다.
    /// </summary>
    private void BlockOffUnusedDoorways()
    {
        foreach (Doorway doorway in room.doorwayList)
        {
            if (doorway.isConnected)
            {
                continue;
            }

            if (collisionTilemap != null)
            {
                BlockDoorwayOnTilemapLayer(collisionTilemap, doorway);
            }

            if (minimapTilemap != null)
            {
                BlockDoorwayOnTilemapLayer(minimapTilemap, doorway);
            }

            if (groundTilemap != null)
            {
                BlockDoorwayOnTilemapLayer(groundTilemap, doorway);
            }

            if (decoration1Tilemap != null)
            {
                BlockDoorwayOnTilemapLayer(decoration1Tilemap, doorway);
            }

            if (decoration2Tilemap != null)
            {
                BlockDoorwayOnTilemapLayer(decoration2Tilemap, doorway);
            }

            if (frontTilemap != null)
            {
                BlockDoorwayOnTilemapLayer(frontTilemap, doorway);
            }
        }
    }

    /// <summary>
    /// Tilemap 레이어에서 출입구를 막습니다.
    /// </summary>
    private void BlockDoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockHorizontalDoorway(tilemap, doorway);
                break;

            case Orientation.west:
            case Orientation.east:
                BlockVerticalDoorway(tilemap, doorway);
                break;

            case Orientation.none:
            default:
                break;
        }
    }

    private void BlockHorizontalDoorway(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        for (int xPosition = 0; xPosition < doorway.doorwayCopyTileWidth; xPosition++)
        {
            for (int yPosition = 0; yPosition < doorway.doorwayCopyTileHeight; yPosition++)
            {
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(
                    new Vector3Int(startPosition.x + xPosition, startPosition.y - yPosition, 0)
                );

                // Ground, Decorations, Minimap, Collision, Front 등 모두 막아야 하므로 모든 Tilemap에 대해 처리할 수 있는 지점이 doorwayStartPosition이 됩니다.
                // - 죄측 -> 우측 / 상단 -> 하단으로 복사합니다.
                tilemap.SetTile(
                    new Vector3Int(startPosition.x + 1 + xPosition, startPosition.y - yPosition, 0),
                    tilemap.GetTile(
                        new Vector3Int(startPosition.x + xPosition, startPosition.y - yPosition, 0)
                    )
                );

                tilemap.SetTransformMatrix(
                    new Vector3Int(startPosition.x + 1 + xPosition, startPosition.y - yPosition, 0),
                    transformMatrix
                );
            }
        }
    }

    private void BlockVerticalDoorway(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        for (int yPosition = 0; yPosition < doorway.doorwayCopyTileHeight; yPosition++)
        {
            for (int xPosition = 0; xPosition < doorway.doorwayCopyTileWidth; xPosition++)
            {
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(
                    new Vector3Int(startPosition.x + xPosition, startPosition.y - yPosition, 0)
                );

                // Ground, Decorations, Minimap, Collision, Front 등 모두 막아야 하므로 모든 Tilemap에 대해 처리할 수 있는 지점이 doorwayStartPosition이 됩니다.
                // - 죄측 -> 우측 / 상단 -> 하단으로 복사합니다.
                tilemap.SetTile(
                    new Vector3Int(startPosition.x + xPosition, startPosition.y - 1 - yPosition, 0),
                    tilemap.GetTile(
                        new Vector3Int(startPosition.x + xPosition, startPosition.y - yPosition, 0)
                    )
                );

                tilemap.SetTransformMatrix(
                    new Vector3Int(startPosition.x + xPosition, startPosition.y - 1 - yPosition, 0),
                    transformMatrix
                );
            }
        }
    }

    /// <summary>
    /// Corridorr가 아닌 경우, 방에 출입구를 추가합니다.
    /// </summary>
    private void AddDoorsToRooms()
    {
        if (room.roomNodeType.isCorridorEW || room.roomNodeType.isCorridorNS)
        {
            return;
        }

        foreach (Doorway doorway in room.doorwayList)
        {
            if (doorway.doorPrefab != null && doorway.isConnected)
            {
                float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

                GameObject door = null;

                if (doorway.orientation == Orientation.north)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(
                        doorway.position.x + tileDistance / 2,
                        doorway.position.y + tileDistance,
                        0f
                    );
                }
                else if (doorway.orientation == Orientation.south)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(
                        doorway.position.x + tileDistance / 2,
                        doorway.position.y,
                        0f
                    );
                }
                else if (doorway.orientation == Orientation.west)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(
                        doorway.position.x,
                        doorway.position.y + tileDistance * 1.25f,
                        0f
                    );
                }
                else if (doorway.orientation == Orientation.east)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(
                        doorway.position.x + tileDistance,
                        doorway.position.y + tileDistance * 1.25f,
                        0f
                    );
                }

                Door doorComponent = door.GetComponent<Door>();
                if (room.roomNodeType.isBossRoom)
                {
                    doorComponent.isBossRoomDoor = true;
                    doorComponent.LockDoor();
                }
            }
        }
    }

    /// <summary>
    /// Collision Tilemap의 Renderer를 비활성화합니다. (화면에 노출되지 않아야 합니다.)
    /// </summary>
    private void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.GetComponent<TilemapRenderer>().enabled = false;
    }
}
