﻿using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class Draggable: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 pointerDisplacement = Vector3.zero;
    private float zDisplacement;

    private Vector3 _originalPosition;
    public Vector3 originalPosition
    {
        get { return _originalPosition; }
        set { _originalPosition = value; }
    }

    private Quaternion _originalRotation;
    public Quaternion originalRotation
    {
        get { return _originalRotation; }
        set { _originalRotation = value; }
    }

    private Transform _originalParent;
    public Transform originalParent
    {
        get { return _originalParent; }
        set { _originalParent = value; }
    }

    Ray ray;
    RaycastHit hit;

    private void OnEnable() {
        GameEvents.instance.onUpdateDraggableOriginalPosition += UpdateDraggableOriginalPosition;
    }

    private void OnDisable() {
        GameEvents.instance.onUpdateDraggableOriginalPosition -= UpdateDraggableOriginalPosition;
    }



    void Start()
    {
        originalPosition = this.transform.position;
        originalRotation = this.transform.rotation;
        originalParent = this.transform.parent;
    }


    private void UpdateDraggableOriginalPosition(GameObject card, float xPos, float yPos)
    {
        Draggable draggable = card.GetComponent<Draggable>();
        if (draggable != null)
        {
            draggable.originalPosition = xPos >= 0 && yPos >= 0 ? new Vector3(xPos, yPos, card.transform.position.z) :   card.transform.position;
            draggable.originalRotation = card.transform.rotation;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        HoverPreview.PreviewsAllowed = false;
        zDisplacement = -Camera.main.transform.position.z + transform.position.z;
        pointerDisplacement = -transform.position + MouseInWorldCoords();
        transform.rotation = Quaternion.Euler(0, 0, 0);
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag");

        Vector3 mousePos = MouseInWorldCoords();
        this.transform.position = new Vector3(mousePos.x - pointerDisplacement.x, mousePos.y - pointerDisplacement.y, transform.position.z);
        //Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.up, Color.red);
        //Debug.DrawLine(this.transform.position, Vector3.right, Color.red);
        //ray = Camera.main.ScreenPointToRay(this.transform.parent.parent.position);
        //if (Physics.Raycast(ray, out hit))
        //{
        //    if (hit.collider != null)
        //    {
        //        Debug.Log("HIT COLLIDER => " + hit.collider.name.ToString());
        //    }
        //}

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Card endOfDragCard = eventData.pointerDrag.GetCardAsset();
        if (endOfDragCard.CardType == Enums.CardType.Event)
        {
            VisualsEvents.current.AddCardToDiscards(eventData.pointerDrag);            
        }
        else
        {
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            eventData.pointerDrag.transform.rotation = originalRotation;
            if (!DOTween.IsTweening(eventData.pointerDrag.transform))
                eventData.pointerDrag.transform.DOMove(new Vector3(originalPosition.x, originalPosition.y, originalPosition.z), 0.5f).SetEase(Ease.OutQuint);
        }
        HoverPreview.PreviewsAllowed = true;


    }

    private Vector3 MouseInWorldCoords()
    {
        var screenMousePos = Input.mousePosition;
        //Debug.Log(screenMousePos);
        screenMousePos.z = zDisplacement;
        return Camera.main.ScreenToWorldPoint(screenMousePos);
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    Debug.Log("Draggable => OnPointerEnter");
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    Debug.Log("Draggable => OnPointerExit");
    //}
}
