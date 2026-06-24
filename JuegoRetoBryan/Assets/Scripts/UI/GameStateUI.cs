using UnityEngine;

public class GameStateUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private WeaponSystem weapon;

    [Header("Textos del menu")]
    [SerializeField] private string gameTitle = "RETO BEK";
    [SerializeField] private string subtitle = "UNIFRANZ";
    [SerializeField] private string course = "Programacion Multimedia";

    // Color de acento (naranja institucional)
    private static readonly Color Accent = new Color(0.95f, 0.45f, 0.10f);

    private GameManager.GameState _state = GameManager.GameState.MainMenu;
    private int _score;
    private int _startAmmo = 6;

    private GUIStyle _title, _subtitle, _course, _button, _scoreStyle;
    private GUIStyle _cardHeader, _keyStyle, _instrStyle, _ammoInfo;
    private GUIStyle _victoryStyle, _gameOverStyle;

    private void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();
        if (weapon == null) weapon = FindObjectOfType<WeaponSystem>();

        if (weapon != null)
            _startAmmo = weapon.MaxAmmo;

        if (gameManager != null)
        {
            _state = gameManager.CurrentState;
            gameManager.OnStateChanged += s => _state = s;
        }

        if (scoreManager != null)
        {
            _score = scoreManager.Score;
            scoreManager.OnScoreChanged += s => _score = s;
        }
    }

    private void OnGUI()
    {
        EnsureStyles();

        switch (_state)
        {
            case GameManager.GameState.MainMenu: DrawMainMenu(); break;
            case GameManager.GameState.Playing: DrawScore(); break;
            case GameManager.GameState.Victory: DrawEndScreen("VICTORIA", _victoryStyle); break;
            case GameManager.GameState.GameOver: DrawEndScreen("GAME OVER", _gameOverStyle); break;
        }
    }

    private void DrawMainMenu()
    {
        DrawPanel();
        float cx = Screen.width / 2f;

        // --- Encabezado ---
        float topY = Screen.height * 0.10f;
        GUI.Label(new Rect(0, topY, Screen.width, 70), gameTitle, _title);

        GUI.color = Accent;
        GUI.DrawTexture(new Rect(cx - 140, topY + 78, 280, 5), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(0, topY + 88, Screen.width, 40), subtitle, _subtitle);
        GUI.Label(new Rect(0, topY + 128, Screen.width, 30), course, _course);

        // --- Tarjeta de instrucciones ---
        float cardW = 540f, cardX = cx - cardW / 2f;
        float cardY = Screen.height * 0.36f;
        float cardH = 260f;
        DrawCard(new Rect(cardX, cardY, cardW, cardH));

        GUI.Label(new Rect(cardX, cardY + 12, cardW, 32), "CONTROLES", _cardHeader);

        float rowY = cardY + 56;
        float rowGap = 40f;

        DrawControlRow(cardX + 40, rowY + rowGap * 0, cardW - 80, "W A S D", "Moverse");
        DrawControlRow(cardX + 40, rowY + rowGap * 1, cardW - 80, "ESPACIO", "Saltar"); // SALTO AGREGADO AQUI
        DrawControlRow(cardX + 40, rowY + rowGap * 2, cardW - 80, "CLICK IZQ", "Disparar");
        DrawControlRow(cardX + 40, rowY + rowGap * 3, cardW - 80, "R", "Recargar");

        GUI.Label(new Rect(cardX, cardY + cardH - 34, cardW, 28), $"Municion inicial:  {_startAmmo} balas", _ammoInfo);

        // --- Botones ---
        float bw = 240, bh = 56, x = cx - bw / 2f;
        float by = cardY + cardH + 30;

        if (GUI.Button(new Rect(x, by, bw, bh), "JUGAR", _button))
            gameManager?.StartGame();

        if (GUI.Button(new Rect(x, by + bh + 14, bw, bh), "SALIR", _button))
            gameManager?.QuitGame();
    }

    private void DrawScore()
    {
        GUI.color = new Color(0f, 0f, 0f, 0.5f);
        GUI.DrawTexture(new Rect(Screen.width - 210, 10, 200, 44), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 200, 14, 190, 40), $"PUNTOS: {_score}", _scoreStyle);
    }

    private void DrawEndScreen(string message, GUIStyle style)
    {
        DrawPanel();
        float cx = Screen.width / 2f;

        GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 80), message, style);
        GUI.Label(new Rect(0, Screen.height * 0.40f, Screen.width, 40), $"Puntaje final: {_score}", _subtitle);

        float bw = 240, bh = 56, x = cx - bw / 2f;
        float y = Screen.height * 0.52f;

        if (GUI.Button(new Rect(x, y, bw, bh), "REINTENTAR", _button))
            gameManager?.RestartGame();

        if (GUI.Button(new Rect(x, y + bh + 14, bw, bh), "MENU PRINCIPAL", _button))
            gameManager?.ReturnToMenu();
    }

    private void DrawControlRow(float x, float y, float w, string key, string action)
    {
        float keyW = 150f, keyH = 32f;
        GUI.color = Accent;
        GUI.DrawTexture(new Rect(x, y, keyW, keyH), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y + 1, keyW, keyH), key, _keyStyle);
        GUI.Label(new Rect(x + keyW + 18, y + 1, w - keyW - 18, keyH), action, _instrStyle);
    }

    private void DrawCard(Rect r)
    {
        GUI.color = Accent;
        GUI.DrawTexture(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), Texture2D.whiteTexture);
        GUI.color = new Color(0.08f, 0.08f, 0.10f, 0.95f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void DrawPanel()
    {
        GUI.color = new Color(0.04f, 0.04f, 0.06f, 0.92f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void EnsureStyles()
    {
        if (_title != null) return;

        _title = new GUIStyle(GUI.skin.label) { fontSize = 60, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        _subtitle = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Accent } };
        _course = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.8f, 0.8f, 0.8f) } };
        _cardHeader = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        _keyStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
        _instrStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };
        _ammoInfo = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.3f) } };
        _button = new GUIStyle(GUI.skin.button) { fontSize = 26, fontStyle = FontStyle.Bold };
        _scoreStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.yellow } };

        _victoryStyle = new GUIStyle(_title) { normal = { textColor = Color.green } };
        _gameOverStyle = new GUIStyle(_title) { normal = { textColor = Color.red } };
    }
}