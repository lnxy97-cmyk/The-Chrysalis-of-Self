using UnityEngine;
using System.IO;

// 存档数据结构
[System.Serializable]
public class GameSaveData
{
    public bool hasPassedLevel1 = false;
    public bool hasPassedLevel2 = false;
    // 可以添加其他数据
}

public class SaveManager : MonoBehaviour
{
    // 单例模式
    public static SaveManager Instance { get; private set; }
    
    private string savePath;
    private GameSaveData currentSave;
    
    void Awake()
    {
        // 确保只有一个实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else
        {
            // 如果已存在实例，销毁新创建的
            Destroy(gameObject);
        }
    }
    
    void InitializeSaveSystem()
    {
        // 设置存档路径
        savePath = Path.Combine(Application.persistentDataPath, "AutoSave.json");
        Debug.Log("存档路径: " + savePath);
        
        // 加载存档
        LoadGame();
    }
    
    // 第一关完成时调用
    public void OnLevel1Completed()
    {
        if (currentSave.hasPassedLevel1) return;
        
        currentSave.hasPassedLevel1 = true;
        SaveGame();
        Debug.Log("第一关完成，已自动存档");
        
        // 可以在这里触发存档动画
    }
    
    // 第二关完成时调用
    public void OnLevel2Completed()
    {
        currentSave.hasPassedLevel2 = true;
        SaveGame();
        Debug.Log("游戏通关！");
    }
    
    // 保存到文件
    private void SaveGame()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSave, true);
            File.WriteAllText(savePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("存档失败: " + e.Message);
        }
    }
    
    // 加载存档
    private void LoadGame()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                currentSave = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("存档加载成功");
            }
            catch
            {
                Debug.LogWarning("存档损坏，创建新存档");
                CreateNewSave();
            }
        }
        else
        {
            Debug.Log("无存档文件，创建新存档");
            CreateNewSave();
        }
    }
    
    // 创建新存档
    private void CreateNewSave()
    {
        currentSave = new GameSaveData();
        currentSave.hasPassedLevel1 = false;
        currentSave.hasPassedLevel2 = false;
    }
    
    // 供外部查询的公共方法
    public bool HasPassedLevel1()
    {
        return currentSave != null && currentSave.hasPassedLevel1;
    }
    
    public bool HasPassedLevel2()
    {
        return currentSave != null && currentSave.hasPassedLevel2;
    }
    
    // 删除存档（用于开始新游戏）
    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        CreateNewSave();
        Debug.Log("存档已重置");
    }
    
    // 调试：打印存档信息
    public void PrintSaveInfo()
    {
        if (currentSave != null)
        {
            Debug.Log($"存档状态: Level1={currentSave.hasPassedLevel1}, Level2={currentSave.hasPassedLevel2}");
        }
    }
}