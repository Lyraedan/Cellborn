using System;
using System.Collections;
using System.Collections.Generic;

public static class ExtentionFunctions
{
    public static int RoundOff(this int i)
    {
        return ((int)Math.Round(i / 10.0)) * 10;
    }
}
