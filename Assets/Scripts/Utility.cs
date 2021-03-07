using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	// https://answers.unity.com/questions/8338/how-to-draw-a-line-using-script.html
	// LineRender 를 이용한 선그리기
	// 문제는 라인하나 그릴때마다 GameObject 를 생성해버린다
	public static void DrawLine(
		Vector3 start, 
		Vector3 end, 
		Color color, 
		float duration = 0.2f)
	{
		GameObject myLine = new GameObject();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find(@"Legacy Shaders/Particles/Alpha Blended Premultiply"));
		lr.SetColors(color, color);
		lr.SetWidth(0.01f, 0.01f);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		GameObject.Destroy(myLine, duration);
	}
	public static void DrawLinesWithLineRenderer(
		List<Vector2> _points, 
		GameObject _renderObject,
		Color _color,
		float _lineWidth = 0.003f,
		bool _polygonPoints = true)
	{
		var renderer = _renderObject.GetComponent<LineRenderer>();
		if(!renderer)
        {
			renderer = _renderObject.AddComponent<LineRenderer>();
		}
		_renderObject.transform.position = Vector3.zero;
		renderer.material = new Material(Shader.Find(@"Legacy Shaders/Particles/Alpha Blended Premultiply"));
		
		renderer.startColor = Color.green;
		renderer.endColor = Color.green;
		
		renderer.startWidth = _lineWidth;
		renderer.endWidth = _lineWidth;

		if(_polygonPoints)
        {
			var tempPoints = new List<Vector2>();
			for (int i = 0; i < _points.Count - 1; ++i)// in _points)
            {
				tempPoints.Add(_points[i]);
				tempPoints.Add(_points[i + 1]);
			}
			// 마지막 라인
			tempPoints.Add(_points[_points.Count - 1]);
			tempPoints.Add(_points[0]);
			_points.Clear();
			tempPoints.ForEach( _ => _points.Add(_));
		}
		renderer.positionCount = _points.Count;
		renderer.SetPositions(_points.Select(_ => new Vector3(_.x, _.y, 0)).ToArray());
	}
}
