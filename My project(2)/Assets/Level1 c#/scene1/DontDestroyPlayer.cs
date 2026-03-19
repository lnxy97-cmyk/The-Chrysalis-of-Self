using UnityEngine;

public class DontDestroyPlayer : MonoBehaviour
{
    private static GameObject playerInstance;

    void Awake()
    {
        // 如果还没有玩家实例，设置这个为实例并保持不销毁
        if (playerInstance == null)
        {
            playerInstance = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        // 如果已经有玩家实例，销毁这个新的
        else
        {
            Destroy(gameObject);
        }
    }
}