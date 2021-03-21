using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class OnBubbleBoomEvent : UnityEvent<EBubbleColor, int> {}
public class OnBubbleFallEvent : UnityEvent<int> {}

public class BubbleContainer : MonoBehaviour
{
	[SerializeField]
	private float cellRadius = 5;

	[SerializeField]
	private GameObject bubbleParent;

	[SerializeField]
	private GameObject sideParent;

	[SerializeField]
	private Collider2D gameOverCheckCollider;

	[SerializeField]
	GameObject Prefab;
	[SerializeField]
	GameObject PreviewBubble;

	[SerializeField]
	private List<LayerMask> sideLayerMasks = new List<LayerMask>();
	public IReadOnlyList<LayerMask> SideLayerMasks => sideLayerMasks;

	private List<Bubble> placedBubbles = new List<Bubble>();
	// Start is called before the first frame update
	private Coroutine coroutineVibrate = null;

	private Vector2 initBubbleParentPoistion = Vector2.zero;
	private Vector2 initSideParentPosition = Vector2.zero;

	public OnBubbleBoomEvent onBubbleBoom = new OnBubbleBoomEvent();
	public OnBubbleFallEvent onBubbleFall = new OnBubbleFallEvent();

	private static Vector2 InvalidCell = new Vector2(-1, -1);

	private void Awake()
	{
		if (sideParent != null)
		{
			//for (int i = 0; i < sideParent.transform.childCount; ++i)
			//{
			//	var sideObject = sideParent.transform.GetChild(i);
			//	if (sideObject != null)
			//	{
			//		sideLayerMasks.Add(sideObject.gameObject.layer);
			//	}
			//}
			initSideParentPosition = sideParent.transform.localPosition;
		}
		if(bubbleParent != null)
		{
			initBubbleParentPoistion = bubbleParent.transform.localPosition;
		}
	}
	public void StartLevel(Level _level)
	{
		StopVibrate();
		// initialize game state
		bubbleParent.transform.localPosition = initBubbleParentPoistion;
		sideParent.transform.localPosition = initSideParentPosition;

		_LoadLevel(_level);

	}

	public LayerMask GetAllSideLayerMask()
	{
		//LayerMask layerMask = 0;
		//foreach (var mask in sideLayerMasks)
		//{
		//	layerMask |= 1 << mask;
		//}
		//string strLayerName = LayerMask.LayerToName(layerMask);
		//return layerMask;


		return sideLayerMasks[0];
	}

	private void _AttachBubble(Bubble _bubble, Vector2 _cell)
	{
		_bubble.PlaceInCell(_cell);

		_bubble.transform.parent = bubbleParent.transform;
		_bubble.transform.localPosition = ConvertCellToPosition(_cell);

		placedBubbles.Add(_bubble);
	}

	public void UpdateShootBubblePreset(ref List<EBubbleColor> _shootBubblePreset)
	{
		_shootBubblePreset.Clear();
		foreach (var bubble in placedBubbles)
		{
			if (!_shootBubblePreset.Exists(_ => _ == bubble.BubbleColor))
			{
				_shootBubblePreset.Add(bubble.BubbleColor);
			}
		}
	}

	private void _LoadLevel(Level _level)
	{
		// 일단 clear 
		foreach (var bubble in placedBubbles)
		{
			GameObject.Destroy(bubble.gameObject);
		}
		placedBubbles.Clear();

		foreach (var bubbleData in _level.Bubbles)
		{

			CreateAndPlaceBubble(new Vector2(bubbleData.Cell.x, bubbleData.Cell.y),
				bubbleData.Color);
		}
	}

	public void StartVibrate(int _step)
	{
		StopVibrate();
		coroutineVibrate = StartCoroutine(_Vibrate(_step));

	}

	public void StopVibrate()
	{
		if (coroutineVibrate != null)
		{
			StopCoroutine(coroutineVibrate);
			coroutineVibrate = null;
		}
		foreach (var bubble in placedBubbles)
		{
			bubble.SetSpritePostion(Vector2.zero);
		}
	}

