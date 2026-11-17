using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Aetherwake.Models;

public class AppliedTechniques
{
    [XmlElement("Technique")]
    public List<AppliedTechnique> Techniques { get; set; } = new();

    public List<string> GetNames()
    {
        return Techniques.Select(x => x.TechniqueName).ToList();
    }
}
