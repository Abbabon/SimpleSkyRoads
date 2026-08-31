using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    #region Singleton Implementation

    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private static readonly object padlock = new object();

    private void Awake()
    {
        lock (padlock)
        {
            if (_instance != null && _instance != this){
                Destroy(this.gameObject);
            }
            else{
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
                Initialize();
            }
        }
    }

    #endregion

    //These are events that external subscribers should use; GameManager does recieve calls from gamplay elements, but it does not update unless you subscribe.
    #region Events

    public delegate void ScoreUpdated();
    public static event ScoreUpdated OnScoreUpdate;

    public delegate void TimerUpdated();
    public static event TimerUpdated OnTimerUpdate;

    public delegate void AsteroidsUpdated();
    public static event AsteroidsUpdated OnAsteroidsUpdate;

    public delegate void HiScoreUpdated();
    public static event HiScoreUpdated OnHiScoreUpdate;

    public delegate void ShieldCollected();
    public static event ShieldCollected OnShieldCollected;

    public delegate void SessionStarted();
    public static event SessionStarted OnSessionStarted;

    public delegate void SessionEnded(bool newHiScore);
    public static event SessionEnded OnSessionEnded;

	public delegate void PickedUpShield();
	public static event PickedUpShield OnPickedUpShield;

	#endregion

	// all these fields are declared as serilaized not for instantiation but for visualization:

	[SerializeField] private float _score;
    public float Score { get { return _score; } }
    [SerializeField] private int _asteroidsAmount;
    public int AsteroidsAmount { get { return _asteroidsAmount; } }
    [SerializeField] private float _sessionPlaytime;
    public float SessionPlaytime { get { return _sessionPlaytime; } }
    [SerializeField] private float _hiScore;
    public float HiScore { get { return _hiScore; } }

    [SerializeField] private float _shieldDuration = 5f;
    public float ShieldDuration { get { return _shieldDuration; } }

    [SerializeField] private bool _isPlayerShielded;
    public bool IsPlayerShielded { get { return _isPlayerShielded; } }

    private float _nextSpeedFactorIncrease;
    
    [SerializeField] private bool _isPlayerBoosting;
    public bool IsPlayerBoosting { get { return _isPlayerBoosting; } set { _isPlayerBoosting = value; } }

    // this dictionary adds spawning time according to the score acheived. (linear addition might be too easy or harsh).
	private Dictionary<float, float> _difficultyIncreasePoints;

    // this is a factor that's *multiplied* with the moving gameobjects' speeds (asteroids, powerups, road)
    [SerializeField] private float _gameplaySpeedFactor;
    public float GameplaySpeedFactor { get { return _gameplaySpeedFactor * (_isPlayerBoosting ? 2 : 1); } }

    // this is a factor that's *added* to the spawn timer, making the spawns more and more frequent
    [SerializeField] private float _gameplayDifficultyFactor;
    public float GameplayDifficultyFactor { get { return _gameplayDifficultyFactor; } }

    [SerializeField] private bool _gameInSession = false;

    public bool GameInSession { get { return _gameInSession; } }

    private bool _startedOnce = false;

    private void Initialize()
    {
        //get hiscore from memory
        Reset();
        _difficultyIncreasePoints = new Dictionary<float, float>()
        {
            { 50,  0.5f},
            { 100,  0.3f},
            { 150,  0.3f},
            { 200,  0.2f},
            { 250,  0.1f},
            { 300,  0.1f},
            { 350,  0.1f},
            { 400,  0.1f},
        };
	}
    
    //called from the GUI on both Start [on main menu] and Retry [on game over]
    public void StartGame()
    {
        Reset();
        _gameInSession = true;
        if (OnSessionStarted != null)
            OnSessionStarted(); 
    }

    private void Reset()
    {
        _hiScore = PlayerPrefs.GetInt("HISCORE" , 0);
        _score = 0;
        _asteroidsAmount = 0;
        _sessionPlaytime = 0f;
        _isPlayerBoosting = false;
        _isPlayerShielded = false;
        _gameplaySpeedFactor = 1f;
        _gameplayDifficultyFactor = 0f;
        _nextSpeedFactorIncrease = 50f;
    }

    private void Update()
    {
        if (_gameInSession)
        {
            _score += Time.deltaTime * (_isPlayerBoosting ? 2 : 1);
            _sessionPlaytime += Time.deltaTime;

            UpdateHiscore();

            //increase difficulty logic
            if ((int)_score != 0 && ((int)_score) >= _nextSpeedFactorIncrease)
            {
                if (_difficultyIncreasePoints.TryGetValue(_nextSpeedFactorIncrease, out float difficultyIncrease)){
                    _gameplayDifficultyFactor += difficultyIncrease;
                }
                _nextSpeedFactorIncrease += 50f;
            }
            
            if (OnScoreUpdate != null)
                OnScoreUpdate();
            if (OnTimerUpdate != null)
                OnTimerUpdate();
        }
        else
        {
            //Get out of 'PRESS ANY KEY', but don't enable this mode again on Game Over screen;
            if (!_startedOnce && Input.anyKey)
            {
                _startedOnce = true;
                StartGame();
            }
        }
    }

    private void UpdateHiscore()
    {
        if (_score > _hiScore)
        {
            _hiScore = _score;
            if (OnHiScoreUpdate != null)
                OnHiScoreUpdate();
        }
    }

    #region Gameplay Events

    // called from an astroid that a player has passed
    public void AsteroidPassed()
    {
        _asteroidsAmount++;
        if (OnAsteroidsUpdate != null)
            OnAsteroidsUpdate();
        _score += 5f;
    }

    // called from an astroid that hit a player
    public void PlayerHit()
    {
        if (!_isPlayerShielded)
        {
            _gameInSession = false;

            SoundManager.Instance.PlaySoundEffect(SoundEffect.Explosion);

            int newHiScore = (int)_score;
            bool isNewHiScore = newHiScore > PlayerPrefs.GetInt("HISCORE", 0);

            if (isNewHiScore)
                PlayerPrefs.SetInt("HISCORE", newHiScore);

            if (OnSessionEnded != null)
                OnSessionEnded(isNewHiScore);
        }
        else
        {
            SoundManager.Instance.PlaySoundEffect(SoundEffect.RockExplosion);
            _isPlayerShielded = false;
        }
    }

    public void PickedUpBattery()
    {
        SoundManager.Instance.PlaySoundEffect(SoundEffect.Shield);
        StartCoroutine("TurnOffShield");
        _isPlayerShielded = true;
        OnPickedUpShield();
    }

    private IEnumerator TurnOffShield(){
        yield return new WaitForSeconds(_shieldDuration);
        _isPlayerShielded = false;
    }


    public void PickedUpCrystal()
    {
        SoundManager.Instance.PlaySoundEffect(SoundEffect.Points);
        _score += 10f;
    }

    #endregion
}
