using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ZPuzzleBubble;

// [출처] https://dobby-the-house-elf.tistory.com/62

public class OnChangeLevelEvent : UnityEvent<int>
{}

public class TestEdit : MonoBehaviour
//	, IPointerDownHandler
{
	private enum EBubbleEditMode
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

	[SerializeField]
	private int currentLevelNumber = 1;

	#region Cell
	[SerializeField]
	private int maxCellX = 8;
	[SerializeField]
	private int maxCellY = 10;
	[SerializeField]
	private float cellRadius = 0.1f;
	#endregion

	#region HexagonLineMaterial
	[SerializeField] 
	private Shader lineShader;
	

	#endregion
	
	private EBubbleEditMode CurrentEditMode = EBubbleEditMode.Add;
	private EBubbleColor CurrentBubbleColor = EBubbleColor.None;
	private List<Bubble> BubbleContainer = new List<Bubble>();

	public OnChangeLevelEvent OnChangeLevel = new OnChangeLevelEvent();

	private void Awake()
	{
		LevelDataManager.Instance.LoadLevelData(
			(_success) => Debug.Log("Load Completed!"));
		if(PreviewBubble)
        {
			PreviewBubble.SetActive(false);
		}

		if (uiController != null)
		{

			uiController.SetCurrentLevelNumber(currentLevelNumber);
			uiController.OnChangeBubbleColor.AddListener((_bubbleColor) =>
				{
					CurrentEditMode = _bubbleColor != EBubbleColor.None ? EBubbleEditMode.Add : EBubbleEditMode.Remove;
					CurrentBubbleColor = _bubbleColor;

					PreviewBubble.SetActive(CurrentEditMode == EBubbleEditMode.Add);
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
					// if (ManageLevelData.LoadFromFile())
					// {
					// 	OnLoad(currentLevelNumber);
					// }
					LevelDataManager.Instance.LoadLevelData( 
						(_success)=> OnLoad(currentLevelNumber));
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
					LevelDataManager.Instance.SaveLevel(currentLevelNumber, level);
					// if (ManageLevelData.Levels.ContainsKey(currentLevelNumber))
					// {
					// 	ManageLevelData.Levels.Remove(currentLevelNumber);
					// }
					// ManageLevelData.Levels.Add(currentLevelNumber, level);
					// ManageLevelData.SaveToFile();
				}
			);

			uiController.OnClickGoToMain.AddListener(() =>
			{
				SceneManager.LoadSceneAsync("Main");
			});
			
			uiController.OnClickTestPlay.AddListener(() =>
			{
				//SceneManager.LoadSceneAsync("InGame");
				TestPlayManager.Instance.PlayTestMode(currentLevelNumber);
			});

			uiController.SetOnChangeLevelListener(OnChangeLevel);
		}
		//ManageLevelData.LoadFromFile();
	}
	void Start()
    {
		//_uiCamera = FindObjectOfType<Camera>();
		_CreateHexagonCells();
	}

	// Update is called once per frame
	void Update()
    {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
		if (hit.collider == null)
		{
			return;
		}
		var currentCell = _GetCellWithCursor(hit.point);
		if(currentCell != null)
        {
			var cell = currentCell.GetValueOrDefault();
			_UpdatePreviewBubble(cell);
			if(Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
            {
				_OnSelectedCell(cell);
			}
		}
	}
	private Vector2? _GetCellWithCursor(Vector2 _cursorPoistion)
    {
		for (int cx = 0; cx < maxCellX; ++cx)
		{
			for (int cy = 0; cy < maxCellY; ++cy)
			{
				var cell = new Vector2(cx, cy);
				Vector2 centerPos = _GetCellCenterPosition(cell);
				// * 정확히 정육각형에 포함돼는지 판단이 어려우므로 원으로 가정해서 처리한다
				if (Vector2.Distance(_cursorPoistion, centerPos) < cellRadius)
				{
					return cell;
				}
			}
		}
		return null;
    }

	private Vector2 _GetCellCenterPosition( Vector2 _cell)
		=> Root.transform.TransformPoint(
			Utility.ConvertCellToPosition(_cell, cellRadius));
	private void _CreateHexagonCells()
	{
		int maxCX = 8;
		int maxCY = 10;
		float cellRadius = 0.1f;

		var cellRoot = new GameObject("HexagonCell");
		cellRoot.transform.SetParent(this.transform);
		cellRoot.transform.position = Vector3.zero;

		List<Vector2> cellsPoints = new List<Vector2>();
		for (int cx = 0; cx < maxCX; ++cx)
		{
			for (int cy = 0; cy < maxCY; ++cy)
            {
                cellsPoints.Clear();
                Vector2 center = Root.transform.TransformPoint(Utility.ConvertCellToPosition(new Vector2(cx, cy), cellRadius)); ;
                Bubble.GetHexgonPoints(center, cellRadius, ref cellsPoints);

                var cellRenderObject = new GameObject($"{cx}:{cy}");
                cellRenderObject.transform.SetParent(cellRoot.transform);
                Utility.DrawLinesWithLineRenderer(cellsPoints, cellRenderObject, Color.green, lineShader, 0.003f, false);
            }
        }
	}
    private void _UpdatePreviewBubble(Vector2 _cell)
	{
		PreviewBubble.transform.localPosition = 
			Utility.ConvertCellToPosition(_cell, cellRadius);

		Utility.SetSpriteAlpha(PreviewBubble, 0.6f);
		Utility.SetBubbleColor(PreviewBubble, CurrentBubbleColor);
	}


	private void OnLoad(int _levelNumber)
	{
		//if( ManageLevelData.LoadFromFile())
		{
			_ClearAllBubbles();
			// if ( ManageLevelData.Levels.ContainsKey(_levelNumber))
			// {
			// 	var level = ManageLevelData.Levels[_levelNumber];
			//
			// 	// 컨테이너에 든 방울 모두 제거
			// 	if(level != null)
			// 	{
			// 		foreach (var bubble in level.Bubbles)
			// 		{
			// 			_CreateAndAttachBubble(
			// 				new Vector2(bubble.Cell.x, bubble.Cell.y),
			// 				bubble.Color);
			// 		}
			// 	}
			// }
			Level currentLevel;
			if ( LevelDataManager.Instance.TryGetLevel(_levelNumber, out currentLevel))
			{
				//var level = ManageLevelData.Levels[_levelNumber];

				// 컨테이너에 든 방울 모두 제거
				if(currentLevel != null)
				{
					foreach (var bubble in currentLevel.Bubbles)
					{
						_CreateAndAttachBubble(
							new Vector2(bubble.Cell.x, bubble.Cell.y),
							bubble.Color);
					}
				}
			}
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

	private void _OnSelectedCell( Vector2 _cell)
	{
		var placedBubble = BubbleContainer.Find(_ => _.PlacedCell == _cell);
		if (CurrentEditMode == EBubbleEditMode.Remove)
		{
			if(placedBubble)
			{
				BubbleContainer.Remove(placedBubble);
				GameObject.Destroy(placedBubble.gameObject);
			}
		}
		else if( CurrentEditMode == EBubbleEditMode.Add)
		{
			// 기존에 이미 배치됐다면
			if(placedBubble != null)
			{
				// 색만 바꾼다
				placedBubble.BubbleColor = CurrentBubbleColor;
			}
			else
			{
				// 새로 만들어 배치한다
				_CreateAndAttachBubble(_cell, CurrentBubbleColor);
			}
		}
	}

	// Bubble 을 배치한다
	private void _CreateAndAttachBubble(
		Vector2 _cell,
		EBubbleColor _bubbleColor = EBubbleColor.Blue)
	{
		Vector2 bubbleCenter = Utility.ConvertCellToPosition(_cell, cellRadius);

		var bubbleInstance = GameObject.Instantiate(Prefab);//, bubbleCenter, Quaternion.identity);

		bubbleInstance.transform.parent = Root.transform;
		bubbleInstance.transform.localPosition = bubbleCenter;

		var bubble = bubbleInstance.GetComponent<Bubble>();

		bubble.BubbleColor = _bubbleColor;
		bubble.PlaceInCell(_cell);

		BubbleContainer.Add(bubble);

	}
 //   #region IPointerDownHandler
 //   public void OnPointerDown(PointerEventData eventData)
 //   {
	//	Debug.Log($"OnPointerDown : {eventData.position}");// eventData.position
	//	//throw new NotImplementedException();
	//}
 //   #endregion 
}
