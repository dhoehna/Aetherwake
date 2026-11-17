using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Xml.Serialization;

namespace Aetherwake.Models;

public class Permutation
{
    [XmlAttribute("Color")]
    public INIGenerator.ColorFilter Color { get; set; }

    [XmlAttribute("Foundation")]
    public INIGenerator.Foundation Foundation { get; set; }

    [XmlAttribute("Brightness")]
    public INIGenerator.Brightness Brightness { get; set; }

    [XmlAttribute("Contrast")]
    public INIGenerator.Contrast Contrast { get; set; }

    [XmlAttribute("Saturation")]
    public INIGenerator.Saturation Saturation { get; set; }

    [XmlAttribute("Sharpening")]
    public INIGenerator.TextureSharpening Sharpening { get; set; }

    [XmlAttribute("DepthOfField")]
    public INIGenerator.DepthOfField DepthOfField { get; set; }

    [XmlAttribute("RayTracing")]
    public INIGenerator.RayTracing RayTracing { get; set; }

    [XmlElement("Techniques")]
    public string Techniques { get; set; } = string.Empty;

    [XmlElement("TechniqueSorting")]
    public string TechniqueSorting { get; set; } = string.Empty;

    [XmlElement("AppliedTechniques")]
    public AppliedTechniques AppliedTechniques { get; set; } = new();
}
