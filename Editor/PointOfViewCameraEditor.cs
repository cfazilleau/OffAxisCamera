using UnityEditor;
using UnityEngine;

namespace CFaz.OffAxisCamera.Editor
{
	[CustomEditor(typeof(PointOfViewCamera))]
	public class PointOfViewCameraEditor : UnityEditor.Editor
	{
		private PointOfViewCamera CameraTarget => (PointOfViewCamera)target;

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.UpdateIfRequiredOrScript();
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
					case "m_Script":
						continue;
				}

				EditorGUILayout.PropertyField(iterator, true);
			}
			while (iterator.NextVisible(false));

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}

		private void OnSceneGUI()
		{
			Rect rect = CameraTarget.PlaneRect;

			Vector3 bl = CameraTarget.transform.TransformPoint(new Vector2(rect.xMin, rect.yMin));
			Vector3 br = CameraTarget.transform.TransformPoint(new Vector2(rect.xMax, rect.yMin));
			Vector3 tl = CameraTarget.transform.TransformPoint(new Vector2(rect.xMin, rect.yMax));
			Vector3 tr = CameraTarget.transform.TransformPoint(new Vector2(rect.xMax, rect.yMax));

			// Draw projection plane
			Handles.color = Color.white;
			Handles.DrawLine(tl, tr);
			Handles.DrawLine(tl, bl);
			Handles.DrawLine(br, tr);
			Handles.DrawLine(br, bl);

			Vector3 povWorld = CameraTarget.transform.TransformPoint(CameraTarget.PointOfViewLocal);

			//Handles.Draws(povWorld, 0.01f);
			Handles.color = Color.gray;
			Handles.DrawDottedLine(povWorld, tr, 1);
			Handles.DrawDottedLine(povWorld, tl, 1);
			Handles.DrawDottedLine(povWorld, br, 1);
			Handles.DrawDottedLine(povWorld, bl, 1);
		}
	}
}
