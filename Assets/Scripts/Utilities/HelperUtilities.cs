using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    public static Camera mainCamera;

    /// <summary>
    /// 마우스의 월드 좌표를 반환합니다.
    /// </summary>
    public static Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Vector3 mouseScreenPosition = Input.mousePosition;

        // Mouse Position을 스크린 경계를 기준으로 Clamp합니다.
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0, Screen.height);

        // Mouse Position을 월드 좌표로 변환합니다.
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        return mouseWorldPosition;
    }

    /// <summary>
    /// 방향 벡터로부터 각도를 반환합니다. (unit: degree)
    /// </summary>
    public static float GetAngleFromVector(Vector3 vector)
    {
        // 탄젠트 값으로 부터 각도를 구합니다.
        float radius = Mathf.Atan2(vector.y, vector.x);

        // 라디안 값을 각도로 변환합니다.
        float degrees = radius * Mathf.Rad2Deg;
        return degrees % 360;
    }

    public static AimDirection GetAimDirection(float angleDegrees)
    {
        AimDirection aimDirection;

        if (angleDegrees >= 22f && angleDegrees <= 67f)
        {
            aimDirection = AimDirection.UpRight;
        }
        else if (angleDegrees > 67f && angleDegrees <= 112f)
        {
            aimDirection = AimDirection.Up;
        }
        else if (angleDegrees > 112f && angleDegrees <= 158f)
        {
            aimDirection = AimDirection.UpLeft;
        }
        else if (
            (angleDegrees > 158 && angleDegrees <= 180f)
            || (angleDegrees <= -135 && angleDegrees > -180f)
        )
        {
            aimDirection = AimDirection.Left;
        }
        else if (angleDegrees > -135f && angleDegrees <= -45f)
        {
            aimDirection = AimDirection.Down;
        }
        else if (
            (angleDegrees > -45f && angleDegrees <= 0f)
            || (angleDegrees > 0f && angleDegrees <= 22f)
        )
        {
            aimDirection = AimDirection.Right;
        }
        else
        {
            aimDirection = AimDirection.Right;
        }

        return aimDirection;
    }

    public static bool ValidateCheckEmptyString(
        Object thisObject,
        string fieldName,
        string stringToCheck
    )
    {
        if (stringToCheck == "")
        {
            Debug.Log($"{fieldName}이 비어있습니다. {thisObject.name} 오브젝트에서 확인해주세요.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 객체 {fieldName}의 값이 null인지 확인합니다. (오류인 경우, true 반환)
    /// </summary>
    public static bool ValidateCheckNullValue(
        Object thisObject,
        string fieldName,
        Object objectToCheck
    )
    {
        if (objectToCheck == null)
        {
            Debug.Log($"{fieldName}이 null입니다. {thisObject.name} 오브젝트에서 확인해주세요.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 객체 {fieldName}의 값이 null인지 확인합니다. (오류인 경우, true 반환)
    public static bool ValidateCheckEnumerableValues(
        Object thisObject,
        string fieldName,
        IEnumerable enumerableObjectToCheck
    )
    {
        bool isError = false;
        int count = 0;

        if (enumerableObjectToCheck == null)
        {
            Debug.Log($"{thisObject.name}의 값은 null이므로 {fieldName}을 참조할 수 없습니다.");
            return true;
        }

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log($"{fieldName}은 null 값을 가지고 있습니다. {thisObject.name} 오브젝트에서 확인해주세요.");
                isError = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            isError = true;
            Debug.Log($"{fieldName}은 값이 없습니다. {thisObject.name} 오브젝트에서 확인해주세요.");
        }

        return isError;
    }

    /// <summary>
    /// 값이 0보다 작거나 같은지 확인합니다. (오류인 경우, true 반환)
    /// </summary>
    public static bool ValidateCheckPositiveValue(
        Object thisObject,
        string fieldName,
        int valueToCheck,
        bool isZeroAllowed
    )
    {
        bool isError = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log($"{fieldName}은 0보다 작을 수 없습니다. {thisObject.name} 오브젝트에서 확인해주세요.");
                isError = true;
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log($"{fieldName}은 0보다 작을 수 없습니다. {thisObject.name} 오브젝트에서 확인해주세요.");
                isError = true;
            }
        }

        return isError;
    }

    /// <summary>
    ///  값이 0보다 작거나 같은지 확인합니다. (오류인 경우, true 반환)
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="valueToCheck"></param>
    /// <param name="isZeroAllowed"></param>
    /// <returns></returns>
    public static bool ValidateCheckPositiveValue(
        Object thisObject,
        string fieldName,
        float valueToCheck,
        bool isZeroAllowed
    )
    {
        bool isError = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log($"{fieldName}은 0보다 작을 수 없습니다. {thisObject.name} 오브젝트에서 확인해주세요.");
                isError = true;
            }
            else
            {
                if (valueToCheck <= 0)
                {
                    Debug.Log($"{fieldName}은 0보다 작을 수 없습니다. {thisObject.name} 오브젝트에서 확인해주세요.");
                    isError = true;
                }
            }
        }

        return isError;
    }

    public static bool ValidateCheckPositiveRange(
        Object thisObject,
        string fieldNameMin,
        float valueToCheckMin,
        string fieldNameMax,
        float valueToCheckMax,
        bool isZeroAllowed
    )
    {
        bool isError = false;

        if (valueToCheckMin > valueToCheckMax)
        {
            Debug.Log(
                $"{fieldNameMin}은 {fieldNameMax}보다 작을 수 없습니다. {thisObject.name} 오브젝트에서 확인해주세요."
            );
            isError = true;
        }

        if (ValidateCheckPositiveValue(thisObject, fieldNameMin, valueToCheckMin, isZeroAllowed))
        {
            isError = true;
        }

        if (ValidateCheckPositiveValue(thisObject, fieldNameMax, valueToCheckMax, isZeroAllowed))
        {
            isError = true;
        }

        return isError;
    }

    /// <summary>
    /// 플레이어로부터 가장 가까운 스폰 위치를 반환합니다.
    /// </summary>
    public static Vector3 GetSpawnPositionNearestToPlayer(Vector3 playerPosition)
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        // Grid의 Local Coord에서 World Coord로 변환해야 합니다! :)
        // Room의 모든 좌표들은 Grid의 Local Coord를 기준으로 표기되어 있습니다.
        Grid grid = currentRoom.instantiatedRoom.grid;
        Vector3 nearestSpawnPosition = new(10000f, 10000f, 0f);

        foreach (Vector2Int spawnPointGrid in currentRoom.spawnPositionArray)
        {
            // 스폰 위치를 월드 좌표로 변환합니다. (Grid 좌표 기준으로 되어 있음)
            Vector3 spawnPositionWorld = grid.CellToWorld((Vector3Int)spawnPointGrid);

            if (
                Vector3.Distance(spawnPositionWorld, playerPosition)
                < Vector3.Distance(nearestSpawnPosition, playerPosition)
            )
            {
                nearestSpawnPosition = spawnPositionWorld;
            }
        }

        return nearestSpawnPosition;
    }
}
