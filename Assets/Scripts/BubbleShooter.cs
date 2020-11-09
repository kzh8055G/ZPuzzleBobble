using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BubbleShooter : MonoBehaviour
{
	[SerializeField]
	GameObject NextBubble;

	[SerializeField]
	GameObject BubblePrefab;

	[SerializeField]
	Stage stage;

	[SerializeField]
	float BubbleVelocity = 2.5f;

	[SerializeField]
	float shootAngleChangeVelocity = 10;	

	[SerializeField]
	float shootGuideLength = 5.0f;

	[SerializeField]
	bool enablePreviewHitBubble = false;

	#region Raycast
	[SerializeField]
	private LayerMask reflectLayerMask;

	private Vector2 raycastDirection;
	private float raycastDistance;
	private RaycastHit2D[] hitBuffer = new RaycastHit2D[3];
	private ContactFilter2D contactFilter;
	private Vector2[] raycastPositions = new Vector2[3];

#endregion
	float shootAngle = 90f;

	private EBubbleColor BubbleColor = EBubbleColor.Blue;

	private TextMesh textShootInfo;

	private void Awake()
	{
		textShootInfo = GetComponentInChildren<TextMesh>();

		//LayerMask.NameToLayer()
		contactFilter.layerMask = reflectLayerMask;
		contactFilter.useLayerMask = true;
		contactFilter.useTriggers = false;

		stage.OnLevelStart.AddListener((_level) =>
			{
				UpdateShootInfoText();
				UpdateNextBubble(stage.GetNextShootBubbleColor());
			}
		);
	}

	// Start is called before the first frame update
	void Start()
    {
		//UpdateShootInfoText();
		//UpdateNextBubble(stage.GetNextShootBubbleColor());
	}

    // Update is called once per frame
    void Update()
    {

		// {@ Change Shoot Angle
		if ( Input.GetKey(KeyCode.A))
		{
			shootAngle += Time.deltaTime* shootAngleChangeVelocity;
		}
		if( Input.GetKey(KeyCode.D))
		{
			shootAngle -= Time.deltaTime * shootAngleChangeVelocity;
		}
		if (Input.GetKey(KeyCode.Z))
		{
			shootAngle += Time.deltaTime * shootAngleChangeVelocity * 0.5f;
		}
		if (Input.GetKey(KeyCode.C))
		{
			shootAngle -= Time.deltaTime * shootAngleChangeVelocity * 0.5f;
		}
		// @}
		// Shooting Bobble
		if (Input.GetKeyDown(KeyCode.S))
		{
			ShootBubble();

			// 다음 방울을 결정
			//var BubbleColors = Enum.GetValues(typeof(Bubble.EBubbleColor)).Cast<Bubble.EBubbleColor>().ToList();
			//BubbleColor = BubbleColors[Random.Range(0, BubbleColors.Count - 1)];
			UpdateNextBubble(stage.GetNextShootBubbleColor());

		}
		// {@ Cheat Change Bubble Color
		if (Input.GetKeyDown(KeyCode.R))
		{
			BubbleColor = EBubbleColor.Red;
			UpdateShootInfoText();
		}
		if (Input.GetKeyDown(KeyCode.G))
		{
			BubbleColor = EBubbleColor.Green;
			UpdateShootInfoText();
		}
		if (Input.GetKeyDown(KeyCode.B))
		{
			BubbleColor = EBubbleColor.Blue;
			UpdateShootInfoText();
		}
		if (Input.GetKeyDown(KeyCode.Y))
		{
			BubbleColor = EBubbleColor.Yellow;
			UpdateShootInfoText();
		}
		// @}

		var ShootDirection = Quaternion.AngleAxis(shootAngle, Vector3.forward) * Vector3.right;
		ShootDirection.Normalize();

		DrawShootGuide(ShootDirection, shootGuideLength);
	}

	private void ShootBubble()
	{
		var Instance = GameObject.Instantiate(BubblePrefab, transform);
		if (Instance != null)
		{
			var bubble = Instance.GetComponent<Bubble>();

			bubble.BubbleColor = BubbleColor;
			bubble.Shoot(shootAngle, BubbleVelocity, stage.GetAllSideLayerMask());

			stage.OnShootBubbleObject(Instance);
		}
	}

	private void UpdateNextBubble(EBubbleColor _bubbleColor)
	{
		BubbleColor = _bubbleColor;
		Utility.SetBubbleColor(NextBubble, _bubbleColor);
		//if (NextBubble != null)
		//{
		//	var animator = NextBubble.GetComponent<Animator>();
		//	if (animator != null)
		//	{
		//		int colorIndex = (int)_bubbleColor;
		//		animator.SetInteger("Color", colorIndex);
		//	}
		//}
	}
	// 발사 가이드를 그린다( 임시다 )
	private void DrawShootGuide(Vector2 _ShootDirection, float _ShootGuideLength)
	{
		List<Vector2> points = new List<Vector2>();
		points.Add(transform.position);

		Vector2 rayDirection = _ShootDirection;
		float CurrentGuideLength = _ShootGuideLength;

		contactFilter.layerMask = stage.GetAllSideLayerMask();
		contactFilter.layerMask |= 1 << BubblePrefab.layer;

		float bubbleRadius = 0.08f;

		while (CurrentGuideLength > 0)
		{
			Vector2 LastestPoint = points[points.Count - 1];
			int count = Physics2D.Raycast(LastestPoint, rayDirection, contactFilter, hitBuffer, CurrentGuideLength);

			// {@ Ray 를 발사해 방울에 hit 하는지 여부 판단

			if(enablePreviewHitBubble)
			{
				Ray ray = new Ray();
				ray.direction = rayDirection;
				ray.origin = LastestPoint;
				//ray.
				Bubble firstHitBubble = null;
				float distanceFromOrigin = 0f;
				if (stage.FindFirstRayIntersectBubble(ray, ref firstHitBubble, ref distanceFromOrigin))
				{
					var hitPosition = LastestPoint + rayDirection * distanceFromOrigin;
					stage.DrawPreviewHitBubble(firstHitBubble, hitPosition, BubbleColor);

					// ray 에 hit 한 방울이 있다면 hit 지점을 points 에 넣고 끝낸다
					points.Add(hitPosition);
					break;
				}
			}
			// @}

			// 히트 했다면
			if (count > 0)
			{
				for (int i = 0; i < hitBuffer.Length; i++)
				{
					if (hitBuffer[i].collider != null)
					{
						var CurrentHitLayer = hitBuffer[i].collider.gameObject.layer;
						if(CurrentHitLayer == BubblePrefab.layer)
						{
							// *Bubble 이 부딪혔다면 다르게 처리해야한다
							Debug.Log("dsdssd");
						}

						Vector2 normal = hitBuffer[i].normal;
						Vector2 prevRayDirection = rayDirection;

						rayDirection = Vector3.Reflect(rayDirection, normal);
						rayDirection.Normalize();

						contactFilter.layerMask = 0;
						var currentSideLayer = hitBuffer[i].collider.gameObject.layer;
						foreach( var sideLayer in stage.SideLayerMasks)
						{
							if(currentSideLayer != sideLayer)
							{
								contactFilter.layerMask |= 1 << sideLayer;
							}
						}
						Vector2 firstPoint = points[points.Count - 1];
						//
						float angleNormal2Ray = Vector2.Angle(normal, -prevRayDirection);
						float temp = bubbleRadius * 1 / Mathf.Sin((90 - angleNormal2Ray) * Mathf.Deg2Rad);

						Vector2 secondPoint = -prevRayDirection * temp + hitBuffer[i].point;

						float distance = (secondPoint - firstPoint).magnitude;
						// 예외처리
						if (distance <= 0)
						{
							CurrentGuideLength = 0;
						}
						else
						{
							CurrentGuideLength -= distance;
							points.Add(secondPoint);
						}
						break;
					}
				}
			}
			else 
			{
				// 히트 하지 못했다면
				Vector2 LastPoint = points[points.Count - 1]; // 마지막
				points.Add(LastPoint + rayDirection * CurrentGuideLength);
				CurrentGuideLength = 0;
				// 여기서 종료 한다
			}
			for (int i = 0; i < hitBuffer.Length; i++)
			{
				hitBuffer[i] = new RaycastHit2D();
			}
		}

		for (int i = 0; i < points.Count - 1; ++i)
		{
			int colorIndex = i % presetColors.Length;
			Debug.DrawLine(points[i], points[i + 1], presetColors[colorIndex]);
		}
	}

	private void UpdateShootInfoText()
	{
		textShootInfo.text = string.Format("{0}", Bubble.GetColorString(BubbleColor));
	}

	private static Color[] presetColors = {
			Color.red,
			Color.white,
			Color.yellow,
			Color.green,
			Color.gray,
			Color.cyan
	};
}
