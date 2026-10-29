using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Controls.Shapes;

namespace MauiLesson_8_SurvivalGame;

public enum ItemType
{
    Resource,
    Food,
    Drink,
    Medical,
    Weapon
}

public class InventoryItem : INotifyPropertyChanged
{
    private int _quantity;

    public string Id { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string RawName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemType Type { get; set; } = ItemType.Resource;
    public int RestoreValue { get; set; } = 0; // Для їжі/води/ліків або шкода для зброї

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    [JsonIgnore]
    public string Name => $"{Icon} {RawName} (x{Quantity})";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class Enemy
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int AttackPower { get; set; }
    public string DropItemId { get; set; } = string.Empty;
    public int DropCount { get; set; } = 1;
}

public class GameDataSaveModel
{
    public int Day { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Hunger { get; set; }
    public int MaxHunger { get; set; }
    public int Thirst { get; set; }
    public int MaxThirst { get; set; }
    public int Energy { get; set; }
    public int MaxEnergy { get; set; }
    public int MaxSlots { get; set; }
    public int WorkbenchLevel { get; set; }
    public int CampLevel { get; set; }
    public int ToolsLevel { get; set; }
    public int StrengthLevel { get; set; }
    public string EquippedWeaponName { get; set; } = "Кулаки";
    public int EquippedWeaponDamage { get; set; } = 5;
    public string CurrentLocation { get; set; } = "Лісовий табір (База)";
    public string CurrentLocationDescription { get; set; } = "";
    public string CurrentLocationStrokeHex { get; set; } = "#2E7D32";
    public string CurrentLocationTitleHex { get; set; } = "#66BB6A";
    public string CurrentLocationBgHex { get; set; } = "#1A261D";
    public List<string> UnlockedLocations { get; set; } = new();
    public List<InventoryItem> Items { get; set; } = new();
}

public class GameState
{
    private static GameState? _instance;
    public static GameState Instance => _instance ??= new GameState();

    // Характеристики персонажа
    public int Day { get; set; } = 1;
    public int Hp { get; set; } = 100;
    public int MaxHp { get; set; } = 100;

    public int Hunger { get; set; } = 80;
    public int MaxHunger { get; set; } = 100;

    public int Thirst { get; set; } = 80;
    public int MaxThirst { get; set; } = 100;

    public int Energy { get; set; } = 90;
    public int MaxEnergy { get; set; } = 100;

    // Параметри бази та навичок
    public int MaxSlots { get; set; } = 20;
    public int WorkbenchLevel { get; set; } = 1;
    public int CampLevel { get; set; } = 1;
    public int ToolsLevel { get; set; } = 1;
    public int StrengthLevel { get; set; } = 1; // Прокачування сили

    // Екіпірована зброя
    public string EquippedWeaponName { get; set; } = "Кулаки";
    public int EquippedWeaponDamage { get; set; } = 5;
    public int TotalAttackDamage => EquippedWeaponDamage + (StrengthLevel - 1) * 3;

    public int FarmDurationSeconds => Math.Max(3, 10 - (ToolsLevel - 1) * 2);

    // Дослідження та відкриття локацій
    public string CurrentLocation { get; set; } = "Лісовий табір (База)";
    public string CurrentLocationDescription { get; set; } = "Безпечне місце. Тут мало диких ресурсів, але можна безпечно збирати хмиз і каміння.";
    public string CurrentLocationStrokeHex { get; set; } = "#2E7D32";
    public string CurrentLocationTitleHex { get; set; } = "#66BB6A";
    public string CurrentLocationBgHex { get; set; } = "#1A261D";

    public List<string> UnlockedLocations { get; set; } = new() { "Лісовий табір (База)", "Густий ліс" };

    // Інвентар
    public ObservableCollection<InventoryItem> Items { get; } = new();

    // Стан поточного бою
    public Enemy? CurrentEnemy { get; set; }
    public bool IsInCombat => CurrentEnemy != null;

