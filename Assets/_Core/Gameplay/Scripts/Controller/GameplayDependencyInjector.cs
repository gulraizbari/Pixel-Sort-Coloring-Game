using Sablo.UI.Settings;
using UnityEngine;

namespace Sablo.Core
{
    public class GameplayDependencyInjector : BaseDependencyInjector
    {
        [SerializeField] private TapController tapController;
        [SerializeField] private Tray tray;
        [SerializeField] private Roller roller;
        [SerializeField] private levelManager levelManager;
        [SerializeField] private SlateBuilder slateBuilder;
        [SerializeField] private BuildingMaker buildingMaker;
        [SerializeField] private GameUi gameUi;
        [SerializeField] private GameLoop gameLoop;
        [SerializeField] private SettingsView settingsView;
        [SerializeField] private Settings settings;
        
        public override void InjectDependencies()
        {
            tapController.TrayHandler = tray;
            tapController.RollerHandler = roller;
            roller.TrayHandler = tray;
            tray.LevelManagerHandler = levelManager;
            slateBuilder.LevelManagerHandler = levelManager;
            roller.LevelManagerHandler = levelManager;
            buildingMaker.LevelManagerHandler = levelManager;
            gameUi.LevelManagerHandler = levelManager;
            roller.SlateBuilderHandler = slateBuilder;
            gameLoop.LevelHandler = levelManager;
            settingsView._settingsHandler = settings;
        }
    }
}