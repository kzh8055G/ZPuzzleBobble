using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 1. 배치된 Bubble, 2. 히트한 Bubble
public class OnBubbleHitEvent : UnityEvent<Bubble, Bubble> { };

public class Bubble : MonoBehaviour
{

	private EBubbleColor bubbleColor = EBubbleColor.Blue;
	public EBubbleColor BubbleColor
	{
		get
		{
			return bubbleColor;
		}
		set
		{
			if (bubbleColor != value)
			{
				bubbleColor = value;
				_UpdateColor();
			}
		}
	}

	public OnBubbleHitEvent OnBubbleHit = new OnBubbleHitEvent();

	public Vector2 PlacedCell { get; private set; }
	public bool IsPlaced { get; private set; }
	
	private Animator animator;
	private SpriteRenderer renderer;

	private TextMesh debugText;

	private void Awake()
	{
		animator = GetComponentInChildren<Animator>();
		renderer = GetComponentInChildren<SpriteRenderer>();

		debugText = GetComponentInChildren<TextMesh>();
		debugText.text = string.Empty;
	}

	void Start()
	{
		_UpdateColor();
	}
	// Update is called once per frame
	void Update()
	{}

	public void PlaceInCell(Vector2 _cell)
	{
		PlacedCell = _cell;
		IsPlaced = true;
		if (debugText != null)
		{
			debugText.text = string.Empty;
			//debugText.text = string.Format("{0},{1} : {2}", Cell.x, Cell.y, GetColorString(BubbleColor));
			//debugText.text = string.Format("{0},{1}", Cell.x, Cell.y);
			//debugText.text = string.Format("{0}", GetColorString(BubbleColor));
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		// already placed, ignore trigger event
		if (IsPlaced)
		{
			return;
		}
		var adjecentBubble = collision.GetComponent<Bubble>();
		if (adjecentBubble != null)
		{
			//Debug.Log(string.Format("adjacent Bubble Cell X : {0}, Cell Y : {1}", adjecentBubble.PlacedCell.x, adjecentBubble.PlacedCell.y));
			OnBubbleHit.Invoke(adjecentBubble, this);
		}
	}

	private void _UpdateColor()
	{
		int colorIndex = (int)BubbleColor;
		animator.SetInteger("Color", colorIndex);
	}

	#region Debug
	public void SetAlpha(float _alpha)
	{
		Utility.SetSpriteAlpha(gameObject, _alpha);
	}

	public void Shoot(float _angle
		, float _velocity
		, LayerMask _contactFilterLayerMask)
	{
		var moveController = GetComponent<BubbleMoveController>();
		if (moveController != null)
		{
			moveController.StartMoving(_angle, _velocity, _contactFilterLayerMask);
		}
	}

	public void SetSpritePostion(Vector2 _position)
	{
		renderer.transform.localPosition = _position;
	}

	//public void StartVibrate(int _step)
	//{

	//	StopVibrate();
	//	coroutineVibrate = StartCoroutine(_Vibrate(_step));

	//}

	//public void StopVibrate()
	//{
	//	if (coroutineVibrate != null)
	//	{
	//		StopCoroutine(coroutineVibrate);
	//		coroutineVibrate = null;
	//	}
	//	if(renderer != null)
	//	{
	//		renderer.transform.localPosition = Vector2.zero;
	//	}
	//}

	//private IEnumerator _Vibrate(int _step)
	//{
	//	// to
	//	const float limit = 0.02f;
	//	const float delta = 0.01f;
	//	float sign = 1;

	//	float delaySeconds = 0.0f;
	//	if (_step == 1)
	//	{
	//		delaySeconds = 0.01f;
	//	}
	//	else if(_step == 2)
	//	{
	//		delaySeconds = 0.006f;
	//	}

	//	float currentX = 0f;
	//	while (true)
	//	{
	//		float currentDelta = sign > 0 ? delta : -delta;
	//		currentX += currentDelta;

	//		if( Mathf.Abs(currentX) > limit)
	//		{
	//			currentX = sign * limit;

	//			sign *= -1; // 
	//		}
	//		renderer.transform.localPosition = new Vector3(currentX, 0);

	//		yield return new WaitForSeconds(delaySeconds);
	//	}
	//}

	public void Boom()
	{
		StartCoroutine(CoroutineBubbleBoom());
	}
	// * 같은 색 매칭됐을시 터치는 연출
	// - 당연히 임시임
	private IEnumerator CoroutineBubbleBoom()
	{
		float targetScale = 2.0f;
		float currentScale = 1.0f;
		while (targetScale >= currentScale)
		{
			currentScale += 0.05f;

			SetAlpha(targetScale - currentScale);
			transform.localScale = new Vector3(currentScale, currentScale, currentScale);
			yield return new WaitForSeconds(0.01f);
		}
		gameObject.SetActive(false);
	}

	public void Fall()
	{
		StartCoroutine(CoroutineBubbleFalling());
	}

	// Corou
	private IEnumerator CoroutineBubbleFalling()
	{
		debugText.text = "Falling";
		//  Alpha 
		for(int i = 0; i < 10; i++)
		{
			// t = 1, i = 0 -> t = 0, i = 10
			float t = 1 - (i * 0.1f);	// 
			float alpha = Mathf.Min(1f, 0.1f + (0.9f * t));
			SetAlpha(alpha);
			yield return new WaitForSeconds(0.05f);
		}
		// 중력 온 !
		var rigidbody = GetComponent<Rigidbody2D>();
		if (rigidbody != null)
		{
			rigidbody.gravityScale = 1.0f;
		}
		// 
		yield return new WaitForSeconds(1f);

		// 삭제
		gameObject.SetActive(false);
	}



	public string GetInfoString()
	{
		return string.Format("{0} : {1}, {2}", GetColorString(BubbleColor), PlacedCell.x, PlacedCell.y);
	}
	
	private static Color[] presetColors = {
			Color.red,
			Color.white,
			Color.yellow,
			Color.green,
			Color.gray,
			Color.cyan
	};

	private static List<Vector2> hexagonPoints = new List<Vector2>();

	static Bubble()
	{
		Vector2 baseVector = Vector2.zero;
		baseVector.y = Mathf.Sin(-30 * Mathf.Deg2Rad);
		baseVector.x = Mathf.Cos(-30 * Mathf.Deg2Rad);

		hexagonPoints.Add(baseVector);

		for (int i = 0; i < 5; ++i)
		{
			float euler = 60 * (i + 1);

			hexagonPoints.Add(Quaternion.AngleAxis(euler, Vector3.forward) * baseVector);
		}
	}

	public void DrawHexagon(float _CellRadius)
	{
		DrawHexgon(transform.position, _CellRadius, Color.green);

		//Vector2 CenterPos = new Vector2(transform.position.x, transform.position.y);
		//for (int i = 0; i < 6; ++i)
		//{
		//	Vector2 firstPoint = CenterPos + hexagonPoints[i] * _CellRadius;
		//	int secondIndex = i >= 5 ? 0 : i + 1;
		//	Vector2 secondPoint = CenterPos + hexagonPoints[secondIndex] * _CellRadius;

		//	Debug.DrawLine(firstPoint, secondPoint, Color.green);// presetColors[i]);
		//}

		//for (int i = 0; i < 6; ++i)
		//{
		//	Vector2 secondPoint = CenterPos + hexagonPoints[i] * _CellRadius;
		//	Debug.DrawLine(CenterPos, secondPoint, Color.yellow);
		//}
		//Vector2 sss = Vector2.zero;//
		//sss.x = CenterPos.x + hexagonPoints[3].x * _CellRadius;
		//sss.y = CenterPos.y;

		//Debug.DrawLine(CenterPos, sss, Color.red);

	}

	#endregion

	#region Utility

	//public static void SetSpriteAlpha(GameObject _object, float _alpha)
	//{
	//	var renderer = _object.GetComponent<SpriteRenderer>();
	//	if (renderer)
	//	{
	//		var color = renderer.material.color;
	//		color.a = _alpha;
	//		renderer.material.color = color;
	//	}
	//}
	public static void DrawHexgon(Vector2 _center, float _CellRadius, Color _color)
	{
		Vector2 CenterPos = new Vector2(_center.x, _center.y);
		for (int i = 0; i < 6; ++i)
		{
			Vector2 firstPoint = CenterPos + hexagonPoints[i] * _CellRadius;
			int secondIndex = i >= 5 ? 0 : i + 1;
			Vector2 secondPoint = CenterPos + hexagonPoints[secondIndex] * _CellRadius;

			Debug.DrawLine(firstPoint, secondPoint, _color);// presetColors[i]);
		}
	}

	public static string GetColorString(EBubbleColor _color)
	{
		string strColor = string.Empty;
		switch (_color)
		{
			case EBubbleColor.Blue:		strColor = "B"; break;
			case EBubbleColor.Green:	strColor = "G"; break;
			case EBubbleColor.Red:		strColor = "R"; break;
			case EBubbleColor.Yellow:	strColor = "Y"; break;
		}
		return strColor;
	}



	#endregion

}
