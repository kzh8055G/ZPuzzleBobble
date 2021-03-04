using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//public interface ILeverDirection
//{
//}
public class LeverController : MonoBehaviour
   , IPointerDownHandler
    , IPointerUpHandler
    , IDragHandler
{
    public UnityEvent<EDirection> OnLeverDirectionChanged = new UnityEvent<EDirection>();

    private EDirection currentLeverDirection = EDirection.None;
    public EDirection LeverDirection => currentLeverDirection;

    private float leverBound;   // cache
    private RectTransform leverTransform;

	private void Awake()
	{
        leverBound = (transform as RectTransform).rect.width / 2;
        leverTransform = transform.GetChild(0).GetComponent<RectTransform>();
	}
    private Vector3 _GetLeverPositionInBound( Vector3 _inputPosition)
	{
        var localInputPos = transform.InverseTransformPoint(_inputPosition);
        float distance = Vector3.Distance(localInputPos, Vector3.zero);
        if (distance > leverBound)
        {
            return localInputPos.normalized * leverBound;
        }
        return localInputPos;
    }

    private void _UpdateLeverDirection()
	{
        EDirection direction = EDirection.None;
        // TODO : 0을 기준으로 판단하는 것보단 별도의 bound 를 주는게 맞아보인다
        if( leverTransform.localPosition.x > 0 )
		{
            direction |= EDirection.Right;
        }
        else if( leverTransform.localPosition.x < 0)
		{
            direction |= EDirection.Left;
        }
        if(leverTransform.localPosition.y > 0 )
		{
            direction |= EDirection.Up;

        }
        else if(leverTransform.localPosition.y < 0)
        {
            direction |= EDirection.Down;
        }

        if(leverTransform.localPosition.magnitude == 0)
		{
            direction = EDirection.None;
        }
        currentLeverDirection = direction;
    }
	public void OnPointerDown(PointerEventData _eventData)
    {
        leverTransform.localPosition = _GetLeverPositionInBound(_eventData.position);
        Debug.Log("OnPointerDown");
        _UpdateLeverDirection();
    }
    public void OnPointerUp(PointerEventData _eventData)
    {
        leverTransform.localPosition = Vector3.zero;
        Debug.Log("OnPointerUp");
        _UpdateLeverDirection();
    }
    //public void OnBeginDrag(PointerEventData _eventData)
    //{
    //    Debug.Log("OnBeginDrag");
    //}
    //public void OnEndDrag(PointerEventData _eventData)
    //{
    //    Debug.Log("OnEndDrag");
    //}
    public void OnDrag(PointerEventData eventData)
	{
        leverTransform.localPosition = _GetLeverPositionInBound(eventData.position);
        Debug.Log("OnDrag");
        _UpdateLeverDirection();
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
