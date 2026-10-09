using UnityEngine;

public class PianoKey : MonoBehaviour
{
    [Header("Настройки клавиши")]
    [Tooltip("Имя ноты (должно совпадать с ключом в NoteSoundsSO)")]
    public string NoteName;

    [Header("Компоненты")]
    public Animator Animator;
    public AudioSource AudioSource;

    [Header("Состояния аниматора")]
    [Tooltip("Название состояния Idle в аниматоре")]
    public string IdleStateName = "Idle";

    [Tooltip("Название триггера нажатия")]
    public string PressTriggerName = "Pressed";

    private bool _isPlaying;

    public void Initialize(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.clip = clip;
        }
        else
        {
            Debug.LogWarning($"Не найден звук для клавиши {NoteName}");
        }
    }

    public void TryPress()
    {
        if (_isPlaying) return;

        if (!IsInIdleState()) return;

        Animator.SetTrigger(PressTriggerName);

        if (AudioSource.clip != null)
        {
            AudioSource.Play();
        }

        _isPlaying = true;
    }

    private bool IsInIdleState()
    {
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(IdleStateName) ||
               stateInfo.IsName("WhiteNoteIdle") ||
               stateInfo.IsName("BlackNoteIdle");
    }

    public void OnAnimationReturnToIdle()
    {
        _isPlaying = false;
    }
}