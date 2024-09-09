using System;
using Sablo.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Sablo.UI.Settings
{
    public class SettingsView : BaseView
    {
        [SerializeField] private SettingsViewRefs _settingsViewRefs;
        
        public ISettings _settingsHandler { get; set; }

        /*public override void Initialize(object model = null)
        {
            base.Initialize(model);
            _settingsHandler = model as ISettings;
        }*/

        /*public override void Show(object model = null)
        {_
            settingsHandler = model as ISettings;
        }*/

        private void OnEnable()
        {
            RegisterALlButtons();
        }

        public override void Register()
        {
            /*_settingsViewRefs.CloseButton.onClick.AddListener(HideSettingsScreen);
            _settingsViewRefs.SaveButton.onClick.AddListener(SaveSettingsStates);*/
            /*_settingsViewRefs.SoundToggleButton.onClick.AddListener(OnToggleSoundFX);
            _settingsViewRefs.MusicToggleButton.onClick.AddListener(OnToggleBackgroundMusic);*/
        }

        private void RegisterALlButtons()
        {
            _settingsViewRefs.SoundToggleButton.onClick.AddListener(OnToggleSoundFX);
            _settingsViewRefs.MusicToggleButton.onClick.AddListener(OnToggleBackgroundMusic);
        }

        private void OnToggleSoundFX()
        {
            _settingsHandler.ToggleSoundFX();
        }

        private void OnToggleBackgroundMusic()
        {
            _settingsHandler.ToggleBackgroundMusic();
        }

        private void HideSettingsScreen()
        {
            /*HapticsHandler.OnClick();*/
            _settingsViewRefs.SettingsView.SetActive(false);
            Unregister();
        }

        public void ToggleSoundOnButton()
        {
            _settingsViewRefs.SoundToggleImage.sprite = _settingsViewRefs.ToggleON;
        }

        public void ToggleSoundOffButton()
        {
            _settingsViewRefs.SoundToggleImage.sprite = _settingsViewRefs.ToggleOFF;
        }

        public void ToggleMusicOnButton()
        {
            _settingsViewRefs.MusicToggleImage.sprite = _settingsViewRefs.ToggleON;
        }

        public void ToggleMusicOffButton()
        {
            _settingsViewRefs.MusicToggleImage.sprite = _settingsViewRefs.ToggleOFF;
        }

        private void SaveSettingsStates()
        {
            _settingsHandler.SaveSettingsState();
        }
    }
}