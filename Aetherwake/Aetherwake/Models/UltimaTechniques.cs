using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Aetherwake.Models;

public class UltimaTechniques
{
    [XmlElement("Technique")]
    public List<Technique> Techniques { get; set; } = new();

    public List<TechniqueParameter> GetTechnique(string name, int variant)
    {
        return Techniques.Single(x => x.TechniqueName == name).TechniqueVariants[variant - 1].Parameters;
    }
}
