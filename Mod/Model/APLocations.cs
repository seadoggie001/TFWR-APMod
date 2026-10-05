using JetBrains.Annotations;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Global

namespace com.seadoggie.TFWRArchipelago.Model;

public class APLocation
{
	public long id { get; set; }
	public string name { get; set; }
	public string description { get; set; }
	public string region { get; set; }
	[CanBeNull] public string achievement { get; set; }
	[CanBeNull] public Requirement[] requirements { get; set; }
	[CanBeNull] public Statistic statistic { get; set; }
	[CanBeNull] public TimedStatistic timed { get; set; }
	
	public class TimedStatistic : Statistic
	{
		public string time{ get; set; }
	}

	public class Statistic
	{
		public string key{ get; set; }
		public string value{ get; set; }
	}

	public class Requirement
	{
		public string name{ get; set; }
		public int count{ get; set; }
	}
}