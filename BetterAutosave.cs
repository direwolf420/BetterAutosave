using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BetterAutosave
{
	public class BetterAutosaveMod : Mod
	{
		internal static Stopwatch saveTime;

		public static LocalizedText AcceptClientChangesText { get; private set; }
		public static LocalizedText AutosavedWorldText { get; private set; }
		public static LocalizedText AutosavedPlayerText { get; private set; }
		public static LocalizedText AutosavedWorldPlayerText { get; private set; }

		public override void Load()
		{
			saveTime = new Stopwatch();

			//Runs when !dedServ
			On_Main.DoUpdate_AutoSave += SaveClient;

			//Runs always
			On_Main.UpdateTime += SaveServer;
			//Can also use ModSystem.PostUpdateTime hook

			string category = $"Configs.Common.";
			AcceptClientChangesText ??= Language.GetOrRegister(this.GetLocalizationKey($"{category}AcceptClientChanges"));

			category = $"Common.";
			AutosavedWorldText ??= Language.GetOrRegister(this.GetLocalizationKey($"{category}AutosavedWorld"));
			AutosavedPlayerText ??= Language.GetOrRegister(this.GetLocalizationKey($"{category}AutosavedPlayer"));
			AutosavedWorldPlayerText ??= Language.GetOrRegister(this.GetLocalizationKey($"{category}AutosavedWorldPlayer"));
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

		private void SaveServer(On_Main.orig_UpdateTime orig)
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

					string message = AutosavedWorldText.ToString();
					if (serverConfig.Notify)
						Print(message);

					WorldGen.saveAndPlay();
				}
			}
		}

		private void SaveClient(On_Main.orig_DoUpdate_AutoSave orig)
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

					string message = AutosavedPlayerText.ToString();
					if (clientConfig.Notify)
						Print(message);

					WorldGen.saveToonWhilePlaying();
				}
			}
			else if (Main.hasFocus && !Main.gameMenu && Main.autoSave && !serverConfig.AutosaveDisabled)
			{
				if (!saveTime.IsRunning)
					saveTime.Start();

				int singleInterval = serverConfig.AutosaveInterval;
				if (saveTime.ElapsedMilliseconds > singleInterval * 1000)
				{
					saveTime.Reset();

					string message = AutosavedWorldPlayerText.ToString();
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
