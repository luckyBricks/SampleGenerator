using System.Diagnostics;

namespace TestGenerator;

public class ClassNames
{
    public static List<string> GetNames()
    {
        Debugger.Break();
        return new List<string> { "aa", "bb" };
    }
}