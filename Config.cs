using Newtonsoft.Json;
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
			message = BetterAutosaveMod.AcceptClientChangesText.ToString();
			//Because of NoSync
			return false;
		}
	}

	//This config should be the first shown, so it should start earlier in the alphabet compared to the other one
	public class AServerConfig : Config
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public const int Min = 10; //10 seconds
		public const int Max = 24 * 60 * 60; //24 hours

		[Range(Min, Max)]
		[DefaultValue(10 * 60)] //10 minutes
		public int AutosaveInterval;

		[DefaultValue(false)]
		public bool AutosaveDisabled;

		[DefaultValue(false)]
		public bool Notify;

		[Header("ServerSingleplayerConfigInfo")]

		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		public bool AutosaveStatus => Main.netMode != NetmodeID.MultiplayerClient || Main.autoSave;

		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		public string CurrentInterval => SecondsToString(AutosaveInterval);

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			Clamp(ref AutosaveInterval, Min, Max);
		}
	}

	public class BClientConfig : Config
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public const int Min = 10; //10 seconds
		public const int Max = 12 * 60 * 60; //12 hours

		[Range(Min, Max)]
		[DefaultValue(5 * 60)] //5 minutes
		public int AutosaveInterval;

		[DefaultValue(false)]
		public bool AutosaveDisabled;

		[DefaultValue(false)]
		public bool Notify;

		[Header("MultiplayerClientConfigInfo")]

		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		public bool AutosaveStatus => true;

		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		public string CurrentInterval => SecondsToString(AutosaveInterval);

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			Clamp(ref AutosaveInterval, Min, Max);
		}
	}
}
