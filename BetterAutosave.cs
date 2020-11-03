using Microsoft.Xna.Framework;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BetterAutosave
{
	public class BetterAutosaveMod : Mod
	{
		internal static Stopwatch saveTime = new Stopwatch();

		public override void Load()
		{
			//Runs when !dedServ
			On.Terraria.Main.DoUpdate_AutoSave += SaveClient;

			//Runs always
			On.Terraria.Main.UpdateTime += SaveServer;
		}

		private void SaveServer(On.Terraria.Main.orig_UpdateTime orig)
		{
			orig();

			if (Main.dedServ)
			{
				//Only make this run on a server
				if (!saveTime.IsRunning)
					saveTime.Start();

				int serverInterval = Config.Get<AServerConfig>().AutosaveInterval;
				if (saveTime.ElapsedMilliseconds > serverInterval * 1000)
				{
					saveTime.Reset();
					Logger.Info("Autosaving World...");
					WorldGen.saveAndPlay();
				}
			}
		}

		private void SaveClient(On.Terraria.Main.orig_DoUpdate_AutoSave orig)
		{
			if (!Main.gameMenu && Main.netMode == NetmodeID.MultiplayerClient)
			{
				if (!saveTime.IsRunning)
					saveTime.Start();

				int clientInterval = Config.Get<BClientConfig>().AutosaveInterval / 2;
				if (saveTime.ElapsedMilliseconds > clientInterval * 1000)
				{
					saveTime.Reset();
					Logger.Info("Autosaving Player...");
					WorldGen.saveToonWhilePlaying();
				}
			}
			else if (!Main.gameMenu && Main.autoSave)
			{
				if (!saveTime.IsRunning)
					saveTime.Start();

				int singleInterval = Config.Get<AServerConfig>().AutosaveInterval;
				if (saveTime.ElapsedMilliseconds > singleInterval * 1000)
				{
					saveTime.Reset();
					Logger.Info("Autosaving World and Player...");
					WorldGen.saveToonWhilePlaying();
					WorldGen.saveAndPlay();
				}
			}
			else if (saveTime.IsRunning)
			{
				saveTime.Stop();
			}
		}

		public override void Unload()
		{
			base.Unload();
		}

		//Original code
		/*
		private static void DoUpdate_AutoSave()
		{
			if (!gameMenu && netMode == 1)
			{
				if (!saveTime.IsRunning)
					saveTime.Start();

				if (saveTime.ElapsedMilliseconds > 300000)
				{
					saveTime.Reset();
					WorldGen.saveToonWhilePlaying();
				}
			}
			else if (!gameMenu && autoSave)
			{
				if (!saveTime.IsRunning)
					saveTime.Start();

				if (saveTime.ElapsedMilliseconds > 600000)
				{
					saveTime.Reset();
					WorldGen.saveToonWhilePlaying();
					WorldGen.saveAndPlay();
				}
			}
			else if (saveTime.IsRunning)
			{
				saveTime.Stop();
			}
		}
		*/
	}
}
