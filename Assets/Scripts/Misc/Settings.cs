using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    #region UNITS
    public const float pixelsPerUnit = 16f;
    public const float tileSizePixels = 16f;
    #endregion

    #region DUNGEON BUILDER SETTINGS
    public const int maxDungeonRebuildAttemptsForNodeGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion

    #region ROOM SETTINGS
    /// <summary>
    /// 특정 방을 처음으로 방문할 때, 방의 시야가 서서히 확보되는 시간입니다.
    /// </summary>
    public const float fadeInTime = 0.5f;

    /// <summary>
    /// 하나의 방에 연결될 수 있는 최대 복도의 개수입니다.
    /// </summary>
    public const int maxChildCorridors = 3;
    #endregion

    #region ANIMATOR PARAMETERS
    // Animation parameters - Player
    public static int aimUp = Animator.StringToHash("aimUp");
    public static int aimUpRight = Animator.StringToHash("aimUpRight");
    public static int aimUpLeft = Animator.StringToHash("aimUpLeft");
    public static int aimRight = Animator.StringToHash("aimRight");
    public static int aimLeft = Animator.StringToHash("aimLeft");
    public static int aimDown = Animator.StringToHash("aimDown");
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");
    public static int rollUp = Animator.StringToHash("rollUp");
    public static int rollRight = Animator.StringToHash("rollRight");
    public static int rollLeft = Animator.StringToHash("rollLeft");
    public static int rollDown = Animator.StringToHash("rollDown");

    public static float baseSpeedForPlayerAnimation = 8f;

    // Animation parameters - Door
    public static int open = Animator.StringToHash("open");
    #endregion

    #region GAME OBJECT TAGS
    public const string playerTag = "Player";
    public const string playerWeaponTag = "PlayerWeapon";
    #endregion
}
