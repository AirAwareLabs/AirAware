using System;
using System.Collections.Generic;
using AirAware.Models;

namespace AirAware.Services;

public class AqiCalculator : IAqiCalculator
{
    // Breakpoint tuple: (C_lo, C_hi, I_lo, I_hi, Category)
    private readonly List<(double Clo, double Chi, int Ilo, int Ihi, string Category)> _pm25Breakpoints =
        new List<(double, double, int, int, string)>
    {
        (0.0, 12.0, 0, 50, "Good"),
        (12.1, 35.4, 51, 100, "Moderate"),
        (35.5, 55.4, 101, 150, "Unhealthy for Sensitive Groups"),
        (55.5, 150.4, 151, 200, "Unhealthy"),
        (150.5, 250.4, 201, 300, "Very Unhealthy"),
        (250.5, 500.4, 301, 500, "Hazardous")
    };

    private readonly List<(double Clo, double Chi, int Ilo, int Ihi, string Category)> _pm10Breakpoints =
        new List<(double, double, int, int, string)>
    {
        (0, 54, 0, 50, "Good"),
        (55, 154, 51, 100, "Moderate"),
        (155, 254, 101, 150, "Unhealthy for Sensitive Groups"),
        (255, 354, 151, 200, "Unhealthy"),
        (355, 424, 201, 300, "Very Unhealthy"),
        (425, 504, 301, 500, "Hazardous")
    };

    public (AqiResult final, AqiResult pm25, AqiResult pm10) Calculate(Reading reading)
    {
        var pm25Result = CalculateForPm25(reading.Pm25);
        var pm10Result = CalculateForPm10(reading.Pm10 ?? 0);

        // Final AQI is the highest individual pollutant AQI
        var final = pm25Result.Value >= pm10Result.Value ? pm25Result : pm10Result;
        return (final, pm25Result, pm10Result);
    }

    public AqiResult CalculateForPm25(double concentration)
    {
        return CalculateFromBreakpoints(concentration, _pm25Breakpoints, "PM2.5");
    }

    public AqiResult CalculateForPm10(double concentration)
    {
        return CalculateFromBreakpoints(concentration, _pm10Breakpoints, "PM10");
    }

    private AqiResult CalculateFromBreakpoints(
        double concentration,
        List<(double Clo, double Chi, int Ilo, int Ihi, string Category)> breakpoints,
        string pollutant)
    {
        // Handle values above highest breakpoint by capping to maximum defined breakpoint (EPA defines up to 500)
        var bp = breakpoints.Find(b => concentration >= b.Clo && concentration <= b.Chi);

        if (bp == default)
        {
            // If below first breakpoint and not matched because of precision, handle explicitly
            if (concentration < breakpoints[0].Clo)
            {
                var first = breakpoints[0];
                int aqi = first.Ilo;
                return new AqiResult(aqi, first.Category, pollutant);
            }

            // If concentration is above highest Chi, cap to highest range using highest interval Ihi
            var last = breakpoints[breakpoints.Count - 1];
            int cappedAqi = last.Ihi;
            return new AqiResult(cappedAqi, last.Category, pollutant);
        }

        // Apply the linear interpolation formula:
        // I = (Ihi - Ilo)/(Chi - Clo) * (C - Clo) + Ilo
        double Ihi = bp.Ihi;
        double Ilo = bp.Ilo;
        double Chi = bp.Chi;
        double Clo = bp.Clo;
        double C = concentration;

        double I = ((Ihi - Ilo) / (Chi - Clo)) * (C - Clo) + Ilo;
        int rounded = (int)Math.Round(I, MidpointRounding.AwayFromZero);

        return new AqiResult(rounded, bp.Category, pollutant);
    }
}