using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Aetherwake.Models;

public class AppliedTechnique
{
    [XmlAttribute("name")]
    public string TechniqueName { get; set; } = string.Empty;

    [XmlAttribute("variant")]
    public int Variant { get; set; }
}
