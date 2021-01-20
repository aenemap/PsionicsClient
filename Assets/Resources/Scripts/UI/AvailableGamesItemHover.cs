using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AvailableGamesItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Color prevColor;
    public void OnPointerEnter(PointerEventData eventData)
    {
        prevColor = this.gameObject.GetComponent<Image>().color;
        this.gameObject.GetComponent<Image>().color = new Color32(255,255,255,60);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        this.gameObject.GetComponent<Image>().color = prevColor;
    }
}
