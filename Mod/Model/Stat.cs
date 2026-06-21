namespace com.seadoggie.TFWRArchipelago.Model;

public class Stat(string statName, double value)
{
    public readonly string Name = statName;
    public readonly double Value = value;
}