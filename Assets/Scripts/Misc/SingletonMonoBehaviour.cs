using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
    where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get { return instance; }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            // MonoBehaviour Singleton 인스턴스는 GameObject에 대응되므로
            // Scene 전환 등으로 새로 생성된 GameObject는 제거해야 합니다.
            Destroy(gameObject);
        }
    }
}
