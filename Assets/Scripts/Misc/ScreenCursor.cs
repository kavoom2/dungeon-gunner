using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCursor : MonoBehaviour
{
    private void Awake()
    {
        // 하드웨어 커서를 숨깁니다.
        Cursor.visible = false;
    }

    private void Update()
    {
        transform.position = Input.mousePosition;
    }
}
