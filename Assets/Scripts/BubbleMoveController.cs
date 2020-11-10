using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMoveController : MonoBehaviour
{
	//[SerializeField]
	private float velocity = 0.0f;//5.0f;

	private float ShootAngle = 0;
	private Vector3 MovingDirection;

	private CircleCollider2D Collider;

	private bool IsMoving = false;

	#region Raycast

	[SerializeField]
	private LayerMask reflectLayerMask;

	private Vector2 raycastDirection;
	private float raycastDistance;
	private RaycastHit2D[] hitBuffer = new RaycastHit2D[3];
	private ContactFilter2D contactFilter;
	private Vector2[] raycastPositions = new Vector2[3];

	#endregion

	private void Awake()
	{
		Collider = GetComponent<CircleCollider2D>();
		 
		//contactFilter.layerMask = reflectLayerMask;
		contactFilter.useLayerMask = true;
		contactFilter.useTriggers = false;
	}

	// Start is called before the first frame update
	void Start()
	{}
	// Update is called once per frame
	void Update()
	{
		if (IsMoving)
		{
			transform.position += MovingDirection * Time.deltaTime * velocity;
			_CheckReflection();
		}
	}

	public void StartMoving(float _angle
		, float _velocity
		, LayerMask _contactFilterLayerMask)
	{
		ShootAngle = _angle;
		MovingDirection = Quaternion.AngleAxis(ShootAngle, Vector3.forward) * Vector3.right;
		MovingDirection.Normalize();

		Collider.isTrigger = true;

		IsMoving = true;

		contactFilter.layerMask = _contactFilterLayerMask;

		velocity = _velocity;
	}

	public void StopMoving()
	{
		IsMoving = false;
	}

	private void _CheckReflection()
	{
		raycastDirection = new Vector2(MovingDirection.x, MovingDirection.y);
		raycastDistance = Collider.radius * 1.2f;

		int count = Physics2D.Raycast(transform.position, raycastDirection, contactFilter, hitBuffer, raycastDistance);

		for (int i = 0; i < hitBuffer.Length; i++)
		{
			if( hitBuffer[i].collider != null )
			{
				MovingDirection = Vector3.Reflect(MovingDirection, hitBuffer[i].normal); //반사각
				break;	
			}
		}
		for (int i = 0; i < hitBuffer.Length; i++)
		{
			hitBuffer[i] = new RaycastHit2D();
		}
	}
}
