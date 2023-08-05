using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    bool pause = false;
    public bool gameOver = false;
    [SerializeField] Canvas gameUI;
    [SerializeField] Canvas gameOverUI;
    [SerializeField] Canvas pauseUI;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;
    Text stageText;
    Button startButton;
    Button button;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        stageText = gameUI.transform.Find("StageText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver)
        {
            ShowGameOverUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !gameOver)
        {
            ShowPauseUI();
        }

    }

    void ShowGameOverUI()
    {
        Time.timeScale = 0;
        button = pauseUI.transform.Find("RetryButton").GetComponent<Button>();
        button.Select();
        gameUI.gameObject.SetActive(false);
        gameOverUI.gameObject.SetActive(true);
    }

    void ShowPauseUI()
    {
        button = pauseUI.transform.Find("MenuButton").GetComponent<Button>();
        button.Select();
        if (pause)
        {
            pause = false;
            Time.timeScale = 1;
            gameUI.gameObject.SetActive(true);
            pauseUI.gameObject.SetActive(false);
        } else
        {
            pause = true;
            Time.timeScale = 0;
            gameUI.gameObject.SetActive(false);
            pauseUI.gameObject.SetActive(true);
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartButtonSoundCheck());
    }
    IEnumerator StartButtonSoundCheck()
    {
        audioSource.PlayOneShot(audioClip);
        Debug.Log("開始");

        yield return new WaitWhile(() => audioSource.isPlaying);

        Debug.Log("終了");

        SceneManager.LoadScene("Stage_1");
    }

    public void Retry()
    {
        StartCoroutine(ButtonSoundCheck());
    }

    IEnumerator ButtonSoundCheck()
    {
        audioSource.PlayOneShot(audioClip);
        Debug.Log("開始");

        yield return new WaitWhile(() => audioSource.isPlaying);

        Debug.Log("終了");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Menu()
    {
        Debug.Log("メニュー画面処理準備中");
    }

    public void StageTextUpdate(string tex)
    {
        stageText.text = tex;
    }
}
