// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT


namespace Content.IntegrationTests.Tests.Interaction;

// This partial class contains various constant prototype IDs common to interaction tests.
// Should make it easier to mass-change hard coded strings if prototypes get renamed.
public abstract partial class InteractionTest
{
    // Tiles
    protected const string Floor = "FloorSteel";
    protected const string FloorItem = "FloorTileItemSteel";
    protected const string Plating = "Plating";
    protected const string Lattice = "Lattice";

    // Tools/steps
    protected const string Wrench = "Wrench";
    protected const string Screw = "Screwdriver";
    protected const string Weld = "WelderExperimental";
    protected const string Pry = "Crowbar";
    protected const string Cut = "Wirecutter";

    // Materials/stacks
    protected const string Steel = "Steel";
    protected const string Glass = "Glass";
    protected const string RGlass = "ReinforcedGlass";
    protected const string Plastic = "Plastic";
    protected const string Cable = "Cable";
    protected const string Rod = "MetalRod";

    // Parts
    protected const string Bin1 = "MatterBinStockPart";
    protected const string Manipulator1 = "MicroManipulatorStockPart";
}