	private IEnumerator _Vibrate(int _step)
	{
		// to
		const float limit = 0.02f;
		const float delta = 0.01f;
		float sign = 1;

		float delaySeconds = 0.0f;
		if (_step == 1)
		{
			delaySeconds = 0.01f;
		}
		else if (_step == 2)
		{
			delaySeconds = 0.006f;
		}

		float currentX = 0f;
		while (true)
		{
			float currentDelta = sign > 0 ? delta : -delta;
			currentX += currentDelta;

			if (Mathf.Abs(currentX) > limit)
			{
				currentX = sign * limit;

				sign *= -1; // 부호
			}
			foreach (var bubble in placedBubbles)
			{
				bubble.SetSpritePostion(new Vector3(currentX, 0));
			}
			yield return new WaitForSeconds(delaySeconds);
		}
	}


	

	public bool OnHitBubble(GameObject _hitGameObject, Bubble _hitBubble)
	{
		if (_hitGameObject == null)
		{
			return false;
		}

		bool collisionWithTop = _hitGameObject.layer == LayerMask.NameToLayer("Reflect_Top");
		var placedBubble = _hitGameObject.GetComponent<Bubble>();
		if (placedBubble != null || collisionWithTop)
		{
			var moveController = _hitBubble.GetComponent<BubbleMoveController>();
			if (moveController != null)
			{
				moveController.StopMoving();
			}
			// hitBubble 은 아직 BubbleParent 의 자식이 아니므로 
			// BubbleParent 기준으로 hitBubble 좌표를 얻어낸다 ( 좌표 일치화를 위해 )
			Vector2 hitBubblePosition = bubbleParent.transform.InverseTransformPoint(_hitBubble.transform.position);
			Vector2 newCell = InvalidCell;
			if (placedBubble != null)
			{
				newCell = _FindHitBubbleAdjacentCell(placedBubble, hitBubblePosition);
			}
			else if (collisionWithTop)
			{
				newCell = _FindCellOnTopHitBubble(hitBubblePosition);
			}

			if(newCell != InvalidCell)
			{
				_AttachBubble(_hitBubble, newCell);
				// 같은 색을 가진 bubble 을 제거
				if (_RemoveSameColorBubbles(_hitBubble))
				{
					// 사라진 Bubble 들이 존재 한다면 이제 연결이 끊어진 Bubble 들을 찾아본다
					_FallLinkOffBubbles();
				}
			}
			return true;
		}
		return false;
	}

	public bool CheckGameOver()
	{
		if (gameOverCheckCollider != null)
		{
			foreach (var bubble in placedBubbles)
			{
				var collider = bubble.GetComponent<Collider2D>();

				if (collider.bounds.Intersects(gameOverCheckCollider.bounds))
				{
					return true;
				}
			}
		}
		return false;
	}
	private bool _RemoveSameColorBubbles(Bubble _bubble)
	{
		bool existRemovedBubbles = false;

		// 새로 추가된 Bubble 근처에 같은 색을 가진 Bubble 이 존재하는지 체크 한다
		List<Bubble> sameColorBubbles = new List<Bubble>();
		sameColorBubbles.Add(_bubble);
		_FindLinkedSameColorBubbles(sameColorBubbles, _bubble);

		// 찾은 목록에서 같은 색이 3개 이상일 경우 지운다!
		if (sameColorBubbles.Count >= 3)
		{
			onBubbleBoom.Invoke(_bubble.BubbleColor, sameColorBubbles.Count);

			foreach (var bubble in sameColorBubbles)
			{

				bubble.Boom();
				placedBubbles.Remove(bubble);
			}
			existRemovedBubbles = true;
		}
		return existRemovedBubbles;
	}

