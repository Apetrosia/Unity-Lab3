using UnityEngine;

public class PianoKey : MonoBehaviour
{
    [Header("Конфигурация")]
    [Tooltip("ScriptableObject с данными о нотах")]
    public NoteSoundsSO NoteSounds;

    [Header("Компоненты")]
    public Animator Animator;
    public AudioSource AudioSource;

    private KeyCode _inputKey;
    private AudioClip _clip;
    private bool _canPress = true;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        string noteName = gameObject.name;
        _inputKey = NoteSounds.GetInputKey(noteName);
        _clip = NoteSounds.GetSound(noteName);

        if (_clip != null)
        {
            AudioSource.clip = _clip;
        }

        Debug.Log($"Клавиша {noteName} инициализирована. Клавиша ввода: {_inputKey}");
    }

    void Update()
    {
        // Проверяем нажатие клавиши
        if (Input.GetKeyDown(_inputKey))
        {
            TryPress();
        }

        // АВТОМАТИЧЕСКИ разблокируем клавишу, когда она вернулась в Idle
        if (!_canPress)
        {
            AnimatorStateInfo state = Animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("WhiteNoteIdle") || state.IsName("BlackNoteIdle"))
            {
                _canPress = true;
            }
        }
    }

    private void TryPress()
    {
        if (!_canPress) return;

        Animator.SetTrigger("Pressed");

        if (AudioSource.clip != null)
        {
            AudioSource.PlayOneShot(_clip);
        }

        _canPress = false;
    }
}