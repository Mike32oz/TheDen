// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Parallax.Biomes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Salvage.Expeditions.Modifiers;

/// <summary>
/// Affects the biome to be used for salvage.
/// </summary>
[Prototype("salvageBiomeMod")]
public sealed partial class SalvageBiomeModPrototype : IPrototype, ISalvageMod
{
    [IdDataField] public string ID { get; } = default!;

    [DataField("desc")] public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Cost for difficulty modifiers.
    /// </summary>
    [DataField("cost")]
    public float Cost { get; private set; } = 0f;

    /// <summary>
    /// Is weather allowed to apply to this biome.
    /// </summary>
    [DataField("weather")]
    public bool Weather = true;

    [DataField("biome", required: true, customTypeSerializer:typeof(PrototypeIdSerializer<BiomeTemplatePrototype>))]
    public string? BiomePrototype;
}
