// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <temporaloroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Body.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.Body.Organ;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Body.Systems
{
    public sealed class StomachSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;

        public const string DefaultSolutionName = "stomach";

        public override void Initialize()
        {
            SubscribeLocalEvent<StomachComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<StomachComponent, EntityUnpausedEvent>(OnUnpaused);
            SubscribeLocalEvent<StomachComponent, ApplyMetabolicMultiplierEvent>(OnApplyMetabolicMultiplier);
        }

        private void OnMapInit(Entity<StomachComponent> ent, ref MapInitEvent args)
        {
            ent.Comp.NextUpdate = _gameTiming.CurTime + ent.Comp.UpdateInterval;
        }

        private void OnUnpaused(Entity<StomachComponent> ent, ref EntityUnpausedEvent args)
        {
            ent.Comp.NextUpdate += args.PausedTime;
        }

        public override void Update(float frameTime)
        {
            var query = EntityQueryEnumerator<StomachComponent, OrganComponent, SolutionContainerManagerComponent>();
            while (query.MoveNext(out var uid, out var stomach, out var organ, out var sol))
            {
                if (_gameTiming.CurTime < stomach.NextUpdate)
                    continue;

                stomach.NextUpdate += stomach.UpdateInterval;

                // Get our solutions
                if (!_solutionContainerSystem.ResolveSolution((uid, sol), DefaultSolutionName, ref stomach.Solution, out var stomachSolution))
                    continue;

                if (organ.Body is not { } body || !_solutionContainerSystem.TryGetSolution(body, stomach.BodySolutionName, out var bodySolution))
                    continue;

                var transferSolution = new Solution();

                var queue = new RemQueue<StomachComponent.ReagentDelta>();
                foreach (var delta in stomach.ReagentDeltas)
                {
                    delta.Increment(stomach.UpdateInterval);
                    if (delta.Lifetime > stomach.DigestionDelay)
                    {
                        if (stomachSolution.TryGetReagent(delta.ReagentQuantity.Reagent, out var reagent))
                        {
                            if (reagent.Quantity > delta.ReagentQuantity.Quantity)
                                reagent = new(reagent.Reagent, delta.ReagentQuantity.Quantity);

                            stomachSolution.RemoveReagent(reagent);
                            transferSolution.AddReagent(reagent);
                        }

                        queue.Add(delta);
                    }
                }

                foreach (var item in queue)
                {
                    stomach.ReagentDeltas.Remove(item);
                }

                _solutionContainerSystem.UpdateChemicals(stomach.Solution.Value);

                // Transfer everything to the body solution!
                _solutionContainerSystem.TryAddSolution(bodySolution.Value, transferSolution);
            }
        }

        private void OnApplyMetabolicMultiplier(
            Entity<StomachComponent> ent,
            ref ApplyMetabolicMultiplierEvent args)
        {
            if (args.Apply)
            {
                ent.Comp.UpdateInterval *= args.Multiplier;
                return;
            }

            // This way we don't have to worry about it breaking if the stasis bed component is destroyed
            ent.Comp.UpdateInterval /= args.Multiplier;
        }

        public bool CanTransferSolution(
            EntityUid uid,
            Solution solution,
            StomachComponent? stomach = null,
            SolutionContainerManagerComponent? solutions = null)
        {
            return Resolve(uid, ref stomach, ref solutions, logMissing: false)
                && _solutionContainerSystem.ResolveSolution((uid, solutions), DefaultSolutionName, ref stomach.Solution, out var stomachSolution)
                // TODO: For now no partial transfers. Potentially change by design
                && stomachSolution.CanAddSolution(solution);
        }

        public bool TryTransferSolution(
            EntityUid uid,
            Solution solution,
            StomachComponent? stomach = null,
            SolutionContainerManagerComponent? solutions = null)
        {
            if (!Resolve(uid, ref stomach, ref solutions, logMissing: false)
                || !_solutionContainerSystem.ResolveSolution((uid, solutions), DefaultSolutionName, ref stomach.Solution)
                || !CanTransferSolution(uid, solution, stomach, solutions))
            {
                return false;
            }

            _solutionContainerSystem.TryAddSolution(stomach.Solution.Value, solution);
            // Add each reagent to ReagentDeltas. Used to track how long each reagent has been in the stomach
            foreach (var reagent in solution.Contents)
            {
                stomach.ReagentDeltas.Add(new StomachComponent.ReagentDelta(reagent));
            }

            return true;
        }
    }
}
