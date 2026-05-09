using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace com.seadoggie.TFWRArchipelago.Constants;

public class APLocation
{
	public static IEnumerable<APLocation> APLocations { get; private set; }
	
	public long id;
	public string name;
	public string description;
	public string region;
	[CanBeNull] public string achievement;
	[CanBeNull] public Requirement[] requirements;
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

	public class Requirement
	{
		public string name;
		public int count;
	}
	
	/// <summary>
	/// Loads all Locations from data.yaml
	/// </summary>
	public static void Initialize()
	{
		try
		{
			string locationText =
				File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"locations.json"));
			List<APLocation> locationData = JsonConvert.DeserializeObject<List<APLocation>>(locationText);
			APLocations = locationData;
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