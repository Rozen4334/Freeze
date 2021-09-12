using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Freeze
{
    [ApiVersion(2, 1)]
    public class Freeze : TerrariaPlugin
    {
        public override string Author
            => "Rozen4334";

        public override string Description
            => "A plugin that freezes users in place!";

        public override string Name
            => "Freeze";

        public override Version Version
            => new Version(1, 0);

        public Freeze(Main game) : base(game)
            => Order = 1;

        private static List<string> FrozenUsers = new List<string>();

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("freeze.use", Execute, "freeze", "fr"));

            Last = DateTime.UtcNow;
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
        }

        private DateTime Last;
        private void OnUpdate(EventArgs args)
        {
            if ((DateTime.UtcNow - Last).TotalSeconds > 5)
                if (TShock.Utils.GetActivePlayerCount() > 0)
                    foreach(var plr in TShock.Players.Where(x => x != null && x.Active && FrozenUsers.Contains(x.IP)))
                        plr.Disable("[Freeze] Player frozen");
        }

        private void Execute(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendErrorMessage("Invalid syntax. Valid syntax: '/freeze <player>'");
                args.Player.SendInfoMessage("This is a toggle command. Using freeze on the same user again will unfreeze the user.");
                return;
            }
            var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

            if (players.Count == 0)
                args.Player.SendErrorMessage("Invalid player!");
            else if (players.Count > 1)
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
            else if (FrozenUsers.Any(x => x == players[0].IP))
            {
                FrozenUsers.Remove(players[0].IP);
                players[0].SendSuccessMessage($"You were unfrozen by {args.Player.Name}");
                args.Player.SendSuccessMessage($"Unfrozen {players[0].Name}");
            }
            else
            {
                FrozenUsers.Add(players[0].IP);
                args.Player.SendSuccessMessage($"Frozen {players[0].Name}");
                players[0].SendErrorMessage($"You were frozen in place by {args.Player.Name}");
                players[0].Disable("[Freeze] Player frozen");
            }
        }
    }
}