    public event Action? StateChanged;

    private static string SaveFilePath => System.IO.Path.Combine(FileSystem.AppDataDirectory, "survival_savegame.json");
    private GameState()
    {
        if (!LoadGame())
        {
            ResetToDefaults();
        }
    }

    public void ResetToDefaults()
    {
        Day = 1;
        Hp = 100;
        MaxHp = 100;
        Hunger = 80;
        MaxHunger = 100;
        Thirst = 80;
        MaxThirst = 100;
        Energy = 90;
        MaxEnergy = 100;
        MaxSlots = 20;
        WorkbenchLevel = 1;
        CampLevel = 1;
        ToolsLevel = 1;
        StrengthLevel = 1;
        EquippedWeaponName = "Кулаки";
        EquippedWeaponDamage = 5;

        CurrentLocation = "Лісовий табір (База)";
        CurrentLocationDescription = "Безпечне місце. Тут мало диких ресурсів, але можна безпечно збирати хмиз і каміння.";
        CurrentLocationStrokeHex = "#2E7D32";
        CurrentLocationTitleHex = "#66BB6A";
        CurrentLocationBgHex = "#1A261D";

        UnlockedLocations = new() { "Лісовий табір (База)", "Густий ліс" };

        Items.Clear();
        AddItem("wood", "🪵", "Деревина", "Базовий матеріал для будівництва та багаття", 6, ItemType.Resource);
        AddItem("stone", "🪨", "Камінь", "Використовується для створення інструментів", 4, ItemType.Resource);
        AddItem("canned_food", "🥫", "Армійські консерви", "Відновлює +35 ситості", 2, ItemType.Food, 35);
        AddItem("water", "💧", "Фляга з водою", "Втамовує спрагу (+40) та дає +15 енергії", 2, ItemType.Drink, 40);
        AddItem("medkit", "🧰", "Аптечка", "Відновлює +45 здоров'я", 1, ItemType.Medical, 45);

        CurrentEnemy = null;
        SaveGame();
        NotifyStateChanged();
    }

    public int TotalUniqueStacks => Items.Count;

    public void NotifyStateChanged()
    {
        SaveGame();
        StateChanged?.Invoke();
    }

    public void ChangeStats(int hpDelta = 0, int hungerDelta = 0, int thirstDelta = 0, int energyDelta = 0)
    {
        Hunger = Math.Clamp(Hunger + hungerDelta, 0, MaxHunger);
        Thirst = Math.Clamp(Thirst + thirstDelta, 0, MaxThirst);
        Energy = Math.Clamp(Energy + energyDelta, 0, MaxEnergy);

        // Якщо голод або спрага на нулі — втрачається здоров'я від виснаження
        int starvationPenalty = 0;
        if (Hunger == 0) starvationPenalty -= 5;
        if (Thirst == 0) starvationPenalty -= 8;

        Hp = Math.Clamp(Hp + hpDelta + starvationPenalty, 0, MaxHp);

        // Перевірка смерті/непритомності
        if (Hp <= 0)
        {
            HandlePlayerDefeat();
        }

        NotifyStateChanged();
    }

    private void HandlePlayerDefeat()
    {
        Hp = 35;
        Hunger = 25;
        Thirst = 25;
        Energy = 40;
        Day += 1;
        CurrentLocation = "Лісовий табір (База)";
        CurrentLocationDescription = "Безпечне місце. Ви прокинулися тут після виснаження або поразки в бою.";
        CurrentLocationStrokeHex = "#2E7D32";
        CurrentLocationTitleHex = "#66BB6A";
        CurrentLocationBgHex = "#1A261D";
        CurrentEnemy = null;
    }

    public int GetItemCount(string id)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        return item?.Quantity ?? 0;
    }

