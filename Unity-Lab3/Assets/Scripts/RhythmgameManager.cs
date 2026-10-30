using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Вся логика игры: запуск песни по клавише, спавн нот по таймеру в корутине,
/// падение нот (корутина на каждую ноту) и оценка нажатий.
/// Повесить на любой объект сцены и назначить Catalog и Config.
/// </summary>
public class RhythmGameManager : MonoBehaviour
{
    [Header("Конфигурация")]
    public SongsCatalogSO Catalog;
    public GameplayConfigSO Config;

    private class ActiveNote
    {
        public string Name;
        public GameObject Go;
        public float SpawnTime;
        public float HitTime;
        public bool Alive = true;
    }

    private readonly List<ActiveNote> _activeNotes = new List<ActiveNote>();
    private readonly Dictionary<string, Transform> _keyTransforms = new Dictionary<string, Transform>();
    private bool _isSongPlaying;

    private void Awake()
    {
        // Запоминаем, где стоит каждая клавиша (имя объекта = имя ноты), чтобы спавнить ноты над ней
        foreach (var key in FindObjectsOfType<PianoKey>())
        {
            _keyTransforms[key.gameObject.name] = key.transform;
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
        // Новую песню можно запустить только после окончания предыдущей
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

    // ───────────────────────── Песня ─────────────────────────

    private IEnumerator PlaySongRoutine(SongInfo song)
    {
        _isSongPlaying = true;

        List<SongNote> notes = song.ParseNotes();
        Debug.Log($"▶ Песня '{song.SongName}' началась ({notes.Count} нот)");

        // Расписание ведём по абсолютному времени, чтобы ошибки кадров не накапливались
        float scheduledTime = Time.time;

        foreach (var note in notes)
        {
            while (Time.time < scheduledTime) yield return null;

            SpawnNote(note.NoteName, scheduledTime);
            scheduledTime += note.Delay;
        }

        // Песня закончена, когда все ноты сыграны или удалены
        while (_activeNotes.Count > 0) yield return null;

        Debug.Log($"■ Песня '{song.SongName}' закончилась");
        _isSongPlaying = false;
    }

    // ───────────────────────── Ноты ─────────────────────────

    private void SpawnNote(string noteName, float spawnTime)
    {
        GameObject prefab = Config.NotePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("В GameplayConfig не задан NotePrefab, нота пропущена");
            return;
        }

        if (!_keyTransforms.TryGetValue(noteName, out Transform keyTransform))
        {
            Debug.LogWarning($"На сцене нет клавиши '{noteName}', нота пропущена");
            return;
        }

        Vector3 pos = new Vector3(keyTransform.position.x, Config.SpawnHeightY, Config.NoteZ);
        var note = new ActiveNote
        {
            Name = noteName,
            Go = Instantiate(prefab, pos, Quaternion.identity),
            SpawnTime = spawnTime,
            HitTime = spawnTime + Config.NoteTravelTime
        };

        _activeNotes.Add(note);
        StartCoroutine(NoteRoutine(note));
    }

    private IEnumerator NoteRoutine(ActiveNote note)
    {
        Transform t = note.Go.transform;
        float speed = Config.NoteSpeed;

        // Нота падает, пока жива и не вышло максимальное время жизни
        while (note.Alive && Time.time - note.SpawnTime < Config.NoteMaxLifetime)
        {
            float elapsed = Time.time - note.SpawnTime;
            Vector3 p = t.position;
            p.y = Config.SpawnHeightY - speed * elapsed;
            t.position = p;
            yield return null;
        }

        // Если дошли сюда по таймауту — нота не была нажата, удаляем
        RemoveNote(note);
    }

    private void RemoveNote(ActiveNote note)
    {
        if (!note.Alive && !_activeNotes.Contains(note)) return;

        note.Alive = false;
        _activeNotes.Remove(note);
        if (note.Go != null) Destroy(note.Go);
    }

    // ───────────────────────── Оценка нажатий ─────────────────────────

    private void OnKeyPressed(string pressedNote)
    {
        if (!_isSongPlaying) return;

        float now = Time.time;

        // Отклонение: dev < 0 — нота ещё не долетела до линии, dev > 0 — уже пролетела
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

        // 1) Нажата верная клавиша для ноты в зоне попадания
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

        // 2) Ближайшая нота ещё слишком далеко от линии — нажали рано
        if (now - nearest.HitTime < -Config.OkWindow)
        {
            Debug.Log($"EARLY ({pressedNote})");
            return;
        }

        // 3) Ближайшая нота в зоне попадания, но клавиша другая
        Debug.Log($"{GetWrongMessage()} (нажато {pressedNote}, нужно {nearest.Name})");
        RemoveNote(nearest);
    }

    private string GetWrongMessage()
    {
        string[] msgs = Config.WrongMessages;
        if (msgs == null || msgs.Length == 0) return "WRONG";
        return msgs[Random.Range(0, msgs.Length)];
    }
}