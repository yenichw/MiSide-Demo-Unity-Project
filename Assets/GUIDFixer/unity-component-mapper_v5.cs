using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text.RegularExpressions;

public class ComponentMapper : EditorWindow
{
    private const int MAX_CONSOLE_LINES = 405;
    private Rect dropArea;
    
    [MenuItem("Window/Component Mapper")]
    public static void ShowWindow()
    {
        GetWindow<ComponentMapper>("Component Mapper");
    }

    private void OnEnable()
    {
        dropArea = new Rect(0, 0, position.width, position.height);
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Drag a GameObject here or use the button below", MessageType.Info);
        
        Event evt = Event.current;
        switch (evt.type)
        {
        case EventType.DragUpdated:
        case EventType.DragPerform:
            if (!dropArea.Contains(evt.mousePosition))
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (GameObject droppedObject in DragAndDrop.objectReferences)
                {
                    if (droppedObject is GameObject)
                    {
                        Selection.activeGameObject = droppedObject;
                        MapSelectedGameObject();
                        break;
                    }
                }
            }
            Event.current.Use();
            break;
        }

        if (GUILayout.Button("Map Selected GameObject"))
        {
            MapSelectedGameObject();
        }
    }

    private class ComponentDetails
    {
        public long fileId;
        public string scriptGuid;
        public string scriptType;
        public bool isEnabled;
        public Dictionary<string, string> properties = new Dictionary<string, string>();
    }

    private string GetRawPropertyValueFromFile(string fileContent, long componentFileId, string propertyName)
    {
        string componentPattern = $"--- !u!\\d+ &{componentFileId}[\\s\\S]*?(?=--- !|$)";
        Match componentMatch = Regex.Match(fileContent, componentPattern);
        
        if (componentMatch.Success)
        {
            string propertyPattern = $"  {propertyName}: {{(.+?)}}";
            Match propertyMatch = Regex.Match(componentMatch.Value, propertyPattern);
            
            if (propertyMatch.Success)
            {
                return propertyMatch.Groups[1].Value.Trim();
            }
        }
        return null;
    }

	private List<long> GetGameObjectComponentFileIDs(GameObject gameObject, string sceneContent)
	{
		var componentFileIDs = new List<long>();
        
		// Get GameObject's FileID
		var goInfo = GetReferenceInfo(gameObject, new Dictionary<UnityEngine.Object, ReferenceInfo>());
        
		// Find GameObject section in scene file
		string gameObjectPattern = $"--- !u!1 &{goInfo.fileId}[\\s\\S]*?(?=--- !|$)";
		Match goMatch = Regex.Match(sceneContent, gameObjectPattern);
        
		if (goMatch.Success)
		{
			// Extract m_Component section
			string componentListPattern = @"m_Component:\s*-([\s\S]*?)(?=\n\w|$)";
			Match componentListMatch = Regex.Match(goMatch.Value, componentListPattern);
            
			if (componentListMatch.Success)
			{
				// Extract individual component fileIDs
				string componentPattern = @"component: {fileID: (\d+)}";
				MatchCollection componentMatches = Regex.Matches(componentListMatch.Value, componentPattern);
                
				foreach (Match match in componentMatches)
				{
					if (long.TryParse(match.Groups[1].Value, out long fileId))
					{
						componentFileIDs.Add(fileId);
					}
				}
			}
		}
        
		return componentFileIDs;
	}

	private ComponentDetails GetComponentDetailsFromSceneFile(string sceneContent, long componentFileId)
	{
		string componentPattern = $"--- !u!114 &{componentFileId}[\\s\\S]*?(?=--- !|$)";
        
		Match componentMatch = Regex.Match(sceneContent, componentPattern);
		if (!componentMatch.Success)
		{
			// Try alternative pattern for non-MonoBehaviour components
			componentPattern = $"--- !u!\\d+ &{componentFileId}[\\s\\S]*?(?=--- !|$)";
			componentMatch = Regex.Match(sceneContent, componentPattern);
			if (!componentMatch.Success) return null;
		}

		var details = new ComponentDetails();
		details.fileId = componentFileId;

		string componentText = componentMatch.Value;
        
		// Extract script GUID if present
		Match guidMatch = Regex.Match(componentText, @"guid: ([a-f0-9]{32})");
		if (guidMatch.Success)
		{
			details.scriptGuid = guidMatch.Groups[1].Value;
		}

		// Extract enabled state
		Match enabledMatch = Regex.Match(componentText, @"m_Enabled: (\d)");
		if (enabledMatch.Success)
		{
			details.isEnabled = enabledMatch.Groups[1].Value == "1";
		}

		// Extract script type if present
		Match scriptTypeMatch = Regex.Match(componentText, @"m_Script: {fileID: (\d+)");
		if (scriptTypeMatch.Success)
		{
			details.scriptType = scriptTypeMatch.Groups[1].Value;
		}

		// Extract component type from header
		Match typeMatch = Regex.Match(componentText, @"--- !u!(\d+)");
		if (typeMatch.Success)
		{
			details.properties["ComponentType"] = typeMatch.Groups[1].Value;
		}

		// Extract any other properties
		MatchCollection propertyMatches = Regex.Matches(componentText, @"  (\w+): (.+)");
		foreach (Match match in propertyMatches)
		{
			string key = match.Groups[1].Value;
			string value = match.Groups[2].Value.Trim();
			if (!details.properties.ContainsKey(key))
			{
				details.properties[key] = value;
			}
		}

		return details;
	}

	private string GetSourcePath(GameObject obj)
	{
		// Check if object is part of a prefab
		var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
		if (prefabStage != null && prefabStage.IsPartOfPrefabContents(obj))
		{
			return prefabStage.assetPath;
		}

		// If not in prefab stage, check if it's a prefab instance
		var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(obj);
		if (prefabAsset != null)
		{
			return AssetDatabase.GetAssetPath(prefabAsset);
		}

		// Otherwise return scene path
		return EditorSceneManager.GetActiveScene().path;
	}

	private string GetFileContent(string path)
	{
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			Debug.LogError($"Cannot find file at path: {path}");
			return null;
		}

		return File.ReadAllText(path);
	}

	private GameObject GetRootObject(GameObject obj)
	{
		var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
		if (prefabStage != null && prefabStage.IsPartOfPrefabContents(obj))
		{
			return prefabStage.prefabContentsRoot;
		}

		return obj;
	}

	private void MapSelectedGameObject()
	{
		GameObject selectedObject = Selection.activeGameObject;
		if (selectedObject == null)
		{
			Debug.LogWarning("Please select a GameObject in the hierarchy.");
			return;
		}

		string sourcePath = GetSourcePath(selectedObject);
		if (string.IsNullOrEmpty(sourcePath))
		{
			Debug.LogError("Cannot determine source file path.");
			return;
		}

        string fileContent = GetFileContent(sourcePath);
        if (string.IsNullOrEmpty(fileContent))
        {
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Mapping for GameObject: {selectedObject.name}");
        builder.AppendLine($"Source Path: {sourcePath}");
        builder.AppendLine($"Hierarchy Path: {GetHierarchyPath(selectedObject)}");

        List<long> componentFileIDs = GetGameObjectComponentFileIDs(selectedObject, fileContent);
        Component[] components = selectedObject.GetComponents<Component>();
        
        int lineCount = 3;
        Dictionary<UnityEngine.Object, ReferenceInfo> referenceCache = new Dictionary<UnityEngine.Object, ReferenceInfo>();

        for (int i = 0; i < componentFileIDs.Count; i++)
        {
            long fileId = componentFileIDs[i];
            Component component = i < components.Length ? components[i] : null;
            
            builder.AppendLine($"\nComponent {i}: {(component != null ? component.GetType().Name : "MISSING/BROKEN")} [FileID: {fileId}]");
            lineCount++;

            if (component == null)
            {
                var sourceDetails = GetComponentDetailsFromSceneFile(fileContent, fileId);
                if (sourceDetails != null)
                {
                    if (!string.IsNullOrEmpty(sourceDetails.scriptGuid))
                    {
                        builder.AppendLine($"Script GUID: {sourceDetails.scriptGuid}");
                        lineCount++;
                    }
                    if (!string.IsNullOrEmpty(sourceDetails.scriptType))
                    {
                        builder.AppendLine($"Script Type ID: {sourceDetails.scriptType}");
                        lineCount++;
                    }
                    builder.AppendLine($"Enabled: {sourceDetails.isEnabled}");
                    lineCount++;

                    foreach (var prop in sourceDetails.properties)
                    {
                        builder.AppendLine($"  {prop.Key}: {prop.Value}");
                        lineCount++;
                    }
                }
                continue;
            }

            SerializedObject serializedComponent = new SerializedObject(component);
            SerializedProperty property = serializedComponent.GetIterator();
            
            builder.AppendLine("Properties:");
            lineCount++;

            while (property.Next(true))
            {
                if (IsPropertyModified(property))
                {
                    string propertyPath = property.propertyPath;
                    string propertyValue = GetPropertyValue(property, fileContent, fileId);

                    if (property.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (property.objectReferenceValue != null)
                        {
                            var objRef = property.objectReferenceValue;
                            var refInfo = GetReferenceInfo(objRef, referenceCache);
                            builder.AppendLine($"  {propertyPath}: {propertyValue}");
                            if (refInfo.fileId != 0)
                            {
                                builder.AppendLine($"    Reference FileID: {refInfo.fileId}");
                                if (!string.IsNullOrEmpty(refInfo.guid))
                                {
                                    builder.AppendLine($"    Reference GUID: {refInfo.guid}");
                                }
                                lineCount += 3;
                            }
                        }
                        else
                        {
                            builder.AppendLine($"  {propertyPath}: {propertyValue}");
                            lineCount++;
                        }
                    }
                    else
                    {
                        builder.AppendLine($"  {propertyPath}: {propertyValue}");
                        lineCount++;
                    }
                }
            }
        }

        string output = builder.ToString();
        HandleOutput(output, lineCount, selectedObject);
    }

	private struct ReferenceInfo
	{
		public long fileId;
		public string guid;
		public bool isPrefab;
	}

	private int GetPersistentComponentID(Component component)
	{
		PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty(
			"inspectorMode", 
			BindingFlags.NonPublic | BindingFlags.Instance
		);
        
		SerializedObject serializedObject = new SerializedObject(component);
		if (inspectorModeInfo != null)
		{
			inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);
		}
        
		SerializedProperty property = serializedObject.FindProperty("m_LocalIdentfierInFile");
		return property != null ? property.intValue : -1;
	}

	private ReferenceInfo GetReferenceInfo(UnityEngine.Object obj, Dictionary<UnityEngine.Object, ReferenceInfo> cache)
	{
		if (cache.TryGetValue(obj, out var cachedInfo))
		{
			return cachedInfo;
		}

		var info = new ReferenceInfo();
        
		// Get the actual FileID using SerializedObject
		SerializedObject serializedObj = new SerializedObject(obj);
		PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty(
			"inspectorMode", 
			BindingFlags.NonPublic | BindingFlags.Instance
		);
        
		if (inspectorModeInfo != null)
		{
			inspectorModeInfo.SetValue(serializedObj, InspectorMode.Debug, null);
		}

		SerializedProperty localIdProp = serializedObj.FindProperty("m_LocalIdentfierInFile");
		if (localIdProp != null)
		{
			info.fileId = localIdProp.longValue;
		}

		// Try to get GUID for assets
		if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long assetFileId))
		{
			if (info.fileId == 0) // If we didn't get a local ID, use the asset file ID
			{
				info.fileId = assetFileId;
			}
			info.guid = guid;
		}

		// Check if it's part of a prefab
		var prefabType = PrefabUtility.GetPrefabAssetType(obj);
		info.isPrefab = prefabType != PrefabAssetType.NotAPrefab;

		cache[obj] = info;
		return info;
	}

	private void HandleOutput(string output, int lineCount, GameObject selectedObject)
	{
		if (lineCount > MAX_CONSOLE_LINES)
		{
			string sourceName = Path.GetFileNameWithoutExtension(GetSourcePath(selectedObject));
			string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string filePath = $"Assets/ComponentMapping_{sourceName}_{selectedObject.name}_{timestamp}.txt";
            
			filePath = string.Join("_", filePath.Split(Path.GetInvalidFileNameChars()));
            
			File.WriteAllText(filePath, output);
			AssetDatabase.Refresh();
            
			Debug.Log($"Component mapping exceeded {MAX_CONSOLE_LINES} lines. Full output saved to: {filePath}");
            
			var truncatedBuilder = new StringBuilder();
			string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			for (int i = 0; i < Math.Min(MAX_CONSOLE_LINES, lines.Length); i++)
			{
				truncatedBuilder.AppendLine(lines[i]);
			}
			truncatedBuilder.AppendLine($"\n... (truncated, see full output in {filePath})");
			Debug.Log(truncatedBuilder.ToString());
		}
		else
		{
			Debug.Log(output);
		}
	}

	private string GetScenePath(GameObject obj)
	{
		return EditorSceneManager.GetActiveScene().path;
	}

	private string GetHierarchyPath(GameObject obj)
	{
		string path = obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = obj.name + "/" + path;
		}
		return path;
	}

	private bool IsPropertyModified(SerializedProperty property)
	{
		return property.isArray || 
			property.hasChildren || 
			property.prefabOverride || 
			property.isInstantiatedPrefab;
	}

    private string GetPropertyValue(SerializedProperty property, string fileContent = null, long componentFileId = 0)
    {
        // If it's an object reference and it's null, try to get the raw value from file
        if (property.propertyType == SerializedPropertyType.ObjectReference && 
            property.objectReferenceValue == null && 
            fileContent != null)
        {
            string rawValue = GetRawPropertyValueFromFile(fileContent, componentFileId, property.propertyPath);
            if (!string.IsNullOrEmpty(rawValue))
            {
                return $"[From File] {rawValue}";
            }
        }

        switch (property.propertyType)
        {
        case SerializedPropertyType.Integer:
            return property.intValue.ToString();
        case SerializedPropertyType.Float:
            return property.floatValue.ToString();
        case SerializedPropertyType.String:
            return property.stringValue;
        case SerializedPropertyType.ObjectReference:
            return property.objectReferenceValue != null ? 
                property.objectReferenceValue.name : "null";
        case SerializedPropertyType.Vector2:
            return property.vector2Value.ToString();
        case SerializedPropertyType.Vector3:
            return property.vector3Value.ToString();
        default:
            return property.propertyType.ToString();
        }
    }
}