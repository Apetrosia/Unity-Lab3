using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    [Header("Конфигурация")]
    public SongsCatalogSO Catalog;
    public GameplayConfigSO Config;

    private class ActiveNote
    {
        public string Name;
        public GameObject gameObject;
        public float SpawnTime;
        public float HitTime;
        public bool Alive = true;
    }

    private readonly List<ActiveNote> _activeNotes = new List<ActiveNote>();
    private readonly Dictionary<string, float> _keysX = new Dictionary<string, float>();
    private bool _isSongPlaying;

    private void Awake()
    {
        // Запоминаем, где стоит каждая клавиша (имя объекта = имя ноты), чтобы спавнить ноты над ней
        foreach (var key in FindObjectsOfType<PianoKey>())
        {
            _keysX[key.gameObject.name] = key.transform.position.x;
        }
    }

    private void OnEnable()
    {
        PianoKey.KeyPressed += OnKeyPressed;
    }

    private void OnDisable()
    {
        PianoKey.KeyPressed -= OnKeyPressed;
    }

    private void Update()
    {
        if (_isSongPlaying) return;

        foreach (var song in Catalog.Songs)
        {
            if (song.TriggerKey != KeyCode.None && Input.GetKeyDown(song.TriggerKey))
            {
                StartCoroutine(PlaySongRoutine(song));
                break;
            }
        }
    }

    private IEnumerator PlaySongRoutine(SongInfo song)
    {
        _isSongPlaying = true;

        List<SongNote> notes = song.ParseNotes();
        Debug.Log($"Песня '{song.SongName}' началась ({notes.Count} нот)");

        float scheduledTime = Time.time;

        foreach (var note in notes)
        {
            while (Time.time < scheduledTime) yield return null;

            SpawnNote(note.NoteName, scheduledTime);
            scheduledTime += note.Delay;
        }

        while (_activeNotes.Count > 0) yield return null;

        Debug.Log($"Песня '{song.SongName}' закончилась");
        _isSongPlaying = false;
    }

    private void SpawnNote(string noteName, float spawnTime)
    {
        GameObject prefab = Config.NotePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("В GameplayConfig не задан NotePrefab");
            return;
        }

        if (!_keysX.TryGetValue(noteName, out float keyX))
        {
            Debug.LogWarning($"На сцене нет клавиши '{noteName}', нота пропущена");
            return;
        }

        Vector3 pos = new Vector3(keyX, Config.SpawnHeightY, Config.NoteZ);
        var note = new ActiveNote
        {
            Name = noteName,
            gameObject = Instantiate(prefab, pos, Quaternion.identity),
            SpawnTime = spawnTime,
            HitTime = spawnTime + Config.NoteTravelTime
        };

        TMP_Text label = note.gameObject.GetComponentInChildren<TMP_Text>();
        label.text = GetDisplayName(noteName);

        _activeNotes.Add(note);
        StartCoroutine(NoteRoutine(note));
    }

    private static string GetDisplayName(string noteName)
    {
        if (!noteName.EndsWith("Sharp")) return noteName;

        string core = noteName.Substring(0, 2); // "A3"
        if (core.Length < 2) return noteName;

        return core[0] + "#" + core[1]; // "A" + "#" + "3"
    }

    private IEnumerator NoteRoutine(ActiveNote note)
    {
        Transform t = note.gameObject.transform;
        float speed = Config.NoteSpeed;

        while (note.Alive && Time.time - note.SpawnTime < Config.NoteMaxLifetime)
        {
            float elapsed = Time.time - note.SpawnTime;
            Vector3 p = t.position;
            p.y = Config.SpawnHeightY - speed * elapsed;
            t.position = p;
            yield return null;
        }

        RemoveNote(note);
    }

    private void RemoveNote(ActiveNote note)
    {
        note.Alive = false;
        _activeNotes.Remove(note);
        Destroy(note.gameObject);
    }

    private void OnKeyPressed(string pressedNote)
    {
        if (!_isSongPlaying) return;

        float now = Time.time;

        ActiveNote matching = null;
        float matchingAbs = float.MaxValue;
        ActiveNote nearest = null;
        float nearestAbs = float.MaxValue;

        foreach (var note in _activeNotes)
        {
            float dev = now - note.HitTime;
            if (dev < -Config.EarlyWindow || dev > Config.OkWindow) continue;

            float abs = Mathf.Abs(dev);

            if (abs < nearestAbs)
            {
                nearest = note;
                nearestAbs = abs;
            }

            if (note.Name == pressedNote && abs <= Config.OkWindow && abs < matchingAbs)
            {
                matching = note;
                matchingAbs = abs;
            }
        }

        // Рядом с линией нет нот — просто играем на пианино, ничего не пишем
        if (nearest == null) return;

        // Нажата верная клавиша для ноты в зоне попадания
        if (matching != null)
        {
            float devMs = (now - matching.HitTime) * 1000f;
            string verdict = matchingAbs <= Config.PerfectWindow ? "PERFECT"
                           : matchingAbs <= Config.GoodWindow ? "GOOD"
                           : "OK";
            Debug.Log($"{verdict} ({pressedNote}, {devMs:+0;-0} мс)");
            RemoveNote(matching);
            return;
        }

        // Ближайшая нота ещё слишком далеко от линии — нажали рано
        if (now - nearest.HitTime < -Config.OkWindow)
        {
            Debug.Log($"EARLY ({pressedNote})");
            return;
        }

        // Ближайшая нота в зоне попадания, но клавиша другая
        Debug.Log($"{GetWrongMessage()} (нажато {pressedNote}, нужно {nearest.Name})");
        RemoveNote(nearest);
    }

    private string GetWrongMessage()
    {
        return Config.WrongMessages[Random.Range(0, Config.WrongMessages.Length)];
    }
}