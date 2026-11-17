using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Aetherwake.Models;

public class Permutations
{
    [XmlElement("Permutation")]
    public List<Permutation> PermutationList { get; set; } = new();
}
