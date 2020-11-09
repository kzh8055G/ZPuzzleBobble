using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

// [출처] https://dobby-the-house-elf.tistory.com/62

public class OnChangeLevelEvent : UnityEvent<int>
{}

public class TestEdit : MonoBehaviour
{
	public enum EBubbleEditMode
	{
		Add,
		Remove,
	}

	[SerializeField]
	GameObject Prefab;

	[SerializeField]
	GameObject PreviewBubble;

	[SerializeField]
	Transform Root;

	[SerializeField]
	private UIController uiController;

	private EBubbleEditMode CurrentEditMode = EBubbleEditMode.Add;
	private EBubbleColor CurrentBubbleColor = EBubbleColor.Red;

	private Camera _uiCamera;

	private List<Bubble> BubbleContainer = new List<Bubble>();

	private readonly float cellRadius = 0.1f;

	[SerializeField]
	private int currentLevelNumber = 1;

	public OnChangeLevelEvent OnChangeLevel = new OnChangeLevelEvent();

	private void Awake()
	{
		PreviewBubble.active = false;

		//OnChangeBubbleColor
		if(uiController != null)
		{

			uiController.SetCurrentLevelNumber(currentLevelNumber);
			uiController.OnChangeBubbleColor.AddListener((_bubbleColor) =>
				{
					CurrentEditMode = _bubbleColor != EBubbleColor.None ? EBubbleEditMode.Add : EBubbleEditMode.Remove;
					CurrentBubbleColor = _bubbleColor;

					PreviewBubble.active = (CurrentEditMode == EBubbleEditMode.Add);

				}
			);
			uiController.OnClickNextLevel.AddListener(() =>
				{
					currentLevelNumber++;

					OnChangeLevel.Invoke(currentLevelNumber);


				}
			);
			uiController.OnClickPrevLevel.AddListener(() =>
				{
					int backupNumber = currentLevelNumber;
					currentLevelNumber = Math.Max(1, currentLevelNumber - 1);
					if (backupNumber != currentLevelNumber)
					{
						OnChangeLevel.Invoke(currentLevelNumber);
					}
				}
			);
			uiController.OnClickLoad.AddListener(() =>
				{
					// Load
					if (ManageLevelData.LoadFromFile())
					{
						OnLoad(currentLevelNumber);
					}
				}
			);
			uiController.OnClickSave.AddListener(() =>
				{
					var level = new Level();
					foreach (var bubble in BubbleContainer)
					{
						var bubbleData = new BubbleData();

						bubbleData.Cell.Set(bubble.PlacedCell);
						bubbleData.Color = bubble.BubbleColor;

						level.Bubbles.Add(bubbleData);
					}
					if (ManageLevelData.Levels.ContainsKey(currentLevelNumber))
					{
						ManageLevelData.Levels.Remove(currentLevelNumber);
					}
					ManageLevelData.Levels.Add(currentLevelNumber, level);
					ManageLevelData.SaveToFile();
				}
			);
			uiController.SetOnChangeLevelListener(OnChangeLevel);
		}
		ManageLevelData.LoadFromFile();
	}


	void Start()
    {
		_uiCamera = FindObjectOfType<Camera>();


		//var instance = GameObject.Instantiate(PrefabLevelData);
		//if(instance != null)
		//{
		//	levelData = instance.GetComponent<LevelData>();
		//}

    }

	// Bubble 을 배치한다
	private void CreateAndPlaceBubble(Vector2 _cell, EBubbleColor _bubbleColor = EBubbleColor.Blue)
	{
		Vector2 bubbleCenter = Stage.ConvertCellToPosition(_cell, cellRadius);

		var bubbleInstance = GameObject.Instantiate(Prefab);//, bubbleCenter, Quaternion.identity);

		bubbleInstance.transform.parent = Root.transform;
		bubbleInstance.transform.localPosition = bubbleCenter;

		var bubble = bubbleInstance.GetComponent<Bubble>();

		bubble.BubbleColor = _bubbleColor;
		bubble.PlaceInCell(_cell);

		BubbleContainer.Add(bubble);

	}

