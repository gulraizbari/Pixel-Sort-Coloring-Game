using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;


public class loadingFiller : MonoBehaviour
{
    [SerializeField] private Image _loadingFill;



    private void Start()
    {
        LoadGameScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void LoadGameScene(int sceneIndex)
    {
        DOTween.To(x => _loadingFill.fillAmount = x, 0, 1, 1.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => 
            {
                SceneManager.LoadSceneAsync(sceneIndex);
            });
    }

    /*private void CheckAndSetCurrentLevelNumber()
    {
        var currentLevelNumber = GetCurrentLevelNumber();
        if (currentLevelNumber > 0)
        {
            _currentLevelPrefs.SetCurrentLevelNumber(currentLevelNumber);
        }
        else
        {
            _currentLevelPrefs.SetCurrentLevelNumber(1);
        }
    }*/

    /*private int GetCurrentLevelNumber()
    {
        return PlayerPrefs.GetInt(Constants.LevelConstants.CurrentLevelNumber, 0);
    }*/
}
