// SPDX-FileCopyrightText: 2024 Milon <milonpl.git@proton.me>
// SPDX-FileCopyrightText: 2025 BlitzTheSquishy <73762869+BlitzTheSquishy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 Tobias Berger <toby@tobot.dev>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._DV.CartridgeLoader.Cartridges;
using Content.Shared.PDA;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.NanoChat;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedNanoChatSystem))]
[AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class NanoChatCardComponent : Component
{
    /// <summary>
    ///     The number assigned to this card.
    /// </summary>
    [DataField, AutoNetworkedField]
    public uint? Number;

    /// <summary>
    ///     All chat recipients stored on this card.
    /// </summary>
    [DataField]
    public Dictionary<uint, NanoChatRecipient> Recipients = new();

    /// <summary>
    ///     All messages stored on this card, keyed by recipient number.
    /// </summary>
    [DataField]
    public Dictionary<uint, List<NanoChatMessage>> Messages = new();

    /// <summary>
    ///     The currently selected chat recipient number.
    /// </summary>
    [DataField]
    public uint? CurrentChat;

    /// <summary>
    ///     The maximum amount of recipients this card supports.
    /// </summary>
    [DataField]
    public int MaxRecipients = 50;

    /// <summary>
    ///     Last time a message was sent, for rate limiting.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan LastMessageTime; // TODO: actually use this, compare against actor and not the card

    /// <summary>
    ///     Whether to send notifications.
    /// </summary>
    [DataField]
    public bool NotificationsMuted;

    /// <summary>
    ///     The PDA that this card is currently inserted to.
    /// </summary>
    [DataField]
    public EntityUid? PdaUid = null;

    ///     Whether the card's number should be listed in NanoChat's lookup
    /// </summary>
    [DataField]
    public bool ListNumber = true;
}
