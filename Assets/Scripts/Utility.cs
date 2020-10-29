using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
	public static void SetSpriteAlpha(GameObject _object, float _alpha)
	{
		var renderer = _object.GetComponent<SpriteRenderer>();
		if (renderer)
		{
			var color = renderer.material.color;
			color.a = _alpha;
			renderer.material.color = color;
		}
	}

	public static void SetBubbleColor(GameObject _bubbleObject, Bubble.EBubbleColor _bubbleColor)
	{
		if (_bubbleObject)
		{
			var animator = _bubbleObject.GetComponent<Animator>();
			if (animator != null)
			{
				int colorIndex = (int)_bubbleColor;
				animator.SetInteger("Color", colorIndex);
			}
		}
	}

}
