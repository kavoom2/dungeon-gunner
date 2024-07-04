using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("던전 레벨")]
    #endregion
    #region Tooltip
    [Tooltip("각 레벨 별 던전 맵 정보를 담은 DungeonLevelSO 리스트입니다.")]
    #endregion
    [SerializeField]
    private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip
    [Tooltip("현재 플레이 하는 DungeonLevelSO의 인덱스입니다. 테스트용이며, 첫 번째 레벨은 '0'에 해당합니다.")]
    #endregion
    [SerializeField]
    private int currentDungeonLevelListIndex = 0;

    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;

    [HideInInspector]
    public GameState gameState;

    protected override void Awake()
    {
        base.Awake();

        playerDetails = GameResources.Instance.currentPlayer.playerDetails;
        InstantiatePlayer();
    }

    /// <summary>
    /// 플레이어를 인스턴스화합니다.
    /// </summary>
    private void InstantiatePlayer()
    {
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);
        player = playerGameObject.GetComponent<Player>();
        player.Initialize(playerDetails);
    }

    private void Start()
    {
        gameState = GameState.gameStarted;
    }

    private void Update()
    {
        HandleGameState();

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameState = GameState.gameStarted;
        }
    }

    /// <summary>
    /// 현재 게임 상태를 처리합니다.
    /// </summary>
    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 플레이어가 현재 위치한 방을 설정합니다.
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
    }

    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        bool dungeonBuildSuccessfully = DungeonBuilder
            .Instance
            .GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuildSuccessfully)
        {
            Debug.LogError("던전을 생성하지 못해 플레이할 수 없습니다.");
        }

        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        // 플레이어를 현재 방의 중앙에 위치시킵니다.
        player.gameObject.transform.position = new Vector3(
            (currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f,
            (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f,
            0f
        );

        // 현재 방에서 가장 가까운 스폰 위치로 플레이어를 이동시킵니다.
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(
            player.gameObject.transform.position
        );
    }

    /// <summary>
    /// 플레이어를 반환합니다.
    /// </summary>
    public Player GetPlayer()
    {
        return player;
    }

    /// <summary>
    /// 현재 방을 반환합니다.
    /// </summary>
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(
            this,
            nameof(dungeonLevelList),
            dungeonLevelList
        );
    }
#endif
    #endregion
}
