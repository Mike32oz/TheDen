// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

/// <summary>
/// Triggers when the salvage magnet is activated
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactMagnetTriggerComponent : Component
{
    /// <summary>
    /// how close to the magnet do you have to be?
    /// </summary>
    [DataField("range")]
    public float Range = 40f;

    /// <summary>
    /// How close do active magboots have to be?
    /// This is smaller because they are weaker magnets
    /// </summary>
    [DataField("magbootRange")]
    public float MagbootRange = 2f;
}
