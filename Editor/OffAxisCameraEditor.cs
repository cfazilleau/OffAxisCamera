using UnityEditor;

namespace CFaz.OffAxisCamera.Editor
{
	[CustomEditor(typeof(OffAxisCamera))]
	public class OffAxisCameraEditor : UnityEditor.Editor
	{
		private OffAxisCamera CameraTarget => (OffAxisCamera)target;

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.UpdateIfRequiredOrScript();
			SerializedProperty iterator = serializedObject.GetIterator();
			iterator.NextVisible(true);

			// Draw properties
			do
			{
				switch (iterator.propertyPath)
				{
					case "m_Script":
						continue;
					case "planeDistance":
					case "planeRotation":
						if (CameraTarget.SnapToTransform != null) continue;
						break;
				}

				EditorGUILayout.PropertyField(iterator, true);
			}
			while (iterator.NextVisible(false));

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}
