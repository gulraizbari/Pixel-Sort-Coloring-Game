using Sablo.Core;
using Sablo.Gameplay;
using UnityEngine;

namespace Sablo.UI.Settings
{
    public class Settings : BaseGameplayModule, ISettings
    {
        [SerializeField] private SettingsView _settingsView;
        private AudioPrefsHandler _audioPrefsHandler;
        /*public ISound SoundHandler { get; set; }*/

        public override void Initialize()
        {
            base.Initialize();
            _audioPrefsHandler = new AudioPrefsHandler();
            _audioPrefsHandler.SetMusicStatus(_audioPrefsHandler.GetMusicStatus());
            _audioPrefsHandler.SetSoundFXStatus(_audioPrefsHandler.GetSoundFXStatus());
            _settingsView.Show();
            CheckMusicValue();
            CheckSoundValue();
        }

        public void ToggleSoundFX()
        {
            if (_audioPrefsHandler.GetSoundFXStatus() == 1)
            {
                _audioPrefsHandler.SetSoundFXStatus(0);
            }
            else
            {
                _audioPrefsHandler.SetSoundFXStatus(1);
            }

            CheckSoundValue();
        }

        private void CheckSoundValue()
        {
            if (_audioPrefsHandler.GetSoundFXStatus() == 1)
            {
                EnableSoundEffects();
                _settingsView.ToggleSoundOnButton();
            }
            else
            {
                DisableSoundEffects();
                _settingsView.ToggleSoundOffButton();
            }
        }

        public void ToggleBackgroundMusic()
        {
            if (_audioPrefsHandler.GetMusicStatus() == 1)
            {
                _audioPrefsHandler.SetMusicStatus(0);
            }
            else
            {
                _audioPrefsHandler.SetMusicStatus(1);
            }

            CheckMusicValue();
        }

        private void CheckMusicValue()
        {
            if (_audioPrefsHandler.GetMusicStatus() == 1)
            {
                EnableBackgroundMusic();
                _settingsView.ToggleMusicOnButton();
            }
            else
            {
                DisableBackgroundMusic();
                _settingsView.ToggleMusicOffButton();
            }
        }

        private void EnableSoundEffects() => AudioManager.instance.EnableSoundEffects();

        private void DisableSoundEffects() => AudioManager.instance.DisableSoundEffects();

        private void EnableBackgroundMusic() => AudioManager.instance.EnableBackgroundMusic();

        private void DisableBackgroundMusic() => AudioManager.instance.DisableBackgroundMusic();

        public void SaveSettingsState()
        {
            _audioPrefsHandler.SaveAudioState();
        }
    }
}