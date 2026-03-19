using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SmallRoomDoor : MonoBehaviour
{
    public string targetSceneName = "SmallRoom";
    private bool playerInRange = false;
    private AsyncOperation asyncLoad;
    
    void Start()
    {
        // 预加载场景
        StartCoroutine(PreloadScene());
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            EnterRoom();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("按F立即进入");
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    
    IEnumerator PreloadScene()
    {
        // 在后台预加载场景
        asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false; // 不立即激活
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    
    void EnterRoom()
    {
        Debug.Log("瞬间进入小房间...");
        
        // 快速保存
        PlayerPrefs.SetFloat("SpawnX", 0f);
        PlayerPrefs.SetFloat("SpawnY", -0.5f);
        PlayerPrefs.Save();
        
        // 激活预加载的场景（瞬间切换）
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = true;
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}