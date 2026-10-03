using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

/// <summary>
/// Screen that has a list of monsters that you can click on to view their name, description, hp, some stats, and
/// a list of their moves which you can click on to play the associated animation and sfx.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Bestiary/NBestiary.cs")]
public class NBestiary : NSubmenu
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NSubmenu.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'ToggleMode' method.
		/// </summary>
		public static readonly StringName ToggleMode = "ToggleMode";

		/// <summary>
		/// Cached name for the 'EnableMoveButtonHotkeys' method.
		/// </summary>
		public static readonly StringName EnableMoveButtonHotkeys = "EnableMoveButtonHotkeys";

		/// <summary>
		/// Cached name for the 'DisableMoveButtonHotkeys' method.
		/// </summary>
		public static readonly StringName DisableMoveButtonHotkeys = "DisableMoveButtonHotkeys";

		/// <summary>
		/// Cached name for the 'ShowMovesPanel' method.
		/// </summary>
		public static readonly StringName ShowMovesPanel = "ShowMovesPanel";

		/// <summary>
		/// Cached name for the 'ShowStatsPanel' method.
		/// </summary>
		public static readonly StringName ShowStatsPanel = "ShowStatsPanel";

		/// <summary>
		/// Cached name for the 'RefreshStatisticsText' method.
		/// </summary>
		public static readonly StringName RefreshStatisticsText = "RefreshStatisticsText";

		/// <summary>
		/// Cached name for the 'DisplayCharacterData' method.
		/// </summary>
		public static readonly StringName DisplayCharacterData = "DisplayCharacterData";

		/// <summary>
		/// Cached name for the 'UpdateDialogueBubbleStyle' method.
		/// </summary>
		public static readonly StringName UpdateDialogueBubbleStyle = "UpdateDialogueBubbleStyle";

		/// <summary>
		/// Cached name for the 'ShowDialogue' method.
		/// </summary>
		public static readonly StringName ShowDialogue = "ShowDialogue";

		/// <summary>
		/// Cached name for the 'HideDialogue' method.
		/// </summary>
		public static readonly StringName HideDialogue = "HideDialogue";

		/// <summary>
		/// Cached name for the 'OnSubmenuOpened' method.
		/// </summary>
		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		/// <summary>
		/// Cached name for the 'OnSubmenuClosed' method.
		/// </summary>
		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		/// <summary>
		/// Cached name for the 'CreateEntries' method.
		/// </summary>
		public static readonly StringName CreateEntries = "CreateEntries";

		/// <summary>
		/// Cached name for the 'CreateFilters' method.
		/// </summary>
		public static readonly StringName CreateFilters = "CreateFilters";

		/// <summary>
		/// Cached name for the 'OnCharacterFilterSelected' method.
		/// </summary>
		public static readonly StringName OnCharacterFilterSelected = "OnCharacterFilterSelected";

		/// <summary>
		/// Cached name for the 'AddEvents' method.
		/// </summary>
		public static readonly StringName AddEvents = "AddEvents";

		/// <summary>
		/// Cached name for the 'OnMonsterClicked' method.
		/// </summary>
		public static readonly StringName OnMonsterClicked = "OnMonsterClicked";

		/// <summary>
		/// Cached name for the 'SelectMonster' method.
		/// </summary>
		public static readonly StringName SelectMonster = "SelectMonster";

		/// <summary>
		/// Cached name for the 'OnMoveButtonClicked' method.
		/// </summary>
		public static readonly StringName OnMoveButtonClicked = "OnMoveButtonClicked";

		/// <summary>
		/// Cached name for the 'GetSideCenter' method.
		/// </summary>
		public static readonly StringName GetSideCenter = "GetSideCenter";

		/// <summary>
		/// Cached name for the 'GetSideFloor' method.
		/// </summary>
		public static readonly StringName GetSideFloor = "GetSideFloor";

		/// <summary>
		/// Cached name for the 'EnableStatsModeHotkeys' method.
		/// </summary>
		public static readonly StringName EnableStatsModeHotkeys = "EnableStatsModeHotkeys";

		/// <summary>
		/// Cached name for the 'DisableStatsModeHotkeys' method.
		/// </summary>
		public static readonly StringName DisableStatsModeHotkeys = "DisableStatsModeHotkeys";

		/// <summary>
		/// Cached name for the 'FilterLeft' method.
		/// </summary>
		public static readonly StringName FilterLeft = "FilterLeft";

		/// <summary>
		/// Cached name for the 'FilterRight' method.
		/// </summary>
		public static readonly StringName FilterRight = "FilterRight";

		/// <summary>
		/// Cached name for the 'SelectFilter' method.
		/// </summary>
		public static readonly StringName SelectFilter = "SelectFilter";

		/// <summary>
		/// Cached name for the 'UpdatePageIcons' method.
		/// </summary>
		public static readonly StringName UpdatePageIcons = "UpdatePageIcons";

		/// <summary>
		/// Cached name for the 'CanBeShown' method.
		/// </summary>
		public static readonly StringName CanBeShown = "CanBeShown";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NSubmenu.PropertyName
	{
		/// <summary>
		/// Cached name for the 'InitialFocusedControl' property.
		/// </summary>
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		/// <summary>
		/// Cached name for the 'BackVfxContainer' property.
		/// </summary>
		public static readonly StringName BackVfxContainer = "BackVfxContainer";

		/// <summary>
		/// Cached name for the 'VfxContainer' property.
		/// </summary>
		public static readonly StringName VfxContainer = "VfxContainer";

		/// <summary>
		/// Cached name for the 'Layout' property.
		/// </summary>
		public static readonly StringName Layout = "Layout";

		/// <summary>
		/// Cached name for the '_monsterNameLabel' field.
		/// </summary>
		public static readonly StringName _monsterNameLabel = "_monsterNameLabel";

		/// <summary>
		/// Cached name for the '_epithet' field.
		/// </summary>
		public static readonly StringName _epithet = "_epithet";

		/// <summary>
		/// Cached name for the '_sidebar' field.
		/// </summary>
		public static readonly StringName _sidebar = "_sidebar";

		/// <summary>
		/// Cached name for the '_bestiaryList' field.
		/// </summary>
		public static readonly StringName _bestiaryList = "_bestiaryList";

		/// <summary>
		/// Cached name for the '_selectionArrow' field.
		/// </summary>
		public static readonly StringName _selectionArrow = "_selectionArrow";

		/// <summary>
		/// Cached name for the '_arrowTween' field.
		/// </summary>
		public static readonly StringName _arrowTween = "_arrowTween";

		/// <summary>
		/// Cached name for the '_initSelectionArrow' field.
		/// </summary>
		public static readonly StringName _initSelectionArrow = "_initSelectionArrow";

		/// <summary>
		/// Cached name for the '_layoutContainer' field.
		/// </summary>
		public static readonly StringName _layoutContainer = "_layoutContainer";

		/// <summary>
		/// Cached name for the '_currentLayout' field.
		/// </summary>
		public static readonly StringName _currentLayout = "_currentLayout";

		/// <summary>
		/// Cached name for the '_characterIcon' field.
		/// </summary>
		public static readonly StringName _characterIcon = "_characterIcon";

		/// <summary>
		/// Cached name for the '_iconTexture' field.
		/// </summary>
		public static readonly StringName _iconTexture = "_iconTexture";

		/// <summary>
		/// Cached name for the '_iconOutlineTexture' field.
		/// </summary>
		public static readonly StringName _iconOutlineTexture = "_iconOutlineTexture";

		/// <summary>
		/// Cached name for the '_dialogueLine' field.
		/// </summary>
		public static readonly StringName _dialogueLine = "_dialogueLine";

		/// <summary>
		/// Cached name for the '_dialogueLabel' field.
		/// </summary>
		public static readonly StringName _dialogueLabel = "_dialogueLabel";

		/// <summary>
		/// Cached name for the '_dialogueBubble' field.
		/// </summary>
		public static readonly StringName _dialogueBubble = "_dialogueBubble";

		/// <summary>
		/// Cached name for the '_dialogueTail' field.
		/// </summary>
		public static readonly StringName _dialogueTail = "_dialogueTail";

		/// <summary>
		/// Cached name for the '_dialogueTailShadow' field.
		/// </summary>
		public static readonly StringName _dialogueTailShadow = "_dialogueTailShadow";

		/// <summary>
		/// Cached name for the '_modeButton' field.
		/// </summary>
		public static readonly StringName _modeButton = "_modeButton";

		/// <summary>
		/// Cached name for the '_isStatsMode' field.
		/// </summary>
		public static readonly StringName _isStatsMode = "_isStatsMode";

		/// <summary>
		/// Cached name for the '_pageLeftIcon' field.
		/// </summary>
		public static readonly StringName _pageLeftIcon = "_pageLeftIcon";

		/// <summary>
		/// Cached name for the '_pageRightIcon' field.
		/// </summary>
		public static readonly StringName _pageRightIcon = "_pageRightIcon";

		/// <summary>
		/// Cached name for the '_moveList' field.
		/// </summary>
		public static readonly StringName _moveList = "_moveList";

		/// <summary>
		/// Cached name for the '_moveContainer' field.
		/// </summary>
		public static readonly StringName _moveContainer = "_moveContainer";

		/// <summary>
		/// Cached name for the '_statsContainer' field.
		/// </summary>
		public static readonly StringName _statsContainer = "_statsContainer";

		/// <summary>
		/// Cached name for the '_filterContainer' field.
		/// </summary>
		public static readonly StringName _filterContainer = "_filterContainer";

		/// <summary>
		/// Cached name for the '_statsLabel' field.
		/// </summary>
		public static readonly StringName _statsLabel = "_statsLabel";

		/// <summary>
		/// Cached name for the '_currentFilter' field.
		/// </summary>
		public static readonly StringName _currentFilter = "_currentFilter";

		/// <summary>
		/// Cached name for the '_selectedEntry' field.
		/// </summary>
		public static readonly StringName _selectedEntry = "_selectedEntry";

		/// <summary>
		/// Cached name for the '_previousScreenshakeTarget' field.
		/// </summary>
		public static readonly StringName _previousScreenshakeTarget = "_previousScreenshakeTarget";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_dialogueTween' field.
		/// </summary>
		public static readonly StringName _dialogueTween = "_dialogueTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/bestiary/bestiary");

	private SerializableProgress _progress;

	private MegaRichTextLabel _monsterNameLabel;

	private MegaLabel _epithet;

	private NScrollableContainer _sidebar;

	private VBoxContainer _bestiaryList;

	private static readonly LocString _locked = new LocString("bestiary", "LOCKED.monsterTitle");

	private Control _selectionArrow;

	private Tween? _arrowTween;

	private static readonly Vector2 _arrowOffset = new Vector2(-42f, 119f);

	private bool _initSelectionArrow = true;

	private Control _layoutContainer;

	private NBestiaryLayout? _currentLayout;

	private Control _characterIcon;

	private TextureRect _iconTexture;

	private TextureRect _iconOutlineTexture;

	private Control _dialogueLine;

	private MegaRichTextLabel _dialogueLabel;

	private Control _dialogueBubble;

	private TextureRect _dialogueTail;

	private TextureRect _dialogueTailShadow;

	private const string _dialogueTailPath = "res://images/ui/dialogue_tail.png";

	private const string _thoughtTailPath = "res://images/ui/thought_tail.png";

	private NBestiaryModeButton _modeButton;

	private bool _isStatsMode;

	private NHotkeyIcon _pageLeftIcon;

	private NHotkeyIcon _pageRightIcon;

	private static readonly StringName _filterLeftHotkey = MegaInput.viewDeckAndTabLeft;

	private static readonly StringName _filterRightHotkey = MegaInput.viewExhaustPileAndTabRight;

	private Control _moveList;

	private Control _moveContainer;

	private Control _statsContainer;

	private Control _filterContainer;

	private MegaRichTextLabel _statsLabel;

	private NBestiaryCharacterFilter _currentFilter;

	private HashSet<ModelId> _discoveredMonsterIds;

	private HashSet<ModelId> _discoveredEncounterIds;

	private NBestiaryEntry? _selectedEntry;

	private Control? _previousScreenshakeTarget;

	private Tween? _tween;

	private Tween? _dialogueTween;

	public static NBestiary? Instance { get; private set; }

	public static string[] AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.Add(_scenePath);
			list.AddRange(NBestiaryEntry.AssetPaths);
			return list.ToArray();
		}
	}

	protected override Control? InitialFocusedControl => _bestiaryList.GetChildren().OfType<NBestiaryEntry>().FirstOrDefault();

	public Control BackVfxContainer { get; private set; }

	public Control VfxContainer { get; private set; }

	public Control? Layout => _currentLayout;

	public static NBestiary? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NBestiary>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		GetNode<MegaLabel>("%MoveHeader").SetTextAutoSize(new LocString("bestiary", "ACTIONS.header").GetFormattedText());
		GetNode<MegaRichTextLabel>("%ConstructionLabel").SetTextAutoSize(new LocString("bestiary", "UNDER_CONSTRUCTION").GetRawText());
		_sidebar = GetNode<NScrollableContainer>("%Sidebar");
		_bestiaryList = GetNode<VBoxContainer>("%BestiaryList");
		_monsterNameLabel = GetNode<MegaRichTextLabel>("%MonsterName");
		_layoutContainer = GetNode<Control>("%LayoutContainer");
		_epithet = GetNode<MegaLabel>("%Epithet");
		_characterIcon = GetNode<Control>("%CharacterIcon");
		_iconTexture = GetNode<TextureRect>("%Icon");
		_iconOutlineTexture = GetNode<TextureRect>("%Outline");
		_dialogueLine = GetNode<Control>("%DialogueLine");
		_dialogueLabel = GetNode<MegaRichTextLabel>("%DialogueText");
		_dialogueBubble = GetNode<Control>("%Bubble");
		_dialogueTail = GetNode<TextureRect>("%DialogueTail");
		_dialogueTailShadow = GetNode<TextureRect>("%DialogueTailShadow");
		_modeButton = GetNode<NBestiaryModeButton>("%ModeButton");
		_modeButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ToggleMode));
		_pageLeftIcon = GetNode<NHotkeyIcon>("%PageLeftIcon");
		_pageRightIcon = GetNode<NHotkeyIcon>("%PageRightIcon");
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdatePageIcons));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdatePageIcons));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdatePageIcons));
		_moveContainer = GetNode<Control>("%MoveContainer");
		_statsContainer = GetNode<Control>("%StatsContainer");
		_selectionArrow = GetNode<Control>("%SelectionArrow");
		_moveList = GetNode<Control>("%MoveList");
		_filterContainer = GetNode<Control>("%PoolFilters");
		_statsLabel = GetNode<MegaRichTextLabel>("%StatisticsFull");
		VfxContainer = GetNode<Control>("%VfxContainer");
		BackVfxContainer = GetNode<Control>("%BackVfxContainer");
	}

	private void ToggleMode(NButton _)
	{
		_isStatsMode = !_isStatsMode;
		if (_isStatsMode)
		{
			_modeButton.SetLabel(new LocString("bestiary", "MODE.viewActions").GetRawText());
			ShowStatsPanel();
			DisplayCharacterData();
			EnableStatsModeHotkeys();
			DisableMoveButtonHotkeys();
		}
		else
		{
			_modeButton.SetLabel(new LocString("bestiary", "MODE.viewStats").GetRawText());
			ShowMovesPanel();
			HideDialogue();
			DisableStatsModeHotkeys();
			EnableMoveButtonHotkeys();
		}
		UpdatePageIcons();
	}

	private void EnableMoveButtonHotkeys()
	{
		foreach (Node child in _moveList.GetChildren())
		{
			((NBestiaryMoveButton)child).Enable();
		}
	}

	/// <summary>
	/// We need to disable these hotkeys when we go to Stats mode because otherwise
	/// paginating through the characters will make the monster perform actions.
	/// </summary>
	private void DisableMoveButtonHotkeys()
	{
		foreach (Node child in _moveList.GetChildren())
		{
			((NBestiaryMoveButton)child).Disable();
		}
	}

	private void ShowMovesPanel()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_statsContainer.Visible = false;
		_moveContainer.Visible = true;
		Control moveContainer = _moveContainer;
		Color modulate = base.Modulate;
		modulate.A = 0f;
		moveContainer.Modulate = modulate;
		_tween.TweenProperty(_moveContainer, "position:x", 242f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(210f);
		_tween.TweenProperty(_moveContainer, "modulate:a", 1f, 0.5);
	}

	private void ShowStatsPanel()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_moveContainer.Visible = false;
		_statsContainer.Visible = true;
		Control statsContainer = _statsContainer;
		Color modulate = base.Modulate;
		modulate.A = 0f;
		statsContainer.Modulate = modulate;
		_tween.TweenProperty(_statsContainer, "position:x", 242f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(210f);
		_tween.TweenProperty(_statsContainer, "modulate:a", 1f, 0.5);
	}

	private void RefreshStatisticsText()
	{
		if (_selectedEntry == null)
		{
			Log.Error("How did this happen?");
			return;
		}
		BestiaryEntry entry = _selectedEntry.Entry;
		EnemyStats enemyStats = null;
		EncounterStats encounterStats = null;
		if (entry.monsterModel != null)
		{
			enemyStats = _progress.EnemyStats.FirstOrDefault((EnemyStats e) => e.Id == entry.monsterModel.Id);
		}
		else
		{
			encounterStats = _progress.EncounterStats.FirstOrDefault((EncounterStats e) => e.Id == entry.encounterModel.Id);
		}
		foreach (Node child in _filterContainer.GetChildren())
		{
			NBestiaryCharacterFilter filter = child as NBestiaryCharacterFilter;
			if (filter == null)
			{
				continue;
			}
			if (enemyStats != null)
			{
				if (filter.character != null)
				{
					FightStats fightStats = enemyStats.FightStats.FirstOrDefault((FightStats f) => f.Character == filter.character.Id);
					filter.kills = fightStats?.Wins ?? 0;
					filter.deaths = fightStats?.Losses ?? 0;
				}
				else
				{
					filter.kills = enemyStats.TotalWins;
					filter.deaths = enemyStats.TotalLosses;
				}
			}
			else if (encounterStats != null)
			{
				if (filter.character != null)
				{
					FightStats fightStats2 = encounterStats.FightStats.FirstOrDefault((FightStats f) => f.Character == filter.character.Id);
					filter.kills = fightStats2?.Wins ?? 0;
					filter.deaths = fightStats2?.Losses ?? 0;
				}
				else
				{
					filter.kills = encounterStats.TotalWins;
					filter.deaths = encounterStats.TotalLosses;
				}
			}
			filter.IsLocked = filter.kills + filter.deaths <= 0;
		}
	}

	private void DisplayCharacterData()
	{
		if (_selectedEntry == null)
		{
			Log.Error("How did this happen?");
			return;
		}
		BestiaryEntry entry = _selectedEntry.Entry;
		LocString locString = new LocString("bestiary", "STATS.layout");
		if (_currentFilter.Total == 0)
		{
			locString.Add("total", 0m);
			locString.Add("kills", 0m);
			locString.Add("deaths", 0m);
			locString.Add("winrate", "--");
		}
		else
		{
			locString.Add("total", _currentFilter.Total);
			locString.Add("kills", _currentFilter.kills);
			locString.Add("deaths", _currentFilter.deaths);
			locString.Add("winrate", _currentFilter.WinRate);
		}
		_statsLabel.SetTextAutoSize(locString.GetFormattedText());
		if (_currentFilter.kills <= 0)
		{
			_dialogueLabel.SetTextAutoSize(_currentFilter.BestiarySeenQuote);
		}
		else
		{
			LocString bestiaryKillQuote = _currentFilter.BestiaryKillQuote;
			if (bestiaryKillQuote == null)
			{
				_dialogueLabel.SetTextAutoSize(new LocString("bestiary", "QUOTE_PLACEHOLDER").GetFormattedText());
			}
			else
			{
				_dialogueLabel.SetTextAutoSize(bestiaryKillQuote.GetFormattedText());
			}
		}
		if (_currentFilter.character == null)
		{
			HideDialogue();
		}
		else
		{
			ShowDialogue();
		}
		UpdateDialogueBubbleStyle();
	}

	private void UpdateDialogueBubbleStyle()
	{
		CharacterModel character = _currentFilter.character;
		Color selfModulate = character?.DialogueColor ?? StsColors.transparentWhite;
		_characterIcon.Modulate = ((character == null) ? StsColors.transparentWhite : Colors.White);
		if (character != null)
		{
			_iconTexture.Texture = character.IconTexture;
			_iconOutlineTexture.Texture = character.IconOutlineTexture;
		}
		_dialogueBubble.SelfModulate = selfModulate;
		_dialogueTail.SelfModulate = selfModulate;
		string path = ((character is Silent) ? "res://images/ui/thought_tail.png" : "res://images/ui/dialogue_tail.png");
		_dialogueTail.Texture = PreloadManager.Cache.GetCompressedTexture2D(path);
	}

	private void ShowDialogue()
	{
		_dialogueTween?.Kill();
		_dialogueTween = CreateTween().SetParallel();
		_dialogueLine.Modulate = StsColors.transparentWhite;
		_dialogueTween.TweenProperty(_dialogueLine, "modulate", Colors.White, 0.1).SetDelay(0.1);
		_dialogueTween.TweenProperty(_dialogueLine, "position:x", 560f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(528f)
			.SetDelay(0.1);
	}

	private void HideDialogue()
	{
		_dialogueTween?.Kill();
		_dialogueTween = CreateTween();
		_dialogueTween.TweenProperty(_dialogueLine, "modulate", StsColors.transparentWhite, 0.1);
	}

	/// <summary>
	/// On screen open. When the player opens the Bestiary.
	/// </summary>
	public override void OnSubmenuOpened()
	{
		Instance = this;
		_progress = SaveManager.Instance.Progress.ToSerializable();
		_previousScreenshakeTarget = NGame.Instance?.ScreenshakeTarget;
		_isStatsMode = !SaveManager.Instance.PrefsSave.IsBestiaryActionsPreferred;
		if (_isStatsMode)
		{
			_modeButton.SetLabel(new LocString("bestiary", "MODE.viewActions").GetRawText());
		}
		else
		{
			_modeButton.SetLabel(new LocString("bestiary", "MODE.viewStats").GetRawText());
		}
		CreateFilters();
		CreateEntries();
		if (_isStatsMode)
		{
			DisplayCharacterData();
			EnableStatsModeHotkeys();
			DisableMoveButtonHotkeys();
		}
		UpdatePageIcons();
	}

	/// <summary>
	/// Called when the Bestiary is closed (Back button)
	/// </summary>
	public override void OnSubmenuClosed()
	{
		DisableStatsModeHotkeys();
		DisableMoveButtonHotkeys();
		_initSelectionArrow = true;
		_selectedEntry = null;
		Instance = null;
		SaveManager.Instance.PrefsSave.IsBestiaryActionsPreferred = !_isStatsMode;
		SaveManager.Instance.SavePrefsFile();
		_currentLayout?.Cleanup();
		if (_previousScreenshakeTarget != null)
		{
			if (_previousScreenshakeTarget.IsValid())
			{
				NGame.Instance?.SetScreenShakeTarget(_previousScreenshakeTarget);
			}
			else
			{
				Log.Warn("The screenshake target is no longer valid. This should never happen.");
				_previousScreenshakeTarget = null;
			}
		}
		else
		{
			NGame.Instance?.ClearScreenShakeTarget();
		}
		_bestiaryList.FreeChildren();
		_lastFocusedControl = null;
	}

	/// <summary>
	/// Initializes the list of monsters based on your save file.
	/// </summary>
	private void CreateEntries()
	{
		_discoveredMonsterIds = (from e in SaveManager.Instance.Progress.EnemyStats.Values
			where e.TotalWins > 0
			select e.Id).ToHashSet();
		_discoveredEncounterIds = (from e in SaveManager.Instance.Progress.EncounterStats.Values
			where e.TotalWins > 0
			select e.Id).ToHashSet();
		foreach (ActModel act in ModelDb.Acts)
		{
			AddAct(act);
		}
		AddEvents();
		Control node = _sidebar.GetNode<Control>("Content");
		Vector2 position = node.Position;
		position.Y = 0f;
		node.Position = position;
		_sidebar.InstantlyScrollToTop();
		NBestiaryEntry nBestiaryEntry = _bestiaryList.GetChildren().OfType<NBestiaryEntry>().FirstOrDefault((NBestiaryEntry e) => e.IsDiscovered && e.IsEnabled);
		if (nBestiaryEntry == null)
		{
			Log.Error("Should not be possible as the Compendium + Bestiary isn't unlocked by default!");
		}
		else
		{
			SelectMonster(nBestiaryEntry);
		}
	}

	private void CreateFilters()
	{
		_filterContainer.FreeChildren();
		AddFilter(null);
		AddFilter(ModelDb.Character<Ironclad>());
		AddFilter(ModelDb.Character<Silent>());
		AddFilter(ModelDb.Character<Regent>());
		AddFilter(ModelDb.Character<Necrobinder>());
		AddFilter(ModelDb.Character<Defect>());
	}

	private void AddFilter(CharacterModel? character)
	{
		NBestiaryCharacterFilter nBestiaryCharacterFilter = NBestiaryCharacterFilter.Create(character);
		nBestiaryCharacterFilter.Connect(NBestiaryCharacterFilter.SignalName.Toggled, Callable.From<NBestiaryCharacterFilter>(OnCharacterFilterSelected));
		_filterContainer.AddChildSafely(nBestiaryCharacterFilter);
		if (character == null)
		{
			_currentFilter = nBestiaryCharacterFilter;
			_currentFilter.IsSelected = true;
		}
	}

	private void OnCharacterFilterSelected(NBestiaryCharacterFilter selectedFilter)
	{
		_currentFilter = selectedFilter;
		foreach (Node child in _filterContainer.GetChildren())
		{
			if (!child.Equals(selectedFilter))
			{
				((NBestiaryCharacterFilter)child).Deselect();
			}
		}
		if (_isStatsMode)
		{
			DisplayCharacterData();
		}
	}

	private void AddAct(ActModel act)
	{
		if (!SaveManager.Instance.Progress.DiscoveredActs.Contains(act.Id))
		{
			return;
		}
		_bestiaryList.AddChildSafely(NBestiaryLabelDivider.Create(act));
		HashSet<ModelId> hashSet = new HashSet<ModelId>();
		List<BestiaryEntry> list = new List<BestiaryEntry>();
		foreach (EncounterModel allEncounter in act.AllEncounters)
		{
			foreach (MonsterModel allPossibleMonster in allEncounter.AllPossibleMonsters)
			{
				if (hashSet.Add(allPossibleMonster.Id) && allPossibleMonster.ShouldShowInCompendium)
				{
					list.Add(BestiaryEntry.FromMonster(allPossibleMonster, allEncounter, allEncounter.RoomType));
				}
			}
		}
		if (act is Hive)
		{
			list.Add(BestiaryEntry.FromEncounter(ModelDb.Encounter<DecimillipedeElite>(), RoomType.Elite));
		}
		AddEntries(list);
	}

	private void AddEvents()
	{
		_bestiaryList.AddChildSafely(NBestiaryLabelDivider.Create(new LocString("bestiary", "EVENTS.title")));
		HashSet<ModelId> hashSet = new HashSet<ModelId>();
		List<BestiaryEntry> list = new List<BestiaryEntry>();
		foreach (EncounterModel eventEncounter in ModelDb.EventEncounters)
		{
			foreach (MonsterModel allPossibleMonster in eventEncounter.AllPossibleMonsters)
			{
				if (hashSet.Add(allPossibleMonster.Id) && allPossibleMonster.ShouldShowInCompendium)
				{
					list.Add(BestiaryEntry.FromMonster(allPossibleMonster, eventEncounter, eventEncounter.RoomType));
				}
			}
		}
		AddEntries(list);
	}

	private void AddEntries(List<BestiaryEntry> entries)
	{
		entries.Sort(delegate(BestiaryEntry e1, BestiaryEntry e2)
		{
			if (e1.roomType != e2.roomType)
			{
				return e1.roomType.CompareTo(e2.roomType);
			}
			if (e1.roomType == RoomType.Boss)
			{
				int num = string.Compare(e1.GetEncounterTitle(), e2.GetEncounterTitle(), StringComparison.CurrentCulture);
				if (num != 0)
				{
					return num;
				}
			}
			return string.Compare(e1.GetEntryTitle(), e2.GetEntryTitle(), StringComparison.CurrentCulture);
		});
		foreach (BestiaryEntry entry in entries)
		{
			NBestiaryEntry nBestiaryEntry = NBestiaryEntry.Create(entry, entry.IsDiscovered(_discoveredMonsterIds, _discoveredEncounterIds));
			_bestiaryList.AddChildSafely(nBestiaryEntry);
			nBestiaryEntry.Connect(NClickableControl.SignalName.Released, Callable.From<NBestiaryEntry>(OnMonsterClicked));
		}
	}

	/// <summary>
	/// A player clicked on a monster in the list on the right.
	/// </summary>
	private void OnMonsterClicked(NBestiaryEntry entry)
	{
		SelectMonster(entry);
	}

	/// <summary>
	/// Loads a specific monster's bestiary entry.
	/// </summary>
	private void SelectMonster(NBestiaryEntry entry)
	{
		if (entry == _selectedEntry)
		{
			return;
		}
		_moveList.FreeChildren();
		_selectedEntry = entry;
		if (entry.IsUnderConstruction)
		{
			_monsterNameLabel.Text = entry.Entry.GetEntryTitle();
			_currentLayout?.Cleanup();
			_currentLayout?.QueueFreeSafely();
		}
		else if (!entry.IsDiscovered)
		{
			_monsterNameLabel.Text = _locked.GetFormattedText();
			_currentLayout?.Cleanup();
			_currentLayout?.QueueFreeSafely();
		}
		else
		{
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_monsterNameLabel.Text = entry.Entry.GetEntryTitle();
			_monsterNameLabel.SelfModulate = StsColors.transparentWhite;
			_epithet.Modulate = StsColors.transparentWhite;
			_tween.TweenProperty(_monsterNameLabel, "position:y", 88f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(24f);
			_tween.TweenProperty(_monsterNameLabel, "self_modulate:a", 1f, 0.5);
			_tween.TweenProperty(_epithet, "modulate:a", 1f, 0.5).SetDelay(0.2);
			_tween.TweenProperty(_dialogueLabel, "position:y", 894f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(958f);
			_tween.TweenProperty(_dialogueLabel, "modulate:a", 1f, 0.5);
			if (_isStatsMode)
			{
				ShowStatsPanel();
			}
			else
			{
				ShowMovesPanel();
			}
			RefreshStatisticsText();
			_currentLayout?.Cleanup();
			if (!entry.Entry.CanReuseLayout(_currentLayout))
			{
				_currentLayout?.QueueFreeSafely();
				_currentLayout = entry.Entry.CreateLayoutNode(this);
				_layoutContainer.AddChildSafely(_currentLayout);
				NGame.Instance?.SetScreenShakeTarget(_currentLayout);
			}
			List<BestiaryMonsterMove> list = _currentLayout.Setup(entry.Entry, _tween);
			for (int i = 0; i < list.Count; i++)
			{
				if (i >= 9)
				{
					Log.Error("Hotkeys for monster Actions beyond 9 are not supported!");
				}
				NBestiaryMoveButton nBestiaryMoveButton = CreateBestiaryMoveButton(list[i], i + 1);
				_moveList.AddChildSafely(nBestiaryMoveButton);
				nBestiaryMoveButton.Connect(NClickableControl.SignalName.Released, Callable.From<NBestiaryMoveButton>(OnMoveButtonClicked));
			}
			if (_isStatsMode)
			{
				DisableMoveButtonHotkeys();
				if (_currentFilter.IsLocked)
				{
					NBestiaryCharacterFilter child = _filterContainer.GetChild<NBestiaryCharacterFilter>(0);
					child.IsSelected = true;
					OnCharacterFilterSelected(child);
				}
				DisplayCharacterData();
			}
		}
		if (_initSelectionArrow)
		{
			Control selectionArrow = _selectionArrow;
			Color modulate = _selectionArrow.Modulate;
			modulate.A = 0f;
			selectionArrow.Modulate = modulate;
			_initSelectionArrow = false;
			TaskHelper.RunSafely(InitializeSelectorArrow(entry));
		}
		else
		{
			Control selectionArrow2 = _selectionArrow;
			Color modulate = _selectionArrow.Modulate;
			modulate.A = 1f;
			selectionArrow2.Modulate = modulate;
			_arrowTween?.Kill();
			_arrowTween = CreateTween().SetParallel();
			_arrowTween.TweenProperty(_selectionArrow, "position", entry.Position + _arrowOffset, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	private NBestiaryMoveButton CreateBestiaryMoveButton(BestiaryMonsterMove move, int moveIndex)
	{
		if (NControllerManager.Instance?.IsUsingDirectionalNavigation ?? false)
		{
			return moveIndex switch
			{
				1 => NBestiaryMoveButton.Create(move, MegaInput.viewDeckAndTabLeft), 
				2 => NBestiaryMoveButton.Create(move, MegaInput.viewExhaustPileAndTabRight), 
				3 => NBestiaryMoveButton.Create(move, MegaInput.viewDiscardPile), 
				4 => NBestiaryMoveButton.Create(move, MegaInput.viewDrawPile), 
				5 => NBestiaryMoveButton.Create(move, MegaInput.topPanel), 
				6 => NBestiaryMoveButton.Create(move, MegaInput.altUp), 
				7 => NBestiaryMoveButton.Create(move, MegaInput.altDown), 
				8 => NBestiaryMoveButton.Create(move, MegaInput.altLeft), 
				9 => NBestiaryMoveButton.Create(move, MegaInput.altRight), 
				_ => NBestiaryMoveButton.Create(move, $"{moveIndex}"), 
			};
		}
		return NBestiaryMoveButton.Create(move, $"mega_select_card_{moveIndex}");
	}

	private async Task InitializeSelectorArrow(NBestiaryEntry entry)
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_selectionArrow.Position = entry.Position + _arrowOffset;
		_arrowTween?.Kill();
		_arrowTween = CreateTween().SetParallel();
		_arrowTween.TweenProperty(_selectionArrow, "modulate:a", 1f, 0.2);
	}

	private void OnMoveButtonClicked(NButton button)
	{
		NBestiaryMoveButton nBestiaryMoveButton = (NBestiaryMoveButton)button;
		PlayMoveAnim(_currentLayout?.GetCreatures() ?? Array.Empty<NCreature>(), nBestiaryMoveButton.Move);
	}

	private static void PlayMoveAnim(IEnumerable<NCreature> creatures, BestiaryMonsterMove move)
	{
		foreach (NCreature creature in creatures)
		{
			if (move.stateId != null)
			{
				MonsterModel monster = creature.Entity.Monster;
				if (monster == null)
				{
					throw new InvalidOperationException($"Non-monster creature {creature} is in the bestiary!");
				}
				monster.SetMoveImmediate((MoveState)monster.MoveStateMachine.States[move.stateId], forceTransition: true);
				TaskHelper.RunSafely(monster.PerformMove());
			}
			else if (move.nonStateMove != null)
			{
				TaskHelper.RunSafely(move.nonStateMove(Array.Empty<Creature>()));
			}
			else if (move.action != null)
			{
				TaskHelper.RunSafely(move.action());
			}
			else if (move.animId != null)
			{
				creature.Visuals.SpineBody?.GetAnimationState().SetAnimation(move.animId, loop: false);
				if (move.animId != "die")
				{
					creature.Visuals.SpineBody?.GetAnimationState().AddAnimation("idle_loop");
				}
				if (move.sfx != null)
				{
					NAudioManager.Instance.PlayOneShot(move.sfx);
				}
			}
			if (move.stopSfxLoops)
			{
				creature.StopAllSfxLoops();
			}
		}
	}

	public NCreature? GetCreatureNode(Creature? creature)
	{
		foreach (NCreature item in _currentLayout?.GetCreatures() ?? Array.Empty<NCreature>())
		{
			if (item.Entity == creature)
			{
				return item;
			}
		}
		return null;
	}

	public Vector2 GetSideCenter()
	{
		if (_currentLayout == null)
		{
			Log.Error("Tried to get current side center, but we're not showing anything!");
			return Vector2.Zero;
		}
		Vector2 zero = Vector2.Zero;
		int num = 0;
		foreach (NCreature creature in _currentLayout.GetCreatures())
		{
			zero += creature.VfxSpawnPosition;
			num++;
		}
		return zero / num;
	}

	public Vector2 GetSideFloor()
	{
		if (_currentLayout == null)
		{
			Log.Error("Tried to get current side floor, but we're not showing anything!");
			return Vector2.Zero;
		}
		Vector2 zero = Vector2.Zero;
		int num = 0;
		foreach (NCreature creature in _currentLayout.GetCreatures())
		{
			zero += creature.GetBottomOfHitbox();
			num++;
		}
		return zero / num;
	}

	private void EnableStatsModeHotkeys()
	{
		NHotkeyManager.Instance.PushHotkeyPressedBinding(_filterLeftHotkey, FilterLeft);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(_filterRightHotkey, FilterRight);
	}

	private void DisableStatsModeHotkeys()
	{
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(_filterLeftHotkey, FilterLeft);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(_filterRightHotkey, FilterRight);
	}

	private void FilterLeft()
	{
		List<NBestiaryCharacterFilter> list = _filterContainer.GetChildren().OfType<NBestiaryCharacterFilter>().ToList();
		int num = list.IndexOf(_currentFilter);
		for (int i = 1; i <= list.Count; i++)
		{
			int index = (num - i + list.Count) % list.Count;
			if (!list[index].IsLocked)
			{
				SelectFilter(list[index]);
				break;
			}
		}
	}

	private void FilterRight()
	{
		List<NBestiaryCharacterFilter> list = _filterContainer.GetChildren().OfType<NBestiaryCharacterFilter>().ToList();
		int num = list.IndexOf(_currentFilter);
		for (int i = 1; i <= list.Count; i++)
		{
			int index = (num + i) % list.Count;
			if (!list[index].IsLocked)
			{
				SelectFilter(list[index]);
				break;
			}
		}
	}

	private void SelectFilter(NBestiaryCharacterFilter filter)
	{
		filter.IsSelected = true;
		OnCharacterFilterSelected(filter);
	}

	private void UpdatePageIcons()
	{
		bool flag = _isStatsMode && NControllerManager.Instance.IsUsingDirectionalNavigation;
		_pageLeftIcon.Visible = flag;
		_pageRightIcon.Visible = flag;
		if (flag)
		{
			_pageLeftIcon.UpdateInput(MegaInput.viewDeckAndTabLeft);
			_pageRightIcon.UpdateInput(MegaInput.viewExhaustPileAndTabRight);
		}
	}

	public static bool CanBeShown()
	{
		if (SaveManager.Instance.Progress.DiscoveredActs.Count == 0)
		{
			return false;
		}
		return SaveManager.Instance.Progress.EnemyStats.Values.Any((EnemyStats e) => e.TotalWins > 0);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(30);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.EnableMoveButtonHotkeys, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableMoveButtonHotkeys, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowMovesPanel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowStatsPanel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshStatisticsText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisplayCharacterData, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateDialogueBubbleStyle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowDialogue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideDialogue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateEntries, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateFilters, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCharacterFilterSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "selectedFilter", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddEvents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMonsterClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SelectMonster, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMoveButtonClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetSideCenter, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetSideFloor, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnableStatsModeHotkeys, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableStatsModeHotkeys, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FilterLeft, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FilterRight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SelectFilter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "filter", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdatePageIcons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CanBeShown, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			NBestiary from = Create();
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleMode && args.Count == 1)
		{
			ToggleMode(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableMoveButtonHotkeys && args.Count == 0)
		{
			EnableMoveButtonHotkeys();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableMoveButtonHotkeys && args.Count == 0)
		{
			DisableMoveButtonHotkeys();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowMovesPanel && args.Count == 0)
		{
			ShowMovesPanel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowStatsPanel && args.Count == 0)
		{
			ShowStatsPanel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshStatisticsText && args.Count == 0)
		{
			RefreshStatisticsText();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisplayCharacterData && args.Count == 0)
		{
			DisplayCharacterData();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateDialogueBubbleStyle && args.Count == 0)
		{
			UpdateDialogueBubbleStyle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowDialogue && args.Count == 0)
		{
			ShowDialogue();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideDialogue && args.Count == 0)
		{
			HideDialogue();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateEntries && args.Count == 0)
		{
			CreateEntries();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateFilters && args.Count == 0)
		{
			CreateFilters();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCharacterFilterSelected && args.Count == 1)
		{
			OnCharacterFilterSelected(VariantUtils.ConvertTo<NBestiaryCharacterFilter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddEvents && args.Count == 0)
		{
			AddEvents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMonsterClicked && args.Count == 1)
		{
			OnMonsterClicked(VariantUtils.ConvertTo<NBestiaryEntry>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectMonster && args.Count == 1)
		{
			SelectMonster(VariantUtils.ConvertTo<NBestiaryEntry>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMoveButtonClicked && args.Count == 1)
		{
			OnMoveButtonClicked(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetSideCenter && args.Count == 0)
		{
			Vector2 from2 = GetSideCenter();
			ret = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (method == MethodName.GetSideFloor && args.Count == 0)
		{
			Vector2 from3 = GetSideFloor();
			ret = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (method == MethodName.EnableStatsModeHotkeys && args.Count == 0)
		{
			EnableStatsModeHotkeys();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableStatsModeHotkeys && args.Count == 0)
		{
			DisableStatsModeHotkeys();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FilterLeft && args.Count == 0)
		{
			FilterLeft();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FilterRight && args.Count == 0)
		{
			FilterRight();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectFilter && args.Count == 1)
		{
			SelectFilter(VariantUtils.ConvertTo<NBestiaryCharacterFilter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePageIcons && args.Count == 0)
		{
			UpdatePageIcons();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanBeShown && args.Count == 0)
		{
			bool from4 = CanBeShown();
			ret = VariantUtils.CreateFrom(in from4);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			NBestiary from = Create();
			ret = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (method == MethodName.CanBeShown && args.Count == 0)
		{
			bool from2 = CanBeShown();
			ret = VariantUtils.CreateFrom(in from2);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.ToggleMode)
		{
			return true;
		}
		if (method == MethodName.EnableMoveButtonHotkeys)
		{
			return true;
		}
		if (method == MethodName.DisableMoveButtonHotkeys)
		{
			return true;
		}
		if (method == MethodName.ShowMovesPanel)
		{
			return true;
		}
		if (method == MethodName.ShowStatsPanel)
		{
			return true;
		}
		if (method == MethodName.RefreshStatisticsText)
		{
			return true;
		}
		if (method == MethodName.DisplayCharacterData)
		{
			return true;
		}
		if (method == MethodName.UpdateDialogueBubbleStyle)
		{
			return true;
		}
		if (method == MethodName.ShowDialogue)
		{
			return true;
		}
		if (method == MethodName.HideDialogue)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		if (method == MethodName.CreateEntries)
		{
			return true;
		}
		if (method == MethodName.CreateFilters)
		{
			return true;
		}
		if (method == MethodName.OnCharacterFilterSelected)
		{
			return true;
		}
		if (method == MethodName.AddEvents)
		{
			return true;
		}
		if (method == MethodName.OnMonsterClicked)
		{
			return true;
		}
		if (method == MethodName.SelectMonster)
		{
			return true;
		}
		if (method == MethodName.OnMoveButtonClicked)
		{
			return true;
		}
		if (method == MethodName.GetSideCenter)
		{
			return true;
		}
		if (method == MethodName.GetSideFloor)
		{
			return true;
		}
		if (method == MethodName.EnableStatsModeHotkeys)
		{
			return true;
		}
		if (method == MethodName.DisableStatsModeHotkeys)
		{
			return true;
		}
		if (method == MethodName.FilterLeft)
		{
			return true;
		}
		if (method == MethodName.FilterRight)
		{
			return true;
		}
		if (method == MethodName.SelectFilter)
		{
			return true;
		}
		if (method == MethodName.UpdatePageIcons)
		{
			return true;
		}
		if (method == MethodName.CanBeShown)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.BackVfxContainer)
		{
			BackVfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.VfxContainer)
		{
			VfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._monsterNameLabel)
		{
			_monsterNameLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._epithet)
		{
			_epithet = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._sidebar)
		{
			_sidebar = VariantUtils.ConvertTo<NScrollableContainer>(in value);
			return true;
		}
		if (name == PropertyName._bestiaryList)
		{
			_bestiaryList = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._selectionArrow)
		{
			_selectionArrow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._arrowTween)
		{
			_arrowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._initSelectionArrow)
		{
			_initSelectionArrow = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._layoutContainer)
		{
			_layoutContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._currentLayout)
		{
			_currentLayout = VariantUtils.ConvertTo<NBestiaryLayout>(in value);
			return true;
		}
		if (name == PropertyName._characterIcon)
		{
			_characterIcon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._iconTexture)
		{
			_iconTexture = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._iconOutlineTexture)
		{
			_iconOutlineTexture = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._dialogueLine)
		{
			_dialogueLine = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._dialogueLabel)
		{
			_dialogueLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._dialogueBubble)
		{
			_dialogueBubble = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._dialogueTail)
		{
			_dialogueTail = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._dialogueTailShadow)
		{
			_dialogueTailShadow = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._modeButton)
		{
			_modeButton = VariantUtils.ConvertTo<NBestiaryModeButton>(in value);
			return true;
		}
		if (name == PropertyName._isStatsMode)
		{
			_isStatsMode = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._pageLeftIcon)
		{
			_pageLeftIcon = VariantUtils.ConvertTo<NHotkeyIcon>(in value);
			return true;
		}
		if (name == PropertyName._pageRightIcon)
		{
			_pageRightIcon = VariantUtils.ConvertTo<NHotkeyIcon>(in value);
			return true;
		}
		if (name == PropertyName._moveList)
		{
			_moveList = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._moveContainer)
		{
			_moveContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._statsContainer)
		{
			_statsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._filterContainer)
		{
			_filterContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._statsLabel)
		{
			_statsLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._currentFilter)
		{
			_currentFilter = VariantUtils.ConvertTo<NBestiaryCharacterFilter>(in value);
			return true;
		}
		if (name == PropertyName._selectedEntry)
		{
			_selectedEntry = VariantUtils.ConvertTo<NBestiaryEntry>(in value);
			return true;
		}
		if (name == PropertyName._previousScreenshakeTarget)
		{
			_previousScreenshakeTarget = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._dialogueTween)
		{
			_dialogueTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Control from;
		if (name == PropertyName.InitialFocusedControl)
		{
			from = InitialFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.BackVfxContainer)
		{
			from = BackVfxContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.VfxContainer)
		{
			from = VfxContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Layout)
		{
			from = Layout;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._monsterNameLabel)
		{
			value = VariantUtils.CreateFrom(in _monsterNameLabel);
			return true;
		}
		if (name == PropertyName._epithet)
		{
			value = VariantUtils.CreateFrom(in _epithet);
			return true;
		}
		if (name == PropertyName._sidebar)
		{
			value = VariantUtils.CreateFrom(in _sidebar);
			return true;
		}
		if (name == PropertyName._bestiaryList)
		{
			value = VariantUtils.CreateFrom(in _bestiaryList);
			return true;
		}
		if (name == PropertyName._selectionArrow)
		{
			value = VariantUtils.CreateFrom(in _selectionArrow);
			return true;
		}
		if (name == PropertyName._arrowTween)
		{
			value = VariantUtils.CreateFrom(in _arrowTween);
			return true;
		}
		if (name == PropertyName._initSelectionArrow)
		{
			value = VariantUtils.CreateFrom(in _initSelectionArrow);
			return true;
		}
		if (name == PropertyName._layoutContainer)
		{
			value = VariantUtils.CreateFrom(in _layoutContainer);
			return true;
		}
		if (name == PropertyName._currentLayout)
		{
			value = VariantUtils.CreateFrom(in _currentLayout);
			return true;
		}
		if (name == PropertyName._characterIcon)
		{
			value = VariantUtils.CreateFrom(in _characterIcon);
			return true;
		}
		if (name == PropertyName._iconTexture)
		{
			value = VariantUtils.CreateFrom(in _iconTexture);
			return true;
		}
		if (name == PropertyName._iconOutlineTexture)
		{
			value = VariantUtils.CreateFrom(in _iconOutlineTexture);
			return true;
		}
		if (name == PropertyName._dialogueLine)
		{
			value = VariantUtils.CreateFrom(in _dialogueLine);
			return true;
		}
		if (name == PropertyName._dialogueLabel)
		{
			value = VariantUtils.CreateFrom(in _dialogueLabel);
			return true;
		}
		if (name == PropertyName._dialogueBubble)
		{
			value = VariantUtils.CreateFrom(in _dialogueBubble);
			return true;
		}
		if (name == PropertyName._dialogueTail)
		{
			value = VariantUtils.CreateFrom(in _dialogueTail);
			return true;
		}
		if (name == PropertyName._dialogueTailShadow)
		{
			value = VariantUtils.CreateFrom(in _dialogueTailShadow);
			return true;
		}
		if (name == PropertyName._modeButton)
		{
			value = VariantUtils.CreateFrom(in _modeButton);
			return true;
		}
		if (name == PropertyName._isStatsMode)
		{
			value = VariantUtils.CreateFrom(in _isStatsMode);
			return true;
		}
		if (name == PropertyName._pageLeftIcon)
		{
			value = VariantUtils.CreateFrom(in _pageLeftIcon);
			return true;
		}
		if (name == PropertyName._pageRightIcon)
		{
			value = VariantUtils.CreateFrom(in _pageRightIcon);
			return true;
		}
		if (name == PropertyName._moveList)
		{
			value = VariantUtils.CreateFrom(in _moveList);
			return true;
		}
		if (name == PropertyName._moveContainer)
		{
			value = VariantUtils.CreateFrom(in _moveContainer);
			return true;
		}
		if (name == PropertyName._statsContainer)
		{
			value = VariantUtils.CreateFrom(in _statsContainer);
			return true;
		}
		if (name == PropertyName._filterContainer)
		{
			value = VariantUtils.CreateFrom(in _filterContainer);
			return true;
		}
		if (name == PropertyName._statsLabel)
		{
			value = VariantUtils.CreateFrom(in _statsLabel);
			return true;
		}
		if (name == PropertyName._currentFilter)
		{
			value = VariantUtils.CreateFrom(in _currentFilter);
			return true;
		}
		if (name == PropertyName._selectedEntry)
		{
			value = VariantUtils.CreateFrom(in _selectedEntry);
			return true;
		}
		if (name == PropertyName._previousScreenshakeTarget)
		{
			value = VariantUtils.CreateFrom(in _previousScreenshakeTarget);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._dialogueTween)
		{
			value = VariantUtils.CreateFrom(in _dialogueTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._monsterNameLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._epithet, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sidebar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bestiaryList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._initSelectionArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._layoutContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentLayout, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconOutlineTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogueLine, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogueLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogueBubble, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogueTail, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogueTailShadow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modeButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isStatsMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pageLeftIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pageRightIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._filterContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statsLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previousScreenshakeTarget, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogueTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.BackVfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Layout, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName backVfxContainer = PropertyName.BackVfxContainer;
		Control from = BackVfxContainer;
		info.AddProperty(backVfxContainer, Variant.From(in from));
		StringName vfxContainer = PropertyName.VfxContainer;
		from = VfxContainer;
		info.AddProperty(vfxContainer, Variant.From(in from));
		info.AddProperty(PropertyName._monsterNameLabel, Variant.From(in _monsterNameLabel));
		info.AddProperty(PropertyName._epithet, Variant.From(in _epithet));
		info.AddProperty(PropertyName._sidebar, Variant.From(in _sidebar));
		info.AddProperty(PropertyName._bestiaryList, Variant.From(in _bestiaryList));
		info.AddProperty(PropertyName._selectionArrow, Variant.From(in _selectionArrow));
		info.AddProperty(PropertyName._arrowTween, Variant.From(in _arrowTween));
		info.AddProperty(PropertyName._initSelectionArrow, Variant.From(in _initSelectionArrow));
		info.AddProperty(PropertyName._layoutContainer, Variant.From(in _layoutContainer));
		info.AddProperty(PropertyName._currentLayout, Variant.From(in _currentLayout));
		info.AddProperty(PropertyName._characterIcon, Variant.From(in _characterIcon));
		info.AddProperty(PropertyName._iconTexture, Variant.From(in _iconTexture));
		info.AddProperty(PropertyName._iconOutlineTexture, Variant.From(in _iconOutlineTexture));
		info.AddProperty(PropertyName._dialogueLine, Variant.From(in _dialogueLine));
		info.AddProperty(PropertyName._dialogueLabel, Variant.From(in _dialogueLabel));
		info.AddProperty(PropertyName._dialogueBubble, Variant.From(in _dialogueBubble));
		info.AddProperty(PropertyName._dialogueTail, Variant.From(in _dialogueTail));
		info.AddProperty(PropertyName._dialogueTailShadow, Variant.From(in _dialogueTailShadow));
		info.AddProperty(PropertyName._modeButton, Variant.From(in _modeButton));
		info.AddProperty(PropertyName._isStatsMode, Variant.From(in _isStatsMode));
		info.AddProperty(PropertyName._pageLeftIcon, Variant.From(in _pageLeftIcon));
		info.AddProperty(PropertyName._pageRightIcon, Variant.From(in _pageRightIcon));
		info.AddProperty(PropertyName._moveList, Variant.From(in _moveList));
		info.AddProperty(PropertyName._moveContainer, Variant.From(in _moveContainer));
		info.AddProperty(PropertyName._statsContainer, Variant.From(in _statsContainer));
		info.AddProperty(PropertyName._filterContainer, Variant.From(in _filterContainer));
		info.AddProperty(PropertyName._statsLabel, Variant.From(in _statsLabel));
		info.AddProperty(PropertyName._currentFilter, Variant.From(in _currentFilter));
		info.AddProperty(PropertyName._selectedEntry, Variant.From(in _selectedEntry));
		info.AddProperty(PropertyName._previousScreenshakeTarget, Variant.From(in _previousScreenshakeTarget));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._dialogueTween, Variant.From(in _dialogueTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.BackVfxContainer, out var value))
		{
			BackVfxContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.VfxContainer, out var value2))
		{
			VfxContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._monsterNameLabel, out var value3))
		{
			_monsterNameLabel = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._epithet, out var value4))
		{
			_epithet = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._sidebar, out var value5))
		{
			_sidebar = value5.As<NScrollableContainer>();
		}
		if (info.TryGetProperty(PropertyName._bestiaryList, out var value6))
		{
			_bestiaryList = value6.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._selectionArrow, out var value7))
		{
			_selectionArrow = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._arrowTween, out var value8))
		{
			_arrowTween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._initSelectionArrow, out var value9))
		{
			_initSelectionArrow = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._layoutContainer, out var value10))
		{
			_layoutContainer = value10.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._currentLayout, out var value11))
		{
			_currentLayout = value11.As<NBestiaryLayout>();
		}
		if (info.TryGetProperty(PropertyName._characterIcon, out var value12))
		{
			_characterIcon = value12.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._iconTexture, out var value13))
		{
			_iconTexture = value13.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._iconOutlineTexture, out var value14))
		{
			_iconOutlineTexture = value14.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._dialogueLine, out var value15))
		{
			_dialogueLine = value15.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._dialogueLabel, out var value16))
		{
			_dialogueLabel = value16.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._dialogueBubble, out var value17))
		{
			_dialogueBubble = value17.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._dialogueTail, out var value18))
		{
			_dialogueTail = value18.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._dialogueTailShadow, out var value19))
		{
			_dialogueTailShadow = value19.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._modeButton, out var value20))
		{
			_modeButton = value20.As<NBestiaryModeButton>();
		}
		if (info.TryGetProperty(PropertyName._isStatsMode, out var value21))
		{
			_isStatsMode = value21.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._pageLeftIcon, out var value22))
		{
			_pageLeftIcon = value22.As<NHotkeyIcon>();
		}
		if (info.TryGetProperty(PropertyName._pageRightIcon, out var value23))
		{
			_pageRightIcon = value23.As<NHotkeyIcon>();
		}
		if (info.TryGetProperty(PropertyName._moveList, out var value24))
		{
			_moveList = value24.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._moveContainer, out var value25))
		{
			_moveContainer = value25.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._statsContainer, out var value26))
		{
			_statsContainer = value26.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._filterContainer, out var value27))
		{
			_filterContainer = value27.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._statsLabel, out var value28))
		{
			_statsLabel = value28.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._currentFilter, out var value29))
		{
			_currentFilter = value29.As<NBestiaryCharacterFilter>();
		}
		if (info.TryGetProperty(PropertyName._selectedEntry, out var value30))
		{
			_selectedEntry = value30.As<NBestiaryEntry>();
		}
		if (info.TryGetProperty(PropertyName._previousScreenshakeTarget, out var value31))
		{
			_previousScreenshakeTarget = value31.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value32))
		{
			_tween = value32.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._dialogueTween, out var value33))
		{
			_dialogueTween = value33.As<Tween>();
		}
	}
}
