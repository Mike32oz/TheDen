// SPDX-FileCopyrightText: 2023 Darkie <darksaiyanis@gmail.com>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Item.ItemToggle.Components;

/// <summary>
/// Handles generic item toggles, like a welder turning on and off, or an e-sword.
/// </summary>
/// <remarks>
/// If you need extended functionality (e.g. requiring power) then add a new component and use events:
/// ItemToggleActivateAttemptEvent, ItemToggleDeactivateAttemptEvent, ItemToggledEvent.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ItemToggleComponent : Component
{
    /// <summary>
    ///     The item's toggle state.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Activated = false;

    /// <summary>
    /// Can the entity be activated in the world.
    /// </summary>
    [DataField]
    public bool OnActivate = true;

    /// <summary>
    /// If this is set to false then the item can't be toggled by pressing Z.
    /// Use another system to do it then.
    /// </summary>
    [DataField]
    public bool OnUse = true;

    /// <summary>
    /// Goobstation
    /// Don't toggle on wielding/unwielding if false
    /// </summary>
    [DataField]
    public bool WieldToggle = true;

    /// <summary>
    ///     Whether the item's toggle can be predicted by the client.
    /// </summary>
    /// /// <remarks>
    /// If server-side systems affect the item's toggle, like charge/fuel systems, then the item is not predictable.
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool Predictable = true;

    /// <summary>
    ///     The noise this item makes when it is toggled on.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? SoundActivate;

    /// <summary>
    ///     The noise this item makes when it is toggled off.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? SoundDeactivate;

    /// <summary>
    ///     The noise this item makes when it is toggled on.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? SoundFailToActivate;
}

/// <summary>
/// Raised directed on an entity when its ItemToggle is attempted to be activated.
/// </summary>
[ByRefEvent]
public record struct ItemToggleActivateAttemptEvent(EntityUid? User)
{
    public bool Cancelled = false;
    public readonly EntityUid? User = User;

    /// <summary>
    /// Pop-up that gets shown to users explaining why the attempt was cancelled.
    /// </summary>
    public string? Popup { get; set; }
}

/// <summary>
/// Raised directed on an entity when its ItemToggle is attempted to be deactivated.
/// </summary>
[ByRefEvent]
public record struct ItemToggleDeactivateAttemptEvent(EntityUid? User)
{
    public bool Cancelled = false;
    public readonly EntityUid? User = User;
}

/// <summary>
/// Raised directed on an entity any sort of toggle is complete.
/// </summary>
[ByRefEvent]
public readonly record struct ItemToggledEvent(bool Predicted, bool Activated, EntityUid? User)
{
    public readonly bool Predicted = Predicted;
    public readonly bool Activated = Activated;
    public readonly EntityUid? User = User;
}