	// Update is called once per frame
	void Update()
    {
		//Debug.Log(string.Format("current mouse point : {0}", Input.mousePosition));
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

		bool pushed = false;
		if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
		{
			pushed = true;
		}
		bool selected = false;
		int maxCX = 8;
		int maxCY = 10;
		float cellRadius = 0.1f;
		float cellRadiusSqrMagnitude = cellRadius * cellRadius;
		for (int cx = 0 ; cx < maxCX ; ++cx)
		{
			for(int cy = 0 ; cy < maxCY ; ++cy)
			{
				// 겹자를 그리고
				Vector2 center = Root.transform.TransformPoint(Stage.ConvertCellToPosition(new Vector2(cx, cy), cellRadius)); ;

				bool isHit = false;
				if(!selected)
				{
					if (hit.collider != null)
					{
						// 정확히 정육각형에 포함돼는지 판단이 어려우므로 원으로 가정해서 처리한다
						if (Vector2.Distance(hit.point, center) < cellRadius)
						{
							//Debug.Log(string.Format("Cell {0}", new Vector2(cx, cy)));
							isHit = true;
							selected = true;

							_UpdatePreviewBubble(new Vector2(cx, cy));

							if (pushed)
							{
								_OnSelectedCell(new Vector2(cx, cy));
								Debug.Log(string.Format("Cell {0}", new Vector2(cx, cy)));
							}
						}
					}
				}
				//Bubble.DrawHexgon(center, cellRadius, isHit ? Color.red : Color.green);
				Bubble.DrawHexgon(center, cellRadius, Color.green);
			}
		}





		_processInput();
	}

	private void _UpdatePreviewBubble(Vector2 _cell)
	{
		//if(!PreviewBubble.active )
		//{
		//	PreviewBubble.SetActive(true);
		//}
		PreviewBubble.transform.localPosition = Stage.ConvertCellToPosition(_cell, cellRadius);
		{
			Utility.SetSpriteAlpha(PreviewBubble, 0.6f);
			Utility.SetBubbleColor(PreviewBubble, CurrentBubbleColor);
		}
	}

	private void _processInput()
	{
		if(Input.GetKeyDown(KeyCode.S))
		{
			//int levelNumber = 1;
			// 일단 level 은 1로 한정
			// Save 
			//var level = new Level();

			//foreach(var bubble in BubbleContainer)
			//{
			//	var bubbleData = new BubbleData();

			//	bubbleData.Cell.Set(bubble.Cell);
			//	bubbleData.Color = bubble.BubbleColor;

			//	level.Bubbles.Add(bubbleData);
			//}
			//if( ManageLevelData.Levels.ContainsKey(currentLevelNumber))
			//{
			//	ManageLevelData.Levels.Remove(currentLevelNumber);
			//}
			//ManageLevelData.Levels.Add(currentLevelNumber, level);
			//ManageLevelData.SaveToFile();
		}
		// next level show
		if( Input.GetKeyDown((KeyCode.N)))
		{
			currentLevelNumber += 1;
		}
		if( Input.GetKeyDown(KeyCode.L))
		{

		}
	}

	private void _ClearAllBubbles()
	{
		foreach (var bubble in BubbleContainer)
		{
			GameObject.Destroy(bubble.gameObject);
		}
		BubbleContainer.Clear();
	}

	private void OnLoad(int _levelNumber)
	{
		if( ManageLevelData.LoadFromFile())
		{
			_ClearAllBubbles();

			if ( ManageLevelData.Levels.ContainsKey(_levelNumber))
			{
				var level = ManageLevelData.Levels[_levelNumber];

				// 컨테이너에 든 방울 모두 제거
				if(level != null)
				{
					foreach (var bubble in level.Bubbles)
					{
						CreateAndPlaceBubble(
							new Vector2(bubble.Cell.x, bubble.Cell.y),
							bubble.Color);
					}
				}
			}
		}
	}

	private void _OnSelectedCell( Vector2 _cell)
	{
		var placedBubble = BubbleContainer.Find(_ => _.PlacedCell == _cell);

		//  bubble 추가
		if (CurrentEditMode == EBubbleEditMode.Remove)
		{
			// 존재한다면 제거한다
			if(placedBubble)
			{
				BubbleContainer.Remove(placedBubble);

				GameObject.Destroy(placedBubble.gameObject);
			}
		}
		else if( CurrentEditMode == EBubbleEditMode.Add)
		{
			// 기존에 이미 배치됐다면
			//var placedBubble = BubbleContainer.Find(_ => _.Cell == _cell);
			if(placedBubble != null)
			{
				// 색만 바꾼다

				placedBubble.BubbleColor = CurrentBubbleColor;
			}
			else
			{
				// 새로 만들어 배치한다
				CreateAndPlaceBubble(_cell, CurrentBubbleColor);
			}
			
		}

	}


	private void OnMouseOver()
	{
		//Debug.Log("dsds");
		
	}
}
