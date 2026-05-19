namespace com.seadoggie.TFWRArchipelago.Utils;

public static class Parse
{
    /// <summary>
    /// Converts a formatted number (1K, 3B, etc) into a double
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static double FormattedDouble(string value)
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
        if (factor > 0) value = value.Substring(0, value.Length - 1);
        // Parse the double and multiply by the factor
        return double.TryParse(value, out double result) ? result * Math.Pow(10, factor) : -1;
    }
}