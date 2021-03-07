using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 1. 배치된 Bubble, 2. 히트한 Bubble
public class OnBubbleCollisionEvent : UnityEvent<GameObject, Bubble> { };

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

	public OnBubbleCollisionEvent OnBubbleCollision = new OnBubbleCollisionEvent();


	public Vector2 PlacedCell { get; private set; }
	public bool IsPlaced { get; private set; }
	
	private Animator animator;
	private SpriteRenderer spriteRenderer;

	private TextMesh debugText;

	private void Awake()
	{
		animator = GetComponentInChildren<Animator>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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
		OnBubbleCollision.Invoke(collision.gameObject, this);
	}

	private void _UpdateColor()
	{
		int colorIndex = (int)BubbleColor;
		animator.SetInteger("Color", colorIndex);
	}

	//#region Debug
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
		spriteRenderer.transform.localPosition = _position;
	}

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
	
	//private static Color[] presetColors = {
	//		Color.red,
	//		Color.white,
	//		Color.yellow,
	//		Color.green,
	//		Color.gray,
	//		Color.cyan
	//};

	private static List<Vector2> hexagonPoints = new List<Vector2>();
	// for optimization
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
	}

	//#endregion

	#region Utility


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

	public static void DrawHexgon(Vector2 _center, float _CellRadius, Color _color)
	{
		Vector2 CenterPos = new Vector2(_center.x, _center.y);
		for (int i = 0; i < 6; ++i)
		{
			Vector2 firstPoint = CenterPos + hexagonPoints[i] * _CellRadius;
			int secondIndex = i >= 5 ? 0 : i + 1;
			Vector2 secondPoint = CenterPos + hexagonPoints[secondIndex] * _CellRadius;

			Utility.DrawLine(firstPoint, secondPoint, _color);// presetColors[i]);
		}
	}
	public static void GetHexgonPoints(Vector2 _center, float _CellRadius, ref List<Vector2> outPoints)
	{
		Vector2 CenterPos = new Vector2(_center.x, _center.y);
		for (int i = 0; i < 6; ++i)
		{
			Vector2 firstPoint = CenterPos + hexagonPoints[i] * _CellRadius;
			int secondIndex = i >= 5 ? 0 : i + 1;
			Vector2 secondPoint = CenterPos + hexagonPoints[secondIndex] * _CellRadius;

			outPoints.Add(firstPoint);
			outPoints.Add(secondPoint);
			//Utility.DrawLine(firstPoint, secondPoint, _color);// presetColors[i]);
		}
	}
	#endregion

}
