using UnityEngine;
using System.Collections;
using System;
using TMPro;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreLabel;
    [SerializeField] private TextMeshProUGUI _asteroidsLabel;
    [SerializeField] private TextMeshProUGUI _timerLabel;
    [SerializeField] private TextMeshProUGUI _hiscoreLabel;

    [SerializeField] private CanvasGroup _HUD;
    [SerializeField] private CanvasGroup _MainMenu;
    [SerializeField] private CanvasGroup _GameOverMenu;

    [SerializeField] private TextMeshProUGUI _newHiScoreLabel;

    private void Awake()
    {
        GameManager.OnScoreUpdate += RefreshScore;
        GameManager.OnTimerUpdate += RefreshTimer;
        GameManager.OnAsteroidsUpdate += RefreshAsteroids;
        GameManager.OnHiScoreUpdate += RefreshHiScore;

        GameManager.OnSessionStarted += StartGameRoutine;
        GameManager.OnSessionStarted += RefreshHiScore;
        GameManager.OnSessionStarted += RefreshAsteroids;
        GameManager.OnSessionEnded += OpenGameOverCanvas;

        // set starting state of all canvases
        _GameOverMenu.alpha = 0f;
        _HUD.alpha = 0f;
        ChangeCanvasButtonsInteractability(_GameOverMenu.gameObject.GetComponentsInChildren<Button>(), false);
    }


    #region Main Menu

    private void StartGameRoutine()
    {
        StartCoroutine(FadeTo(_HUD, 1f, 0.5f));
        StartCoroutine(FadeTo(_MainMenu, 0f, 0.5f));
        ChangeCanvasButtonsInteractability(_MainMenu.gameObject.GetComponentsInChildren<Button>(), false);
    }

    #endregion

    #region GameOver

    private void OpenGameOverCanvas(bool newHiScore)
    {
        ChangeCanvasButtonsInteractability(_GameOverMenu.gameObject.GetComponentsInChildren<Button>(), true);
        StartCoroutine(FadeTo(_GameOverMenu, 1f, 0.5f));
        if (newHiScore){
            _newHiScoreLabel.enabled = true;
        }
        else{
            _newHiScoreLabel.enabled = false;
        }
    }

    public void PressedRetry()
    {
        StartCoroutine(FadeTo(_GameOverMenu, 0f, 0.5f));
        ChangeCanvasButtonsInteractability(_GameOverMenu.gameObject.GetComponentsInChildren<Button>(), false);

        GameManager.Instance.StartGame();
    }

    #endregion

    #region HUD

    public void RefreshScore()
    {
        _scoreLabel.text = NormalizeFloat(GameManager.Instance.Score);
    }

    public void RefreshTimer()
    {
        _timerLabel.text = NormalizeFloat(GameManager.Instance.SessionPlaytime, false);
    }

    public void RefreshAsteroids()
    {
        _asteroidsLabel.text = NormalizeFloat(GameManager.Instance.AsteroidsAmount, false);
    }

    public void RefreshHiScore()
    {
        _hiscoreLabel.text = NormalizeFloat(GameManager.Instance.HiScore);
    }

    #endregion

    #region CommonMethods

    // in this game I want to present scores as 5-character strings. The score is held as a float so some minor conversion is needed.
    private string NormalizeFloat(float rawFloat, bool pad = true)
    {
        return ((int)rawFloat).ToString().PadLeft(pad ? 5 : 0, '0');
    }

    IEnumerator FadeTo(CanvasGroup canvas, float aValue, float aTime)
    {
        float alpha = canvas.alpha;

        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            canvas.alpha = Mathf.Lerp(alpha, aValue, t);
            yield return null;
        }

        //make sure the value is set, Lerp has a tendency to not make a perfect job.:
        canvas.alpha = aValue;
    }

    private void ChangeCanvasButtonsInteractability(Button[] buttons, bool interactability)
    {
        foreach (Button button in buttons)
        {
            button.interactable = interactability;
        }
    }


    #endregion
}
