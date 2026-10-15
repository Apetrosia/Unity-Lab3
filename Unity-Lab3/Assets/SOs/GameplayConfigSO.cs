using UnityEngine;

[CreateAssetMenu(fileName = "Gameplay Config", menuName = "Rhythm/Gameplay Config")]
public class GameplayConfigSO : ScriptableObject
{
    [Tooltip("Высота (Y), на которой спавнятся все ноты")]
    public float SpawnHeightY = 10f;

    [Tooltip("Высота (Y) линии удара (клавиш)")]
    public float HitLineY = 0f;

    [Tooltip("За сколько секунд нота должна долететь от спавна до линии удара")]
    public float NoteTravelTime = 2.0f;

    public float NoteSpeed => (SpawnHeightY - HitLineY) / NoteTravelTime;
}