using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class SmallRoomMirror : MonoBehaviour
{
    [Header("=== 镜子基础信息 ===")]
    public int mirrorID = 0;
    public string mirrorName = "镜子";
    public bool isCorrectMirror = false;
    
    [Header("=== 图片系统 ===")]
    public Sprite displayImage;
    [TextArea(3, 5)]
    public string imageDescription = "这是一面神秘的镜子...";
    public Vector2 imageOffset = new Vector2(2f, 0f);
    public float imageScale = 1.2f;
    
    [Header("=== 交互设置 ===")]
    public KeyCode interactionKey = KeyCode.F;
    public float interactionRange = 2.0f;
    public float interactionCooldown = 0.5f;
    
    [Header("=== 视觉效果 ===")]
    public Sprite normalSprite;
    public Sprite highlightSprite;
    public Sprite inspectedSprite;
    public Color normalColor = Color.white;
    public Color highlightColor = new Color(1f, 1f, 0.8f, 1f);
    public Color inspectedColor = new Color(0.8f, 0.8f, 1f, 1f);
    public Color correctColor = new Color(0.6f, 1f, 0.6f, 1f);
    public Color wrongColor = new Color(1f, 0.6f, 0.6f, 1f);
    
    [Header("=== 线索系统 ===")]
    public List<string> clues = new List<string>();
    public int cluePriority = 0;
    
    [Header("=== 音效系统 ===")]
    public AudioClip interactSound;
    public AudioClip highlightSound;
    public AudioClip inspectSound;
    
    [Header("=== 事件系统 ===")]
    public UnityEvent<SmallRoomMirror> OnMirrorInteract;
    public UnityEvent<SmallRoomMirror> OnMirrorHighlight;
    public UnityEvent<SmallRoomMirror> OnMirrorUnhighlight;
    public UnityEvent<SmallRoomMirror> OnMirrorInspected;
    public UnityEvent<SmallRoomMirror> OnMirrorSelected;
    
    // 组件引用
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D interactionCollider;
    private AudioSource audioSource;
    private Animator animator;
    
    // 状态变量
    private bool isPlayerNearby = false;
    private bool canInteract = true;
    private bool hasBeenInspected = false;
    private bool isHighlighted = false;
    private float lastInteractionTime = 0f;
    private GameObject hintObject;
    
    void Awake()
    {
        InitializeComponents();
        SetupInteractionCollider();
    }
    
    void Start()
    {
        UpdateAppearance();
    }
    
    void Update()
    {
        HandlePlayerInteraction();
        UpdateHintPosition();
    }
    
    #region 核心方法
    
    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        animator = GetComponent<Animator>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void SetupInteractionCollider()
    {
        interactionCollider = GetComponent<BoxCollider2D>();
        if (interactionCollider == null)
        {
            interactionCollider = gameObject.AddComponent<BoxCollider2D>();
            interactionCollider.isTrigger = true;
        }
    }
    
    void HandlePlayerInteraction()
    {
        if (!canInteract || !isPlayerNearby) return;
        
        if (Input.GetKeyDown(interactionKey))
        {
            if (Time.time - lastInteractionTime >= interactionCooldown)
            {
                Interact();
                lastInteractionTime = Time.time;
            }
        }
    }
    
    public void Interact()
    {
        if (!canInteract) return;
        
        Debug.Log($"与镜子 {mirrorName} 交互");
        
        PlaySound(interactSound);
        OnMirrorInteract?.Invoke(this);
        MarkAsInspected();
        
        // 显示图片
        ShowMirrorImage();
        
        StartCoroutine(CooldownInteraction());
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player2"))
        {
            isPlayerNearby = true;
            ShowInteractionHint(true);
            HighlightMirror(true);
            
            // 尝试通知管理器，如果存在的话
            if (SmallRoomMirrorManager.Instance != null)
            {
                SmallRoomMirrorManager.Instance.SetCurrentNearbyMirror(this);
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("player2"))
        {
            isPlayerNearby = false;
            ShowInteractionHint(false);
            HighlightMirror(false);
            
            if (SmallRoomMirrorManager.Instance != null)
            {
                SmallRoomMirrorManager.Instance.SetCurrentNearbyMirror(null);
            }
        }
    }
    
    #endregion
    
    #region 视觉效果
    
    void HighlightMirror(bool highlight)
    {
        if (isHighlighted == highlight) return;
        
        isHighlighted = highlight;
        
        if (highlight)
        {
            PlaySound(highlightSound);
            OnMirrorHighlight?.Invoke(this);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }
        else
        {
            OnMirrorUnhighlight?.Invoke(this);
            UpdateAppearance();
        }
    }
    
    void MarkAsInspected()
    {
        if (hasBeenInspected) return;
        
        hasBeenInspected = true;
        
        PlaySound(inspectSound);
        OnMirrorInspected?.Invoke(this);
        UpdateAppearance();
        
        // 如果有管理器，通知它
       if (SmallRoomMirrorManager.Instance != null)
{
    // 使用反射或者检查方法是否存在
    System.Type type = SmallRoomMirrorManager.Instance.GetType();
    System.Reflection.MethodInfo method = type.GetMethod("OnMirrorInspected", 
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    
    if (method != null)
    {
        method.Invoke(SmallRoomMirrorManager.Instance, new object[] { this });
    }
    }
    }
    
    public void UpdateAppearance()
    {
        if (spriteRenderer == null) return;
        
        if (hasBeenInspected)
        {
            spriteRenderer.color = inspectedColor;
        }
        else if (isHighlighted)
        {
            spriteRenderer.color = highlightColor;
        }
        else
        {
            spriteRenderer.color = normalColor;
        }
    }
    
    #endregion
    
        #region 图片显示
    
    void ShowMirrorImage()
    {
        if (displayImage == null)
        {
            Debug.LogWarning($"镜子 {mirrorName} 没有设置显示图片");
            return;
        }

        // 如果管理器存在，尝试使用管理器的显示方法
        if (SmallRoomMirrorManager.Instance != null)
        {
            try
            {
                // 尝试调用管理器的方法
                SmallRoomMirrorManager.Instance.ShowMirrorImage(this);
                return; // 成功调用，直接返回
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"调用管理器失败: {e.Message}");
                // 如果失败，使用备用方案
            }
        }
        
        // 没有管理器或调用失败，自己显示
        StartCoroutine(ShowImageTemporarily());
    }
    
    IEnumerator ShowImageTemporarily()
    {
        // 创建临时显示对象
        GameObject imageObj = new GameObject("TempMirrorImage");
        SpriteRenderer sr = imageObj.AddComponent<SpriteRenderer>();
        sr.sprite = displayImage;
        sr.sortingOrder = 1000;
        
        // 设置位置（在玩家右侧）
        Vector3 displayPos = transform.position + (Vector3)imageOffset;
        imageObj.transform.position = displayPos;
        imageObj.transform.localScale = Vector3.one * imageScale;
        
        // 显示3秒
        yield return new WaitForSeconds(3f);
        
        // 销毁
        Destroy(imageObj);
    }
    
    IEnumerator CooldownInteraction()
    {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
    
    #endregion
    
    #region 提示系统
    
    void ShowInteractionHint(bool show)
    {
        if (show && hintObject == null)
        {
            CreateHintObject();
        }
        
        if (hintObject != null)
        {
            hintObject.SetActive(show);
        }
    }
    
    void CreateHintObject()
    {
        hintObject = new GameObject($"MirrorHint_{mirrorName}");
        hintObject.transform.SetParent(transform);
        hintObject.transform.localPosition = Vector3.up * 1.5f;
        
        // 使用TextMesh显示提示
        TextMesh textMesh = hintObject.AddComponent<TextMesh>();
        textMesh.text = $"按 {interactionKey} 查看";
        textMesh.fontSize = 20;
        textMesh.characterSize = 0.1f;
        textMesh.color = Color.yellow;
    }
    
    void UpdateHintPosition()
    {
        if (hintObject != null && hintObject.activeSelf)
        {
            float floatOffset = Mathf.Sin(Time.time * 2f) * 0.1f;
            hintObject.transform.localPosition = Vector3.up * (1.5f + floatOffset);
        }
    }
    
    #endregion
    
    #region 辅助方法
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public bool HasBeenInspected()
    {
        return hasBeenInspected;
    }
    
    public Sprite GetDisplayImage()
    {
        return displayImage;
    }
    
    public string GetDescription()
    {
        return imageDescription;
    }
    
    #endregion
    
    void OnDestroy()
    {
        if (hintObject != null)
        {
            Destroy(hintObject);
        }
    }
}