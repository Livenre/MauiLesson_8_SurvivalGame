namespace MauiLesson_8_SurvivalGame;

public partial class ExplorationPage : ContentPage
{
    private bool _isFarming = false;
    private readonly Random _random = new();

    public ExplorationPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        HudView.RefreshHud();
        UpdateMapButtons();
        ApplyLocationThemeFromState();
        UpdateFarmLabels();
        CheckCombatState();
    }

    private void UpdateMapButtons()
    {
        var state = GameState.Instance;

        SetupLocationButton(BtnDesert, "Піщана пустеля", "#C68400", "🏜️");
        SetupLocationButton(BtnRuins, "Покинуті руїни", "#546E7A", "🏚️");
        SetupLocationButton(BtnCave, "Гірська печера", "#4527A0", "⛰️");
    }

    private void SetupLocationButton(Button btn, string locName, string colorHex, string icon)
    {
        bool isUnlocked = GameState.Instance.UnlockedLocations.Contains(locName);
        if (isUnlocked)
        {
            btn.Text = $"{icon} {locName}";
            btn.BackgroundColor = Color.FromArgb(colorHex);
            btn.TextColor = Colors.White;
        }
        else
        {
            btn.Text = $"🔒 {locName}";
            btn.BackgroundColor = Color.FromArgb("#333333");
            btn.TextColor = Color.FromArgb("#9E9E9E");
        }
    }

    private void ApplyLocationThemeFromState()
    {
        var state = GameState.Instance;
        CurrentLocationTitle.Text = $"Поточна локація: {state.CurrentLocation}";
        CurrentLocationDesc.Text = state.CurrentLocationDescription;

        CurrentLocationBorder.Stroke = Color.FromArgb(state.CurrentLocationStrokeHex);
        CurrentLocationTitle.TextColor = Color.FromArgb(state.CurrentLocationTitleHex);
        CurrentLocationBorder.BackgroundColor = Color.FromArgb(state.CurrentLocationBgHex);
    }

    private void UpdateFarmLabels()
    {
        if (!_isFarming)
        {
            int fullDuration = GameState.Instance.FarmDurationSeconds;
            int fastDuration = Math.Max(2, fullDuration / 2);

            MainFarmButton.Text = $"🎲 Пофармитися на удачу ({fastDuration} сек | -7⚡ -4🍖 -5💧)";
            TargetedHeaderLabel.Text = $"Цільовий видобуток ({fullDuration} сек | -15⚡ -8🍖 -9💧):";
        }
    }

    private async void OnLocationSelected(object sender, EventArgs e)
    {
        if (_isFarming || GameState.Instance.IsInCombat)
        {
            ExplorationLogLabel.Text = "⚠️ Неможливо змінити локацію під час дії або бою!";
            return;
        }

        if (sender is Button btn && btn.CommandParameter is string targetName)
        {
            var state = GameState.Instance;

            // Якщо локація ще закрита — пропонуємо відкрити її
            if (!state.UnlockedLocations.Contains(targetName))
            {
                await TryUnlockLocation(targetName);
                return;
            }

            if (state.CurrentLocation == targetName)
            {
                ExplorationLogLabel.Text = $"ℹ️ Ви вже знаходитесь у локації «{targetName}».";
                return;
            }

            bool isReturningToCamp = targetName.Contains("Лісовий табір");
            const int travelEnergyCost = 5;
            const int travelHungerCost = 3;
            const int travelThirstCost = 4;

            if (!isReturningToCamp)
            {
                if (state.Energy < travelEnergyCost || state.Hunger < travelHungerCost || state.Thirst < travelThirstCost)
                {
                    ExplorationLogLabel.Text = "⚡ Недостатньо сил або води для далекої подорожі! Відпочиньте у таборі.";
                    return;
                }
            }

            state.CurrentLocation = targetName;
            SetLocationDetails(targetName);
            state.ChangeStats(hpDelta: 0, hungerDelta: -travelHungerCost, thirstDelta: -travelThirstCost, energyDelta: -travelEnergyCost);

            ApplyLocationThemeFromState();
            ExplorationLogLabel.Text = $"🚶 Ви перейшли у локацію «{targetName}» (-{travelEnergyCost}⚡, -{travelHungerCost}🍖, -{travelThirstCost}💧).";
        }
    }

    private async Task TryUnlockLocation(string locName)
    {
        var state = GameState.Instance;
        int requiredEnergy = 25;
        string costText = "25 Енергії та 1 Фляга з водою";

        bool confirm = await DisplayAlert("Відкриття нової локації",
            $"Локація «{locName}» ще не розвідана!\nДля відкриття шляху потрібно: {costText}.\nРозпочати розвідку?", "Так, розвідати", "Скасувати");

        if (!confirm) return;

        if (state.Energy < requiredEnergy || state.GetItemCount("water") < 1)
        {
            await DisplayAlert("Брак ресурсів", "У вас недостатньо енергії (потрібно 25) або немає фляги з водою!", "OK");
            return;
        }

        state.ChangeStats(energyDelta: -requiredEnergy);
        state.RemoveItem("water", 1);
        state.UnlockedLocations.Add(locName);
        state.NotifyStateChanged();

        UpdateMapButtons();
        await DisplayAlert("Успіх!", $"Локацію «{locName}» успішно розвідано та відкрито для подорожей!", "Чудово");
    }

    private void SetLocationDetails(string name)
    {
        var state = GameState.Instance;
        if (name == "Лісовий табір (База)")
        {
            state.CurrentLocationDescription = "Безпечне місце. Тут мало диких ресурсів, але можна безпечно збирати хмиз і каміння.";
            state.CurrentLocationStrokeHex = "#2E7D32";
            state.CurrentLocationTitleHex = "#66BB6A";
            state.CurrentLocationBgHex = "#1A261D";
        }
        else if (name == "Густий ліс")
        {
            state.CurrentLocationDescription = "Багато деревини, диких ягід та чистих струмків. Можуть напасти дикі вовки.";
            state.CurrentLocationStrokeHex = "#4CAF50";
            state.CurrentLocationTitleHex = "#81C784";
            state.CurrentLocationBgHex = "#1B281E";
        }
        else if (name == "Піщана пустеля")
        {
            state.CurrentLocationDescription = "Спекотна пустеля. Багато каміння та тканини зі старих караванів. Водяться гігантські скорпіони.";
            state.CurrentLocationStrokeHex = "#FFA000";
            state.CurrentLocationTitleHex = "#FFD54F";
            state.CurrentLocationBgHex = "#2B2216";
        }
        else if (name == "Покинуті руїни")
        {
            state.CurrentLocationDescription = "Небезпечні залишки будівель. Можна знайти залізо, консерви та аптечки. Тут блукають мародери.";
            state.CurrentLocationStrokeHex = "#78909C";
            state.CurrentLocationTitleHex = "#B0BEC5";
            state.CurrentLocationBgHex = "#1E2528";
        }
        else if (name == "Гірська печера")
        {
            state.CurrentLocationDescription = "Темна печера з покладами цінного заліза та каменю. Лігво небезпечного печерного ведмедя.";
            state.CurrentLocationStrokeHex = "#7E57C2";
            state.CurrentLocationTitleHex = "#B39DDB";
            state.CurrentLocationBgHex = "#221B2E";
        }
    }

    private async void OnMainFarmClicked(object sender, EventArgs e)
    {
        await StartFarmingProcess("lucky_scavenge");
    }

    private async void OnTargetedGatherClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string targetType)
        {
            await StartFarmingProcess(targetType);
        }
    }

    private async Task StartFarmingProcess(string mode)
    {
        if (_isFarming || GameState.Instance.IsInCombat)
            return;

        var state = GameState.Instance;
        bool isLuckyMode = mode == "lucky_scavenge";

        int energyCost = isLuckyMode ? 7 : 15;
        int hungerCost = isLuckyMode ? 4 : 8;
        int thirstCost = isLuckyMode ? 5 : 9;
        int durationSeconds = isLuckyMode ? Math.Max(2, state.FarmDurationSeconds / 2) : state.FarmDurationSeconds;

        if (state.Energy < energyCost || state.Hunger < hungerCost || state.Thirst < thirstCost)
        {
            ExplorationLogLabel.Text = $"⚠️ Занадто мало сил або води! Потрібно: {energyCost}⚡, {hungerCost}🍖, {thirstCost}💧.";
            return;
        }

        _isFarming = true;
        SetButtonsEnabled(false);

        int totalSteps = durationSeconds * 10;
        string actionTitle = isLuckyMode ? "Швидка розвідка локації" : "Цільовий видобуток";
        ExplorationLogLabel.Text = $"⏳ {actionTitle} у локації «{state.CurrentLocation}»...";

        for (int step = 1; step <= totalSteps; step++)
        {
            await Task.Delay(100);
            double progress = (double)step / totalSteps;
            double remainingSeconds = Math.Max(0, (totalSteps - step) / 10.0);

            FarmProgressBar.Progress = progress;
            FarmPercentLabel.Text = $"{(int)(progress * 100)}%";
            FarmTimerLabel.Text = $"{actionTitle}... Залишилося: {remainingSeconds:0.0} сек";
        }

        state.ChangeStats(hpDelta: 0, hungerDelta: -hungerCost, thirstDelta: -thirstCost, energyDelta: -energyCost);

        string rewardMessage = isLuckyMode ? GiveLuckyScavengeLoot() : GiveTargetedLoot(mode);

        FarmTimerLabel.Text = "Дію завершено!";
        FarmProgressBar.Progress = 0;
        FarmPercentLabel.Text = "0%";
        ExplorationLogLabel.Text = $"✅ Отримано: {rewardMessage} (-{energyCost}⚡, -{hungerCost}🍖, -{thirstCost}💧)";

        _isFarming = false;
        SetButtonsEnabled(true);
        UpdateFarmLabels();

        // 30% шанс випадкової зустрічі з ворогом поза базою
        if (!state.CurrentLocation.Contains("Лісовий табір") && _random.NextDouble() < 0.30)
        {
            TriggerCombat();
        }
    }

    private string GiveLuckyScavengeLoot()
    {
        var state = GameState.Instance;
        string loc = state.CurrentLocation;
        var gained = new List<string>();

        if (loc.Contains("Густий ліс"))
        {
            int wood = _random.Next(2, 5);
            state.AddItem("wood", "🪵", "Деревина", "Базовий матеріал для крафту та вогнища", wood);
            gained.Add($"🪵 Деревина x{wood}");
        }
        else if (loc.Contains("Піщана пустеля"))
        {
            int stone = _random.Next(2, 4);
            state.AddItem("stone", "🪨", "Камінь", "Використовується для створення інструментів", stone);
            gained.Add($"🪨 Камінь x{stone}");
        }
        else if (loc.Contains("Покинуті руїни"))
        {
            int cloth = _random.Next(1, 3);
            state.AddItem("cloth", "🧵", "Тканина", "Матеріал для бинтів та рюкзака", cloth);
            gained.Add($"🧵 Тканина x{cloth}");
        }
        else if (loc.Contains("Гірська печера"))
        {
            int iron = _random.Next(1, 3);
            state.AddItem("iron", "⛓️", "Залізна руда", "Міцна руда для зброї та інструментів", iron);
            gained.Add($"⛓️ Залізна руда x{iron}");
        }
        else
        {
            int wood = _random.Next(1, 3);
            state.AddItem("wood", "🪵", "Деревина", "Базовий матеріал", wood);
            gained.Add($"🪵 Деревина x{wood}");
        }

        // 25% шанс на схованку
        if (_random.NextDouble() < 0.25)
        {
            state.AddItem("water", "💧", "Фляга з водою", "Втамовує спрагу (+40)", 1, ItemType.Drink, 40);
            gained.Add("🎁 СХОВАНКА: 💧 Фляга з водою x1");
        }

        return string.Join(", ", gained);
    }

    private string GiveTargetedLoot(string mode)
    {
        var state = GameState.Instance;
        string loc = state.CurrentLocation;
        var gained = new List<string>();

        if (mode == "wood")
        {
            if (loc.Contains("Гірська печера") || loc.Contains("Піщана пустеля"))
            {
                state.AddItem("wood", "🪵", "Деревина", "Сухий хмиз", 1);
                return "🪵 Сухий хмиз x1 (Дерев тут немає)";
            }
            int bonus = loc.Contains("Густий ліс") ? 3 : 0;
            int count = _random.Next(4, 7) + bonus;
            state.AddItem("wood", "🪵", "Деревина", "Базовий матеріал", count);
            return $"🪵 Деревина x{count}";
        }
        if (mode == "stone")
        {
            int stoneCount = _random.Next(3, 6);
            state.AddItem("stone", "🪨", "Камінь", "Матеріал для крафту", stoneCount);
            gained.Add($"🪨 Камінь x{stoneCount}");
            if (loc.Contains("Гірська печера") || _random.NextDouble() < 0.4)
            {
                int ironCount = _random.Next(1, 3);
                state.AddItem("iron", "⛓️", "Залізна руда", "Руда для зброї", ironCount);
                gained.Add($"⛓️ Залізна руда x{ironCount}");
            }
            return string.Join(", ", gained);
        }
        if (mode == "food")
        {
            int berries = _random.Next(3, 6);
            state.AddItem("berries", "🍓", "Лісові ягоди", "Відновлюють +15 ситості та +10 спраги", berries, ItemType.Food, 15);
            gained.Add($"🍓 Ягоди x{berries}");

            if (_random.NextDouble() < 0.5)
            {
                state.AddItem("water", "💧", "Фляга з водою", "Втамовує спрагу (+40)", 1, ItemType.Drink, 40);
                gained.Add("💧 Вода x1");
            }
            return string.Join(", ", gained);
        }

        // mode == "loot"
        int clothCount = _random.Next(2, 4);
        state.AddItem("cloth", "🧵", "Тканина", "Матеріал для крафту", clothCount);
        gained.Add($"🧵 Тканина x{clothCount}");
        if (loc.Contains("Покинуті руїни") && _random.NextDouble() < 0.5)
        {
            state.AddItem("canned_food", "🥫", "Армійські консерви", "Відновлює +35 ситості", 1, ItemType.Food, 35);
            gained.Add("🥫 Консерви x1");
        }
        return string.Join(", ", gained);
    }

    // --- БОЙОВА СИСТЕМА ---
    private void OnHuntClicked(object sender, EventArgs e)
    {
        if (_isFarming || GameState.Instance.IsInCombat) return;

        if (GameState.Instance.CurrentLocation.Contains("Лісовий табір"))
        {
            ExplorationLogLabel.Text = "ℹ️ У таборі безпечно! Перейдіть у Ліс, Пустелю, Руїни або Печеру, щоб знайти ворогів.";
            return;
        }

        TriggerCombat();
    }

    private void TriggerCombat()
    {
        var state = GameState.Instance;
        string loc = state.CurrentLocation;

        Enemy enemy;
        if (loc.Contains("Густий ліс"))
            enemy = new Enemy { Name = "Дикий вовк", Icon = "🐺", Hp = 35, MaxHp = 35, AttackPower = 7, DropItemId = "raw_meat", DropCount = 2 };
        else if (loc.Contains("Піщана пустеля"))
            enemy = new Enemy { Name = "Гігантський скорпіон", Icon = "🦂", Hp = 45, MaxHp = 45, AttackPower = 10, DropItemId = "cloth", DropCount = 3 };
        else if (loc.Contains("Покинуті руїни"))
            enemy = new Enemy { Name = "Озброєний мародер", Icon = "🥷", Hp = 55, MaxHp = 55, AttackPower = 12, DropItemId = "medkit", DropCount = 1 };
        else // Гірська печера
            enemy = new Enemy { Name = "Печерний ведмідь", Icon = "🐻", Hp = 70, MaxHp = 70, AttackPower = 15, DropItemId = "iron", DropCount = 3 };

        state.CurrentEnemy = enemy;
        CheckCombatState();
        CombatLogLabel.Text = $"⚠️ Увага! На вас напав {enemy.Icon} {enemy.Name}!";
    }

    private void CheckCombatState()
    {
        var enemy = GameState.Instance.CurrentEnemy;
        if (enemy != null)
        {
            CombatBorder.IsVisible = true;
            EnemyNameLabel.Text = $"{enemy.Icon} {enemy.Name}";
            EnemyDamageLabel.Text = $"Шкода ворога: {enemy.AttackPower}";
            EnemyHpLabel.Text = $"{enemy.Hp} / {enemy.MaxHp}";
            EnemyHpBar.Progress = (double)enemy.Hp / enemy.MaxHp;
            SetButtonsEnabled(false);
        }
        else
        {
            CombatBorder.IsVisible = false;
            SetButtonsEnabled(true);
        }
    }

    private void OnAttackEnemyClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        var enemy = state.CurrentEnemy;
        if (enemy == null) return;

        // Гравець завдає шкоди
        int playerDamage = state.TotalAttackDamage + _random.Next(-2, 3);
        playerDamage = Math.Max(1, playerDamage);
        enemy.Hp = Math.Max(0, enemy.Hp - playerDamage);

        if (enemy.Hp <= 0)
        {
            // Перемога!
            string rewardText = GiveEnemyDrop(enemy);
            CombatLogLabel.Text = $"🏆 Перемога над {enemy.Name}! Ви завдали {playerDamage} шкоди. Отримано: {rewardText}";
            state.CurrentEnemy = null;
            CheckCombatState();
            return;
        }

        // Ворог б'є у відповідь
        int enemyDamage = enemy.AttackPower + _random.Next(-2, 3);
        enemyDamage = Math.Max(1, enemyDamage);
        state.ChangeStats(hpDelta: -enemyDamage);

        EnemyHpLabel.Text = $"{enemy.Hp} / {enemy.MaxHp}";
        EnemyHpBar.Progress = (double)enemy.Hp / enemy.MaxHp;

        CombatLogLabel.Text = $"⚔️ Ви вдарили на {playerDamage} шкоди! Ворог відповів ударом на {enemyDamage} HP.";

        if (state.Hp <= 35 && state.CurrentLocation.Contains("Лісовий табір"))
        {
            CombatBorder.IsVisible = false;
            ExplorationLogLabel.Text = "💀 Ви зазнали поразки в бою та прокинулися в таборі знесиленим!";
        }
    }

    private string GiveEnemyDrop(Enemy enemy)
    {
        var state = GameState.Instance;
        if (enemy.DropItemId == "raw_meat")
        {
            state.AddItem("raw_meat", "🥩", "Сире м'ясо", "Поживне м'ясо дичини. Можна підсмажити на багатті", enemy.DropCount);
            return $"🥩 Сире м'ясо x{enemy.DropCount}";
        }
        if (enemy.DropItemId == "cloth")
        {
            state.AddItem("cloth", "🧵", "Тканина", "Матеріал для крафту", enemy.DropCount);
            return $"🧵 Тканина x{enemy.DropCount}";
        }
        if (enemy.DropItemId == "medkit")
        {
            state.AddItem("medkit", "🧰", "Аптечка", "Відновлює здоров'я", enemy.DropCount, ItemType.Medical, 45);
            return $"🧰 Аптечка x{enemy.DropCount}";
        }
        if (enemy.DropItemId == "iron")
        {
            state.AddItem("iron", "⛓️", "Залізна руда", "Цінна руда", enemy.DropCount);
            return $"⛓️ Залізна руда x{enemy.DropCount}";
        }
        return "Нічого";
    }

    private void OnCombatHealClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        var med = state.Items.FirstOrDefault(i => i.Type == ItemType.Medical);
        if (med == null)
        {
            CombatLogLabel.Text = "❌ У вас немає бинтів або аптечок!";
            return;
        }

        state.ChangeStats(hpDelta: med.RestoreValue);
        state.RemoveItem(med.Id, 1);
        CombatLogLabel.Text = $"🩹 Ви використали {med.RawName} (+{med.RestoreValue} HP)!";
    }

    private void OnFleeClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.Energy < 15)
        {
            CombatLogLabel.Text = "❌ Замало енергії для втечі (потрібно 15⚡)!";
            return;
        }

        state.ChangeStats(energyDelta: -15);
        state.CurrentEnemy = null;
        CheckCombatState();
        ExplorationLogLabel.Text = "🏃 Ви успішно втекли від ворога у безпечне місце (-15⚡ Енергії).";
    }

    private void SetButtonsEnabled(bool isEnabled)
    {
        MainFarmButton.IsEnabled = isEnabled;
        HuntButton.IsEnabled = isEnabled;
        BtnGatherWood.IsEnabled = isEnabled;
        BtnGatherStone.IsEnabled = isEnabled;
        BtnGatherFood.IsEnabled = isEnabled;
        BtnGatherLoot.IsEnabled = isEnabled;
        BtnCamp.IsEnabled = isEnabled;
        BtnForest.IsEnabled = isEnabled;
        BtnDesert.IsEnabled = isEnabled;
        BtnRuins.IsEnabled = isEnabled;
        BtnCave.IsEnabled = isEnabled;
    }
}