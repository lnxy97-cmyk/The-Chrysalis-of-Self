using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class DoorController : MonoBehaviour
{
    [Header("门设置")]
    public string doorName = "door";
    public KeyCode interactKey = KeyCode.E;
    public bool isAccessible = false; //  
    
    [Header("场景设置")]
    public string startSceneName = "SampleScene"; // 开始页面场景名
    public string nextSceneName = "Level2";  // 正确选择后的下一关
    
    [Header("正确答案")]
    public int correctMirrorID = 0;
    
    [Header("UI界面")]
    public GameObject selectionUIPanel; // 镜子选择UI面板
    public Button[] mirrorButtons;      // 4个镜子按钮
    public Text hintText;               // 传统Text提示文本
    public TextMeshProUGUI tmpHintText; // TextMeshPro提示文本
    
    [Header("视觉效果")]
    public Sprite doorClosedSprite;
    public Sprite doorOpenSprite;
    public Color normalColor = Color.white;
    public Color highlightedColor = new Color(1, 0.8f, 0.2f); // 可交互时高亮
    
    [Header("音效")]
    public AudioClip doorOpenSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip deathSound;        // 死亡音效
    public AudioClip interactSound;
    public AudioClip panelOpenSound;
    
    [Header("按钮文本内容")]
    public string[] buttonTexts = new string[] { "镜子1", "镜子2", "镜子3", "镜子4" };
    
    [Header("调试模式")]
    public bool debugMode = true; // 调试模式，可关闭
    
    // 组件引用（2D游戏专用）
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private BoxCollider2D interactionCollider; // 2D碰撞器
    private bool isOpen = false;
    private bool playerInRange = false;
    private bool isUIOpen = false;
    
    void Awake()
    {
        InitializeComponents();
        InitializeUI();
    }
    
    void Start()
    {
        if (debugMode) Debug.Log("=== DoorController 启动 ===");
        
        // 获取正确答案（增强空值处理）
        if (SmallRoomMirrorManager.Instance != null)
        {
            correctMirrorID = SmallRoomMirrorManager.Instance.GetCorrectMirrorID();
            Debug.Log($"正确答案是：镜子{correctMirrorID + 1}");
        }
        else
        {
            Debug.LogWarning(" 没有找到 SmallRoomMirrorManager 实例！使用随机答案");
            correctMirrorID = Random.Range(0, 4);
            Debug.Log($"临时随机答案：镜子{correctMirrorID + 1}");
        }
        
        // 更新门外观
        UpdateDoorAppearance();
        
        // 强制关闭UI面板，确保初始不显示
        ForceCloseSelectionUI();
    }
    
    // 强制关闭UI面板（确保初始状态正确）
    void ForceCloseSelectionUI()
    {
        if (selectionUIPanel != null)
        {
            selectionUIPanel.SetActive(false);
            isUIOpen = false;
            Time.timeScale = 1f; // 恢复游戏时间
            if (debugMode) Debug.Log(" 强制关闭镜子选择面板");
        }
        else
        {
            Debug.LogWarning("selectionUIPanel 为 null，无法关闭");
        }
    }
    
    // 初始化组件（确保使用2D碰撞器）
    void InitializeComponents()
    {
        // 获取或添加 SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            if (debugMode) Debug.Log(" 添加了 SpriteRenderer 组件");
        }
        
        // 获取或添加 2D碰撞器
        interactionCollider = GetComponent<BoxCollider2D>();
        if (interactionCollider == null)
        {
            interactionCollider = gameObject.AddComponent<BoxCollider2D>();
            if (debugMode) Debug.Log("添加了 BoxCollider2D 组件");
        }
        
        // 设置2D碰撞器为触发器
        interactionCollider.isTrigger = true;
        interactionCollider.offset = Vector2.zero;
        interactionCollider.size = new Vector2(2f, 3f); // 调整为合适大小
        
        if (debugMode)
        {
            Debug.Log($"2D碰撞器设置完成:");
            Debug.Log($"  - isTrigger: {interactionCollider.isTrigger}");
            Debug.Log($"  - offset: {interactionCollider.offset}");
            Debug.Log($"  - size: {interactionCollider.size}");
        }
        
        // 获取或添加 AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            if (debugMode) Debug.Log("添加了 AudioSource 组件");
        }
        
        // 设置门初始精灵
        if (doorClosedSprite != null)
        {
            spriteRenderer.sprite = doorClosedSprite;
            if (debugMode) Debug.Log(" 设置了门初始精灵");
        }
        else
        {
            Debug.LogWarning(" doorClosedSprite 为 null，未设置初始精灵");
        }
    }
    
    // 初始化UI（设置按钮事件和文本）
    void InitializeUI()
    {
        if (selectionUIPanel == null)
        {
            Debug.LogError(" 请设置选择UI面板！");
            return;
        }
        
        if (debugMode) Debug.Log($"UI初始化: 找到 {mirrorButtons.Length} 个按钮");
        
        // 设置按钮点击事件
        for (int i = 0; i < mirrorButtons.Length && i < 4; i++)
        {
            if (mirrorButtons[i] == null)
            {
                Debug.LogError($" 第 {i + 1} 个按钮未设置！");
                continue;
            }
            
            int mirrorIndex = i; // 闭包捕获索引
            mirrorButtons[i].onClick.RemoveAllListeners();
            mirrorButtons[i].onClick.AddListener(() => OnMirrorButtonClicked(mirrorIndex));
            
            // 设置按钮文本（支持Text和TextMeshPro）
            SetButtonText(mirrorButtons[i], i);
        }
        
        // 添加ESC关闭功能（如果有关闭按钮）
        Button closeButton = selectionUIPanel.GetComponentInChildren<Button>(true);
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSelectionUI);
            if (debugMode) Debug.Log(" 找到并设置关闭按钮事件");
        }
    }
    
    // 设置按钮文本（同时支持Text和TextMeshPro）
    void SetButtonText(Button button, int index)
    {
        string textContent = index < buttonTexts.Length ? buttonTexts[index] : $"镜子{index + 1}";
        
        // 优先设置TextMeshProUGUI
        TextMeshProUGUI tmpText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = textContent;
            if (debugMode) Debug.Log($" 按钮 {index + 1} TMP文本设置为: {textContent}");
            return;
        }
        
        // 其次设置传统Text
        Text text = button.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = textContent;
            if (debugMode) Debug.Log($" 按钮 {index + 1} Text文本设置为: {textContent}");
            return;
        }
        
        Debug.LogWarning($" 按钮 {index + 1} 没有找到Text或TextMeshProUGUI组件");
    }
    
    void Update()
    {
        // 调试快捷键：F3显示当前状态
        if (debugMode && Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log($"=== 门当前状态 ===");
            Debug.Log($"playerInRange: {playerInRange}");
            Debug.Log($"isAccessible: {isAccessible}");
            Debug.Log($"isOpen: {isOpen}");
            Debug.Log($"isUIOpen: {isUIOpen}");
            Debug.Log($"按E条件: {playerInRange && isAccessible && !isOpen}");
        }
        
        // 检测按E打开/关闭UI
        if (playerInRange && isAccessible && !isOpen && Input.GetKeyDown(interactKey))
        {
            if (debugMode) 
            {
                Debug.Log($" 按 {interactKey} 键检测通过！");
                Debug.Log($"条件: 范围内({playerInRange}) && 可访问({isAccessible}) && 未打开({!isOpen})");
            }
            
            if (!isUIOpen)
            {
                OpenSelectionUI();
            }
            else
            {
                CloseSelectionUI();
            }
        }
        else if (debugMode && Input.GetKeyDown(interactKey))
        {
            // 显示条件不满足的原因
            Debug.Log($" 按 {interactKey} 键但条件不满足:");
            Debug.Log($"   玩家在范围: {playerInRange}");
            Debug.Log($"   门可访问: {isAccessible}");
            Debug.Log($"   门未打开: {!isOpen}");
        }
        
        // 按ESC关闭UI
        if (isUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSelectionUI();
        }
    }
    
    // 2D触发进入事件（玩家靠近门）
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (debugMode)
        {
            Debug.Log($" 2D触发检测:");
            Debug.Log($"   碰撞物体: {collision.name}");
            Debug.Log($"   物体标签: '{collision.tag}'");
            Debug.Log($"   匹配玩家标签('player2'): {collision.CompareTag("player2")}");
        }
        
        if (collision.CompareTag("player2"))
        {
            playerInRange = true;
            if (debugMode) Debug.Log($" 玩家进入门范围！");
            
            // 显示交互提示
            if (isAccessible && !isOpen)
            {
                ShowInteractionHint(true);
                Debug.Log($" 提示: 按 {interactKey} 选择镜子");
            }
            else
            {
                ShowInteractionHint(false);
            }
        }
    }
    
    // 2D触发离开事件（玩家远离门）
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("player2"))
        {
            playerInRange = false;
            if (debugMode) Debug.Log(" 玩家离开门范围");
            
            // 隐藏交互提示
            ShowInteractionHint(false);
            
            // 离开时关闭UI
            if (isUIOpen)
            {
                CloseSelectionUI();
            }
        }
    }
    
    // 打开镜子选择UI
    void OpenSelectionUI()
    {
        if (selectionUIPanel == null) 
        {
            Debug.LogError(" 无法打开UI: selectionUIPanel 为 null");
            return;
        }
        
        isUIOpen = true;
        selectionUIPanel.SetActive(true);
        
        if (debugMode) Debug.Log($" 打开UI面板");
        
        // 播放音效
        PlaySound(panelOpenSound);
        PlaySound(interactSound);
        
        // 慢动作效果（增强沉浸感）
        Time.timeScale = 0.2f;
        
        // 设置提示文本
        string hintContent = "选择正确的镜子\n（线索在房间的镜子中）";
        if (hintText != null) 
        {
            hintText.text = hintContent;
            if (debugMode) Debug.Log($" 设置传统提示文本: {hintContent}");
        }
        if (tmpHintText != null) 
        {
            tmpHintText.text = hintContent;
            if (debugMode) Debug.Log($" 设置TMP提示文本: {hintContent}");
        }
        
        Debug.Log(" 镜子选择界面已打开");
    }
    
    // 关闭镜子选择UI
    void CloseSelectionUI()
    {
        if (selectionUIPanel == null) return;
        
        isUIOpen = false;
        selectionUIPanel.SetActive(false);
        
        if (debugMode) Debug.Log(" 关闭UI面板");
        
        // 恢复游戏速度
        Time.timeScale = 1f;
    }
    
    // 镜子按钮点击事件
    void OnMirrorButtonClicked(int mirrorIndex)
    {
        Debug.Log($" 选择了镜子 {mirrorIndex + 1}");
        
        // 关闭UI
        CloseSelectionUI();
        
        // 验证选择
        if (mirrorIndex == correctMirrorID)
        {
            OnCorrectSelection();
        }
        else
        {
            OnWrongSelection(mirrorIndex);
        }
    }
    
    // 正确选择处理
    void OnCorrectSelection()
    {
        Debug.Log("door is open");
        
        // 播放音效
        PlaySound(correctSound);
        PlaySound(doorOpenSound);
        
        // 开门动画
        StartCoroutine(OpenDoorAnimation());
        
        // 显示正确反馈
        ShowFeedback("open", Color.green);
        
        // 延迟后进入下一关
        Invoke(nameof(LoadNextLevel), 1.5f);
    }
    
    // 错误选择处理（角色死亡回到开始页面）
    void OnWrongSelection(int mirrorID)
    {
        Debug.Log("door is closed");
        
        // 播放死亡音效（优先使用死亡音效，否则使用错误音效）
        if (deathSound != null)
        {
            PlaySound(deathSound);
            if (debugMode) Debug.Log(" 播放死亡音效");
        }
        else
        {
            PlaySound(wrongSound);
            if (debugMode) Debug.Log(" 播放错误音效（死亡音效未设置）");
        }
        
        // 显示死亡反馈
        ShowFeedback(" dead", Color.red);
        
        // 清理状态
        CloseSelectionUI();
        Time.timeScale = 1f;
        
        // 延迟0.5秒后回到开始页面（给玩家反应时间）
        Invoke(nameof(ReturnToStartScene), 0.5f);
    }
    
    // 开门动画
    IEnumerator OpenDoorAnimation()
    {
        isOpen = true;
        
        // 更换开门精灵
        if (doorOpenSprite != null)
        {
            spriteRenderer.sprite = doorOpenSprite;
            if (debugMode) Debug.Log(" 更换为开门精灵");
        }
        else
        {
            Debug.LogWarning(" doorOpenSprite 为 null，未更换开门精灵");
        }
        
        // 发光动画
        float duration = 1.0f;
        float timer = 0;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            
            // 发光效果
            float glow = Mathf.PingPong(t * 4f, 0.5f) + 0.5f;
            spriteRenderer.color = new Color(glow, glow, 1f);
            
            // 轻微浮动
            transform.position += Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.01f;
            
            yield return null;
        }
        
        spriteRenderer.color = Color.white;
        if (debugMode) Debug.Log(" 开门动画完成");
    }
    
    // 显示反馈文本
    void ShowFeedback(string message, Color color)
    {
        // 创建临时UI文本
        if (selectionUIPanel != null && selectionUIPanel.transform.parent != null)
        {
            GameObject feedbackObj = new GameObject("Feedback");
            feedbackObj.transform.SetParent(selectionUIPanel.transform.parent, false);
            
            // 使用TextMeshProUGUI
            TextMeshProUGUI feedbackTmpText = feedbackObj.AddComponent<TextMeshProUGUI>();
            feedbackTmpText.text = message;
            feedbackTmpText.color = color;
            feedbackTmpText.fontSize = 30;
            feedbackTmpText.fontStyle = FontStyles.Bold;
            feedbackTmpText.alignment = TextAlignmentOptions.Center;
            
            RectTransform rect = feedbackObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 100);
            rect.anchoredPosition = Vector2.zero;
            
            // 添加淡出动画
            StartCoroutine(FadeOutFeedback(feedbackObj, feedbackTmpText));
            
            // 2秒后销毁
            Destroy(feedbackObj, 2f);
            
            if (debugMode) Debug.Log($" 显示反馈文本: {message}");
        }
        else
        {
            // 备选方案：使用World Space UI
            GameObject feedbackObj = new GameObject("WorldFeedback");
            feedbackObj.transform.position = transform.position + Vector3.up * 2f;
            
            TextMesh textMesh = feedbackObj.AddComponent<TextMesh>();
            textMesh.text = message;
            textMesh.color = color;
            textMesh.fontSize = 20;
            textMesh.anchor = TextAnchor.MiddleCenter;
            
            Destroy(feedbackObj, 2f);
            
            if (debugMode) Debug.Log($" 显示世界空间反馈文本: {message}");
        }
    }
    
    // TextMeshProUGUI淡出动画
    IEnumerator FadeOutFeedback(GameObject obj, TextMeshProUGUI text)
    {
        float duration = 1.5f;
        float timer = 0;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            
            // 淡出并上浮
            Color color = text.color;
            color.a = 1 - t;
            text.color = color;
            
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition += Vector2.up * t * 50f;
            }
            
            yield return null;
        }
    }
    
    // 显示/隐藏交互提示
    void ShowInteractionHint(bool canInteract)
    {
        if (canInteract)
        {
            if (debugMode) Debug.Log($" 显示提示: 按 {interactKey} 选择镜子");
            spriteRenderer.color = highlightedColor; // 高亮显示
        }
        else
        {
            spriteRenderer.color = normalColor; // 恢复正常颜色
        }
    }
    
    // 播放音效
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else if (clip == null)
        {
            Debug.LogWarning(" 音效资源为 null，无法播放");
        }
        else if (audioSource == null)
        {
            Debug.LogWarning(" AudioSource 为 null，无法播放音效");
        }
    }
    
    // 更新门外观
    void UpdateDoorAppearance()
    {
        if (spriteRenderer != null)
        {
            if (isOpen)
            {
                spriteRenderer.color = Color.white;
            }
            else if (isAccessible)
            {
                spriteRenderer.color = playerInRange ? highlightedColor : normalColor;
            }
            else
            {
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 灰色（不可交互）
            }
            
            if (debugMode) Debug.Log($"更新门外观: 颜色={spriteRenderer.color}");
        }
        else
        {
            Debug.LogWarning(" SpriteRenderer 为 null，无法更新门外观");
        }
    }
    
    // 门可访问性设置（由SmallRoomMirrorManager调用）
    public void SetAccessible(bool accessible)
    {
        isAccessible = accessible;
        
        if (debugMode) Debug.Log($" 设置门可访问性: {accessible}");
        
        UpdateDoorAppearance();
        
        if (accessible)
        {
            Debug.Log(" 门现在可以进入了！");
            
            // 如果玩家已经在门附近，立即显示提示
            if (playerInRange)
            {
                ShowInteractionHint(true);
                Debug.Log($" 玩家已在门附近，显示提示");
            }
        }
        else
        {
            Debug.Log(" 门被锁住了");
        }
    }
    
    // 进入下一关
    void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // 保存进度（可选）
            PlayerPrefs.SetInt("LastCompletedLevel", GetCurrentLevel());
            PlayerPrefs.Save();
            
            // 恢复游戏时间
            Time.timeScale = 1f;
            
            Debug.Log($"进入下一关: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning(" 没有设置下一关场景名！");
            ShowFeedback("恭喜通关！", Color.yellow);
        }
    }
    
    // 回到开始页面
    void ReturnToStartScene()
    {
        if (!string.IsNullOrEmpty(startSceneName))
        {
            Debug.Log($" 返回开始页面: {startSceneName}");
            // 确保场景存在于Build Settings中
            if (IsSceneInBuildSettings(startSceneName))
            {
                SceneManager.LoadScene(startSceneName);
            }
            else
            {
                Debug.LogError($" 开始场景 '{startSceneName}' 不在Build Settings中！重新加载当前场景");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            Debug.LogWarning(" 未设置开始场景名称！重新加载当前场景");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    // 检查场景是否在Build Settings中（增强安全性）
    bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    // 获取当前关卡（辅助方法）
    int GetCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level"))
        {
            string levelStr = sceneName.Substring(5);
            int level;
            if (int.TryParse(levelStr, out level))
            {
                return level;
            }
        }
        return 1;
    }
}