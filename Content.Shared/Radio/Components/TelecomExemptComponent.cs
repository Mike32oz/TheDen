// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Radio.Components;

/// <summary>
/// This is used for a radio that doesn't need a telecom server in order to broadcast.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TelecomExemptComponent : Component;
