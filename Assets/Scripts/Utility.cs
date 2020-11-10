using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{

	public static void SetSpriteAlpha(GameObject _object, float _alpha)
	{
		var renderer = _object.GetComponentInChildren<SpriteRenderer>();
		if (renderer)
		{
			var color = renderer.material.color;
			color.a = _alpha;
			renderer.material.color = color;
		}
	}

	public static void SetBubbleColor(GameObject _bubbleObject, EBubbleColor _bubbleColor)
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

	#region Cell

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
	public static IReadOnlyList<Vector2> GetAdjecentCellOffsets(Vector2 _cell)
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

	public static float GetAngleBetweenVector(Vector3 from, Vector3 to)
	{
		return Mathf.Acos(Mathf.Clamp(Vector3.Dot(from.normalized, to.normalized), -1f, 1f)) * 57.29578f;
	}


}
