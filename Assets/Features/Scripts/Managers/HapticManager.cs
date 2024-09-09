using UnityEngine;
using Lofelt.NiceVibrations;
public class HapticManager : MonoBehaviour
{
    //   For click/pick any object
    float Amplitude = 0.3f;
    float Duration = 0.02f;
    //Type = Light Impact

    //For collection/park/place etc
    float C_Amplitude = 0.5f;
    float C_Duration = 0.017f;
    //Type = Light Impact.
    public static HapticManager Instance;
    private void Awake()
    {   
        PlayerPrefs.SetInt("haptics",0);
        Instance = this;
    }
    public void TriggerHapticFeedback(float amplitude, float frequency, float duration)
    {
        /*if (PlayerPrefs.GetInt("haptics") == 0)
        {*/
            Debug.Log("<color=orange>Vibrato</color>");
            HapticController.fallbackPreset = HapticPatterns.PresetType.LightImpact;
            HapticPatterns.PlayConstant(amplitude, frequency, duration);
        // }
        
    }

    //SoundManager.instance?.TriggerHapticFeedback(0.5f, 1f, 0.017f); //On Item Drop
}
