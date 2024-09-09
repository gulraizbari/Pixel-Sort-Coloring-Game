using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameUi : MonoBehaviour
{
    [SerializeField] private Button retryButton;
    [SerializeField] private Button retryButton2;
    [SerializeField] private Button playOnButton;
    [SerializeField] private GameObject levelWinPanel;
    [SerializeField] private Panel levelCompletePanel;
    [SerializeField] private Panel levelFailPanel;
    // [SerializeField]  private GameObject settingsPanel;
    [SerializeField] private Panel tutorialPanel;
    [SerializeField] private Panel settingsPanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button okButton;
    [SerializeField] private Button levelNextButton;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button crossBtn;
    [SerializeField] private Sprite[] coinLightDarkSprites;
    [SerializeField] private Image coinImage;
    [SerializeField] private Button exitBtn;
    [SerializeField] private GameObject fillTutorialPanel;
    public TMP_Text coinsVal;
    public TMP_Text coinsVal2;
    public TMP_Text levelNoText;
    public List<LevelNumber> levelNumberObjects;
    public VideoPlayer tutorialPlayer;
    public TMP_Text tutorialHeadingTxt;
    public TMP_Text tutorialDescription;
    public List<TutorialData> tutorialData;

    public IlevelManager LevelManagerHandler;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        RegisterRetryButton();
        RegisterRetryButton2();
        RegisterNextButton();
        RegisterLevelNextButtonButton();
        RegisterSettingsButton();
        RegisterSaveButton();
        RegisterCrossButton();
        RegisterPlayOnButton();
        RegisterOKButton();
        RegisterExitButton();
    }

    private void RegisterRetryButton()
    {
        retryButton.onClick.AddListener(OnClickRetry);
    }
    
    private void RegisterExitButton()
    {
        exitBtn.onClick.AddListener(OnClickExitBtn);
    }
    
    private void RegisterPlayOnButton()
    {
        playOnButton.onClick.AddListener(OnClickPlayOnBtn);
    }
    
    private void RegisterLevelNextButtonButton()
    {
        levelNextButton.onClick.AddListener(OnClickNextLevel);
    }
    
    private void RegisterNextButton()
    {
        nextButton.onClick.AddListener(OnClickNext);
    }
    
    private void RegisterOKButton()
    {
        okButton.onClick.AddListener(OnOKButton);
    }
    
    private void RegisterSettingsButton()
    {
        settingsBtn.onClick.AddListener(OnClickSettings);
    }
    
    private void RegisterCrossButton()
    {
        crossBtn.onClick.AddListener(OnClickCross);
       
    }
    
    private void RegisterSaveButton()
    {
        saveBtn.onClick.AddListener(OnClickCross);
       
    }

    private void OnClickPlayOnBtn()
    {   
        GameLoop.Instance.DecreaseCoins(100);
        TurnOffFailPanel();
        GameLoop.Instance.MakeStackOutOfPocket();
    }
    

    private void OnClickRetry()
    {   
        Time.timeScale = 1;
        DOTween.KillAll();
        StopAllCoroutines();
        SceneManager.LoadScene("Gameplay");
    }

    private void OnClickExitBtn()
    {
        tutorialPanel.panelObj.SetActive(false);
        AudioManager.instance.CrossPanelSound();
        tutorialPanel.panelBg.transform.localScale = Vector3.zero;
        GameLoop.Instance.isUiActive = false;
    }

    private void OnClickSettings()
    {
        settingsPanel.panelObj.SetActive(true);
        GameLoop.Instance.isUiActive = true;
        AudioManager.instance.ButtonClick();
        settingsPanel.panelBg.transform.DOScale(Vector3.one, 0.4f);
    }
    
    private void OnClickCross()
    {
        settingsPanel.panelObj.SetActive(false);
        GameLoop.Instance.isUiActive = false;
        AudioManager.instance.CrossPanelSound();
        settingsPanel.panelBg.transform.localScale = Vector3.zero;
    }


    private void RegisterRetryButton2()
    {
        retryButton2.onClick.AddListener(OnClickRetry2);
    }

    private void OnClickRetry2()
    {
        Time.timeScale = 1;
        DOTween.KillAll();
        StopAllCoroutines();
        SceneManager.LoadScene("Gameplay");
    }

    public void TurnOnFailPanel()
    {
        levelFailPanel.panelObj.SetActive(true);
        AudioManager.instance.PopSound();
        levelFailPanel.panelBg.transform.DOScale(Vector3.one, 0.4f);
    }
    
    public void TurnOffFailPanel()
    {
        levelFailPanel.panelObj.SetActive(false);
        levelFailPanel.panelBg.transform.localScale = Vector3.zero;
    }

    public void DisablePlayOnButton()
    {
        coinImage.sprite = coinLightDarkSprites[1];
        playOnButton.interactable = false;
    }
    
    public void EnablePlayOnButton()
    {   
        coinImage.sprite = coinLightDarkSprites[0];
        playOnButton.interactable = true;
    }
    
    public void EnableNextButton()
    {
        nextButton.interactable = true;
    }
    
    public void DisableNextButton()
    {
        nextButton.interactable = false;
    }
    
    
    public void TurnOnWinPanel()
    {
        levelWinPanel.SetActive(true);
    }
    
    public void EnableTutorialPanel()
    {   
        GameLoop.Instance.isUiActive = true;
        tutorialPanel.panelObj.SetActive(true);
        AudioManager.instance.PopSound();
        tutorialPanel.panelBg.transform.DOScale(Vector3.one, 0.4f);
    }
    
   
    
    private void OnOKButton()
    {   
        levelWinPanel.gameObject.SetActive(false);
        GameLoop.Instance.AddCoins(50);
       levelCompletePanel.panelObj.gameObject.SetActive(true);
       levelCompletePanel.panelBg.transform.DOScale(Vector3.one, 0.4f);
       AudioManager.instance.PopSound();
       GameLoop.Instance.isUiActive = true;
       //AudioManager.instance.ButtonClick();
    }

    private void OnClickNext()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Gameplay");
        levelCompletePanel.panelBg.transform.localScale = Vector3.zero;
        GameLoop.Instance.isUiActive = false;
        AudioManager.instance.ButtonClick();
    }
    
    private void OnClickNextLevel()
    {
        /*int currentLevel = LevelManagerHandler.GetCurrentLevelIndex();
        PlayerPrefs.SetInt("level", currentLevel+1);*/
        Time.timeScale = 1;
        SceneManager.LoadScene("Gameplay");
        LevelManagerHandler.IncrementLevel();  
    }

    public void EnableFillTutorial()
    {
        fillTutorialPanel.SetActive(true);
        GameLoop.Instance.isUiActive = true;
    }
    
    public void DisableFillTutorial()
    {
        if (fillTutorialPanel.activeSelf)
        {   
            fillTutorialPanel.SetActive(false);
            GameLoop.Instance.isUiActive = false;
        }
            
    }

    public void PlayVideo()
    {
        if (tutorialPlayer.isPrepared)
        {
            tutorialPlayer.Play();
        }
    }
}

[Serializable]
public class LevelNumber
{
    public GameObject parentLevelNumber;
    public List<GameObject> fillers;
}

[Serializable]
public class Panel
{
    public GameObject panelBg;
    public GameObject panelObj;
}

[Serializable]
public class TutorialData
{
    public String tutorialHeading;
    public VideoClip tutorialClip;
    public String tutorialDescription;
}