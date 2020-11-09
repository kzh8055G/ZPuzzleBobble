using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class OnLevelStartEvent : UnityEvent<int> {};
public class OnLevelClearEvent : UnityEvent<int> {};
public class OnScoreGetEvent : UnityEvent<int> {};

public class Stage : MonoBehaviour
{
	[SerializeField]
	GameObject Prefab;
	[SerializeField]
	GameObject PreviewBubble;

	[SerializeField]
	private Vector2 StartPosition;

	[SerializeField]
	private float CellRadius = 5;

	[SerializeField]
	private GameObject BubbleParent;

	[SerializeField]
	private GameObject SideParent;

	[SerializeField]
	private Collider2D gameOverCheckCollider;

	#region Sound
	[SerializeField]
	private AudioClip SinglePlayBGM;

	[SerializeField]
	private AudioClip StageClearBGM;

	[SerializeField]
	private AudioClip GameOverBGM;

	[SerializeField]
	private AudioSource AudioSourceComponent;
	#endregion

	#region Level

	private int currentLevelNumber = 1;
	private Level CurrentLevel = null;
	#endregion

	private int currentShootCount = 0;

	// TODO : 이것도 레벨에서 값을 저장하는게 어떠한가?
	private int vibrateStartShootCount = 3;

	public OnLevelStartEvent OnLevelStart = new OnLevelStartEvent();
	public OnLevelClearEvent OnLevelClear = new OnLevelClearEvent();
	public OnScoreGetEvent OnScoreGet = new OnScoreGetEvent();

	private List<Bubble> BubbleContainer = new List<Bubble>();

	public IReadOnlyList<LayerMask> SideLayerMasks => sideLayerMasks;
	private List<LayerMask> sideLayerMasks = new List<LayerMask>();

	private List<EBubbleColor> shootBubblePreset = new List<EBubbleColor>();

	private Coroutine coroutineVibrate = null;

	private Vector2 initBubbleParentPoistion = Vector2.zero;
	private Vector2 initSideParentPosition = Vector2.zero;

	#region Score

	CaculateScoreOnBubbleHit scoreCalculator = new CaculateScoreOnBubbleHit();

	#endregion
	#region Score
	private struct CaculateScoreOnBubbleHit
	{
		public void Clear()
		{
			boomCountOnShoot = 0;
			fallCountOnShoot = 0;

		}
		public void OnBoomBubbles(int _count)
		{
			boomCountOnShoot = _count;
		}
		public void OnFallBubbles(int _count)
		{
			fallCountOnShoot = _count;
		}
		public int Calculate()
		{
			int scoreByBoom = (boomCountOnShoot * 10);
			int scoreByFall = fallCountOnShoot > 0 ? (int)(Math.Pow(2, fallCountOnShoot) * 10) : 0;
			return scoreByBoom + scoreByFall;
		}
		private int boomCountOnShoot;
		private int fallCountOnShoot;
	}
	#endregion


	void Awake()
	{
		if (SideParent != null)
		{
			for (int i = 0; i < SideParent.transform.childCount; ++i)
			{
				var sideObject = SideParent.transform.GetChild(i);
				if (sideObject != null)
				{
					sideLayerMasks.Add(sideObject.gameObject.layer);
				}
			}

			initSideParentPosition = SideParent.transform.localPosition;
		}
		AudioSourceComponent = GetComponent<AudioSource>();

		initBubbleParentPoistion = BubbleParent.transform.localPosition;

		ManageLevelData.LoadFromFile();
	}

	//private  Transform GetTopTransform()
	//{
	//	if (SideParent != null)
	//	{
	//		for (int i = 0; i < SideParent.transform.childCount; ++i)
	//		{
	//			var sideObject = SideParent.transform.GetChild(i);
	//			if (sideObject != null)
	//			{
	//				if( sideObject.gameObject.layer == LayerMask.NameToLayer("Reflect_Top"))
	//				{
	//					return sideObject;
	//				}
	//				//sideLayerMasks.Add(sideObject.gameObject.layer);
	//			}
	//		}
	//	}
	//	return null;
	//}

	void Start()
	{
		// load level data
		_StartLevel();
	}

	public EBubbleColor GetNextShootBubbleColor()
	{
		return shootBubblePreset[Random.Range(0, shootBubblePreset.Count - 1)];
	}

