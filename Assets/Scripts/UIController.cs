using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

	public OnChangeBubbleColorEvent OnChangeBubbleColor = new OnChangeBubbleColorEvent();

	public UnityEvent OnClickLoad = new UnityEvent();
	public UnityEvent OnClickSave = new UnityEvent();

	public UnityEvent OnClickNextLevel = new UnityEvent();
	public UnityEvent OnClickPrevLevel = new UnityEvent();

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

		foreach(var bubbleColor in bubbleColorDataList)
		{
			bubbleColor.ButtonColor.onClick.AddListener(() =>
				{
					OnChangeBubbleColor.Invoke(bubbleColor.Color);
				}
			);
		}

		buttonNextLevel.onClick.AddListener(() => OnClickNextLevel.Invoke());
		buttonPrevLevel.onClick.AddListener(() => OnClickPrevLevel.Invoke());

		buttonLoad.onClick.AddListener(() => OnClickLoad.Invoke());
		buttonSave.onClick.AddListener(() => OnClickSave.Invoke());

	}

	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
