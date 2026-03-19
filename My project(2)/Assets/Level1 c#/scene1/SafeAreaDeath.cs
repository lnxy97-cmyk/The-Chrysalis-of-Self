using UnityEngine;

public class SafeAreaDeath : MonoBehaviour
{
    [Header("=== 安全区设置 ===")]
    public Vector2 safeAreaCenter = Vector2.zero;
    public float safeAreaWidth = 20f;
    public float safeAreaHeight = 10f;
    
    [Header("=== 显示设置 ===")]
    [Tooltip("在游戏中显示安全区边框")]
    public bool showBorderInGame = false;  // 默认不显示
    
    [Tooltip("在编辑器中显示安全区边框（仅Scene视图）")]
    public bool showBorderInEditor = true;
    
    [Tooltip("边框颜色")]
    public Color borderColor = new Color(0, 1, 0, 0.8f);
    
    // 私有变量
    private Vector3 playerStartPosition;
    private GameObject safeAreaVisual;
    private float leftBound, rightBound, bottomBound, topBound;
    
    void Start()
    {
        SetupSafeArea();
    }
    
    void Update()
    {
        CheckPlayerPosition();
    }
    
    void SetupSafeArea()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStartPosition = player.transform.position;
            Debug.Log($" 玩家初始位置已保存: {playerStartPosition}");
        }
        
        CalculateBounds();
        
        // 只在需要时创建可视化
        if (showBorderInGame)
        {
            CreateSafeAreaVisual();
        }
        
        Debug.Log($" 安全区已激活！游戏内边框显示: {showBorderInGame}");
    }
    
    void CalculateBounds()
    {
        leftBound = safeAreaCenter.x - safeAreaWidth * 0.5f;
        rightBound = safeAreaCenter.x + safeAreaWidth * 0.5f;
        bottomBound = safeAreaCenter.y - safeAreaHeight * 0.5f;
        topBound = safeAreaCenter.y + safeAreaHeight * 0.5f;
    }
    
    void CheckPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Vector2 playerPos = player.transform.position;
        
        if (playerPos.x < leftBound || playerPos.x > rightBound || 
            playerPos.y < bottomBound || playerPos.y > topBound)
        {
            Debug.Log($" 玩家死亡！位置 {playerPos} 超出安全区");
            RespawnPlayer(player);
        }
    }
    
    void RespawnPlayer(GameObject player)
    {
        Debug.Log("重新开始...");
        
        // 禁用玩家控制
        PlayerController2D controller = player.GetComponent<PlayerController2D>();
        if (controller != null)
        {
            controller.SetMovementEnabled(false);
        }
        
        // 回到初始位置
        player.transform.position = playerStartPosition;
        
        // 重置物理状态
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // 重新启用控制
        StartCoroutine(EnablePlayerControl(controller));
        
        Debug.Log($"玩家已重生到: {playerStartPosition}");
    }
    
    System.Collections.IEnumerator EnablePlayerControl(PlayerController2D controller)
    {
        yield return null;
        if (controller != null)
        {
            controller.SetMovementEnabled(true);
        }
    }
    
    void CreateSafeAreaVisual()
    {
        // 如果不需要显示，直接返回
        if (!showBorderInGame) return;
        
        safeAreaVisual = new GameObject("SafeAreaVisual");
        SpriteRenderer renderer = safeAreaVisual.AddComponent<SpriteRenderer>();
        
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        renderer.sprite = sprite;
        renderer.color = borderColor;
        
        safeAreaVisual.transform.position = safeAreaCenter;
        safeAreaVisual.transform.localScale = new Vector3(safeAreaWidth, safeAreaHeight, 1f);
        renderer.sortingOrder = -5;
    }
    
    // 在Scene视图中显示安全区边界（仅编辑器）
    void OnDrawGizmosSelected()
    {
        if (!showBorderInEditor) return;
        
        Gizmos.color = borderColor;
        CalculateBounds();
        
        Vector3 center = new Vector3((leftBound + rightBound) * 0.5f, 
                                   (bottomBound + topBound) * 0.5f, 0);
        Vector3 size = new Vector3(rightBound - leftBound, topBound - bottomBound, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}