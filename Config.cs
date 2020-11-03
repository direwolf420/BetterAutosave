using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace BetterAutosave
{
	public abstract class Config : ModConfig
	{
		//Separate AutosaveInterval variables to conform with vanilla (halved for multi client)

		public static T Get<T>() where T : Config => ModContent.GetInstance<T>();

		public static void Clamp(ref int value, int min, int max)
		{
			value = value < min ? min : (value > max ? max : value);
		}

		public static string SecondsToString(int seconds)
		{
			return new TimeSpan(TimeSpan.TicksPerSecond * seconds).ToString(@"hh\:mm\:ss");
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
		{
			message = "Only the host of this world can change the config! Do so in singleplayer.";
			//Because of NoSync
			return false;
		}
	}

	//This config should be the first shown, so it should start earlier in the alphabet compared to the other one
	[Label("Server+Singleplayer Config")]
	public class AServerConfig : Config
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public const int Min = 30; //30 seconds
		public const int Max = 24 * 60 * 60; //24 hours

		[Tooltip("Autosave interval in seconds")]
		[Label("Autosave Interval")]
		[Range(Min, Max)]
		[DefaultValue(10 * 60)] //10 minutes
		public int AutosaveInterval;

		//Header"==================================="
		[Header("==Server+Singleplayer Config Info==" + "\n" +
			"Will save the world+player in singleplayer," + "\n" +
			"and only the world in multiplayer"
			)]
		[Label("Autosave enabled")]
		[Tooltip("This mod will always work if enabled on the server. In singleplayer, only if 'Autosave' is enabled in the settings.")]
		public bool AutosaveStatus => Main.netMode != NetmodeID.MultiplayerClient || Main.autoSave;

		[Label("Current Interval (hours:minutes:seconds)")]
		public string CurrentInterval => SecondsToString(AutosaveInterval);

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			Clamp(ref AutosaveInterval, Min, Max);
		}
	}

	[Label("Multiplayer Client Config")]
	public class BClientConfig : Config
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public const int Min = 30; //30 seconds
		public const int Max = 12 * 60 * 60; //12 hours

		[Tooltip("Autosave interval in seconds")]
		[Label("Autosave Interval")]
		[Range(Min, Max)]
		[DefaultValue(5 * 60)] //5 minutes
		public int AutosaveInterval;

		//Header"==================================="
		[Header("======Multiplayer Client Info======" + "\n" +
			"Will save the player only in multiplayer"
			)]
		[Label("Autosave enabled")]
		[Tooltip("Autosave will always work in multiplayer for the client")]
		public bool AutosaveStatus => true;

		[Label("Current Interval (hours:minutes:seconds)")]
		public string CurrentInterval => SecondsToString(AutosaveInterval);

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			Clamp(ref AutosaveInterval, Min, Max);
		}
	}
}
