using UnityEditor;
using UnityEngine;

namespace CFaz.OffAxisCamera
{
	/// <summary>
	/// Off Axis Camera component, allowing the camera to follow
	/// a "projection plane" not necessarily aligned with the camera.
	/// Allowing for perspective deformation and off-axis near and far planes.
	/// This is useful to create portals or mirrors for example
	/// </summary>
	[ExecuteAlways]
	[RequireComponent(typeof(Camera))]
	public class OffAxisCamera : MonoBehaviour
	{

		#region Serialized Variables

		[SerializeField]
		[Tooltip("Size of the projection plane.")]
		private Vector2 planeSize = Vector2.one;

		[SerializeField]
		[Tooltip("Clamp near plane of the attached camera to the projection plane.")]
		private bool useProjectionAsNearPlane = true;

		[Space]
		[SerializeField]
		[Tooltip("Snap projection plane to a world transform instead of specifying plane distance and rotation.")]
		private Transform snapToTransform = null;

		[SerializeField]
		[Tooltip("Projection plane distance from the camera.")]
		private float planeDistance = 1;

		[SerializeField]
		[Tooltip("Projection plane local rotation relative to the camera.")]
		private Quaternion planeRotation = Quaternion.identity;

		#endregion

		#region Private variables

		// Camera component attached to this gameObject
		private Camera _camera;

		// Half of the plane size
		private Vector2 _halfSize;

		// Projection plane corners
		private Vector3 _botLeft;
		private Vector3 _botRight;
		private Vector3 _topLeft;
		private Vector3 _topRight;

		// Projection plane Right, Up, and Backward vectors
		private Vector3 _planeRight;
		private Vector3 _planeUp;
		private Vector3 _planeForward;

		#endregion

		#region Public Properties

		/// <summary>
		/// Camera attached to this component.
		/// </summary>
		public Camera Camera => _camera;

		/// <summary>
		/// Size of the projection plane.
		/// </summary>
		public Vector2 PlaneSize
		{
			get => planeSize;
			set
			{
				planeSize = value;
				_halfSize = planeSize * 0.5f;
			}
		}

		/// <summary>
		/// Clamp near plane of the attached camera to the projection plane.
		/// </summary>
		public bool UseProjectionAsNearPlane
		{
			get => useProjectionAsNearPlane;
			set => useProjectionAsNearPlane = value;
		}

		/// <summary>
		/// Snap projection plane to a world transform instead of specifying plane distance and rotation.
		/// </summary>
		public Transform SnapToTransform
		{
			get => snapToTransform;
			set => snapToTransform = value;
		}

		/// <summary>
		/// Projection plane distance from the camera. (overriden by <see cref="SnapToTransform"/>>)
		/// </summary>
		public float PlaneDistance
		{
			get => planeDistance;
			set => planeDistance = value;
		}

		/// <summary>
		/// Projection plane local rotation relative to the camera. (overriden by <see cref="SnapToTransform"/>>)
		/// </summary>
		public Quaternion PlaneRotation
		{
			get => planeRotation;
			set => planeRotation = value;
		}

		#endregion

		#region Unity Event Functions

		private void Awake()
		{
			_camera = GetComponent<Camera>();
			_halfSize = planeSize * 0.5f;
		}

		private void LateUpdate()
		{
			// #### Compute Plane Dimensions
			if (snapToTransform)
			{
				_botLeft = snapToTransform.TransformPoint(new Vector3(-_halfSize.x, -_halfSize.y));
				_botRight = snapToTransform.TransformPoint(new Vector3(_halfSize.x, -_halfSize.y));
				_topLeft = snapToTransform.TransformPoint(new Vector3(-_halfSize.x, _halfSize.y));
				_topRight = snapToTransform.TransformPoint(new Vector3(_halfSize.x, _halfSize.y));
			}
			else
			{
				Vector3 offset = Vector3.forward * planeDistance;
				_botLeft = transform.TransformPoint(offset + planeRotation * new Vector3(-_halfSize.x, -_halfSize.y));
				_botRight = transform.TransformPoint(offset + planeRotation * new Vector3(_halfSize.x, -_halfSize.y));
				_topLeft = transform.TransformPoint(offset + planeRotation * new Vector3(-_halfSize.x, _halfSize.y));
				_topRight = transform.TransformPoint(offset + planeRotation * new Vector3(_halfSize.x, _halfSize.y));
			}

			_planeRight = (_botRight - _botLeft).normalized;
			_planeUp = (_topLeft - _botLeft).normalized;
			_planeForward = Vector3.Cross(_planeRight, _planeUp);

			// Handle camera behind plane
			if (Vector3.Dot(_planeForward, _botLeft - transform.position) < 0)
			{
				// Invert forward and right
				_planeRight = -_planeRight;
				_planeForward = -_planeForward;
				// Swap corners
				(_botLeft, _botRight, _topLeft, _topRight) = (_botRight, _botLeft, _topRight, _topLeft);
			}

			// #### Calculate Matrices
			Vector3 position = transform.position;

			// We only need 3 points to identify a plane (quad)
			Vector3 localBotLeft = _botLeft - position;
			Vector3 localBotRight = _botRight - position;
			Vector3 localTopLeft = _topLeft - position;

			// Projection plane distance from the camera
			float d = Vector3.Dot(localBotLeft, _planeForward);

			if (useProjectionAsNearPlane)
				_camera.nearClipPlane = d;

			// Setup projection matrix
			float near = _camera.nearClipPlane;
			float far = _camera.farClipPlane;

			float nearOverDist = near / d;
			float left = Vector3.Dot(_planeRight, localBotLeft) * nearOverDist;
			float right = Vector3.Dot(_planeRight, localBotRight) * nearOverDist;
			float bottom = Vector3.Dot(_planeUp, localBotLeft) * nearOverDist;
			float top = Vector3.Dot(_planeUp, localTopLeft) * nearOverDist;

			Matrix4x4 projection = Matrix4x4.Frustum(left, right, bottom, top, near, far);

			// Setup plane world to camera matrix
			Matrix4x4 worldToCamera = Matrix4x4.identity;
			worldToCamera.SetRow(0, _planeRight);
			worldToCamera.SetRow(1, _planeUp);
			worldToCamera.SetRow(2, -_planeForward);
			worldToCamera *= Matrix4x4.Translate(-position);

			// Set Camera matrices
			_camera.worldToCameraMatrix = worldToCamera;
			_camera.projectionMatrix = projection;
		}

		private void OnValidate()
		{
			// Clamp plane size
			planeSize.x = Mathf.Max(0.01f, planeSize.x);
			planeSize.y = Mathf.Max(0.01f, planeSize.y);
			_halfSize = planeSize * 0.5f;

			// Clamp distance
			planeDistance = Mathf.Max(0.01f, planeDistance);
		}

		private void OnDrawGizmos()
		{
			if (!Selection.Contains(gameObject) && (snapToTransform == null || !Selection.Contains(snapToTransform.gameObject)))
				return;

			// Draw projection plane
			Gizmos.color = Color.white;
			Gizmos.DrawLine(_topLeft, _topRight);
			Gizmos.DrawLine(_topLeft, _botLeft);
			Gizmos.DrawLine(_botRight, _topRight);
			Gizmos.DrawLine(_botRight, _botLeft);
		}

		#endregion

	}
}
