using UnityEngine;
using UnityEngine.Events;

public class Mirror : MonoBehaviour
{
    [Header("镜子设置")]
    public int mirrorID;
    public Sprite imagePiece;
    public bool isActivated = false;
    
    [Header("图片显示设置")]
    [Tooltip("图片显示的大小比例：1=原大小，2=两倍大小，0.5=一半大小")]
    public float displayScale = 1f;
    
    [Header("图片显示位置")]
    [Tooltip("图片显示的位置偏移（相对于玩家）")]
    public Vector2 displayOffset = new Vector2(2f, 0f);
    [Tooltip("使用世界坐标固定位置（如果不想相对于玩家）")]
    public Vector2 worldPosition = Vector2.zero;
    [Tooltip("是否使用世界坐标固定位置")]
    public bool useWorldPosition = false;
    
    [Header("事件")]
    public UnityEvent<Mirror> OnMirrorActivated;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            MirrorManager.Instance.SetCurrentNearbyMirror(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (MirrorManager.Instance.GetCurrentNearbyMirror() == this)
            {
                MirrorManager.Instance.SetCurrentNearbyMirror(null);
            }
        }
    }

    public void Interact()
    {
        Debug.Log($"与镜子 {mirrorID} 交互");
        OnMirrorActivated?.Invoke(this);
        
        if (!isActivated)
        {
            isActivated = true;
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 0.5f, 1f);
        }
    }

    /// <summary>
    /// 获取图片显示位置
    /// </summary>
    public Vector3 GetDisplayPosition(GameObject player)
    {
        if (useWorldPosition)
        {
            // 使用固定的世界坐标
            return worldPosition;
        }
        else
        {
            // 使用相对于玩家的位置
            Vector3 playerPosition = player.transform.position;
            return playerPosition + new Vector3(displayOffset.x, displayOffset.y, 0);
        }
    }

    /// <summary>
    /// 设置显示位置偏移
    /// </summary>
    public void SetDisplayOffset(Vector2 newOffset)
    {
        displayOffset = newOffset;
    }

    /// <summary>
    /// 设置世界坐标位置
    /// </summary>
    public void SetWorldPosition(Vector2 newPosition)
    {
        worldPosition = newPosition;
    }
}