    public bool AddItem(string id, string icon, string rawName, string description, int count, ItemType type = ItemType.Resource, int restoreValue = 0)
    {
        var existing = Items.FirstOrDefault(i => i.Id == id);
        if (existing != null)
        {
            existing.Quantity += count;
            NotifyStateChanged();
            return true;
        }

        if (Items.Count >= MaxSlots)
            return false;

        Items.Add(new InventoryItem
        {
            Id = id,
            Icon = icon,
            RawName = rawName,
            Description = description,
            Quantity = count,
            Type = type,
            RestoreValue = restoreValue
        });

        NotifyStateChanged();
        return true;
    }

    public bool RemoveItem(string id, int count = 1)
    {
        var existing = Items.FirstOrDefault(i => i.Id == id);
        if (existing == null || existing.Quantity < count)
            return false;

        existing.Quantity -= count;
        if (existing.Quantity <= 0)
            Items.Remove(existing);

        NotifyStateChanged();
        return true;
    }

    // --- Автозбереження та завантаження ---
    public void SaveGame()
    {
        try
        {
            var data = new GameDataSaveModel
            {
                Day = Day,
                Hp = Hp,
                MaxHp = MaxHp,
                Hunger = Hunger,
                MaxHunger = MaxHunger,
                Thirst = Thirst,
                MaxThirst = MaxThirst,
                Energy = Energy,
                MaxEnergy = MaxEnergy,
                MaxSlots = MaxSlots,
                WorkbenchLevel = WorkbenchLevel,
                CampLevel = CampLevel,
                ToolsLevel = ToolsLevel,
                StrengthLevel = StrengthLevel,
                EquippedWeaponName = EquippedWeaponName,
                EquippedWeaponDamage = EquippedWeaponDamage,
                CurrentLocation = CurrentLocation,
                CurrentLocationDescription = CurrentLocationDescription,
                CurrentLocationStrokeHex = CurrentLocationStrokeHex,
                CurrentLocationTitleHex = CurrentLocationTitleHex,
                CurrentLocationBgHex = CurrentLocationBgHex,
                UnlockedLocations = UnlockedLocations,
                Items = Items.ToList()
            };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SaveFilePath, json);
        }
        catch
        {
            // У разі збою запису програма продовжує працювати стабільно
        }
    }

    public bool LoadGame()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
                return false;

            string json = File.ReadAllText(SaveFilePath);
            var data = JsonSerializer.Deserialize<GameDataSaveModel>(json);
            if (data == null)
                return false;

            Day = data.Day;
            Hp = data.Hp;
            MaxHp = data.MaxHp;
            Hunger = data.Hunger;
            MaxHunger = data.MaxHunger;
            Thirst = data.Thirst;
            MaxThirst = data.MaxThirst;
            Energy = data.Energy;
            MaxEnergy = data.MaxEnergy;
            MaxSlots = data.MaxSlots;
            WorkbenchLevel = data.WorkbenchLevel;
            CampLevel = data.CampLevel;
            ToolsLevel = data.ToolsLevel;
            StrengthLevel = data.StrengthLevel;
            EquippedWeaponName = data.EquippedWeaponName;
            EquippedWeaponDamage = data.EquippedWeaponDamage;
            CurrentLocation = data.CurrentLocation;
            CurrentLocationDescription = data.CurrentLocationDescription;
            CurrentLocationStrokeHex = data.CurrentLocationStrokeHex;
            CurrentLocationTitleHex = data.CurrentLocationTitleHex;
            CurrentLocationBgHex = data.CurrentLocationBgHex;
            UnlockedLocations = data.UnlockedLocations ?? new() { "Лісовий табір (База)", "Густий ліс" };

