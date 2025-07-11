// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Emp;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Clothing.Components;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Content.Shared.RadialSelector;
using Content.Shared.StatusEffect;
using Content.Shared.Verbs;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult.Spells;

public sealed class BloodCultSpellsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly EmpSystem _empSystem = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BaseCultSpellComponent, ComponentStartup>(OnSpellStartup);
        SubscribeLocalEvent<BaseCultSpellComponent, EntityTargetActionEvent>(OnCultTargetEvent);
        SubscribeLocalEvent<BaseCultSpellComponent, ActionGettingDisabledEvent>(OnActionGettingDisabled);

        SubscribeLocalEvent<BloodCultSpellsHolderComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<BloodCultSpellsHolderComponent, GetVerbsEvent<ExamineVerb>>(OnGetVerbs);
        SubscribeLocalEvent<BloodCultSpellsHolderComponent, RadialSelectorSelectedMessage>(OnSpellSelected);
        SubscribeLocalEvent<BloodCultSpellsHolderComponent, CreateSpeellDoAfterEvent>(OnSpellCreated);

        SubscribeLocalEvent<BloodCultEmpEvent>(OnEmp);
        SubscribeLocalEvent<SummonEquipmentEvent>(OnSummonEquipment);
    }

    #region BaseHandlers

    private void OnSpellStartup(Entity<BaseCultSpellComponent> action, ref ComponentStartup args)
    {
        if (_actions.TryGetActionData(action, out var actionData, false) && actionData is { UseDelay: not null })
            _actions.StartUseDelay(action);
    }

    private void OnCultTargetEvent(Entity<BaseCultSpellComponent> spell, ref EntityTargetActionEvent args)
    {
        if (_statusEffects.HasStatusEffect(args.Performer, "Muted"))
        {
            args.Handled = true;
            return;
        }

        if (spell.Comp.BypassProtection)
            return;

        if (HasComp<MindShieldComponent>(args.Target) || HasComp<PsionicInsulationComponent>(args.Target))
            args.Handled = true;
    }

    private void OnActionGettingDisabled(Entity<BaseCultSpellComponent> spell, ref ActionGettingDisabledEvent args)
    {
        if (TryComp(args.Performer, out BloodCultSpellsHolderComponent? spellsHolder))
            spellsHolder.SelectedSpells.Remove(spell);

        _actions.RemoveAction(args.Performer, spell);
    }

    private void OnComponentStartup(Entity<BloodCultSpellsHolderComponent> cultist, ref ComponentStartup args) =>
        cultist.Comp.MaxSpells = cultist.Comp.DefaultMaxSpells;

    private void OnGetVerbs(Entity<BloodCultSpellsHolderComponent> cultist, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (args.User != args.Target)
            return;

        var addVerb = new ExamineVerb
        {
            Category = VerbCategory.BloodSpells,
            Text = Loc.GetString("blood-cult-select-spells-verb"),
            Priority = 1,
            Act = () => SelectBloodSpells(cultist)
        };
        var removeVerb = new ExamineVerb
        {
            Category = VerbCategory.BloodSpells,
            Text = Loc.GetString("blood-cult-remove-spells-verb"),
            Priority = 0,
            Act = () => RemoveBloodSpells(cultist)
        };

        args.Verbs.Add(removeVerb);
        args.Verbs.Add(addVerb);
    }

    private void OnSpellSelected(Entity<BloodCultSpellsHolderComponent> cultist, ref RadialSelectorSelectedMessage args)
    {
        if (!cultist.Comp.AddSpellsMode)
        {
            if (EntityUid.TryParse(args.SelectedItem, out var actionUid))
            {
                _actions.RemoveAction(cultist, actionUid);
                cultist.Comp.SelectedSpells.Remove(actionUid);
            }

            return;
        }

        if (cultist.Comp.SelectedSpells.Count >= cultist.Comp.MaxSpells)
        {
            _popup.PopupEntity(Loc.GetString("blood-cult-spells-too-many"), cultist, cultist, PopupType.Medium);
            return;
        }

        var createSpellEvent = new CreateSpeellDoAfterEvent
        {
            ActionProtoId = args.SelectedItem
        };

        var doAfter = new DoAfterArgs(
            EntityManager,
            cultist.Owner,
            cultist.Comp.SpellCreationTime,
            createSpellEvent,
            cultist.Owner)
        {
            BreakOnMove = true
        };

        if (_doAfter.TryStartDoAfter(doAfter, out var doAfterId))
            cultist.Comp.DoAfterId = doAfterId;
    }

    private void OnSpellCreated(Entity<BloodCultSpellsHolderComponent> cultist, ref CreateSpeellDoAfterEvent args)
    {
        cultist.Comp.DoAfterId = null;
        if (args.Handled || args.Cancelled)
            return;

        var action = _actions.AddAction(cultist, args.ActionProtoId);
        if (action.HasValue)
            cultist.Comp.SelectedSpells.Add(action.Value);
    }

    #endregion

    #region SpellsHandlers

    private void OnEmp(BloodCultEmpEvent ev)
    {
        if (ev.Handled)
            return;

        _empSystem.EmpPulse(_transform.GetMapCoordinates(ev.Performer), ev.Range, ev.EnergyConsumption, ev.Duration);
        ev.Handled = true;
    }

    private void OnSummonEquipment(SummonEquipmentEvent ev)
    {
        if (ev.Handled)
            return;

        foreach (var (slot, protoId) in ev.Prototypes)
        {
            var entity = Spawn(protoId, _transform.GetMapCoordinates(ev.Performer));
            if (!_hands.TryPickupAnyHand(ev.Performer, entity) && !ev.Force)
            {
                _popup.PopupEntity(Loc.GetString("cult-magic-no-empty-hand"), ev.Performer, ev.Performer);
                _actions.SetCooldown(ev.Action, TimeSpan.FromSeconds(1));
                QueueDel(entity);
                return;
            }

            if (!TryComp(entity, out ClothingComponent? _))
                continue;

            _inventory.TryUnequip(ev.Performer, slot);
            _inventory.TryEquip(ev.Performer, entity, slot, force: true);
        }

        ev.Handled = true;
    }

    #endregion

    #region Helpers

    private void SelectBloodSpells(Entity<BloodCultSpellsHolderComponent> cultist)
    {
        if (!_proto.TryIndex(cultist.Comp.PowersPoolPrototype, out var pool))
            return;

        if (cultist.Comp.SelectedSpells.Count >= cultist.Comp.MaxSpells)
        {
            _popup.PopupEntity(Loc.GetString("blood-cult-spells-too-many"), cultist, cultist, PopupType.Medium);
            return;
        }

        cultist.Comp.AddSpellsMode = true;

        var radialList = new List<RadialSelectorEntry>();
        foreach (var spellId in pool.Powers)
        {
            var entry = new RadialSelectorEntry
            {
                Prototype = spellId
            };

            radialList.Add(entry);
        }

        var state = new RadialSelectorState(radialList, true);

        _ui.SetUiState(cultist.Owner, RadialSelectorUiKey.Key, state);
        _ui.TryToggleUi(cultist.Owner, RadialSelectorUiKey.Key, cultist.Owner);
    }

    private void RemoveBloodSpells(Entity<BloodCultSpellsHolderComponent> cultist)
    {
        if (cultist.Comp.SelectedSpells.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("blood-cult-no-spells"), cultist, cultist, PopupType.Medium);
            return;
        }

        cultist.Comp.AddSpellsMode = false;

        var radialList = new List<RadialSelectorEntry>();
        foreach (var spell in cultist.Comp.SelectedSpells)
        {
            var entry = new RadialSelectorEntry
            {
                Prototype = spell.ToString(),
                Icon = GetActionIcon(spell)
            };

            radialList.Add(entry);
        }

        var state = new RadialSelectorState(radialList, true);

        _ui.SetUiState(cultist.Owner, RadialSelectorUiKey.Key, state);
        _ui.TryToggleUi(cultist.Owner, RadialSelectorUiKey.Key, cultist.Owner);
    }

    private SpriteSpecifier? GetActionIcon(EntityUid actionUid)
    {
        if (TryComp(actionUid, out EntityTargetActionComponent? targetAction))
            return targetAction.Icon;
        if (TryComp(actionUid, out WorldTargetActionComponent? worldTargetAction))
            return worldTargetAction.Icon;
        if (TryComp(actionUid, out InstantActionComponent? instantActionComponent))
            return instantActionComponent.Icon;

        return null;
    }

    #endregion
}
