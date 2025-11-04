using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustEffect : MonoBehaviour
{
    public Animator dustEffectsAnimator { get; private set; }
    
    public Vector3 positionOffset;
    private bool isActive = false;

    private void Start()
    {
        dustEffectsAnimator = GetComponent<Animator>();
    }

    public void StartDust(Vector3 position, int facingDir)
    {
        if (!isActive)
        {
            Vector3 offset = new Vector3(positionOffset.x * facingDir, positionOffset.y, positionOffset.z);     //计算偏移：水平偏移根据Player的朝向，垂直偏移不变
            transform.position = position + offset;                                                             //更新位置：Player的位置加上偏移
        
            if (facingDir < 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = Vector3.one;
            
            gameObject.SetActive(true);
            isActive = true;
            dustEffectsAnimator.SetTrigger("StartDust");
        }
    }
    
    public void StopDust()
    {
        if (isActive)
        {
            isActive = false;
            //dustEffectsAnimator.SetTrigger("StopDust");
            StartCoroutine(DeactivateAfterAnimation());                                                    //延迟关闭，确保停止动画播放完成
        }
    }
    
    private IEnumerator DeactivateAfterAnimation()
    {
        yield return null;                                                                                       //等待一帧确保动画状态已切换
        yield return new WaitForSeconds(dustEffectsAnimator.GetCurrentAnimatorStateInfo(0).length);     //等待当前动画播放完成
        //gameObject.SetActive(false);
        dustEffectsAnimator.SetTrigger("StopDust");
    }
}
