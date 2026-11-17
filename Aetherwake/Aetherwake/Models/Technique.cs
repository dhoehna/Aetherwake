using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Aetherwake.Models;

public class Technique
{
    [XmlAttribute("name")]
    public string TechniqueName { get; set; } = string.Empty;

    [XmlElement("Variant")]
    public List<TechniqueVariant> TechniqueVariants { get; set; } = new();
}
