using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndingVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer; // 引用视频播放器组件
    public string mainMenuSceneName = "MainMenu"; // 主菜单场景名（可修改）
    public bool exitGameAfterPlay = false; // 播放完成后是否退出游戏（true=退出，false=返回主菜单）

    void Start()
    {
        // 隐藏鼠标指针（可选，增强沉浸感）
        Cursor.visible = false;
        
        // 绑定视频播放完成事件
        videoPlayer.loopPointReached += OnVideoEnd;
        
        // 自动播放视频（若需延迟播放，可加 Invoke("PlayVideo", 1f)）
        videoPlayer.Play();
    }

    // 视频播放完成后执行
    void OnVideoEnd(VideoPlayer vp)
    {
        if (exitGameAfterPlay)
        {
            // 退出游戏（编辑器中按 Ctrl+Shift+B 测试时，此代码无效，打包后生效）
            Application.Quit();
        }
        else
        {
            // 返回主菜单场景（需在 Build Settings 中添加场景）
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // 可选：按 ESC 键跳过视频返回主菜单
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            videoPlayer.Stop();
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}