	private void _UpdateShootBubblePreset()
	{
		shootBubblePreset.Clear();
		foreach ( var bubble in BubbleContainer)
		{
			if( !shootBubblePreset.Exists( _ => _ == bubble.BubbleColor))
			{
				shootBubblePreset.Add(bubble.BubbleColor);
			}
		}
	}

	private void _StartLevel()
	{
		StopVibrate();

		// initialize game state
		BubbleParent.transform.localPosition = initBubbleParentPoistion;
		SideParent.transform.localPosition = initSideParentPosition;

		if (ManageLevelData.GetLevel(currentLevelNumber, out CurrentLevel))
		{
			_LoadCurrentLevel();
		}
		_UpdateShootBubblePreset();

		if (AudioSourceComponent != null)
		{
			AudioSourceComponent.clip = SinglePlayBGM;
			AudioSourceComponent.loop = true;
			AudioSourceComponent.Play();
		}

		currentShootCount = 0;

		OnLevelStart.Invoke(currentLevelNumber);

	}

	private void _LoadCurrentLevel()
	{
		// 일단 clear 
		foreach( var bubble in BubbleContainer)
		{
			GameObject.Destroy(bubble.gameObject);
		}
		BubbleContainer.Clear();

		foreach (var bubbleData in CurrentLevel.Bubbles )
		{

			CreateAndPlaceBubble(new Vector2(bubbleData.Cell.x, bubbleData.Cell.y),
				bubbleData.Color);
		}
	}

	private void _GoToNextLevel()
	{
		currentLevelNumber += 1;

		_LoadCurrentLevel();
	}

	//
	public LayerMask GetAllSideLayerMask()
	{
		LayerMask layerMask = 0;
		foreach(var mask in sideLayerMasks)
		{
			layerMask |= 1 << mask;
		}
		return layerMask;
	}
	//


	public void OnShootBubbleObject(GameObject _bubbleInstance)
	{
		var bubble = _bubbleInstance.GetComponent<Bubble>();
		if (bubble != null)
		{
			bubble.OnBubbleHit.AddListener(_OnHitBubble);
		}

	}

