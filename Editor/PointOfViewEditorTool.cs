using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace CFaz.OffAxisCamera.Editor
{
	[EditorTool("Point of view tool", typeof(PointOfViewCamera))]
	public class PointOfViewEditorTool : EditorTool
	{
		private PointOfViewCamera CameraTarget => (PointOfViewCamera)target;

		private readonly int[] _handleIds =
		{
			"CameraPOVHandleTop".GetHashCode(),
			"CameraPOVHandleBottom".GetHashCode(),
			"CameraPOVHandleLeft".GetHashCode(),
			"CameraPOVHandleRight".GetHashCode()
		};

		public override GUIContent toolbarIcon => new GUIContent()
		{
			image = EditorGUIUtility.IconContent("Profiler.Video").image,
			text = "Point of view tool",
			tooltip = "Edit the point of view of the Camera"
		};

		public override void OnToolGUI(EditorWindow window)
		{
			if (CameraTarget == null)
				return;

			Transform transform = CameraTarget.transform;
			Rect rect = CameraTarget.PlaneRect;

			// Camera POV position handle
			EditorGUI.BeginChangeCheck();

			Quaternion rot = Tools.pivotRotation == PivotRotation.Local ? transform.rotation : Quaternion.identity;
			Vector3 newPosition = Handles.PositionHandle(CameraTarget.PointOfView, rot);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(CameraTarget, "Change Point Of View Position");
				CameraTarget.PointOfView = newPosition;
				EditorUtility.SetDirty(CameraTarget);
			}

			// Camera plane dimensions handles
			EditorGUI.BeginChangeCheck();
			Handles.color = Color.white;

			// New handle Position (local)
			Vector3 up = transform.up;
			Vector3 right = transform.right;
			float upOffset    = SliderHandleLocalOffset(_handleIds[0], transform, new Vector2(0, rect.yMax), up).y;
			float downOffset  = SliderHandleLocalOffset(_handleIds[1], transform, new Vector2(0, rect.yMin), up).y;
			float rightOffset = SliderHandleLocalOffset(_handleIds[2], transform, new Vector2(rect.xMax, 0), right).x;
			float leftOffset  = SliderHandleLocalOffset(_handleIds[3], transform, new Vector2(rect.xMin, 0), right).x;

			// If dimensions changed
			if (EditorGUI.EndChangeCheck())
			{
				if (Event.current.alt || Event.current.shift)
				{
					float vertOffset = upOffset - downOffset;
					float horOffset = rightOffset - leftOffset;

					// If alt is pressed, resize all handles relative to the aspect ratio
					if (Event.current.alt)
					{
						float ratio = rect.width / rect.height;

						upOffset = vertOffset + horOffset / ratio;
						downOffset = -vertOffset - horOffset / ratio;
						rightOffset = horOffset + vertOffset * ratio;
						leftOffset = -horOffset - vertOffset * ratio;
					}
					// If shift is pressed, resize opposite handle as well
					else
					{
						upOffset = vertOffset;
						downOffset = -vertOffset;
						rightOffset = horOffset;
						leftOffset = -horOffset;
					}
				}

				// Set new plane rect
				rect.yMax += upOffset;
				rect.yMin += downOffset;
				rect.xMax += rightOffset;
				rect.xMin += leftOffset;

				Undo.RecordObjects(new Object[] { CameraTarget, transform }, "Changed POV Camera Plane Dimensions");
				CameraTarget.PlaneRect = rect;
				EditorUtility.SetDirty(CameraTarget);
			}
		}

		private static Vector3 SliderHandleLocalOffset(int controlId, Transform transform, Vector3 localPosition, Vector3 direction)
		{
			Vector3 worldPos = transform.TransformPoint(localPosition);
			float size = HandleUtility.GetHandleSize(worldPos) * 0.03f;
			Vector3 slider = Handles.Slider(controlId, worldPos, direction, size, Handles.DotHandleCap, Event.current.control ? 1f : 0f);
			return transform.InverseTransformPoint(slider) - localPosition;
		}
	}
}
