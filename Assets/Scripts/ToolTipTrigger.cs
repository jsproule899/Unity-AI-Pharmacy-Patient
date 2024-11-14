using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    public string content;
    private Coroutine delayedShow;
    public void OnPointerEnter(PointerEventData eventData)
    {
       delayedShow = StartCoroutine(DelayedShow(1f));
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopCoroutine(delayedShow);
        ToolTipSystem.Hide();
    }

    private IEnumerator DelayedShow(float delayInSeconds){
        yield return new WaitForSeconds(delayInSeconds);
        ToolTipSystem.Show(content);
    }
}

