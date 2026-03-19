using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SmallRoomMirrorManager : MonoBehaviour
{
    public static SmallRoomMirrorManager Instance;
    
    [Header("镜子系统")]
    public List<SmallRoomMirror> mirrors = new List<SmallRoomMirror>();
    public int correctMirrorID = 0; // 正确答案镜子ID
    
    [Header("门系统")]
    public DoorController doorController; // 修复：类型改为 DoorController，变量名改为小写开头
    public bool requireMirrorInspection = true; // 是否需要查看镜子才能到门
    public bool requireAllMirrors = false; // 是否需要查看所有镜子才能到门
    
    [Header("UI系统")]
    public Canvas gameCanvas;
    public GameObject mirrorImagePanel; // 显示镜子图片的面板
    public Image mirrorImageDisplay;    // 显示镜子图片的Image
    public Text mirrorDescriptionText;  // 图片描述文本
    public Text clueText;               // 线索提示文本
    public GameObject interactionHint;  // 交互提示
    
    [Header("提示设置")]
    public Sprite interactionHintSprite;
    public float hintOffsetY = 1.5f;
    public Color inspectedMirrorColor = new Color(0.8f, 0.8f, 1f, 1f);
    
    // 私有变量
    private SmallRoomMirror currentNearbyMirror;
    private GameObject currentHintObject;
    private GameObject playerObject;
    private Scene2Player playerController;
    private bool isImageShowing = false;
    private List<int> inspectedMirrorIDs = new List<int>(); // 已查看的镜子ID
    private Dictionary<int, SmallRoomMirror> mirrorDictionary = new Dictionary<int, SmallRoomMirror>();
    private bool canGoToDoor = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializePlayer();
        InitializeMirrors();
        InitializeUI();
        UpdateClueDisplay();
        
        // 强制关闭所有UI，确保初始状态正确
        ForceCloseAllUI();
    }

    void InitializePlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag("player2");
        
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<Scene2Player>();
            if (playerController == null)
            {
                Debug.LogError("玩家对象上没有找到Scene2Player组件！");
            }
            else
            {
                Debug.Log("成功找到Scene2Player组件");
            }
        }
        else
        {
            Debug.LogError("找不到玩家对象！");
        }
    }
    
    void InitializeMirrors()
    {
        // 创建镜子字典方便快速查找
        foreach (SmallRoomMirror mirror in mirrors)
        {
            if (mirror != null)
            {
                mirrorDictionary[mirror.mirrorID] = mirror;
                mirror.OnMirrorInteract.AddListener(OnMirrorInteract);
                
                // 设置初始颜色
                SpriteRenderer sr = mirror.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.white;
                }
            }
        }
        
        Debug.Log($"镜子系统初始化完成，共 {mirrors.Count} 面镜子，正确答案：镜子{correctMirrorID + 1}");
    }
    
    void InitializeUI()
    {
        if (gameCanvas == null)
        {
            gameCanvas = FindObjectOfType<Canvas>();
        }
        
        // 创建UI元素（如果不存在）
        if (mirrorImagePanel == null)
        {
            CreateMirrorImageUI();
        }
        
        // 隐藏所有UI
        if (mirrorImagePanel != null)
        {
            mirrorImagePanel.SetActive(false);
            Debug.Log("InitializeUI: 镜像显示面板已关闭");
        }
    }
    
    // 强制关闭所有UI的方法
    void ForceCloseAllUI()
    {
        Debug.Log("=== 开始强制关闭所有UI ===");
        
        // 关闭镜像显示面板
        if (mirrorImagePanel != null)
        {
            mirrorImagePanel.SetActive(false);
            isImageShowing = false;
            Debug.Log("✅ 强制关闭镜像显示面板");
            Debug.Log($"   面板状态: {mirrorImagePanel.activeSelf}");
        }
        
        // 关闭交互提示
        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
            Debug.Log("✅ 强制关闭交互提示");
            Debug.Log($"   提示状态: {interactionHint.activeSelf}");
        }
        
        // 关闭线索文本（如果需要）
        if (clueText != null)
        {
            clueText.gameObject.SetActive(true); // 线索文本默认显示，用于提示玩家
            Debug.Log("✅ 线索文本保持显示");
        }
        
        Debug.Log("=== 强制关闭UI完成 ===");
    }
    
    void Update()
    {
        UpdateInteractionHint();
        
        // 检测镜子交互
        if (!isImageShowing && currentNearbyMirror != null && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"按下F键，与镜子 {currentNearbyMirror.mirrorName} 交互");
            currentNearbyMirror.Interact();
        }
        
        // 关闭图片
        if (isImageShowing && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            CloseImage();
        }
        
        // 检测门交互 - 现在由 DoorController 自己处理
        // 我们只需要确保门是可交互的
    } // 大括号已存在，结构完整
    
    void UpdateInteractionHint()
    {
        bool showMirrorHint = currentNearbyMirror != null && !isImageShowing;
        
        // 更新镜子交互提示
        if (showMirrorHint)
        {
            ShowHint("按 F 查看镜子");
        }
        else
        {
            HideHint();
        }
    }
    
    void ShowHint(string message)
    {
        if (interactionHint == null)
        {
            CreateHintUI();
        }
        
        if (interactionHint != null)
        {
            interactionHint.SetActive(true);
            Text hintText = interactionHint.GetComponentInChildren<Text>();
            if (hintText != null)
            {
                hintText.text = message;
            }
            
            // 更新位置（在玩家头顶）
            if (playerObject != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(
                    playerObject.transform.position + Vector3.up * 1.5f
                );
                interactionHint.transform.position = screenPos;
            }
        }
    }
    
    void HideHint()
    {
        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }
    
    void CreateHintUI()
    {
        if (gameCanvas == null) return;
        
        interactionHint = new GameObject("InteractionHint");
        interactionHint.transform.SetParent(gameCanvas.transform);
        
        // 添加背景
        Image bg = interactionHint.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        bg.rectTransform.sizeDelta = new Vector2(200, 60);
        
        // 添加文字
        GameObject textObj = new GameObject("HintText");
        textObj.transform.SetParent(interactionHint.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = "提示";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.rectTransform.sizeDelta = new Vector2(180, 40);
        
        interactionHint.SetActive(false);
    }
    
    public void SetCurrentNearbyMirror(SmallRoomMirror mirror)
    {
        if (isImageShowing) return;
        
        if (currentNearbyMirror != mirror)
        {
            currentNearbyMirror = mirror;
            
            if (mirror != null)
            {
                Debug.Log($"靠近镜子: {mirror.mirrorName}，按F查看专属图片");
            }
            else
            {
                Debug.Log("离开镜子范围");
            }
        }
    }
    
    private void OnMirrorInteract(SmallRoomMirror mirror)
    {
        if (isImageShowing) return;
        
        if (mirror.displayImage != null)
        {
            ShowMirrorImage(mirror);
            RecordMirrorInspection(mirror);
            UpdateMirrorAppearance(mirror);
        }
        else
        {
            Debug.LogError($"镜子 {mirror.mirrorName} 没有设置图片！");
        }
    }
    
    public void ShowMirrorImage(SmallRoomMirror mirror) 
    {
        if (mirror == null) return;
        
        // 禁用玩家移动
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
        }
        
        // 显示UI面板
        if (mirrorImagePanel != null)
        {
            mirrorImagePanel.SetActive(true);
            
            // 设置图片
            if (mirrorImageDisplay != null)
            {
                mirrorImageDisplay.sprite = mirror.displayImage;
            }
            
            // 设置描述
            if (mirrorDescriptionText != null)
            {
                mirrorDescriptionText.text = $"镜子 {mirror.mirrorName}\n\n{mirror.imageDescription}";
            }
        }
        
        isImageShowing = true;
        Debug.Log($"显示镜子 {mirror.mirrorName} 的专属图片");
    }
    
    void RecordMirrorInspection(SmallRoomMirror mirror)
    {
        if (!inspectedMirrorIDs.Contains(mirror.mirrorID))
        {
            inspectedMirrorIDs.Add(mirror.mirrorID);
            Debug.Log($"已查看镜子 {mirror.mirrorName}，总查看数：{inspectedMirrorIDs.Count}/{mirrors.Count}");
            
            // 检查是否可以前往门
            CheckDoorAccess();
        }
    }
    
    void UpdateMirrorAppearance(SmallRoomMirror mirror)
    {
        SpriteRenderer sr = mirror.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = inspectedMirrorColor;
        }
    }
    
    void CheckDoorAccess()
    {
        if (requireAllMirrors)
        {
            canGoToDoor = (inspectedMirrorIDs.Count >= mirrors.Count);
        }
        else if (requireMirrorInspection)
        {
            canGoToDoor = (inspectedMirrorIDs.Count > 0);
        }
        else
        {
            canGoToDoor = true;
        }
        
        // 告诉 DoorController 门是否可交互
        if (doorController != null)
        {
            doorController.SetAccessible(canGoToDoor);
            Debug.Log($"门可访问性设置为: {canGoToDoor}");
        }
        
        UpdateClueDisplay();
    }
    
    void UpdateClueDisplay()
    {
        if (clueText != null)
        {
            string clue = "";
            
            if (inspectedMirrorIDs.Count == 0)
            {
                clue = "查看镜子获取线索...";
            }
            else if (inspectedMirrorIDs.Count < mirrors.Count && requireAllMirrors)
            {
                clue = $"还需查看 {mirrors.Count - inspectedMirrorIDs.Count} 面镜子";
            }
            else
            {
                clue = "已获得足够线索，可以去门前选择了";
            }
            
            clueText.text = clue;
        }
    }
    
    void CloseImage()
    {
        // 隐藏UI面板
        if (mirrorImagePanel != null)
        {
            mirrorImagePanel.SetActive(false);
        }
        
        // 启用玩家移动
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }
        
        isImageShowing = false;
        Debug.Log("图片已关闭");
    }
    
    // 公开方法供其他脚本调用
    public void OnMirrorSelected(int mirrorID)
    {
        // 这个方法由 DoorController 的按钮调用
        Debug.Log($"SmallRoomMirrorManager: 玩家选择了镜子 {mirrorID + 1}");
    }
    
    public bool IsImageShowing()
    {
        return isImageShowing;
    }
    
    public int GetCorrectMirrorID()
    {
        return correctMirrorID;
    }
    
    public int GetInspectedMirrorCount()
    {
        return inspectedMirrorIDs.Count;
    }
    
    public bool CanGoToDoor()
    {
        return canGoToDoor;
    }
    
    void CreateMirrorImageUI()
    {
        if (gameCanvas == null) return;
        
        mirrorImagePanel = new GameObject("MirrorImagePanel");
        mirrorImagePanel.transform.SetParent(gameCanvas.transform);
        
        // 设置面板（全屏半透明背景）
        Image panelImage = mirrorImagePanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.9f);
        panelImage.rectTransform.anchorMin = Vector2.zero;
        panelImage.rectTransform.anchorMax = Vector2.one;
        panelImage.rectTransform.offsetMin = Vector2.zero;
        panelImage.rectTransform.offsetMax = Vector2.zero;
        
        // 创建图片显示区域
        GameObject imageContainer = new GameObject("ImageContainer");
        imageContainer.transform.SetParent(mirrorImagePanel.transform);
        mirrorImageDisplay = imageContainer.AddComponent<Image>();
        mirrorImageDisplay.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        mirrorImageDisplay.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        
        // 创建描述文本
        GameObject descObj = new GameObject("DescriptionText");
        descObj.transform.SetParent(mirrorImagePanel.transform);
        mirrorDescriptionText = descObj.AddComponent<Text>();
        mirrorDescriptionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        mirrorDescriptionText.color = Color.white;
        mirrorDescriptionText.alignment = TextAnchor.LowerCenter;
        mirrorDescriptionText.rectTransform.anchorMin = new Vector2(0.1f, 0.05f);
        mirrorDescriptionText.rectTransform.anchorMax = new Vector2(0.9f, 0.2f);
        
        mirrorImagePanel.SetActive(false);
    }
}