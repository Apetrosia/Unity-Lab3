using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Note Sounds", menuName = "Rhythm/Note Sounds")]
public class NoteSoundsSO : ScriptableObject
{
    [Tooltip("Ключи должны точно соответствовать названиям объектов клавиш на сцене (C4, C4Sharp, D4 и т.д.)")]
    public List<NoteInfo> NoteSounds = new List<NoteInfo>();

    private Dictionary<string, NoteInfo> _soundDict;

    public void Initialize()
    {
        _soundDict = new Dictionary<string, NoteInfo>();
        foreach (var note in NoteSounds)
        {
            if (!_soundDict.ContainsKey(note.NoteName))
            {
                _soundDict[note.NoteName] = note;
            }
        }
    }

    public NoteInfo GetNoteData(string noteName)
    {
        if (_soundDict == null) Initialize();

        if (_soundDict.TryGetValue(noteName, out NoteInfo data))
        {
            return data;
        }

        Debug.LogWarning($"Данные для ноты '{noteName}' не найдены в NoteSoundsSO");
        return default;
    }

    public AudioClip GetSound(string noteName)
    {
        return GetNoteData(noteName).Sound;
    }

    public KeyCode GetInputKey(string noteName)
    {
        return GetNoteData(noteName).InputKey;
    }
}

[System.Serializable]
public struct NoteInfo
{
    public string NoteName;
    public AudioClip Sound;
    public KeyCode InputKey;
}