using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using MeasureItCS2.Systems;

namespace MeasureItCS2
{
    public class Mod : IMod
    {
        public static readonly string Id = "MeasureItCS2";
        public const string Version = "1.0.0";

        public static ILog Log = LogManager.GetLogger(Id).SetShowsErrorsInUI(false);

        public static Setting Settings { get; private set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            Log.Info($"[{Id}] OnLoad");

            Settings = new Setting(this);
            Settings.RegisterKeyBindings();
            AssetDatabase.global.LoadSettings(Id, Settings, new Setting(this));

            foreach (string localeId in LocaleSource.SupportedLocaleIds)
            {
                GameManager.instance.localizationManager.AddSource(localeId, new LocaleSource(Settings, localeId));
            }

            Settings.RegisterInOptionsUI();

            updateSystem.UpdateAt<MeasureToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<MeasureUISystem>(SystemUpdatePhase.UIUpdate);
        }

        public void OnDispose()
        {
            Log.Info($"[{Id}] OnDispose");

            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }
    }
}
