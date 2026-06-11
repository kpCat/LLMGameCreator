# WinForms Designer Rules

Этот документ фиксирует обязательный стиль UI-кода для `LLMGameCreator.WinForms`.

## Главный принцип

Весь визуальный код WinForms-форм и UserControl должен быть вынесен в `InitializeComponent()` в отдельный `.Designer.cs` partial-файл.

Файл `.cs` содержит только:

- зависимости, полученные через DI;
- constructor flow;
- подписки на события;
- обработчики событий;
- загрузку/обновление данных;
- бизнес-логику presentation-слоя;
- вызовы Application/Runtime сервисов.

Файл `.Designer.cs` содержит:

- поля визуальных контролов;
- создание контролов;
- layout;
- Dock/Anchor/Size/Location/Text/Name;
- ColumnHeader/ColumnStyle/RowStyle;
- `SuspendLayout`/`ResumeLayout`;
- `Dispose(bool disposing)` для Form, если нужен `components`.

## Запрещено

Не создавать визуальные контролы напрямую в constructor/body основного `.cs` файла:

```csharp
public MyPageControl()
{
    var button = new Button(); // нельзя
    Controls.Add(button);      // нельзя
}
```

Не смешивать в одном методе:

- создание UI;
- чтение файлов;
- обращение к LLM;
- runtime execution;
- генерацию данных.

## Допустимо

В `.cs` можно подписывать события после `InitializeComponent()`:

```csharp
public ProjectsPageControl(ICurrentGamePackageService currentGamePackageService)
{
    _currentGamePackageService = currentGamePackageService;
    InitializeComponent();

    _browseButton.Click += (_, _) => BrowseFolder();
    _loadButton.Click += async (_, _) => await LoadSelectedFolderAsync();
}
```

В `.Designer.cs` можно оставлять простые статичные свойства контролов.

## Исключения

Custom-drawing controls, например map canvas, могут содержать rendering/input logic в основном `.cs`, но их базовые WinForms-свойства всё равно должны жить в `InitializeComponent()`.

Пример:

- `RuntimeMapCanvas.cs` — `OnPaint`, `OnKeyDown`, runtime input mapping;
- `RuntimeMapCanvas.Designer.cs` — `BackColor`, `TabStop`, `Size`, `DoubleBuffered`.

## Почему это важно

- Visual Studio Designer сможет открыть форму/контрол.
- UI становится читаемым.
- Codex меньше путает layout и бизнес-логику.
- Будущие правки дизайна можно делать руками через дизайнер.
- MainForm и страницы не превращаются в god-controls.
