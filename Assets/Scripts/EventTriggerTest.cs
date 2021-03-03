using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventTriggerTest : MonoBehaviour
{
    private EventTrigger eventTrigger;

    public void AddDownListener(Action<PointerEventData> _handler)
	{
        EventTrigger.Entry entry_PointerDown = new EventTrigger.Entry();
        entry_PointerDown.eventID = EventTriggerType.PointerDown;
        entry_PointerDown.callback.AddListener((data) => { _handler((PointerEventData)data); });
        eventTrigger.triggers.Add(entry_PointerDown);
    }
    public void AddUpListener(Action<PointerEventData> _handler)
	{
        EventTrigger.Entry entry_PointerUp = new EventTrigger.Entry();
        entry_PointerUp.eventID = EventTriggerType.PointerUp;
        entry_PointerUp.callback.AddListener((data) => { _handler((PointerEventData)data); });
        eventTrigger.triggers.Add(entry_PointerUp);
    }

    private void Awake()
    {
        eventTrigger = gameObject.AddComponent<EventTrigger>();

        //EventTrigger.Entry entry_Drag = new EventTrigger.Entry();
        //entry_Drag.eventID = EventTriggerType.Drag;
        //entry_Drag.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
        //eventTrigger.triggers.Add(entry_Drag);

        //EventTrigger.Entry entry_EndDrag = new EventTrigger.Entry();
        //entry_EndDrag.eventID = EventTriggerType.EndDrag;
        //entry_EndDrag.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
        //eventTrigger.triggers.Add(entry_EndDrag);
    }

    //void OnPointerDown(PointerEventData data)
    //{
    //    Debug.Log("Pointer Down");
    //}

    //void OnDrag(PointerEventData data)
    //{
    //    Debug.Log("Drag");
    //}

    //void OnEndDrag(PointerEventData data)
    //{
    //    Debug.Log("End Drag");
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
