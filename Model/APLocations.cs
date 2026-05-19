using JetBrains.Annotations;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Global

namespace com.seadoggie.TFWRArchipelago.Model;

public class APLocation
{
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
}