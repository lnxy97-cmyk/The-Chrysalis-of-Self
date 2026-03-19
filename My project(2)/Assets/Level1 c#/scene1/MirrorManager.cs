using UnityEngine;
using System.Collections.Generic;

public class MirrorManager : MonoBehaviour
{
    public static MirrorManager Instance;

    [Header("所有镜子")]
    public List<Mirror> allMirrors = new List<Mirror>();

    private Mirror currentNearbyMirror;
    private GameObject currentDisplayImage;
    private bool isImageShowing = false;
    private GameObject playerObject;
    private PlayerController2D playerController;
    private Rigidbody2D playerRigidbody;
    private Vector2 playerOriginalVelocity;
    private bool wasPlayerMovementEnabled = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("找不到带有'Player'标签的对象！");
            return;
        }

        // 获取玩家组件
        playerController = playerObject.GetComponent<PlayerController2D>();
        playerRigidbody = playerObject.GetComponent<Rigidbody2D>();

        if (playerController == null)
        {
            Debug.LogWarning("未找到PlayerController2D组件，将使用物理方式控制移动");
        }

        // 监听所有镜子的事件
        foreach (Mirror mirror in allMirrors)
        {
            if (mirror != null)
            {
                mirror.OnMirrorActivated.AddListener(OnMirrorActivated);
            }
        }
    }

    private void Update()
    {
        // 只有在没有显示图片时才能进行交互
        if (!isImageShowing && Input.GetKeyDown(KeyCode.F) && currentNearbyMirror != null)
        {
            currentNearbyMirror.Interact();
        }
        
        // 关闭图片
        if (Input.GetKeyDown(KeyCode.Escape) && isImageShowing)
        {
            CloseImage();
        }
        
        // 鼠标右键也可以关闭图片
        if (Input.GetMouseButtonDown(1) && isImageShowing)
        {
            CloseImage();
        }
    }

    // 禁用玩家移动
    private void DisablePlayerMovement()
    {
        if (playerObject == null) return;

        // 保存玩家当前状态
        if (playerRigidbody != null)
        {
            playerOriginalVelocity = playerRigidbody.velocity;
            playerRigidbody.velocity = Vector2.zero;
            playerRigidbody.simulated = false;
        }

        // 禁用玩家控制器
        if (playerController != null)
        {
            wasPlayerMovementEnabled = playerController.movementEnabled;
            playerController.SetMovementEnabled(false);
        }

        Debug.Log("玩家移动已完全禁用");
    }

    // 启用玩家移动
    private void EnablePlayerMovement()
    {
        if (playerObject == null) return;

        // 恢复物理模拟
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = true;
        }

        // 启用玩家控制器
        if (playerController != null && wasPlayerMovementEnabled)
        {
            playerController.SetMovementEnabled(true);
        }

        Debug.Log("玩家移动已启用");
    }

    public void SetCurrentNearbyMirror(Mirror mirror)
    {
        // 如果正在显示图片，不允许切换当前镜子
        if (isImageShowing) return;

        currentNearbyMirror = mirror;
        
        if (mirror != null)
        {
            Debug.Log($"靠近镜子 {mirror.mirrorID}，按 F 键查看图片");
        }
    }

    public Mirror GetCurrentNearbyMirror()
    {
        return currentNearbyMirror;
    }

    private void OnMirrorActivated(Mirror activatedMirror)
    {
        if (activatedMirror.imagePiece != null)
        {
            ShowImageInWorld(activatedMirror);
        }
        else
        {
            Debug.LogError($"镜子 {activatedMirror.mirrorID} 没有设置图片！");
        }
    }

    private void ShowImageInWorld(Mirror mirror)
    {
        // 如果已经有图片在显示，先关闭它
        if (isImageShowing)
        {
            CloseImage();
        }

        // 禁用玩家移动（角色完全停止）
        DisablePlayerMovement();

        // 创建显示图片
        currentDisplayImage = new GameObject($"DisplayImage_Mirror{mirror.mirrorID}");
        
        SpriteRenderer spriteRenderer = currentDisplayImage.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = mirror.imagePiece;
        spriteRenderer.sortingOrder = 999;
        
        Vector3 displayPosition = mirror.GetDisplayPosition(playerObject);
        currentDisplayImage.transform.position = displayPosition;
        
        float scale = mirror.displayScale;
        currentDisplayImage.transform.localScale = new Vector3(scale, scale, 1f);
        
        CreateBackgroundFrame(currentDisplayImage.transform, mirror.imagePiece.bounds.size, scale);
        
        isImageShowing = true;
        Debug.Log($"显示镜子 {mirror.mirrorID} 的图片");
        Debug.Log("玩家移动已锁定，按 ESC 或鼠标右键关闭图片");
    }

    private void CreateBackgroundFrame(Transform parent, Vector2 imageSize, float scale)
    {
        GameObject background = new GameObject("Background");
        background.transform.SetParent(parent);
        background.transform.localPosition = Vector3.zero;
        
        SpriteRenderer bgRenderer = background.AddComponent<SpriteRenderer>();
        bgRenderer.color = new Color(1f, 1f, 1f, 0.9f);
        background.transform.localScale = new Vector3(imageSize.x * 1.1f, imageSize.y * 1.1f, 1f);
        bgRenderer.sortingOrder = -1;
    }

    // 关闭图片
    private void CloseImage()
    {
        if (currentDisplayImage != null)
        {
            Destroy(currentDisplayImage);
            currentDisplayImage = null;
        }
        
        // 启用玩家移动
        EnablePlayerMovement();
        
        isImageShowing = false;
        Debug.Log("图片已关闭，玩家移动已恢复");
    }

    public bool IsImageShowing()
    {
        return isImageShowing;
    }
    
    // 强制关闭所有图片
    public void ForceCloseAllImages()
    {
        if (isImageShowing)
        {
            CloseImage();
        }
    }
}