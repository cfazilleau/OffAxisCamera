using UnityEditor;
using UnityEngine;

namespace CFaz.OffAxisCamera.Editor
{
	[CustomEditor(typeof(OffAxisCamera), true)]
	public class OffAxisCameraEditor : UnityEditor.Editor
	{
		private OffAxisCamera CameraTarget => (OffAxisCamera)target;

		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();

			// Get property iterator and enter it
			SerializedProperty iterator = serializedObject.GetIterator();
			iterator.NextVisible(true);

			// Draw Toolbar
			EditorGUILayout.EditorToolbarForTarget(EditorGUIUtility.TrTempContent("Edit Camera POV"), this);
			GUILayout.Space(5);

			// Draw properties
			do
			{
				switch (iterator.propertyPath)
				{
					// ignore script property
					case "m_Script":
						continue;
				}
				EditorGUILayout.PropertyField(iterator, true);
			}
			while (iterator.NextVisible(false));

			serializedObject.ApplyModifiedProperties();
		}

		public virtual void OnSceneGUI()
		{
			Rect rect = CameraTarget.PlaneRect;

			Vector3 tr = CameraTarget.transform.TransformPoint(rect.max);
			Vector3 tl = CameraTarget.transform.TransformPoint(new Vector2(rect.xMin, rect.yMax));
			Vector3 br = CameraTarget.transform.TransformPoint(new Vector2(rect.xMax, rect.yMin));
			Vector3 bl = CameraTarget.transform.TransformPoint(rect.min);
			Vector3 povWorld = CameraTarget.transform.TransformPoint(CameraTarget.PointOfViewLocal);

			// Draw projection plane
			Handles.color = Color.white;
			Handles.DrawLine(tl, tr);
			Handles.DrawLine(tl, bl);
			Handles.DrawLine(br, tr);
			Handles.DrawLine(br, bl);

			// Draws dotted line from the POV to the plane corners
			Handles.color = Color.gray;
			Handles.DrawDottedLine(povWorld, tr, 1);
			Handles.DrawDottedLine(povWorld, tl, 1);
			Handles.DrawDottedLine(povWorld, br, 1);
			Handles.DrawDottedLine(povWorld, bl, 1);
		}
	}
}