	// 연결이 끊어진 Bubble 들을 제거한다
	private void _FallLinkOffBubbles()
	{
		// 시작은 천장에 붙어있는 Bubble 들을 모은다.
		// y 가 0 인 것들( 천장에 붙어있는 )을 모두 모은다
		List<Bubble> linkedBubble = new List<Bubble>();
		List<Bubble> SearchTopBubbles = placedBubbles.FindAll((_bubble) => _bubble.PlacedCell.y == 0).ToList();

		//linkedBubble.AddRange();
		foreach (var startBubble in SearchTopBubbles)
		{
			_FindLinkToTopBubbles(ref linkedBubble, startBubble);
		}
		// 이제 연결된 모든 Bubble List(linkedBubble) 를 얻었으므로 
		// 이제 BubbleContainer 엔 있지만 linkedBubble 에 없는 bubble 들이 삭제될 bubble 이다
		List<Bubble> OffLinkedBubble = placedBubbles.FindAll((_bubble) =>
		{
			return _bubble.PlacedCell.y != 0 && !linkedBubble.Exists(_ => _.PlacedCell == _bubble.PlacedCell);
		}
		);
		// 이제 삭제
		foreach (var removeBubble in OffLinkedBubble)
		{
			removeBubble.Fall();
			placedBubbles.Remove(removeBubble);
		}
		onBubbleFall.Invoke(OffLinkedBubble.Count);
	}
	// Bubble 을 배치한다
	public void CreateAndPlaceBubble(Vector2 _cell, EBubbleColor _bubbleColor)
	{
		Vector2 bubbleCenter = ConvertCellToPosition(_cell);

		var bubbleInstance = GameObject.Instantiate(Prefab);//, bubbleCenter, Quaternion.identity);

		bubbleInstance.transform.parent = bubbleParent.transform;
		bubbleInstance.transform.localPosition = bubbleCenter;

		var bubble = bubbleInstance.GetComponent<Bubble>();

		bubble.BubbleColor = _bubbleColor;
		bubble.PlaceInCell(_cell);


		placedBubbles.Add(bubble);
	}

	// 천장과 연결된 Bubble 들을 찾는다
	private void _FindLinkToTopBubbles(ref List<Bubble> _linkedBubbles, Bubble _bubble)
	{
		foreach (var offset in Utility.GetAdjecentCellOffsets(_bubble.PlacedCell))
		{
			var searchCell = _bubble.PlacedCell + offset;
			// * y 가 0 인 경우는 천장이므로 스킵
			if (searchCell.y == 0)
			{
				continue;
			}
			// 찾으려는 Cell 에 방울이 존재하지 않는다면 스킵
			Bubble foundBubble = placedBubbles.Find(_ => _.PlacedCell == searchCell);
			if (foundBubble == null)
			{
				continue;
			}

			// 이미 체크된 Bubble 이면 스킵
			if (_linkedBubbles.Contains(foundBubble))
			{
				continue;
			}
			// marked
			_linkedBubbles.Add(foundBubble);
			_FindLinkToTopBubbles(ref _linkedBubbles, foundBubble);
		}
	}

	// 연결되고 같은 색을 가진 Bubble 을 찾는다
	private void _FindLinkedSameColorBubbles(List<Bubble> _outSameColorBubbles, Bubble _centerBubble)
	{
		foreach (var cell in Utility.GetAdjecentCellOffsets(_centerBubble.PlacedCell))
		{
			var searchCell = _centerBubble.PlacedCell + cell;

			if (searchCell.x < 0 || searchCell.y < 0)
			{
				continue;
			}
			// 찾으려는 Cell 에 방울이 존재하지 않는다면 스킵
			Bubble foundBubble = placedBubbles.Find(_ => _.PlacedCell == searchCell);
			if (foundBubble == null)
			{
				continue;
			}
			if (foundBubble.BubbleColor == _centerBubble.BubbleColor)
			{
				if (!_outSameColorBubbles.Contains(foundBubble))
				{
					_outSameColorBubbles.Add(foundBubble);
					_FindLinkedSameColorBubbles(_outSameColorBubbles, foundBubble);
				}
			}
		}
	}

	private Vector2 _FindCellOnTopHitBubble(Vector2 _hitBubblePosition)
	{
		// 천장 cell 중 가장 가까운 cell 좌표를 넘긴다
		int foundCellX = -1;
		float MinDist = float.PositiveInfinity;
		for ( int i = 0; i < 8; ++i)
		{
			var cellCenterPosition = ConvertCellToPosition(new Vector2(i, 0));
			float distance = (cellCenterPosition - _hitBubblePosition).magnitude;

			if (MinDist > distance)
			{
				MinDist = distance;
				foundCellX = i;
			}
		}
		return new Vector2(foundCellX, 0);
	}

