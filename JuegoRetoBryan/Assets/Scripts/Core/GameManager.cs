using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cerebro del game loop (Etapa 4).
/// Controla los estados del juego (Menu, Jugando, Victoria, Derrota), pausa el
/// tiempo, gestiona el cursor y decide las condiciones de victoria y derrota.
///
/// Esta totalmente desacoplado de la UI: solo expone el evento OnStateChanged.
/// La derrota se dispara cuando el PlayerHealth muere; la victoria cuando todos
/// los enemigos (EnemyHealth) han sido derrotados.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, Playing, Victory, GameOver }

    public static GameManager Instance { get; private set; }

    /// <summary>True solo mientras se esta jugando. Lo consultan los sistemas
    /// de input (arma) para no actuar en menus o pantallas finales.</summary>
    public static bool IsPlaying =>
        Instance != null && Instance.CurrentState == GameState.Playing;

    [Header("Inicio")]
    [Tooltip("Si esta activo, el juego arranca en el menu principal (pausado).")]
    [SerializeField] private bool startInMenu = true;

    [Header("Referencias (auto si se dejan vacias)")]
    [SerializeField] private PlayerHealth playerHealth;

    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    /// <summary>Notifica a la UI cada vez que cambia el estado del juego.</summary>
    public event System.Action<GameState> OnStateChanged;

    private int _enemiesRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDeath -= HandleEnemyDeath;
        if (playerHealth != null)
            playerHealth.OnDeath -= HandlePlayerDeath;
    }

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.OnDeath += HandlePlayerDeath;

        if (startInMenu)
            SetState(GameState.MainMenu);
        else
            StartGame();
    }

    /// <summary>Empieza/Reanuda una partida nueva. Llamado por el boton JUGAR.</summary>
    public void StartGame()
    {
        _enemiesRemaining = FindObjectsOfType<EnemyHealth>().Length;
        SetState(GameState.Playing);
    }

    /// <summary>Reinicia la escena actual desde cero.</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Vuelve al menu principal recargando la escena en estado menu.</summary>
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HandlePlayerDeath()
    {
        if (CurrentState == GameState.Playing)
            SetState(GameState.GameOver);
    }

    private void HandleEnemyDeath(EnemyHealth enemy)
    {
        if (CurrentState != GameState.Playing)
            return;

        _enemiesRemaining = Mathf.Max(0, _enemiesRemaining - 1);
        if (_enemiesRemaining == 0)
            SetState(GameState.Victory);
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;

        // Pausa el juego en todo lo que no sea "jugando"
        Time.timeScale = newState == GameState.Playing ? 1f : 0f;

        // Bloquea/libera el cursor segun corresponda
        bool playing = newState == GameState.Playing;
        Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !playing;

        OnStateChanged?.Invoke(newState);
    }
}
