using UnityEngine;
using UnityEngine.UI;

public class TextFadeController : MonoBehaviour
{
    public Text targetText;  // 要调整的Text组件
    
    void Start()
    {
        // 设置半透明（Alpha = 0.6）
        SetTextTransparency(0.6f);
    }
    
    // 设置透明度（0=透明，1=不透明）
    public void SetTextTransparency(float alpha)
    {
        if (targetText != null)
        {
            Color color = targetText.color;
            color.a = Mathf.Clamp01(alpha);  // 确保值在0-1之间
            targetText.color = color;
        }
    }
    
    // 淡入效果
    public void FadeIn(float duration = 1f)
    {
        StartCoroutine(FadeText(0f, 1f, duration));
    }
    
    // 淡出效果
    public void FadeOut(float duration = 1f)
    {
        StartCoroutine(FadeText(1f, 0f, duration));
    }
    
    System.Collections.IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = targetText.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            targetText.color = color;
            yield return null;
        }
        
        color.a = endAlpha;
        targetText.color = color;
    }
}