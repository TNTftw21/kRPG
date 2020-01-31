﻿using System.Collections.Generic;
using Terraria.ModLoader;

namespace kRPG2.Commands
{
    public class RitualsCommand : ModCommand
    {
        public override string Command => "toggleritual";

        public override string Description => "Switches a ritual on or off";
        public override CommandType Type => CommandType.Chat;

        public override string Usage => "/toggleritual <name>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            try
            {
                var ritual = kRPG2.ritualByName[args[0]];
                var character = caller.Player.GetModPlayer<PlayerCharacter>();
                character.Rituals[ritual] = !character.Rituals[ritual];
            }
            catch (KeyNotFoundException)
            {
                throw new UsageException("Ritual not found: " + args);
            }
        }
    }
}