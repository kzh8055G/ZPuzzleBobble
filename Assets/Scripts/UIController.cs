using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

#region Events
public class OnChangeBubbleColorEvent : UnityEvent<EBubbleColor>
{}

#endregion

[Serializable]
public class BubbleColorData
{
	public EBubbleColor Color;
	public Button ButtonColor;

}

public class UIController : MonoBehaviour
{
	[SerializeField]
	private List<BubbleColorData> bubbleColorDataList = new List<BubbleColorData>();

	#region Button

	

	
	[SerializeField]
	private Button buttonPrevLevel;

	[SerializeField]
	private Button buttonNextLevel;

	[SerializeField]
	private Text textCurrentLevel;

	[SerializeField]
	private Button buttonLoad;

	[SerializeField]
	private Button buttonSave;

	[SerializeField] 
	private Button buttonGoToMain;

	[SerializeField] 
	private Button buttonTestPlay;
	
	#endregion
	
	[SerializeField]
	private float ButtonOutLineWidth = 0.003f;

	[SerializeField] 
	private Shader lineShader;
	
	public OnChangeBubbleColorEvent OnChangeBubbleColor = new OnChangeBubbleColorEvent();

	public UnityEvent OnClickLoad = new UnityEvent();
	public UnityEvent OnClickSave = new UnityEvent();

	public UnityEvent OnClickNextLevel = new UnityEvent();
	public UnityEvent OnClickPrevLevel = new UnityEvent();
	
	//[FormerlySerializedAs("onClickGoToMain")] 
	public UnityEvent OnClickGoToMain = new UnityEvent();
	public UnityEvent OnClickTestPlay = new UnityEvent();
	
	public void SetOnChangeLevelListener(OnChangeLevelEvent _event)
	{
		_event.AddListener((_currentLevel) =>
			{
				SetCurrentLevelNumber(_currentLevel);
			}
		);
	}

	public void SetCurrentLevelNumber(int _levelNumber)
	{
		textCurrentLevel.text = _levelNumber.ToString();
	}

	private void Awake()
	{
		// TODO : 색 변경 버튼이 눌리면 noti 를 날려야한다

		foreach(var data in bubbleColorDataList)
		{
			data.ButtonColor.onClick.AddListener(() =>
				{
					_DrawButtonOutline(data.ButtonColor);
					OnChangeBubbleColor.Invoke(data.Color);
				}
			);
		}

		buttonNextLevel.onClick.AddListener(() => OnClickNextLevel.Invoke());
		buttonPrevLevel.onClick.AddListener(() => OnClickPrevLevel.Invoke());

		buttonLoad.onClick.AddListener(() => OnClickLoad.Invoke());
		buttonSave.onClick.AddListener(() => OnClickSave.Invoke());
		
		buttonGoToMain.onClick.AddListener( ()=> OnClickGoToMain?.Invoke());
		buttonTestPlay.onClick.AddListener(()=> OnClickTestPlay?.Invoke());
	}

	private void _DrawButtonOutline( Button _button)
    {
		var rectT = _button.transform as RectTransform;
		if (rectT)
		{
			Vector2 centerPos = rectT.position;
			float halfHeight = rectT.rect.size.x * rectT.lossyScale.x / 2;
			float halfWidth = rectT.rect.size.y * rectT.lossyScale.y / 2;

			var LT = Camera.main.ScreenToWorldPoint(new Vector2(centerPos.x - halfWidth, centerPos.y + halfHeight));
			var RB = Camera.main.ScreenToWorldPoint(new Vector2(centerPos.x + halfWidth, centerPos.y - halfHeight));

			List<Vector2> points = new List<Vector2>();
			// left, top
			points.Add(LT);
			// right, top
			points.Add(new Vector2(RB.x, LT.y));
			// right, bottom
			points.Add(RB);
			// left, bottom
			points.Add(new Vector2(LT.x, RB.y));

			float lineWidth = ButtonOutLineWidth * Mathf.Max(rectT.lossyScale.x, 1f); 
			Utility.DrawLinesWithLineRenderer(points, gameObject, Color.green, lineShader, lineWidth);
		}
	}
}
