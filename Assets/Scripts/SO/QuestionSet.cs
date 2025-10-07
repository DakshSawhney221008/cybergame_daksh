using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject containing a set of questions for the quiz game.
/// Scene Wiring: Create asset via Create -> Quiz Game -> Question Set in Project window.
/// Assign the created asset to NetworkQuizManager's questionSet field in the inspector.
/// </summary>
[CreateAssetMenu(fileName = "New Question Set", menuName = "Quiz Game/Question Set")]
public class QuestionSet : ScriptableObject
{
    [SerializeField]
    [Tooltip("Name of this question set")]
    public string setName = "Default Quiz";
    
    [SerializeField]
    [Tooltip("List of questions in this set")]
    public List<Question> questions = new List<Question>();
    
    [SerializeField, Range(30, 300)]
    [Tooltip("Time limit per question in seconds")]
    public int timePerQuestion = 60;
    
    /// <summary>
    /// Gets the total number of questions in this set
    /// </summary>
    public int QuestionCount => questions?.Count ?? 0;
    
    /// <summary>
    /// Gets a question by index, returns null if index is invalid
    /// </summary>
    public Question GetQuestion(int index)
    {
        if (questions == null || index < 0 || index >= questions.Count)
            return null;
        
        return questions[index];
    }
    
    /// <summary>
    /// Validates all questions in the set
    /// </summary>
    public bool IsValid()
    {
        if (questions == null || questions.Count == 0)
            return false;
        
        foreach (Question question in questions)
        {
            if (question == null || !question.IsValid())
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Adds a sample question set for testing
    /// </summary>
    [ContextMenu("Add Sample Questions")]
    public void AddSampleQuestions()
    {
        questions.Clear();
        
        questions.Add(new Question(
            "What is the capital of France?",
            new string[] { "London", "Paris", "Berlin", "Madrid" },
            1
        ));
        
        questions.Add(new Question(
            "Which planet is known as the Red Planet?",
            new string[] { "Venus", "Mars", "Jupiter", "Saturn" },
            1
        ));
        
        questions.Add(new Question(
            "What is 2 + 2?",
            new string[] { "3", "4", "5", "6" },
            1
        ));
        
        questions.Add(new Question(
            "In which year did World War II end?",
            new string[] { "1944", "1945", "1946", "1947" },
            1
        ));
        
        questions.Add(new Question(
            "What is the largest mammal in the world?",
            new string[] { "Elephant", "Blue Whale", "Giraffe", "Hippopotamus" },
            1
        ));
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
