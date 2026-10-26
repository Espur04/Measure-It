using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;
using MeasureItCS2.Systems;

namespace MeasureItCS2
{
    [FileLocation(nameof(MeasureItCS2))]
    [SettingsUIGroupOrder(GroupTool, GroupPanel, GroupKeybinds, GroupInformation)]
    [SettingsUIShowGroupName(GroupTool, GroupPanel, GroupKeybinds, GroupInformation)]
    public class Setting : ModSetting
    {
        public const string GroupTool = "Measurement";
        public const string GroupPanel = "Panel";
        public const string GroupKeybinds = "Keybinds";
        public const string GroupInformation = "Information";

        private bool m_ForceGroundHeight;
        private bool m_LockCurrentHeight;

        public Setting(IMod mod) : base(mod)
        {
            SetDefaults();
        }

        [SettingsUIDropdown(typeof(Setting), nameof(GetDistanceUnitItems))]
        [SettingsUISection(GroupTool)]
        public DistanceUnit UnitOfDistance { get; set; }

        [SettingsUIDropdown(typeof(Setting), nameof(GetDistanceUnitItems))]
        [SettingsUISection(GroupTool)]
        public DistanceUnit UnitOfHeight { get; set; }

        [SettingsUIDropdown(typeof(Setting), nameof(GetSlopeUnitItems))]
        [SettingsUISection(GroupTool)]
        public SlopeUnit UnitOfSlope { get; set; }

        [SettingsUIDropdown(typeof(Setting), nameof(GetDirectionUnitItems))]
        [SettingsUISection(GroupTool)]
        public DirectionUnit UnitOfDirection { get; set; }

        [SettingsUIDropdown(typeof(Setting), nameof(GetAreaUnitItems))]
        [SettingsUISection(GroupTool)]
        public AreaUnit UnitOfArea { get; set; }

        [SettingsUIDropdown(typeof(Setting), nameof(GetMeasureColorItems))]
        [SettingsUISection(GroupTool)]
        public MeasureColorOption MeasureColor { get; set; }

        [SettingsUISection(GroupTool)]
        public bool SnapToNodes { get; set; }

        [SettingsUISection(GroupTool)]
        public bool ForceGroundHeight
        {
            get => m_ForceGroundHeight;
            set
            {
                m_ForceGroundHeight = value;
                if (value)
                {
                    m_LockCurrentHeight = false;
                }
            }
        }

        [SettingsUISection(GroupTool)]
        public bool LockCurrentHeight
        {
            get => m_LockCurrentHeight;
            set
            {
                m_LockCurrentHeight = value;
                if (value)
                {
                    m_ForceGroundHeight = false;
                }
            }
        }

        [SettingsUIHidden]
        public bool CityPanelPlaced { get; set; }
        [SettingsUIHidden]
        public float CityPanelX { get; set; }
        [SettingsUIHidden]
        public float CityPanelY { get; set; }
        [SettingsUIHidden]
        public bool EditorPanelPlaced { get; set; }
        [SettingsUIHidden]
        public float EditorPanelX { get; set; }
        [SettingsUIHidden]
        public float EditorPanelY { get; set; }

        [SettingsUIButton]
        [SettingsUISection(GroupPanel)]
        public bool ResetPanelPosition
        {
            set => MeasureUISystem.RequestResetPanelPosition();
        }

        [SettingsUISection(GroupInformation)]
        public string Version => Mod.Version;

        [SettingsUIKeyboardBinding(BindingKeyboard.Digit5, ctrl: true)]
        [SettingsUISection(GroupKeybinds)]
        public ProxyBinding ToggleToolBinding { get; set; }

        [SettingsUIKeyboardBinding(BindingKeyboard.Z, ctrl: true)]
        [SettingsUISection(GroupKeybinds)]
        public ProxyBinding AddPointBinding { get; set; }

        [SettingsUIKeyboardBinding(BindingKeyboard.X, ctrl: true)]
        [SettingsUISection(GroupKeybinds)]
        public ProxyBinding UndoLastPointBinding { get; set; }

        public override void SetDefaults()
        {
            UnitOfDistance = DistanceUnit.Meters;
            UnitOfHeight = DistanceUnit.Meters;
            UnitOfSlope = SlopeUnit.Degree;
            UnitOfDirection = DirectionUnit.Degree;
            UnitOfArea = AreaUnit.SquareMeters;
            MeasureColor = MeasureColorOption.Red;
            SnapToNodes = true;
            ForceGroundHeight = false;
            LockCurrentHeight = false;
            CityPanelPlaced = false;
            CityPanelX = 0f;
            CityPanelY = 0f;
            EditorPanelPlaced = false;
            EditorPanelX = 0f;
            EditorPanelY = 0f;
        }

        public DropdownItem<int>[] GetDistanceUnitItems() => new[]
        {
            Item(DistanceUnit.Meters, "Unit.Meters"),
            Item(DistanceUnit.Kilometers, "Unit.Kilometers"),
            Item(DistanceUnit.Feet, "Unit.Feet"),
            Item(DistanceUnit.Miles, "Unit.Miles"),
        };

        public DropdownItem<int>[] GetSlopeUnitItems() => new[]
        {
            Item(SlopeUnit.Degree, "Unit.Degrees"),
            Item(SlopeUnit.Percentage, "Unit.Percentage"),
        };

        public DropdownItem<int>[] GetDirectionUnitItems() => new[]
        {
            Item(DirectionUnit.Degree, "Unit.Degrees"),
            Item(DirectionUnit.Point, "Unit.Cardinal"),
        };

        public DropdownItem<int>[] GetAreaUnitItems() => new[]
        {
            Item(AreaUnit.SquareMeters, "Area.SquareMeters"),
            Item(AreaUnit.Hectares, "Area.Hectares"),
            Item(AreaUnit.SquareKilometers, "Area.SquareKilometers"),
            Item(AreaUnit.SquareFeet, "Area.SquareFeet"),
            Item(AreaUnit.Acres, "Area.Acres"),
        };

        public DropdownItem<int>[] GetMeasureColorItems() => new[]
        {
            Item(MeasureColorOption.Red, "Color.Red"),
            Item(MeasureColorOption.Yellow, "Color.Yellow"),
            Item(MeasureColorOption.Magenta, "Color.Magenta"),
            Item(MeasureColorOption.Green, "Color.Green"),
            Item(MeasureColorOption.Cyan, "Color.Cyan"),
            Item(MeasureColorOption.White, "Color.White"),
            Item(MeasureColorOption.Orange, "Color.Orange"),
        };

        private static DropdownItem<int> Item<T>(T value, string key) where T : System.Enum
        {
            return new DropdownItem<int>
            {
                value = System.Convert.ToInt32(value),
                displayName = LocaleSource.GetLocalizedText(key),
            };
        }
    }

    public enum DistanceUnit
    {
        Meters = 0,
        Kilometers = 1,
        Feet = 2,
        Miles = 3,
    }

    public enum SlopeUnit
    {
        Degree = 0,
        Percentage = 1,
    }

    public enum DirectionUnit
    {
        Degree = 0,
        Point = 1,
    }

    public enum AreaUnit
    {
        SquareMeters = 0,
        Hectares = 1,
        SquareKilometers = 2,
        SquareFeet = 3,
        Acres = 4,
    }

    public enum MeasureColorOption
    {
        Yellow = 0,
        Magenta = 1,
        Red = 2,
        Green = 3,
        Cyan = 4,
        White = 5,
        Orange = 6,
    }
}
