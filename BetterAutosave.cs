using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BetterAutosave
{
	public class BetterAutosaveMod : Mod
	{
		internal static Stopwatch saveTime;

		public override void Load()
		{
			saveTime = new Stopwatch();

			//Runs when !dedServ
			On.Terraria.Main.DoUpdate_AutoSave += SaveClient;

			//Runs always
			On.Terraria.Main.UpdateTime += SaveServer;
		}

		public override void Unload()
		{
			saveTime = null;
		}

		public void Print(string message)
		{
			Logger.Info(message);
			if (Main.netMode == NetmodeID.Server)
			{
				Console.WriteLine(message);
			}
			else
			{
				Main.NewText(message, Color.LightBlue);
			}
		}

		private void SaveServer(On.Terraria.Main.orig_UpdateTime orig)
		{
			orig();

			var serverConfig = Config.Get<AServerConfig>();

			if (Main.dedServ && !serverConfig.AutosaveDisabled)
			{
				//Only make this run on a server
				if (!saveTime.IsRunning)
					saveTime.Start();

				int serverInterval = serverConfig.AutosaveInterval;
				if (saveTime.ElapsedMilliseconds > serverInterval * 1000)
				{
					saveTime.Reset();

					string message = "Autosaved World";
					if (serverConfig.Notify)
						Print(message);

					WorldGen.saveAndPlay();
				}
			}
		}

		private void SaveClient(On.Terraria.Main.orig_DoUpdate_AutoSave orig)
		{
			var clientConfig = Config.Get<BClientConfig>();
			var serverConfig = Config.Get<AServerConfig>();

			if (!Main.gameMenu && Main.netMode == NetmodeID.MultiplayerClient && !clientConfig.AutosaveDisabled)
			{
				if (!saveTime.IsRunning)
					saveTime.Start();

				int clientInterval = clientConfig.AutosaveInterval;
				if (saveTime.ElapsedMilliseconds > clientInterval * 1000)
				{
					saveTime.Reset();

					string message = "Autosaved Player";
					if (clientConfig.Notify)
						Print(message);

					WorldGen.saveToonWhilePlaying();
				}
			}
			else if (!Main.gameMenu && Main.autoSave && !serverConfig.AutosaveDisabled)
			{
				if (!saveTime.IsRunning)
					saveTime.Start();

				int singleInterval = serverConfig.AutosaveInterval;
				if (saveTime.ElapsedMilliseconds > singleInterval * 1000)
				{
					saveTime.Reset();

					string message = "Autosaved World and Player";
					if (serverConfig.Notify)
						Print(message);

					WorldGen.saveToonWhilePlaying();
					WorldGen.saveAndPlay();
				}
			}
			else if (saveTime.IsRunning)
			{
				saveTime.Stop();
			}
		}
	}
}
