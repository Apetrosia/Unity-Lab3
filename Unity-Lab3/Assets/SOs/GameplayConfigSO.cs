using UnityEngine;

[CreateAssetMenu(fileName = "Gameplay Config", menuName = "Rhythm/Gameplay Config")]
public class GameplayConfigSO : ScriptableObject
{
    [Header("Падающая нота")]
    [Tooltip("Префаб падающей ноты (один на все ноты)")]
    public GameObject NotePrefab;

    [Header("Движение нот")]
    [Tooltip("Высота (Y), на которой спавнятся все ноты")]
    public float SpawnHeightY = 10f;

    [Tooltip("Координата Z, на которой находятся все падающие ноты")]
    public float NoteZ = -8.1f;

    [Tooltip("Высота (Y) линии удара (клавиш)")]
    public float HitLineY = 0f;

    [Tooltip("За сколько секунд нота должна долететь от спавна до линии удара")]
    public float NoteTravelTime = 2.0f;

    [Tooltip("Максимальное время жизни ноты (сек) с момента спавна. Если ноту не нажали, она удалится сама. Должно быть больше NoteTravelTime")]
    public float NoteMaxLifetime = 3.5f;

    [Header("Окна попадания (секунды от момента касания линии)")]
    [Tooltip("|отклонение| <= этого значения -> PERFECT")]
    public float PerfectWindow = 0.05f;

    [Tooltip("|отклонение| <= этого значения -> GOOD")]
    public float GoodWindow = 0.12f;

    [Tooltip("|отклонение| <= этого значения -> OK (это же внешняя граница попадания)")]
    public float OkWindow = 0.25f;

    [Tooltip("Если нота ещё не долетела до линии больше чем на OkWindow, но не дальше чем на это значение -> EARLY")]
    public float EarlyWindow = 0.6f;

    [Header("Сообщения")]
    [Tooltip("Варианты сообщений при нажатии неверной клавиши (выбирается случайный)")]
    public string[] WrongMessages = { "WRONG", "WRONG KEY!", "WRONG NOTE", "NOPE", "MISSED KEY" };

    public float NoteSpeed => (SpawnHeightY - HitLineY) / NoteTravelTime;

    private void OnValidate()
    {
        NoteTravelTime = Mathf.Max(0.01f, NoteTravelTime);
        NoteMaxLifetime = Mathf.Max(NoteTravelTime + 1f, NoteMaxLifetime);
        GoodWindow = Mathf.Max(GoodWindow, PerfectWindow);
        OkWindow = Mathf.Max(OkWindow, GoodWindow);
        EarlyWindow = Mathf.Max(EarlyWindow, OkWindow);
    }
}