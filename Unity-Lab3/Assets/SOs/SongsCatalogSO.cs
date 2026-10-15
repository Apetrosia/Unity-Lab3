using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Songs Catalog", menuName = "Rhythm/Songs Catalog")]
public class SongsCatalogSO : ScriptableObject
{
    [Tooltip("Глобальные данные о нотах (звуки, префабы, клавиши) для всех песен в этом каталоге")]
    public NoteSoundsSO GlobalNoteSounds;

    [Tooltip("Список всех доступных песен в игре")]
    public List<SongInfo> Songs = new List<SongInfo>();
}

[System.Serializable]
public class SongInfo
{
    [Tooltip("Название песни для отображения")]
    public string SongName;

    [Tooltip("TXT файл с данными песни (формат: Нота,Задержка)")]
    public TextAsset SongDataFile;

    [Tooltip("Клавиша на клавиатуре для запуска этой песни")]
    public KeyCode TriggerKey;
}