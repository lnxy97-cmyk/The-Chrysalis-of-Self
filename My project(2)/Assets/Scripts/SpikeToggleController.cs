using System.Collections;
using UnityEngine;

// 地刺核心控制脚本（无接口、直接调用）
public class SpikeToggleController : MonoBehaviour
{
    public float moveSpeed = 3f;          // 地刺移动速度（可调）
    public float spikeRiseY = -7.34f;     // 地刺升起后的Y坐标（露出地面）
    public float spikeRetractY = -8.97f;  // 地刺缩回后的Y坐标（藏入地形）
    public bool startRaised = false;      // 初始状态：是否升起（根据按钮类型选）
    
    [SerializeField]private bool isRaised;                 // 当前状态：是否升起（只读，看效果）
    private bool isMoving;                // 防止重复移动的锁

    void Start()
    {
        // 初始化地刺位置和状态
        isRaised = startRaised;
        transform.position = new Vector2(transform.position.x, isRaised ? spikeRiseY : spikeRetractY);
    }
    
    // 地刺平滑移动协程（核心移动逻辑）
    private IEnumerator MoveSpikeToTarget(float targetY)
    {
        isMoving = true;
        Vector2 targetPos = new Vector2(transform.position.x, targetY);

        // 平滑移动到目标位置（避免瞬移）
        while (Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 强制到位，避免浮点误差
        transform.position = targetPos;
        isMoving = false;
    }

    // 快捷方法（可选）：强制升起（按钮也可直接调这个，不用切换）
    public void ForceRaiseSpike()
    {
        if (isMoving || isRaised) return;
        isRaised = true;
        StartCoroutine(MoveSpikeToTarget(spikeRiseY));
    }

    // 快捷方法（可选）：强制缩回（按钮也可直接调这个，不用切换）
    public void ForceRetractSpike()
    {
        if (isMoving || !isRaised) return;
        isRaised = false;
        StartCoroutine(MoveSpikeToTarget(spikeRetractY));
    }
}