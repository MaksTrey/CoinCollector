using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoinCollector
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private CoinSpawner _coinSpawner;
        [SerializeField] private Text _victoryText;
        [SerializeField] private GameObject _gameWinPanel;
        [SerializeField] private GameObject _gameLoosePanel;
        [SerializeField] private AudioSource _coinSfx;
        [SerializeField] private PlayerChaserAI _dog;
        [SerializeField] private CountdownTimer _countdownTimer;

        public event System.Action OnGameStart;
        public event System.Action<PlayerBaseController> OnPlayerVictory;

        private PlayerBaseController[] _players;
        private int _playersAlive = 2;

        public bool GameStarted { get; private set; } = false;
        public CoinSpawner CoinSpawner => _coinSpawner;

        private void Awake()
        {
            Instance = this;
            SetupScreenResolution();
        }

        private void SetupScreenResolution()
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
        }

        public void StartGame()
        {
            _players = FindObjectsOfType<PlayerBaseController>();
            GameStarted = true;
            OnGameStart?.Invoke();
            _coinSpawner.StartSpawning();
            _countdownTimer.StartTimer();
        }

        public void SetDifficultGame(Level level)
        {
            _countdownTimer.SetTimeRemaining(level);
            _dog.SetSpeedLevel(level);
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void CheckPlayerVictory()
        {
            _coinSfx.Play();
            if(GetTotalCoinsCollected() >= 50)
            {
                HandleVictory();
            }
        }

        private int GetTotalCoinsCollected()
        {
            int totalCoins = 0;
            foreach(var player in _players)
            {
                totalCoins += player.CoinCount;
            }
            return totalCoins;
        }

        private void HandleVictory()
        {
            GameStarted = false;
            OnPlayerVictory?.Invoke(null);
            _countdownTimer.StopTimer();
            _coinSpawner.StopSpawning();
        }

        public void OnPlayerDied()
        {
            _playersAlive--;
            if(_playersAlive <= 0)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            EndGame();
            _gameLoosePanel.SetActive(true);
        }

        public void OnTimerEnded()
        {
            GameOver();
        }

        public void PauseGame()
        {
            Time.timeScale = 0f;
            _countdownTimer.PauseTimer();
            _coinSpawner.PauseSpawning();
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
            _countdownTimer.ResumeTimer();
            _coinSpawner.ResumeSpawning();
        }

        private void OnEnable()
        {
            OnPlayerVictory += HandlePlayerVictory;
        }

        private void OnDisable()
        {
            OnPlayerVictory -= HandlePlayerVictory;
        }

        private void HandlePlayerVictory(PlayerBaseController player)
        {
            _victoryText.text = "������!!";
            _gameWinPanel.SetActive(true);
            EndGame();
        }

        private void EndGame()
        {
            GameStarted = false;
            _countdownTimer.StopTimer();
            _coinSpawner.StopSpawning();
            DisablePlayers();
        }

        private void DisablePlayers()
        {
            foreach(var player in _players)
            {
                player.gameObject.SetActive(false);
            }
        }
    }
}
