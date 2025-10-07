using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility for creating and managing QuestionSet assets.
/// This script provides menu items and custom inspector functionality for question management.
/// No scene wiring required - this is an editor-only utility script.
/// </summary>
public class QuestionSetEditor
{
    [MenuItem("Assets/Create/Quiz Game/Question Set", false, 0)]
    public static void CreateQuestionSetAsset()
    {
        QuestionSet asset = ScriptableObject.CreateInstance<QuestionSet>();
        
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (System.IO.Path.GetExtension(path) != "")
        {
            path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }
        
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Question Set.asset");
        
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        
        // Add sample questions to new assets
        asset.AddSampleQuestions();
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }
    
    [MenuItem("Quiz Game/Create Sample Question Set")]
    public static void CreateSampleQuestionSet()
    {
        QuestionSet asset = ScriptableObject.CreateInstance<QuestionSet>();
        asset.setName = "Sample Quiz Questions";
        asset.AddSampleQuestions();
        
        string assetPath = "Assets/Sample Question Set.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Created sample question set at {assetPath}");
        Selection.activeObject = asset;
    }
    
    [MenuItem("Quiz Game/Validate All Question Sets")]
    public static void ValidateAllQuestionSets()
    {
        string[] guids = AssetDatabase.FindAssets("t:QuestionSet");
        
        if (guids.Length == 0)
        {
            Debug.Log("No QuestionSet assets found in project.");
            return;
        }
        
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            QuestionSet questionSet = AssetDatabase.LoadAssetAtPath<QuestionSet>(path);
            
            if (questionSet != null)
            {
                if (questionSet.IsValid())
                {
                    validCount++;
                    Debug.Log($"✓ Valid: {questionSet.setName} ({path}) - {questionSet.QuestionCount} questions");
                }
                else
                {
                    invalidCount++;
                    Debug.LogWarning($"✗ Invalid: {questionSet.setName} ({path}) - Check questions for missing data");
                }
            }
        }
        
        Debug.Log($"Validation complete: {validCount} valid, {invalidCount} invalid question sets found.");
    }
}

/// <summary>
/// Custom property drawer for Question class to improve inspector appearance
/// </summary>
[CustomPropertyDrawer(typeof(Question))]
public class QuestionPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // Calculate heights and positions
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float textAreaHeight = 60f;
        
        Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
        Rect textRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, textAreaHeight);
        Rect optionsLabelRect = new Rect(position.x, position.y + lineHeight + textAreaHeight + spacing * 2, position.width, lineHeight);
        
        // Show foldout
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            // Question text
            SerializedProperty textProp = property.FindPropertyRelative("text");
            EditorGUI.PropertyField(textRect, textProp, new GUIContent("Question Text"));
            
            // Options label
            EditorGUI.LabelField(optionsLabelRect, "Answer Options");
            
            // Options array
            SerializedProperty optionsProp = property.FindPropertyRelative("options");
            if (optionsProp.arraySize != 4)
            {
                optionsProp.arraySize = 4;
            }
            
            for (int i = 0; i < 4; i++)
            {
                char letter = (char)('A' + i);
                Rect optionRect = new Rect(position.x, optionsLabelRect.y + lineHeight + spacing + i * (lineHeight + spacing), position.width, lineHeight);
                SerializedProperty optionProp = optionsProp.GetArrayElementAtIndex(i);
                EditorGUI.PropertyField(optionRect, optionProp, new GUIContent($"Option {letter}"));
            }
            
            // Correct answer index
            Rect correctRect = new Rect(position.x, optionsLabelRect.y + lineHeight + spacing + 4 * (lineHeight + spacing), position.width, lineHeight);
            SerializedProperty correctProp = property.FindPropertyRelative("correctIndex");
            
            string[] options = new string[] { "A", "B", "C", "D" };
            correctProp.intValue = EditorGUI.Popup(correctRect, "Correct Answer", correctProp.intValue, options);
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float textAreaHeight = 60f;
        
        if (!property.isExpanded)
        {
            return lineHeight;
        }
        
        // Foldout + text area + options label + 4 options + correct answer
        return lineHeight + spacing + textAreaHeight + spacing + lineHeight + spacing + 4 * (lineHeight + spacing) + lineHeight + spacing;
    }
}
