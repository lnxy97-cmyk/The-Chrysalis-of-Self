using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("按钮引用")]
    [SerializeField] Button startButton;
    [SerializeField] Button continueButton;
    
    [Header("场景名称")]
    [SerializeField] string level1Scene = "Level1";
    [SerializeField] string level2Scene = "Level2";
    
    void Start()
    {
        // 确保所有按钮都有监听器
        SetupButtons();
        
        // 检查存档状态，决定是否显示继续按钮
        UpdateContinueButton();
    }
    
    void SetupButtons()
    {
        // 开始新游戏按钮
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners(); // 清除旧的
            startButton.onClick.AddListener(OnStartNewGame);
        }
        
        // 继续游戏按钮
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueGame);
        }
    }
    
    void UpdateContinueButton()
    {
        // 检查是否有存档，如果有则启用继续按钮
        if (continueButton != null)
        {
            bool canContinue = false;
            
            // 等待一帧确保 SaveManager 初始化完成
            StartCoroutine(CheckSaveAfterFrame());
        }
    }
    
    System.Collections.IEnumerator CheckSaveAfterFrame()
    {
        yield return null; // 等待一帧
        
        // 现在检查 SaveManager
        if (SaveManager.Instance != null)
        {
            continueButton.gameObject.SetActive(SaveManager.Instance.HasPassedLevel1());
        }
        else
        {
            Debug.LogWarning("SaveManager 未找到，继续按钮将被隐藏");
            continueButton.gameObject.SetActive(false);
        }
    }
    
    void OnStartNewGame()
    {
        Debug.Log("开始新游戏");
        
        // 如果有存档管理器，重置存档
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }
        
        // 加载第一关
        SceneManager.LoadScene(level1Scene);
    }
    
    void OnContinueGame()
    {
        Debug.Log("继续游戏");
        
        // 检查存档状态
        bool canContinue = false;
        
        if (SaveManager.Instance != null)
        {
            canContinue = SaveManager.Instance.HasPassedLevel1();
        }
        
        // 根据存档状态决定加载哪一关
        if (canContinue)
        {
            SceneManager.LoadScene(level2Scene);
        }
        else
        {
            SceneManager.LoadScene(level1Scene);
        }
    }
    
    // 可选：添加一个第二关锁定的图标
    void Update()
    {
        // 如果按钮被禁用，可以显示提示
        if (continueButton != null && !continueButton.gameObject.activeSelf)
        {
            // 这里可以添加"需要先通关第一关"的提示逻辑
        }
    }
}