	private void OnChangeShootCount()
	{
		//int shootCount = currentShootCount  % currentShootCount;
		if (currentShootCount > vibrateStartShootCount)
		{
			int vibrationStep = (currentShootCount - vibrateStartShootCount);
			bool downTop = (vibrationStep > 2);
			if(downTop)
			{
				// reset
				currentShootCount = 0;
				StopVibrate();
				_DownTop();
			}
			else
			{

				StartVibrate(vibrationStep);
			}
		}
	}
	#region Control Bubble Vibration


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
		foreach (var bubble in BubbleContainer)
		{
			bubble.SetSpritePostion(Vector2.zero);
		}
	}

	//private Vector2 bubbleVibrationOffset = Vector2.zero;

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
			foreach(var bubble in BubbleContainer)
			{
				bubble.SetSpritePostion(new Vector3(currentX, 0));
			}
			yield return new WaitForSeconds(delaySeconds);
		}
	}
	#endregion


	private void _DownTop()
	{
		float deltaHeight = (CellRadius + 0.5f * CellRadius);

		Vector3 prevPosition = BubbleParent.transform.localPosition;
		BubbleParent.transform.localPosition =
			new Vector2(prevPosition.x, prevPosition.y - deltaHeight);

		// 천장 도 내려야된다
		if (SideParent != null)
		{
			var currPostion = SideParent.transform.localPosition;
			SideParent.transform.localPosition =
				new Vector2(currPostion.x, currPostion.y - deltaHeight);
		}
	}
	// Bubble 
	private void AttachBubble(Bubble _bubble, Vector2 _cell)
	{
		_bubble.PlaceInCell(_cell);

		_bubble.transform.parent = BubbleParent.transform;
		_bubble.transform.localPosition = ConvertCellToPosition(_cell);

		

		BubbleContainer.Add(_bubble);
	}
	
	private void _OnHitBubble(Bubble _placedBubble, Bubble _hitBubble)
	{
		scoreCalculator.Clear();
		// _hitBubble 위치에 가장 가까운 Cell 을 찾는다

		// hitBubble 은 아직 BubbleParent 의 자식이 아니므로 
		// BubbleParent 기준으로 hitBubble 좌표를 얻어낸다 ( 좌표 일치화를 위해 )
		Vector2 hitBubblePosition = BubbleParent.transform.InverseTransformPoint(_hitBubble.transform.position);
		var newCell = FindHitBubbleAdjacentCell(_placedBubble, hitBubblePosition);
		if (newCell != Vector2.zero)
		{
			// 일단 움직임을 멈추고
			var moveController = _hitBubble.GetComponent<BubbleMoveController>();
			if (moveController != null)
			{
				moveController.StopMoving();
			}
			// 배치한다
			AttachBubble(_hitBubble, newCell);
			// 같은 색을 가진 bubble 을 제거
			if (_RemoveSameColorBubbles(_hitBubble))
			{
				// 사라진 Bubble 들이 존재 한다면 이제 연결이 끊어진 Bubble 들을 찾아본다
				_FallLinkOffBubbles();
			}
		}

		_UpdateShootBubblePreset();
		//_CollectPlacedBubbleColors();
		//
		if ( !_CheckLevelClear() )
		{
			++currentShootCount;
			OnChangeShootCount();
		}

		if(_CheckGameOver())
		{
			_OnGameOver();		
		}
		OnScoreGet.Invoke(scoreCalculator.Calculate());


	}
	// TODO : 
	private void _OnGameOver()
	{
		if (AudioSourceComponent != null)
		{
			AudioSourceComponent.Stop();
			AudioSourceComponent.loop = false;
			AudioSourceComponent.clip = GameOverBGM;
			AudioSourceComponent.Play();

			//AudioSourceComponent.pla
		}
		Debug.Log("Game Over");
	}

	private void _CollectPlacedBubbleColors()
	{
		//BubbleContainer
	}

	private bool _CheckGameOver()
	{
		if(gameOverCheckCollider != null)
		{
			foreach (var bubble in BubbleContainer)
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


	private bool _CheckLevelClear()
	{
		bool clear = BubbleContainer.Count <= 0;
		if(clear)
		{
			StartCoroutine(_OnLevelClear());
		}
		return clear;
	}

	private IEnumerator _OnLevelClear()
	{
		if (AudioSourceComponent != null)
		{
			AudioSourceComponent.Stop();
			AudioSourceComponent.loop = false;
			AudioSourceComponent.clip = StageClearBGM;
			AudioSourceComponent.Play();
		}
		StopVibrate();

		OnLevelClear.Invoke(currentLevelNumber);


		yield return new WaitForSeconds(3);

		// 다음 레벨로 넘어간다

		++currentLevelNumber;
		_StartLevel();
	}

	// 방금 배치된 bubble 에 연결된 것중 같은 색을 갖는 bubble 을 삭제한다
	private bool _RemoveSameColorBubbles(Bubble _bubble)
	{
		bool existRemovedBubbles = false;

		// 새로 추가된 Bubble 근처에 같은 색을 가진 Bubble 이 존재하는지 체크 한다
		List<Bubble> sameColorBubbles = new List<Bubble>();
		sameColorBubbles.Add(_bubble);
		FindLinkedSameColorBubbles(sameColorBubbles, _bubble);

		// 찾은 목록에서 같은 색이 3개 이상일 경우 지운다!
		if (sameColorBubbles.Count >= 3)
		{
			scoreCalculator.OnBoomBubbles(sameColorBubbles.Count);

			foreach (var bubble in sameColorBubbles)
			{

				bubble.Boom();
				BubbleContainer.Remove(bubble);
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
		List<Bubble> SearchTopBubbles = BubbleContainer.FindAll((_bubble) => _bubble.PlacedCell.y == 0).ToList();

		//linkedBubble.AddRange();
		foreach (var startBubble in SearchTopBubbles)
		{
			FindLinkToTopBubbles(ref linkedBubble, startBubble);
		}
		// 이제 연결된 모든 Bubble List(linkedBubble) 를 얻었으므로 
		// 이제 BubbleContainer 엔 있지만 linkedBubble 에 없는 bubble 들이 삭제될 bubble 이다
		List<Bubble> OffLinkedBubble = BubbleContainer.FindAll((_bubble) =>
			{
				return _bubble.PlacedCell.y != 0 && !linkedBubble.Exists(_ => _.PlacedCell == _bubble.PlacedCell);
			}
		);
		// 이제 삭제
		foreach (var removeBubble in OffLinkedBubble)
		{
			removeBubble.Fall();
			BubbleContainer.Remove(removeBubble);
		}

		scoreCalculator.OnFallBubbles(OffLinkedBubble.Count);
	}

	private void PlaceInitBubbles()
	{
		// * 이렇게 처리한 건 물론 임시다
		var BubbleColors = Enum.GetValues(typeof(EBubbleColor)).Cast<EBubbleColor>().ToList();
		for (int x = 0; x < 8 ; x++)
		{
			for (int y = 0; y < 1 ; y++)
			{
				int ColorIndex = Random.Range(0, BubbleColors.Count - 1);

				CreateAndPlaceBubble(new Vector2( x, y), BubbleColors[ColorIndex]);
			}
		}
	}

	// Bubble 을 배치한다
	private void CreateAndPlaceBubble(Vector2 _cell, EBubbleColor _bubbleColor = EBubbleColor.Blue)
	{
		Vector2 bubbleCenter = ConvertCellToPosition(_cell);

		var bubbleInstance = GameObject.Instantiate(Prefab);//, bubbleCenter, Quaternion.identity);

		bubbleInstance.transform.parent = BubbleParent.transform;
		bubbleInstance.transform.localPosition = bubbleCenter;

		var bubble = bubbleInstance.GetComponent<Bubble>();

		bubble.BubbleColor = _bubbleColor;
		bubble.PlaceInCell(_cell);


		BubbleContainer.Add(bubble);
	}


	#region Search Method

	// 천장과 연결된 Bubble 들을 찾는다
	private void FindLinkToTopBubbles(ref List<Bubble> _linkedBubbles, Bubble _bubble)
	{
		foreach (var offset in GetAdjecentCellOffsets(_bubble.PlacedCell))
		{
			var searchCell = _bubble.PlacedCell + offset;
			// * y 가 0 인 경우는 천장이므로 스킵
			if (searchCell.y == 0)
			{
				continue;
			}
			// 찾으려는 Cell 에 방울이 존재하지 않는다면 스킵
			Bubble foundBubble = BubbleContainer.Find(_ => _.PlacedCell == searchCell);
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
			FindLinkToTopBubbles(ref _linkedBubbles, foundBubble);
		}
	}

	// 연결되고 같은 색을 가진 Bubble 을 찾는다
	private void FindLinkedSameColorBubbles(List<Bubble> _outSameColorBubbles, Bubble _centerBubble)
	{
		foreach (var cell in GetAdjecentCellOffsets(_centerBubble.PlacedCell))
		{
			var searchCell = _centerBubble.PlacedCell + cell;

			if (searchCell.x < 0 || searchCell.y < 0)
			{
				continue;
			}
			// 찾으려는 Cell 에 방울이 존재하지 않는다면 스킵
			Bubble foundBubble = BubbleContainer.Find(_ => _.PlacedCell == searchCell);
			if (foundBubble == null)
			{
				continue;
			}
			if (foundBubble.BubbleColor == _centerBubble.BubbleColor)
			{
				if (!_outSameColorBubbles.Contains(foundBubble))
				{
					_outSameColorBubbles.Add(foundBubble);
					FindLinkedSameColorBubbles(_outSameColorBubbles, foundBubble);
				}
			}
		}
	}

	// _hitBubble 에 부딪힌 bubble 이 배치될 위치를 반환
	public Vector2 FindHitBubbleAdjacentCell(Bubble _hitBubble, Vector2 _NewBubbleCenterPoistion)
	{
		Vector2 adjecentCell = Vector2.zero;
		float MinDist = float.PositiveInfinity;
		foreach (var offset in GetAdjecentCellOffsets(_hitBubble.PlacedCell))
		{
			var searchCell = _hitBubble.PlacedCell + offset;

			if (searchCell.x < 0)
			{
				continue;
			}
			if (searchCell.y < 0)
			{
				continue;
			}
			// 찾으려는 Cell 에 이미 방울이 존재한다면 스킵
			if (BubbleContainer.Exists(_ => _.PlacedCell == searchCell))
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
	#endregion


	private int vibrateStep = 0;
	// Update is called once per frame
	void Update()
	{
		// debug
		//foreach( var bubble in BubbleContainer)
		//{
		//	bubble.DrawHexagon(CellRadius);
		//}

		if(Input.GetKeyDown(KeyCode.Q))
		{

		}

		//if(Input.GetKeyDown(KeyCode.V))
		//{
		//	++vibrateStep;
		//	if (vibrateStep > 2)
		//	{
		//		vibrateStep = 0;
		//	}
		//	foreach (var bubble in BubbleContainer)
		//	{
		//		if (vibrateStep == 0)
		//		{
		//			bubble.StopVibrate();
		//		}
		//		else
		//		{
		//			bubble.StartVibrate(vibrateStep);
		//		}
		//	}
		//	if(vibrateStep == 0)
		//	{
		//		Vector3 position = BubbleParent.transform.position;
		//		BubbleParent.transform.position =
		//			new Vector2(position.x, position.y - (CellRadius + 0.5f * CellRadius));//
		//	}
		//}
	}

	// ??? 도저히 어떻게 구현해야할지 모르겠다 ㅡ,.ㅡ
	private Vector2 ConvertPositionToCell(Vector2 _Postion)
	{
		return Vector2.zero;
	}

	private Vector2 ConvertCellToPosition(Vector2 _cell)
	{
		return ConvertCellToPosition(_cell, CellRadius);
	}

	public void TestIntersectRay(Ray _ray)
	{
		List<Bubble> intersectBubbles = new List<Bubble>();

		float minDistance = float.MaxValue;
		Bubble nearestBubble = null;

		foreach(var bubble in BubbleContainer)
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

		foreach (var bubble in BubbleContainer)
		{
			//bubble.SetAlpha(1.0f);
			var collider = bubble.GetComponent<Collider2D>();
			float distance = 0.0f;
			if (collider.bounds.IntersectRay(_ray, out distance))
			{
				if (distance < minDistance )
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

	public void DrawPreviewHitBubble(Bubble _hitBubble, Vector2 _hitPosition, EBubbleColor _color)
	{
		var previewBubblePosition = BubbleParent.transform.InverseTransformPoint(_hitPosition);
		var cell = FindHitBubbleAdjacentCell(_hitBubble, previewBubblePosition);

		PreviewBubble.SetActive(true);
		PreviewBubble.transform.localPosition = ConvertCellToPosition(cell);
		{
			Utility.SetSpriteAlpha(PreviewBubble, 0.3f);
			Utility.SetBubbleColor(PreviewBubble, _color);
		}
	}


	#region Utility

	// Cell 을 실제 위치로 변환
	public static Vector2 ConvertCellToPosition(Vector2 _cell, float _cellRadius)
	{
		// x : _cellY 가 짝, 홀수 이냐에 따라 x 시작 offset 이 달라진다.
		// 짝수 면 offset 0, 홀수면 offset 은 radius * cos 60
		Vector2 offset = Vector2.zero;
		offset.x = _cell.y % 2 == 0 ? 0 : _cellRadius * Mathf.Cos(30 * Mathf.Deg2Rad);
		offset.y = _cellRadius;

		Vector2 Center;

		Center.x = offset.x + 2 * _cellRadius * Mathf.Cos(30 * Mathf.Deg2Rad) * _cell.x;
		Center.y = offset.y - (_cellRadius + 0.5f * _cellRadius) * _cell.y;

		return Center;
	}

	// 전달된 Cell 의 주위 Cell Offset 목록을 반환
	private static IReadOnlyList<Vector2> GetAdjecentCellOffsets(Vector2 _cell)
	{
		return _cell.y % 2 == 0 ? cellOffsetEven.AsReadOnly() : cellOffsetOdd.AsReadOnly();
	}

	// 짝수
	private static List<Vector2> cellOffsetEven = new List<Vector2> {
		new Vector2(1, 0)		// right
		, new Vector2(0, -1)	// right_up
		, new Vector2(-1, -1)	// left_up
		, new Vector2(-1, 0)	// left
		, new Vector2(-1, 1)	// left_bottom
		, new Vector2(0, 1)		// right_bottom
	};
	// 홀수
	private static List<Vector2> cellOffsetOdd = new List<Vector2> {
		new Vector2( 1, 0)		// right
		, new Vector2( 1, -1)	// right_up
		, new Vector2( 0, -1)	// left_up
		, new Vector2( -1, 0)	// left
		, new Vector2( 0, 1)	// left_bottom
		, new Vector2( 1, 1)	// right_bottom
	};

	#endregion


}
