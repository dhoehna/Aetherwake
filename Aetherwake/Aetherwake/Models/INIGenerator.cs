using System;
using System.Collections.Generic;
using System.Text;

namespace Aetherwake.Models;

public static class INIGenerator
{
    public enum ColorFilter
    {
        Vanilla,
        Summer,
        Winter
    }

    public enum Foundation
    {
        Vanilla,
        Overdose
    }

    public enum Brightness
    {
        Dim,
        Normal,
        Vivid
    }

    public enum Contrast
    {
        Soft,
        Balanced,
        Sharp
    }

    public enum Saturation
    {
        Muted,
        Natural,
        Rich
    }

    public enum TextureSharpening
    {
        On,
        Off
    }

    public enum DepthOfField
    {
        Off,
        Close,
        Far
    }

    public enum RayTracing
    {
        Off,
        Low,
        Medium,
        High,
        Ultra
    }

    public static List<string> GetAllStrings()
    {
        var allCombonations = new List<string>();

        foreach(var thisColorFilter in Enum.GetValues(typeof(ColorFilter)))
        {
            foreach (var thisFoundation in Enum.GetValues(typeof(Foundation)))
            {
                foreach (var thisBrightness in Enum.GetValues(typeof(Brightness)))
                {
                    foreach (var thisContrast in Enum.GetValues(typeof(Contrast)))
                    {
                        foreach (var thisSaturation in Enum.GetValues(typeof(Saturation)))
                        {
                            foreach (var thisTextureSharpening in Enum.GetValues(typeof(TextureSharpening)))
                            {
                                foreach (var thisDepthOfField in Enum.GetValues(typeof(DepthOfField)))
                                {
                                    foreach (var thisRayTracing in Enum.GetValues(typeof(RayTracing)))
                                    {
                                        var builder = new StringBuilder();

                                        builder.Append($"CFI{(int)thisColorFilter}_");
                                        builder.Append($"FON{(int)thisFoundation}_");
                                        builder.Append($"BRI{(int)thisBrightness}_");
                                        builder.Append($"CON{(int)thisContrast}_");
                                        builder.Append($"SAT{(int)thisSaturation}_");
                                        builder.Append($"TSH{(int)thisTextureSharpening}_");
                                        builder.Append($"DOF{(int)thisDepthOfField}_");
                                        builder.Append($"RTR{(int)thisRayTracing}.png");

                                        allCombonations.Add(builder.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return allCombonations;
    }
}
