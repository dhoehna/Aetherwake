using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Aetherwake.Models;

public class TechniqueInformation
{
    [XmlElement("UltimaTechniques")]
    public UltimaTechniques UltimaTechniques { get; set; } = new();
}
