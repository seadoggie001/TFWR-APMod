using System.Reflection;
using JetBrains.Annotations;

namespace com.seadoggie.TFWRArchipelago.Constants;

public class APLocation
{
	public static IEnumerable<APLocation> APLocations { get; private set; }
	
	public long id;
	public string name;
	public string region;
	[CanBeNull] public string achievement;
	[CanBeNull] public string[] requirements;
	[CanBeNull] public Statistic statistic;
	[CanBeNull] public TimedStatistic timed;
	
	public class TimedStatistic : Statistic
	{
		public string time;
	}

	public class Statistic
	{
		public string key;
		public string value;
	}

	public static void Load()
	{
		try
		{
			string yamlText =
				File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"data.yaml"));
			YamlDotNet.Serialization.IDeserializer deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
				.IgnoreUnmatchedProperties()
				.Build();
			YamlData yamlData = deserializer.Deserialize<YamlData>(yamlText);
			APLocations = yamlData.locations;
		}
		catch (Exception e)
		{
			Plugin.LogError($"[{nameof(APLocation)}] Failed to load APLocation data", e);
		}
	}
	
	/// <summary>
	/// Converts a formatted number (1K, 3B, etc) into a double
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static double Parse(string value)
	{
		char lastChar = value[value.Length - 1];
		// Check if there is a factor
		int factor = lastChar switch
		{
			'B' => 9,
			'M' => 6,
			'K' => 3,
			_ => 0
		};
		// Remove the last character if there's a factor
		if (factor > 0) value = value.Substring(0, value.Length-1);
		// Parse the double and multiply by the factor
		return double.TryParse(value, out double result) ? result * Math.Pow(10, factor) : -1;
	}
}

public class YamlData
{
	public IEnumerable<APLocation> locations;
}