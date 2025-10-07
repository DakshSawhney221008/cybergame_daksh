using System;
using UnityEngine;

/// <summary>
/// Data structure representing a single quiz question.
/// Scene Wiring: This is a data class, no GameObject assignment needed.
/// </summary>
[Serializable]
public class Question
{
    [SerializeField, TextArea(3, 5)]
    [Tooltip("The question text to display to players")]
    public string text;
    
    [SerializeField]
    [Tooltip("Four answer options (A, B, C, D)")]
    public string[] options = new string[4];
    
    [SerializeField, Range(0, 3)]
    [Tooltip("Index of the correct answer (0=A, 1=B, 2=C, 3=D)")]
    public int correctIndex;
    
    public Question()
    {
        text = "";
        options = new string[] { "", "", "", "" };
        correctIndex = 0;
    }
    
    public Question(string questionText, string[] answerOptions, int correct)
    {
        text = questionText;
        options = answerOptions ?? new string[4];
        correctIndex = Mathf.Clamp(correct, 0, 3);
    }
    
    /// <summary>
    /// Validates that the question has all required data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(text)) return false;
        if (options == null || options.Length != 4) return false;
        
        foreach (string option in options)
        {
            if (string.IsNullOrEmpty(option)) return false;
        }
        
        return correctIndex >= 0 && correctIndex < 4;
    }
}
