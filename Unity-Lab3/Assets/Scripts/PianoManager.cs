using System.Collections.Generic;
using UnityEngine;

public class PianoManager : MonoBehaviour
{
    [Header("Конфигурация")]
    public NoteSoundsSO NoteSounds;

    [Header("Клавиши")]
    [Tooltip("Перетащи сюда все объекты клавиш или оставь пустым для автопоиска")]
    public PianoKey[] Keys;

    private Dictionary<KeyCode, PianoKey> _keyMap;

    void Start()
    {
        NoteSounds.Initialize();

        if (Keys == null || Keys.Length == 0)
        {
            Keys = FindObjectsOfType<PianoKey>();
        }

        _keyMap = new Dictionary<KeyCode, PianoKey>();
        foreach (PianoKey key in Keys)
        {
            AudioClip clip = NoteSounds.GetSound(key.NoteName);
            KeyCode inputKey = NoteSounds.GetInputKey(key.NoteName);

            key.Initialize(clip);

            if (!_keyMap.ContainsKey(inputKey))
            {
                _keyMap[inputKey] = key;
            }
            else
            {
                Debug.LogWarning($"Дублирующаяся клавиша ввода: {inputKey} для ноты {key.NoteName}");
            }
        }

        Debug.Log($"PianoManager инициализирован. Найдено клавиш: {Keys.Length}");
    }

    void Update()
    {
        foreach (var kvp in _keyMap)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                kvp.Value.TryPress();
            }
        }
    }
}