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

		private void SaveServer(On.Terraria.Main.orig_UpdateTime orig)
		{
			orig();

			if (Main.dedServ && !Config.Get<AServerConfig>().AutosaveDisabled)
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
			if (!Main.gameMenu && Main.netMode == NetmodeID.MultiplayerClient && !Config.Get<BClientConfig>().AutosaveDisabled)
			{
				if (!saveTime.IsRunning)
					saveTime.Start();

				int clientInterval = Config.Get<BClientConfig>().AutosaveInterval;
				if (saveTime.ElapsedMilliseconds > clientInterval * 1000)
				{
					saveTime.Reset();
					Logger.Info("Autosaving Player...");
					WorldGen.saveToonWhilePlaying();
				}
			}
			else if (!Main.gameMenu && Main.autoSave && !Config.Get<AServerConfig>().AutosaveDisabled)
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
	}
}
