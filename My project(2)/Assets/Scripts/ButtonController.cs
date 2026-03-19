using System;
using System.Collections;
using UnityEngine;

public enum ButtonType
{
    ElevatorButton,    
    SpikeToggleDown,  
    SpikeToggleUp     
}

public class ButtonController : MonoBehaviour
{
    public ButtonType type;
    public Transform buttonVisual;
    public float pressDepth = 0.5f;
    public float moveSpeed = 10f;
    public ElevatorController targetElevator;
    public SpikeToggleController[] targetSpikes;
    
    
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    public bool isPressed = false; // 防止重复触发的标记
    
    private void Start()
    {
        originalPosition = buttonVisual.localPosition;
        pressedPosition = originalPosition + Vector3.down * pressDepth;
    }
    
    // 玩家进入按钮区域（按下按钮）
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPressed)
        {
            isPressed = true;
            AnimateButton(pressedPosition);
            TriggerTargetLogic();
        }
    }

    // 玩家离开按钮区域（松开按钮）
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isPressed)
        {
            isPressed = false;
            AnimateButton(originalPosition); // 按钮回弹动画
            // 电梯按钮松开后不立即控制电梯回升，由电梯自身延迟逻辑处理
        }
    }

    // 按钮动画控制
    private void AnimateButton(Vector3 targetPosition)
    {
        StopAllCoroutines(); // 停止当前动画，防止冲突
        StartCoroutine(MoveButton(targetPosition));
    }

    // 按钮移动协程
    private IEnumerator MoveButton(Vector3 targetPosition)
    {
        while (Vector3.Distance(buttonVisual.localPosition, targetPosition) > 0.01f)
        {
            buttonVisual.localPosition = Vector3.MoveTowards(
                buttonVisual.localPosition,
                targetPosition,
                Time.deltaTime * moveSpeed
            );
            yield return null;
        }

        buttonVisual.localPosition = targetPosition; // 确保精准到位
    }

    // 根据按钮类型执行目标功能
    private void TriggerTargetLogic()
    {
        switch (type)
        {
            case ButtonType.ElevatorButton:
                targetElevator?.Descend();
                break;
            case ButtonType.SpikeToggleDown:
                TriggerAllSpikesRetract();
                break;
            case ButtonType.SpikeToggleUp:
                TriggerAllSpikesRaise();
                break;
        }
    }

    private void TriggerAllSpikesRetract()
    {
        if (targetSpikes == null || targetSpikes.Length == 0)
        {
            return;
        }

        foreach (var spike in targetSpikes)
        {
            spike?.ForceRetractSpike();
        }
    }
    
    private void TriggerAllSpikesRaise()
    {
        if (targetSpikes == null || targetSpikes.Length == 0)
        {
            return;
        }
        foreach (var spike in targetSpikes)
        {
            spike?.ForceRaiseSpike(); // 强制升起（不切换，直接升）
        }
    }
    
    // 电梯触发逻辑（保留）
    private void TriggerElevator()
    {
        targetElevator?.Descend();
    }


}
