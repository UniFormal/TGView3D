using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    TMPro.TextMeshProUGUI Text;
    Coroutine Last;
    // Start is called before the first frame update
    void Start()
    {
        Text = GetComponentInChildren<HoverAttacher>().GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {

        Last = StartCoroutine(DelayedEnable());
    }
    

    IEnumerator DelayedEnable()
    {
        yield return new WaitForSeconds(.4f);
        Text.enabled = true;
        yield return new WaitForSeconds(10f);
        Text.enabled = false;

    }


    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        StopCoroutine(Last);
        Text.enabled = false;
    }

 
}
