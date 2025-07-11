// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class ParrotAccentSystem : EntitySystem
{
    private static readonly Regex WordCleanupRegex = new Regex("[^A-Za-z0-9 -]");

    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParrotAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    private void OnAccentGet(Entity<ParrotAccentComponent> entity, ref AccentGetEvent args)
    {
        args.Message = Accentuate(entity, args.Message);
    }

    public string Accentuate(Entity<ParrotAccentComponent> entity, string message)
    {
        // Sometimes repeat the longest word at the end of the message, after a squawk! SQUAWK! Sometimes!
        if (_random.Prob(entity.Comp.LongestWordRepeatChance))
        {
            // Don't count non-alphanumeric characters as parts of words
            var cleaned = WordCleanupRegex.Replace(message, string.Empty);
            // Split on whitespace and favor words towards the end of the message
            var words = cleaned.Split(null).Reverse();
            // Find longest word
            var longest = words.MaxBy(word => word.Length);
            if (longest?.Length >= entity.Comp.LongestWordMinLength)
            {
                message = EnsurePunctuation(message);

                // Capitalize the first letter of the repeated word
                longest = string.Concat(longest[0].ToString().ToUpper(), longest.AsSpan(1));

                message = string.Format("{0} {1} {2}!", message, GetRandomSquawk(entity), longest);
                return message; // No more changes, or it's too much
            }
        }

        if (_random.Prob(entity.Comp.SquawkPrefixChance))
        {
            // AWWK! Sometimes add a squawk at the begining of the message
            message = string.Format("{0} {1}", GetRandomSquawk(entity), message);
        }
        else
        {
            // Otherwise add a squawk at the end of the message! RAWWK!
            message = EnsurePunctuation(message);
            message = string.Format("{0} {1}", message, GetRandomSquawk(entity));
        }

        return message;
    }

    /// <summary>
    /// Adds a "!" to the end of the string, if there isn't already a sentence-ending punctuation mark.
    /// </summary>
    private string EnsurePunctuation(string message)
    {
        if (!message.EndsWith('!') && !message.EndsWith('?') && !message.EndsWith('.'))
            return message + '!';
        return message;
    }

    /// <summary>
    /// Returns a random, localized squawk sound.
    /// </summary>
    private string GetRandomSquawk(Entity<ParrotAccentComponent> entity)
    {
        return Loc.GetString(_random.Pick(entity.Comp.Squawks));
    }
}