            Items.Clear();
            if (data.Items != null)
            {
                foreach (var it in data.Items)
                {
                    Items.Add(it);
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Статичний HUD із відображенням HP, Голоду, Спраги та Енергії
/// </summary>
public class PlayerHudView : ContentView
{
    private readonly Label _dayLocLabel;
    private readonly Label _hpLabel;
    private readonly ProgressBar _hpBar;
    private readonly Label _hungerLabel;
    private readonly ProgressBar _hungerBar;
    private readonly Label _thirstLabel;
    private readonly ProgressBar _thirstBar;
    private readonly Label _energyLabel;
    private readonly ProgressBar _energyBar;

    private bool _showSlots = true;
    public bool ShowSlots
    {
        get => _showSlots;
        set
        {
            if (_showSlots != value)
            {
                _showSlots = value;
                RefreshHud();
            }
        }
    }

    public PlayerHudView()
    {
        _dayLocLabel = new Label
        {
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#CCCCCC"),
            VerticalOptions = LayoutOptions.Center
        };

        _hpLabel = new Label { FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#EF5350") };
        _hpBar = new ProgressBar { ProgressColor = Color.FromArgb("#EF5350"), HeightRequest = 5 };

        _hungerLabel = new Label { FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#FFA726") };
        _hungerBar = new ProgressBar { ProgressColor = Color.FromArgb("#FFA726"), HeightRequest = 5 };

        _thirstLabel = new Label { FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#29B6F6") };
        _thirstBar = new ProgressBar { ProgressColor = Color.FromArgb("#29B6F6"), HeightRequest = 5 };

        _energyLabel = new Label { FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#42A5F5") };
        _energyBar = new ProgressBar { ProgressColor = Color.FromArgb("#42A5F5"), HeightRequest = 5 };

        var statsGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 8
        };

        var hpStack = new VerticalStackLayout { Spacing = 3, Children = { _hpLabel, _hpBar } };
        var hungerStack = new VerticalStackLayout { Spacing = 3, Children = { _hungerLabel, _hungerBar } };
        var thirstStack = new VerticalStackLayout { Spacing = 3, Children = { _thirstLabel, _thirstBar } };
        var energyStack = new VerticalStackLayout { Spacing = 3, Children = { _energyLabel, _energyBar } };

        statsGrid.Add(hpStack, 0, 0);
        statsGrid.Add(hungerStack, 1, 0);
        statsGrid.Add(thirstStack, 2, 0);
        statsGrid.Add(energyStack, 3, 0);

        var mainStack = new VerticalStackLayout
        {
            Spacing = 6,
            Children = { _dayLocLabel, statsGrid }
        };

        Content = new Border
        {
            BackgroundColor = Color.FromArgb("#141414"),
            Stroke = Color.FromArgb("#333333"),
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(12, 10),
            Content = mainStack
        };

        GameState.Instance.StateChanged += RefreshHud;
        RefreshHud();
    }

    public void RefreshHud()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var s = GameState.Instance;

            string weaponInfo = $"⚔️ {s.EquippedWeaponName} ({s.TotalAttackDamage})";

            _dayLocLabel.Text = ShowSlots
                ? $"📅 День {s.Day} | 📍 {s.CurrentLocation} | {weaponInfo} | 🎒 {s.TotalUniqueStacks}/{s.MaxSlots}"
                : $"📅 День {s.Day} | 📍 {s.CurrentLocation} | {weaponInfo}";

            _hpLabel.Text = $"❤️ {s.Hp}/{s.MaxHp}";
            _hpBar.Progress = (double)s.Hp / s.MaxHp;

            _hungerLabel.Text = $"🍖 {s.Hunger}/{s.MaxHunger}";
            _hungerBar.Progress = (double)s.Hunger / s.MaxHunger;

            _thirstLabel.Text = $"💧 {s.Thirst}/{s.MaxThirst}";
            _thirstBar.Progress = (double)s.Thirst / s.MaxThirst;

            _energyLabel.Text = $"⚡ {s.Energy}/{s.MaxEnergy}";
            _energyBar.Progress = (double)s.Energy / s.MaxEnergy;
        });
    }
}