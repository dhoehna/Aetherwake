using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Aetherwake.Models;

public class TechniqueVariant
{
    [XmlElement("Param")]
    public List<TechniqueParameter> Parameters { get; set; } = new();
}
