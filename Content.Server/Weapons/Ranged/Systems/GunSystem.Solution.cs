// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <temporaloroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Vapor;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Map;

namespace Content.Server.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;

    protected override void InitializeSolution()
    {
        base.InitializeSolution();

        SubscribeLocalEvent<SolutionAmmoProviderComponent, MapInitEvent>(OnSolutionMapInit);
        SubscribeLocalEvent<SolutionAmmoProviderComponent, SolutionContainerChangedEvent>(OnSolutionChanged);
    }

    private void OnSolutionMapInit(Entity<SolutionAmmoProviderComponent> entity, ref MapInitEvent args)
    {
        UpdateSolutionShots(entity.Owner, entity.Comp);
    }

    private void OnSolutionChanged(Entity<SolutionAmmoProviderComponent> entity, ref SolutionContainerChangedEvent args)
    {
        if (args.Solution.Name == entity.Comp.SolutionId)
            UpdateSolutionShots(entity.Owner, entity.Comp, args.Solution);
    }

    protected override void UpdateSolutionShots(EntityUid uid, SolutionAmmoProviderComponent component, Solution? solution = null)
    {
        var shots = 0;
        var maxShots = 0;
        if (solution == null && !_solutionContainer.TryGetSolution(uid, component.SolutionId, out _, out solution))
        {
            component.Shots = shots;
            component.MaxShots = maxShots;
            Dirty(uid, component);
            return;
        }

        shots = (int) (solution.Volume / component.FireCost);
        maxShots = (int) (solution.MaxVolume / component.FireCost);

        component.Shots = shots;
        component.MaxShots = maxShots;
        Dirty(uid, component);

        UpdateSolutionAppearance(uid, component);
    }

    protected override (EntityUid Entity, IShootable) GetSolutionShot(EntityUid uid, SolutionAmmoProviderComponent component, EntityCoordinates position)
    {
        var (ent, shootable) = base.GetSolutionShot(uid, component, position);

        if (!_solutionContainer.TryGetSolution(uid, component.SolutionId, out var solution, out _))
            return (ent, shootable);

        var newSolution = _solutionContainer.SplitSolution(solution.Value, component.FireCost);

        if (newSolution.Volume <= FixedPoint2.Zero)
            return (ent, shootable);

        if (TryComp<AppearanceComponent>(ent, out var appearance))
        {
            Appearance.SetData(ent, VaporVisuals.Color, newSolution.GetColor(ProtoManager).WithAlpha(1f), appearance);
            Appearance.SetData(ent, VaporVisuals.State, true, appearance);
        }

        // Add the solution to the vapor and actually send the thing
        if (_solutionContainer.TryGetSolution(ent, VaporComponent.SolutionName, out var vaporSolution, out _))
        {
            _solutionContainer.TryAddSolution(vaporSolution.Value, newSolution);
        }
        return (ent, shootable);
    }
}
