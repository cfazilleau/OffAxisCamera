using UnityEditor;

namespace CFaz.OffAxisCamera.Editor
{
	[CustomEditor(typeof(OffAxisCamera))]
	public class OffAxisCameraEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.UpdateIfRequiredOrScript();
			SerializedProperty iterator = serializedObject.GetIterator();
			iterator.NextVisible(true);

			bool showPlaneSettings = false;

			do
			{
				if (iterator.propertyPath == "m_Script")
					continue;

				if (iterator.propertyPath == "snapToTransform")
					showPlaneSettings = iterator.objectReferenceValue == null;

				if (!showPlaneSettings &&
				    (iterator.propertyPath == "planeDistance" ||
				     iterator.propertyPath == "planeRotation"))
					continue;

				EditorGUILayout.PropertyField(iterator, true);
			}
			while (iterator.NextVisible(false));

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}
