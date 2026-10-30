using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[CreateAssetMenu(fileName = "Songs Catalog", menuName = "Rhythm/Songs Catalog")]
public class SongsCatalogSO : ScriptableObject
{
    [Tooltip("Глобальные данные о нотах (звуки, префабы, клавиши) для всех песен в этом каталоге")]
    public NoteSoundsSO GlobalNoteSounds;

    [Tooltip("Список всех доступных песен в игре")]
    public List<SongInfo> Songs = new List<SongInfo>();

    /// <summary>Ищет песню, привязанную к нажатой клавише.</summary>
    public bool TryGetSongByKey(KeyCode key, out SongInfo song)
    {
        foreach (var s in Songs)
        {
            if (s != null && s.TriggerKey != KeyCode.None && s.TriggerKey == key)
            {
                song = s;
                return true;
            }
        }

        song = null;
        return false;
    }
}

/// <summary>Одна строка файла песни: какую ноту заспавнить и сколько ждать до следующей.</summary>
public struct SongNote
{
    public string NoteName;
    public float Delay;
}

[System.Serializable]
public class SongInfo
{
    [Tooltip("Название песни для отображения")]
    public string SongName;

    [Tooltip("TXT файл с данными песни. Каждая строка: Нота,Задержка (например C4,0.5). " +
             "Задержка — сколько секунд ждать ПОСЛЕ спавна этой ноты до спавна следующей. Дробная часть через точку.")]
    public TextAsset SongDataFile;

    [Tooltip("Клавиша на клавиатуре для запуска этой песни")]
    public KeyCode TriggerKey;

    /// <summary>Разбирает TXT файл в список нот. Некорректные строки пропускаются с предупреждением.</summary>
    public List<SongNote> ParseNotes()
    {
        var result = new List<SongNote>();
        if (SongDataFile == null)
        {
            Debug.LogWarning($"У песни '{SongName}' не задан файл данных");
            return result;
        }

        string[] lines = SongDataFile.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim().Trim('\uFEFF');
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 2)
            {
                Debug.LogWarning($"Песня '{SongName}', строка {i + 1}: ожидался формат 'Нота,Задержка', получено '{line}'");
                continue;
            }

            string noteName = parts[0].Trim();
            if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float delay))
            {
                Debug.LogWarning($"Песня '{SongName}', строка {i + 1}: не удалось разобрать задержку '{parts[1]}'");
                continue;
            }

            result.Add(new SongNote { NoteName = noteName, Delay = Mathf.Max(0f, delay) });
        }

        return result;
    }
}