using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInGame : MonoBehaviour
{
	[SerializeField]
	private Stage stage;
	[SerializeField]
	private TextMeshProUGUI textRoundNumber;
	[SerializeField]
	private TextMeshProUGUI textScore;
	[SerializeField]
	private TextMeshProUGUI textElapsedTime;
	#region Timer
	private bool enableTimer = false;
	private float currentElapsedTime = 0f;

	private int currentScore = 0;

	#endregion

	private void Awake()
	{
		stage.OnLevelStart.AddListener((_levelNumber) =>
			{
				// round
				textRoundNumber.text = string.Format("Round {0}", _levelNumber);
				// timer
				enableTimer = true;
				textElapsedTime.text = string.Empty;
				currentElapsedTime = 0;
				// score
			}
		);
		stage.OnLevelClear.AddListener((_levelNumber) =>
			{
				if (enableTimer) enableTimer = false;
			}
		);
		stage.OnScoreGet.AddListener((_score) =>
			{
				currentScore += _score;
				_UpdateScore();


			}
		);
		currentScore = 0;
		_UpdateScore();
	}

	// Start is called before the first frame update
	void Start()
    {

	}

    // Update is called once per frame
    void Update()
    {
		//if( )

		if(enableTimer)
		{
			currentElapsedTime += Time.deltaTime;


			if (textElapsedTime != null)
			{
				int elapsedTimeSec = (int)currentElapsedTime;
				textElapsedTime.text = string.Format("{0} Sec", elapsedTimeSec);
			}
		}
    }
	private void _UpdateScore()
	{
		textScore.text = string.Format("{0}", currentScore);
	}
}
