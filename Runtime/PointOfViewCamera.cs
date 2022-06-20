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
	public class PointOfViewCamera : MonoBehaviour
	{
		#region Serialized Variables

		[SerializeField]
		[Tooltip("Size of the projection plane.")]
		private Vector2 planeSize = Vector2.one;

		[SerializeField]
		[Tooltip("Point Of View of the camera in local coordinates.")]
		private Vector3 cameraPovLocal = Vector3.back;

		[SerializeField]
		[Tooltip("Clamp near plane of the attached camera to the projection plane.")]
		private bool clampNearPlane = true;

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
		/// Point of view plane rect in local coordinates, centered on the camera position.
		/// Setting this will add the new rect center value to the camera position.
		/// </summary>
		public Rect PlaneRect
		{
			// set center to 0 to have all min and max positions relative to the center
			get => new(){ size = PlaneSize, center = Vector2.zero };
			set
			{
				PlaneSize = value.size;
				Transform tr = transform;
				tr.position += tr.right * value.center.x + tr.up * value.center.y;
			}
		}

		/// <summary>
		/// Clamp near plane of the attached camera to the projection plane.
		/// </summary>
		public bool UseProjectionAsNearPlane
		{
			get => clampNearPlane;
			set => clampNearPlane = value;
		}

		/// <summary>
		/// Point Of View of the camera in world coordinates.
		/// </summary>
		public Vector3 PointOfView
		{
			get => transform.TransformPoint(cameraPovLocal);
			set => cameraPovLocal = transform.InverseTransformPoint(value);
		}

		/// <summary>
		/// Point Of View of the camera in local coordinates.
		/// </summary>
		public Vector3 PointOfViewLocal
		{
			get => cameraPovLocal;
			set => cameraPovLocal = value;
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
			Transform tr = transform;

			Vector3 povWorld = PointOfView;
			Vector3 forward = tr.forward;

			// invert coordinates if the camera is behind the projection plane
			float invert = -Mathf.Sign(Vector3.Dot(forward, povWorld - tr.position));

			// We only need 3 points to identify the projection plane
			_botLeft  = tr.TransformPoint(new Vector3(invert * -_halfSize.x, -_halfSize.y));
			_botRight = tr.TransformPoint(new Vector3(invert * _halfSize.x, -_halfSize.y));
			_topLeft  = tr.TransformPoint(new Vector3(invert * -_halfSize.x, _halfSize.y));

			_planeRight   = invert * tr.right;
			_planeUp      = invert * tr.up;
			_planeForward = invert * forward;

			// #### Calculate Matrices
			Vector3 localBotLeft  = _botLeft - povWorld;
			Vector3 localBotRight = _botRight - povWorld;
			Vector3 localTopLeft  = _topLeft - povWorld;

			// Projection plane distance from the camera
			float cameraDistance = -PointOfViewLocal.z * invert;

			// Clamp near camera plane to projection plane
			if (clampNearPlane)
				_camera.nearClipPlane = cameraDistance;

			// Setup projection matrix
			float near = _camera.nearClipPlane;
			float far  = _camera.farClipPlane;

			float nearOverDist = near / cameraDistance;
			float left   = Vector3.Dot(_planeRight, localBotLeft) * nearOverDist;
			float right  = Vector3.Dot(_planeRight, localBotRight) * nearOverDist;
			float bottom = Vector3.Dot(_planeUp, localBotLeft) * nearOverDist;
			float top    = Vector3.Dot(_planeUp, localTopLeft) * nearOverDist;

			Matrix4x4 projection = Matrix4x4.Frustum(left, right, bottom, top, near, far);

			// Setup plane world to camera matrix
			Matrix4x4 worldToCamera = Matrix4x4.identity;
			worldToCamera.SetRow(0, _planeRight);
			worldToCamera.SetRow(1, _planeUp);
			worldToCamera.SetRow(2, -_planeForward);
			worldToCamera *= Matrix4x4.Translate(-povWorld);

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
		}

		#endregion
	}
}
