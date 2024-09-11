using UnityEngine;
using Lofelt.NiceVibrations;

public class HapticManager : MonoBehaviour
{
    public float Amplitude = 0.3f;
    public float Duration = 0.02f;
    public float C_Amplitude = 0.5f;
    public float C_Duration = 0.017f;

    public static HapticManager Instance;

    private void Awake()
    {
        PlayerPrefs.SetInt("haptics", 0);
        Instance = this;
    }

    public void TriggerHapticFeedback(float amplitude, float frequency, float duration)
    {
        HapticController.fallbackPreset = HapticPatterns.PresetType.LightImpact;
        HapticPatterns.PlayConstant(amplitude, frequency, duration);
    }
}