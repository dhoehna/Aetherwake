using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Aetherwake.Models;

public class TechniqueParameter
{
    [XmlAttribute("name")]
    public string ParameterName { get; set; } = string.Empty;

    [XmlAttribute("value")]
    public string ParameterValue { get; set; } = string.Empty;
}
