using System.Collections.Generic;
using Embervale.Core;
using Embervale.Core.Events;
using Embervale.Localization;
using Embervale.Quests;
using Godot;

namespace Embervale.UI;

/// <summary>
/// The quest journal: a read-only overlay toggled with the <c>journal</c> action (J), built on
/// the 30.5F <see cref="UiPanel"/> framework. Unlike the character screen it is non-modal — it
/// neither captures the mouse nor sets <c>UiState.MenuOpen</c>, so it can be left up while
/// playing. It lists active quests with per-objective progress and a completed section.
/// </summary>
public partial class QuestLogPanel : UiPanel
{
    private QuestLogComponent? _log;
    private VBoxContainer _list = null!;

    protected override bool Modal => false;

    protected override string? ToggleAction => GameInput.Journal;

    protected override void BuildShell(PanelContainer shell)
    {
        // Top-left, below the HUD's clock/weather widget (30.5B placement sweep).
        shell.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        shell.OffsetLeft = 16;
        shell.OffsetTop = 64;
        shell.CustomMinimumSize = new Vector2(360, 0);

        MarginContainer margin = UiTheme.Padding(12);
        shell.AddChild(margin);

        _list = new VBoxContainer();
        _list.AddThemeConstantOverride("separation", 3);
        margin.AddChild(_list);
    }

    protected override void OnReady()
    {
        EventBus.Instance?.Subscribe<QuestStartedEvent>(OnQuestStarted);
        EventBus.Instance?.Subscribe<QuestObjectiveAdvancedEvent>(OnObjectiveAdvanced);
        EventBus.Instance?.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
        EventBus.Instance?.Subscribe<GameLoadedEvent>(OnGameLoaded);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<QuestStartedEvent>(OnQuestStarted);
        EventBus.Instance?.Unsubscribe<QuestObjectiveAdvancedEvent>(OnObjectiveAdvanced);
        EventBus.Instance?.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
        EventBus.Instance?.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
    }

    public void SetQuestLog(QuestLogComponent? log)
    {
        _log = log;
        MarkDirty();
    }

    private void OnQuestStarted(QuestStartedEvent e) => MarkDirty();

    private void OnObjectiveAdvanced(QuestObjectiveAdvancedEvent e) => MarkDirty();

    private void OnQuestCompleted(QuestCompletedEvent e) => MarkDirty();

    private void OnGameLoaded(GameLoadedEvent e) => MarkDirty();

    protected override void Rebuild()
    {
        UiTheme.ClearChildren(_list);

        AddHeader(Loc.T("questlog.title"));

        if (_log == null || _log.Quests.Count == 0)
        {
            AddLine(Loc.T("questlog.none"));
            return;
        }

        bool anyActive = false;
        foreach (QuestProgress progress in _log.Quests)
        {
            if (progress.Status == QuestStatus.Active)
            {
                anyActive = true;
                BuildQuest(progress);
            }
        }

        if (!anyActive)
        {
            AddLine(Loc.T("questlog.no_active"));
        }

        bool completedHeader = false;
        foreach (QuestProgress progress in _log.Quests)
        {
            if (progress.Status != QuestStatus.Completed)
            {
                continue;
            }

            if (!completedHeader)
            {
                AddHeader(Loc.T("questlog.completed"));
                completedHeader = true;
            }

            AddLine($"✓ {Loc.T(progress.Quest.Title)}", UiTheme.Good);
        }
    }

    private void BuildQuest(QuestProgress progress)
    {
        AddLine(Loc.T(progress.Quest.Title), UiTheme.Accent);

        List<ObjectiveResource> objectives = progress.Quest.ObjectiveList();
        for (int i = 0; i < objectives.Count; i++)
        {
            ObjectiveResource objective = objectives[i];
            bool done = progress.IsObjectiveComplete(i);
            string mark = done ? "✓" : "•";
            AddLine($"   {mark} {Loc.T(objective.ShortLabel())}  {progress.Counts[i]}/{objective.RequiredCount}",
                done ? UiTheme.Good : UiTheme.Text);
        }
    }

    private void AddHeader(string text)
    {
        _list.AddChild(UiTheme.Header(text));
    }

    private void AddLine(string text, Color? color = null)
    {
        _list.AddChild(UiTheme.Body(text, color));
    }
}
