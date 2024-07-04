using UnityEngine;

[System.Serializable]
public class Doorway
{
    public Vector2Int position;
    public Orientation orientation;
    public GameObject doorPrefab;

    #region Header
    [Header("복사할 문의 좌상단 위치")]
    #endregion
    public Vector2Int doorwayStartCopyPosition;

    #region Header
    [Header("복사할 문의 너비")]
    #endregion
    public int doorwayCopyTileWidth;

    #region Header
    [Header("복사할 문의 높이")]
    #endregion
    public int doorwayCopyTileHeight;

    [HideInInspector]
    public bool isConnected = false;

    [HideInInspector]
    public bool isUnavailable = false;
}
