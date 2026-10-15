using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Note Sounds", menuName = "Rhythm/Note Sounds")]
public class NoteSoundsSO : ScriptableObject
{
    [Tooltip("Ключи должны точно соответствовать названиям объектов клавиш на сцене (C4, C4Sharp, D4 и т.д.)")]
    public List<NoteSoundPair> NoteSounds = new List<NoteSoundPair>();

    private Dictionary<string, NoteSoundPair> _soundDict;

    public void Initialize()
    {
        _soundDict = new Dictionary<string, NoteSoundPair>();
        foreach (var pair in NoteSounds)
        {
            if (!_soundDict.ContainsKey(pair.NoteName))
            {
                _soundDict[pair.NoteName] = pair;
            }
        }
    }

    public NoteSoundPair GetNoteData(string noteName)
    {
        if (_soundDict == null) Initialize();

        if (_soundDict.TryGetValue(noteName, out NoteSoundPair data))
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

    public GameObject GetNotePrefab(string noteName)
    {
        return GetNoteData(noteName).NotePrefab;
    }
}

[System.Serializable]
public struct NoteSoundPair
{
    public string NoteName;
    public AudioClip Sound;
    public KeyCode InputKey;
    public GameObject NotePrefab;
}