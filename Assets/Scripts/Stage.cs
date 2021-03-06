﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using ZPuzzleBubble;
using Random = UnityEngine.Random;

public class OnLevelStartEvent : UnityEvent<int> {};
public class OnLevelClearEvent : UnityEvent<int> {};
public class OnScoreGetEvent : UnityEvent<int> {};


public struct CaculateScoreOnBubbleHit
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

public class Stage : MonoBehaviour
{
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
	private Level currentLevel = null;
	#endregion

	private CompositeDisposable disposables = new CompositeDisposable();
	
	private ReactiveProperty<int> currentShootCount = new ReactiveProperty<int>(0);
	
	// TODO : 이것도 레벨에서 값을 저장하는게 어떠한가?
	private int vibrateStartShootCount = 1;

	public OnLevelStartEvent OnLevelStart = new OnLevelStartEvent();
	public OnLevelClearEvent OnLevelClear = new OnLevelClearEvent();
	public OnScoreGetEvent OnScoreGet = new OnScoreGetEvent();

	public UnityEvent OnBubbleShootReady = new UnityEvent();

	// TODO : bubbleContainer 로 옮겨야 한다
	public IReadOnlyList<LayerMask> SideLayerMasks => bubbleContainer.SideLayerMasks;

	private List<EBubbleColor> shootBubblePreset = new List<EBubbleColor>();

	private BubbleContainer bubbleContainer;

	private CaculateScoreOnBubbleHit scoreCalculator = new CaculateScoreOnBubbleHit();

	void Awake()
	{
		AudioSourceComponent = GetComponent<AudioSource>();

		bubbleContainer = GetComponent<BubbleContainer>();

		bubbleContainer.onBubbleBoom.AddListener(
			(_color, _count) => scoreCalculator.OnBoomBubbles(_count));

		bubbleContainer.onBubbleFall.AddListener( 
			(_count)=> scoreCalculator.OnFallBubbles(_count));


		bubbleContainer.OnDownTopCompleted
			.Subscribe(_ => currentShootCount.Value = 0)
			.AddTo(disposables);

		currentShootCount
			.Subscribe(_ => _OnChangeShootCount())
			.AddTo(disposables);
	}
	
	void Start()
	{
		LevelDataManager.Instance.LoadLevelData((_success) =>
		{
			if (_success)
			{
				_OnLevelLoaded();
			}
			// TODO : 로드 실패시에 대한 대응
		});
	}

	public void PlayTestMode(int _level)
	{
		currentLevelNumber = _level;
	}
	public EBubbleColor GetNextShootBubbleColor()
	{
		return shootBubblePreset[Random.Range(0, shootBubblePreset.Count)];
	}

	public LayerMask GetAllSideLayerMask()
	{
		return bubbleContainer.GetAllSideLayerMask();
	}

	private void _UpdateShootBubblePreset()
	{
		bubbleContainer.UpdateShootBubblePreset(ref shootBubblePreset);
	}

	private void _OnLevelLoaded()
	{
		if (LevelDataManager.Instance.TryGetLevel(currentLevelNumber, out currentLevel))
		{
			bubbleContainer.StartLevel(currentLevel);
		}
		_UpdateShootBubblePreset();

		if (AudioSourceComponent != null)
		{
			AudioSourceComponent.clip = SinglePlayBGM;
			AudioSourceComponent.loop = true;
			AudioSourceComponent.Play();
		}

		currentShootCount.Value = 0;
		
		OnLevelStart.Invoke(currentLevelNumber);

	}

	private void _GoToNextLevel()
	{
		currentLevelNumber += 1;

		bubbleContainer.StartLevel(currentLevel);
	}

	private void _OnHitBubble(GameObject _hitGameObject, Bubble _hitBubble)
	{
		scoreCalculator.Clear();

		if( bubbleContainer.OnHitBubble(_hitGameObject, _hitBubble) )
		{
			OnScoreGet.Invoke(scoreCalculator.Calculate());

			if(_CheckLevelClear())
			{
				return;
			}
			if(_CheckGameOver())
			{
				_OnGameOver();
				return;
			}
			++currentShootCount.Value;
			_UpdateShootBubblePreset();
			OnBubbleShootReady.Invoke();
		}
	}

	public void OnShootBubbleObject(GameObject _bubbleInstance)
	{
		var bubble = _bubbleInstance.GetComponent<Bubble>();
		if (bubble != null)
		{
			bubble.OnBubbleCollision.AddListener(_OnHitBubble);
		}
	}

	private void _OnChangeShootCount()
	{
		if (currentShootCount.Value >= vibrateStartShootCount)
		{
			int vibrationStep = (currentShootCount.Value - vibrateStartShootCount);
			bool downTop = (vibrationStep > 2);

			if(downTop)
			{
				// reset
				currentShootCount.Value = 0;
				bubbleContainer.DownTop();
			}
			else
			{

				bubbleContainer.StartVibrate(vibrationStep);
			}
		}
	}

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

	private bool _CheckGameOver()
	{
		return bubbleContainer.CheckGameOver();
	}

	private bool _CheckLevelClear()
	{
		bool clear = bubbleContainer.IsEmpty;
		if (clear)
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

		bubbleContainer.StopVibrate();

		OnLevelClear.Invoke(currentLevelNumber);

		yield return new WaitForSeconds(3);

		// go to next level
		++currentLevelNumber;
		_OnLevelLoaded();
	}
	// TODO : 
	private Vector2 ConvertPositionToCell(Vector2 _Postion)
	{
		return Vector2.zero;
	}
	public void TestIntersectRay(Ray _ray)
	{
		bubbleContainer.TestIntersectRay(_ray);
	}

	public bool FindFirstRayIntersectBubble(Ray _ray, ref Bubble _bubble, ref float _distance)
	{
		return bubbleContainer.FindFirstRayIntersectBubble(_ray, ref _bubble,ref _distance);
	}

	// Update is called once per frame
	void Update()
	{
		// debug
		//foreach( var bubble in BubbleContainer)
		//{
		//	bubble.DrawHexagon(CellRadius);
		//}

		if (Input.GetKeyDown(KeyCode.Q))
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
}
