using UnityEngine;

/// <summary>
/// Contador de puntos (Etapa 4).
/// Se suscribe al evento estatico EnemyHealth.OnAnyEnemyDeath y suma los puntos
/// del enemigo derrotado. Expone OnScoreChanged para que la UI se actualice
/// sin acoplarse a la logica.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }

    /// <summary>Notifica a la UI el nuevo puntaje.</summary>
    public event System.Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
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
    }

    private void HandleEnemyDeath(EnemyHealth enemy)
    {
        AddScore(enemy != null ? enemy.ScoreValue : 0);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void ResetScore()
    {
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }
}
