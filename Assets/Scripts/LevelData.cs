using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableVector2
{
	public float x;
	public float y;

	public void Set(Vector2 _)
	{
		x = _.x;
		y = _.y;
	}
}

[Serializable]
public class BubbleData
{
	public SerializableVector2 Cell = new SerializableVector2();
	public EBubbleColor Color;
}

[Serializable]
public class Level
{
	public int LevelNumber;

	public List<BubbleData> Bubbles = new List<BubbleData>();

	public float LimitTime;

}
