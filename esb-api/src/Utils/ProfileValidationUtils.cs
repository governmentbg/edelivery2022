using System;
using System.Text.RegularExpressions;

namespace ED.EsbApi;

public sealed class ProfileValidationUtils
{
    private static readonly char[] allowedSpecCharsForNames = {
            '\u002D', // - : HYPHEN-MINUS {hyphen or minus sign}
            '\u2010', // ‐ : HYPHEN
            '\u2011', // ‑ : NON-BREAKING HYPHEN
            '\u2012', // ‒ : FIGURE DASH
            '\u2013', // – : EN DASH
            '\u2014', // — : EM DASH
            '\u2015', // ― : HORIZONTAL BAR {quotation dash}
            '\u005F', // _ : LOW LINE
            '\u0027', // ' : APOSTROPHE {APL quote}
            '\u0022', // " : QUOTATION MARK
            '\u2019', // ’ : RIGHT SINGLE QUOTATION MARK {single comma quotation mark}
            '\u201E', // „ : DOUBLE LOW-9 QUOTATION MARK {low double comma quotation mark}
            '\u201C', // “ : LEFT DOUBLE QUOTATION MARK {double turned comma quotation mark}
        };

    public static bool IsValidName(string? name) =>
        name == null
        || Regex.IsMatch(name, $"^[а-я0-9\\s{new string(allowedSpecCharsForNames)}]*$", RegexOptions.IgnoreCase);

    public static readonly Regex EmailRegex = new(
        "^[a-z0-9!#$%&'*+/=?^_`{|}~.-]+@([a-z0-9-]+(?:\\.[a-z0-9-]+)*)$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    public static bool IsValidEmail(string? email) =>
        email == null || EmailRegex.IsMatch(email);

    private static readonly int[] egnCoef = { 2, 4, 8, 5, 10, 9, 7, 3, 6 };
    private static readonly int[] lncCoef = { 21, 19, 17, 13, 11, 9, 7, 3, 1 };

    public static bool IsValidEGN(string? egn)
    {
        if (egn == null)
        {
            return false;
        }

        if (!Regex.IsMatch(egn, "^\\d{10}$"))
        {
            return false;
        }

        int[] nums = new int[9];
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            nums[i] = int.Parse(egn.Substring(i, 1));
            sum += (nums[i] * egnCoef[i]);
        }

        int rem = sum % 11;
        if (rem == 10)
        {
            rem = 0;
        }

        if (rem != int.Parse(egn.Substring(9, 1)))
        {
            return false;
        }

        int yy = (nums[0] * 10) + nums[1];
        int mm = (nums[2] * 10) + nums[3];
        int dd = (nums[4] * 10) + nums[5];

        if (mm >= 21 && mm <= 32)
        {
            mm -= 20;
            yy += 1800;
        }
        else if (mm >= 41 && mm <= 52)
        {
            mm -= 40;
            yy += 2000;
        }
        else
        {
            yy += 1900;
        }

        try
        {
            _ = new DateTime(yy, mm, dd);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return false;
        }

        return true;
    }

    public static bool IsValidLNC(string? lnc)
    {
        if (lnc == null)
        {
            return false;
        }

        if (!Regex.IsMatch(lnc, "^\\d{10}$"))
        {
            return false;
        }

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += int.Parse(lnc.Substring(i, 1)) * lncCoef[i];
        }

        if (sum % 10 != int.Parse(lnc.Substring(9, 1)))
        {
            return false;
        }

        return true;
    }
}
