using UnityEngine;

namespace Sablo.UI.Settings
{
    public class AudioPrefsHandler
    {
        public int GetSoundFXStatus()
        {
            var soundStatus = PlayerPrefs.GetInt(Constants.SoundStatusConstants.SoundStatus, 1);
            return soundStatus;
        }

        public void SetSoundFXStatus(int status)
        {
            PlayerPrefs.SetInt(Constants.SoundStatusConstants.SoundStatus, status);
        }

        public int GetMusicStatus()
        {
            var soundStatus = PlayerPrefs.GetInt(Constants.MusicStatusConstants.MusicStatus, 1);
            return soundStatus;
        }

        public void SetMusicStatus(int status)
        {
            PlayerPrefs.SetInt(Constants.MusicStatusConstants.MusicStatus, status);
        }

        public void SaveAudioState()
        {
            PlayerPrefs.Save();
        }
    }
}