	// _hitBubble 에 부딪힌 bubble 이 배치될 위치를 반환
	private Vector2 _FindHitBubbleAdjacentCell(Bubble _placedBubble, Vector2 _NewBubbleCenterPoistion)
	{
		Vector2 adjecentCell = Vector2.zero;
		float MinDist = float.PositiveInfinity;
		foreach (var offset in Utility.GetAdjecentCellOffsets(_placedBubble.PlacedCell))
		{
			var searchCell = _placedBubble.PlacedCell + offset;

			if (searchCell.x < 0)
			{
				continue;
			}
			if (searchCell.y < 0)
			{
				continue;
			}
			// 찾으려는 Cell 에 이미 방울이 존재한다면 스킵
			if (placedBubbles.Exists(_ => _.PlacedCell == searchCell))
			{
				continue;
			}
			var cellPostion = ConvertCellToPosition(searchCell);
			float distance = (cellPostion - _NewBubbleCenterPoistion).magnitude;

			if (MinDist > distance)
			{
				MinDist = distance;
				adjecentCell = searchCell;
			}
		}
		return adjecentCell;
	}

	private Vector2 ConvertCellToPosition(Vector2 _cell)
	{
		return Utility.ConvertCellToPosition(_cell, cellRadius);
	}

	public bool IsEmpty => placedBubbles.Count <= 0;

	public void TestIntersectRay(Ray _ray)
	{
		List<Bubble> intersectBubbles = new List<Bubble>();

		//float minDistance = float.MaxValue;
		//Bubble nearestBubble = null;

		foreach (var bubble in placedBubbles)
		{

			bubble.SetAlpha(1.0f);
			float distance = 0.0f;
			var collider = bubble.GetComponent<Collider2D>();

			if (collider.bounds.IntersectRay(_ray, out distance))
			{
				bubble.SetAlpha(0.1f);
				Debug.Log(string.Format("Cell ({0}, {1}) - {2}", bubble.PlacedCell.x, bubble.PlacedCell.y, distance));
			}
		}
	}

	public bool FindFirstRayIntersectBubble(Ray _ray, ref Bubble _bubble, ref float _distance)
	{
		List<Bubble> intersectBubbles = new List<Bubble>();

		float minDistance = float.MaxValue;
		Bubble nearestBubble = null;

		foreach (var bubble in placedBubbles)
		{
			//bubble.SetAlpha(1.0f);
			var collider = bubble.GetComponent<Collider2D>();
			float distance = 0.0f;
			if (collider.bounds.IntersectRay(_ray, out distance))
			{
				if (distance < minDistance)
				{
					minDistance = distance;
					nearestBubble = bubble;
				}
			}
		}
		if (nearestBubble != null)
		{
			_bubble = nearestBubble;
			_distance = minDistance;
			return true;
		}
		return false;
	}


	public void DownTop()
	{
		StopVibrate();

		float deltaHeight = (cellRadius + 0.5f * cellRadius);

		Vector3 prevPosition = bubbleParent.transform.localPosition;
		bubbleParent.transform.localPosition =
			new Vector2(prevPosition.x, prevPosition.y - deltaHeight);

		// 사이드도 내려야한다
		if (sideParent != null)
		{
			var currPostion = sideParent.transform.localPosition;
			sideParent.transform.localPosition =
				new Vector2(currPostion.x, currPostion.y - deltaHeight);
		}
	}

	#region debug 

	public void DrawPreviewHitBubble(Bubble _hitBubble, Vector2 _hitPosition, EBubbleColor _color)
	{
		var previewBubblePosition = bubbleParent.transform.InverseTransformPoint(_hitPosition);
		var cell = _FindHitBubbleAdjacentCell(_hitBubble, previewBubblePosition);

		PreviewBubble.SetActive(true);
		PreviewBubble.transform.localPosition = ConvertCellToPosition(cell);
		{
			Utility.SetSpriteAlpha(PreviewBubble, 0.3f);
			Utility.SetBubbleColor(PreviewBubble, _color);
		}
	}

	// for debug
	private void _PlaceInitBubbles()
	{
		// * 이렇게 처리한 건 물론 임시다
		var BubbleColors = Enum.GetValues(typeof(EBubbleColor)).Cast<EBubbleColor>().ToList();
		for (int x = 0; x < 8; x++)
		{
			for (int y = 0; y < 1; y++)
			{
				int ColorIndex = UnityEngine.Random.Range(0, BubbleColors.Count - 1);

				CreateAndPlaceBubble(new Vector2(x, y), BubbleColors[ColorIndex]);
			}
		}
	}

	#endregion

}
