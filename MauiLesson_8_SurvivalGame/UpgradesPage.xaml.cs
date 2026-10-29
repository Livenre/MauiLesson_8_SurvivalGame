namespace MauiLesson_8_SurvivalGame;

public partial class UpgradesPage : ContentPage
{
    public UpgradesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        HudView.RefreshHud();
        RefreshUpgradesUI();
    }

    private void RefreshUpgradesUI()
    {
        var state = GameState.Instance;

        int wood = state.GetItemCount("wood");
        int stone = state.GetItemCount("stone");
        int iron = state.GetItemCount("iron");
        int cloth = state.GetItemCount("cloth");
        int meat = state.GetItemCount("raw_meat");

        ResourcesSummaryLabel.Text = $"Ваші ресурси: 🪵 {wood} | 🪨 {stone} | ⛓️ {iron} | 🧵 {cloth} | 🥩 {meat}";

        WorkbenchTitleLabel.Text = $"Верстак (Рівень {state.WorkbenchLevel})";
        BackpackTitleLabel.Text = $"Місткість рюкзака ({state.MaxSlots} слотів)";
        CampTitleLabel.Text = $"Укріплення табору (Рівень {state.CampLevel})";
        ToolsTitleLabel.Text = $"Інструменти (Рівень {state.ToolsLevel} — Фарм {state.FarmDurationSeconds} сек)";
        StrengthTitleLabel.Text = $"Тренування сили (Рівень {state.StrengthLevel} — Базова шкода +{(state.StrengthLevel - 1) * 3})";
    }

    // --- КРАФТИНГ ---
    private void OnCraftSpearClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.GetItemCount("wood") < 5 || state.GetItemCount("stone") < 2)
        {
            UpgradeLogLabel.Text = "❌ Не вистачає: Деревина x5, Камінь x2!";
            return;
        }

        state.RemoveItem("wood", 5);
        state.RemoveItem("stone", 2);
        state.AddItem("spear", "🗡️", "Дерев'яний спис", "Саморобна зброя. Натисніть «Дія» в інвентарі, щоб екіпірувати", 1, ItemType.Weapon, 8);
        RefreshUpgradesUI();
        UpgradeLogLabel.Text = "✅ Скрафчено «Дерев'яний спис»! Він доданий у ваш інвентар.";
    }

    private void OnCraftMacheteClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.WorkbenchLevel < 2)
        {
            UpgradeLogLabel.Text = "❌ Потрібен Верстак 2-го рівня для виготовлення металевої зброї!";
            return;
        }

        if (state.GetItemCount("iron") < 4 || state.GetItemCount("wood") < 3)
        {
            UpgradeLogLabel.Text = "❌ Не вистачає: Залізна руда x4, Деревина x3!";
            return;
        }

        state.RemoveItem("iron", 4);
        state.RemoveItem("wood", 3);
        state.AddItem("machete", "⚔️", "Залізний мачете", "Гострий важкий клинок. Натисніть «Дія» в інвентарі, щоб екіпірувати", 1, ItemType.Weapon, 18);
        RefreshUpgradesUI();
        UpgradeLogLabel.Text = "✅ Скрафчено «Залізний мачете»! Він доданий у ваш інвентар.";
    }

    private void OnCookMeatClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.GetItemCount("raw_meat") < 1 || state.GetItemCount("wood") < 1)
        {
            UpgradeLogLabel.Text = "❌ Потрібно: Сире м'ясо x1 та Деревина x1 (для багаття)!";
            return;
        }

        state.RemoveItem("raw_meat", 1);
        state.RemoveItem("wood", 1);
        state.AddItem("cooked_meat", "🍖", "Смажене м'ясо", "Поживна гаряча страва. Відновлює +45 ситості", 1, ItemType.Food, 45);
        RefreshUpgradesUI();
        UpgradeLogLabel.Text = "✅ Приготовано «Смажене м'ясо» (+45 ситості)!";
    }

    private void OnCraftBandageClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.GetItemCount("cloth") < 2)
        {
            UpgradeLogLabel.Text = "❌ Потрібно: Тканина x2 для створення бинта!";
            return;
        }

        state.RemoveItem("cloth", 2);
        state.AddItem("bandage", "🩹", "Саморобний бинт", "Зупиняє кровотечу. Відновлює +25 HP", 1, ItemType.Medical, 25);
        RefreshUpgradesUI();
        UpgradeLogLabel.Text = "✅ Скрафчено «Саморобний бинт» (+25 HP)!";
    }

    // --- ПРОКАЧУВАННЯ ---
    private void OnUpgradeWorkbenchClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.GetItemCount("wood") < 8 || state.GetItemCount("stone") < 5)
        {
            UpgradeLogLabel.Text = "❌ Потрібно: Деревина x8, Камінь x5 для покращення верстака!";
            return;
        }

        state.WorkbenchLevel++;
        state.RemoveItem("wood", 8);
        state.RemoveItem("stone", 5);
        state.NotifyStateChanged();

        RefreshUpgradesUI();
        UpgradeLogLabel.Text = $"✅ Верстак покращено до Рівня {state.WorkbenchLevel}! Відкрито рецепт Залізного мачете.";
    }

    private void OnUpgradeBackpackClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.GetItemCount("cloth") < 3 || state.GetItemCount("wood") < 3)
        {
            UpgradeLogLabel.Text = "❌ Потрібно: Тканина x3, Деревина x3!";
            return;
        }

        state.MaxSlots += 5;
        state.RemoveItem("cloth", 3);
        state.RemoveItem("wood", 3);
        state.NotifyStateChanged();

        RefreshUpgradesUI();
        UpgradeLogLabel.Text = $"✅ Місткість рюкзака збільшено до {state.MaxSlots} слотів!";
    }

    private void OnUpgradeCampClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.GetItemCount("wood") < 8 || state.GetItemCount("iron") < 2)
        {
            UpgradeLogLabel.Text = "❌ Потрібно: Деревина x8, Залізна руда x2!";
            return;
        }

        state.CampLevel++;
        state.MaxHp += 20;
        state.RemoveItem("wood", 8);
        state.RemoveItem("iron", 2);
        state.ChangeStats(hpDelta: 20);

        RefreshUpgradesUI();
        UpgradeLogLabel.Text = $"✅ Табір укріплено! Максимальне здоров'я зросло до {state.MaxHp}.";
    }

    private void OnUpgradeToolsClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.GetItemCount("iron") < 2 || state.GetItemCount("stone") < 3)
        {
            UpgradeLogLabel.Text = "❌ Потрібно: Залізна руда x2, Камінь x3!";
            return;
        }

        state.ToolsLevel++;
        state.RemoveItem("iron", 2);
        state.RemoveItem("stone", 3);
        state.NotifyStateChanged();

        RefreshUpgradesUI();
        UpgradeLogLabel.Text = $"✅ Інструменти покращено до Рівня {state.ToolsLevel}! Фарм прискорено.";
    }

    private void OnUpgradeStrengthClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.GetItemCount("raw_meat") < 2 || state.GetItemCount("wood") < 4)
        {
            UpgradeLogLabel.Text = "❌ Для тренування сили потрібно: Сире м'ясо x2 та Деревина x4!";
            return;
        }

        state.StrengthLevel++;
        state.RemoveItem("raw_meat", 2);
        state.RemoveItem("wood", 4);
        state.NotifyStateChanged();

        RefreshUpgradesUI();
        UpgradeLogLabel.Text = $"✅ Рівень сили підвищено до {state.StrengthLevel}! Ваша атака зросла на +3.";
    }
}