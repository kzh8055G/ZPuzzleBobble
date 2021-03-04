using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DownHandler : MonoBehaviour
    , IPointerDownHandler
    , IPointerUpHandler
    , IBeginDragHandler
    , IEndDragHandler
{

    public void OnPointerDown(PointerEventData ped)
    {
        //buttonDown = true;
    }
    public void OnPointerUp(PointerEventData _ped)
	{

	}

    public void OnBeginDrag(PointerEventData eventData)
    {

    }
    public void OnEndDrag(PointerEventData eventData)
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
