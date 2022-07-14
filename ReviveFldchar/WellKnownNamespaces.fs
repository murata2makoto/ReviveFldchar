module JTC1.SC34WG4.WellKnownNamespaces
open System.Xml.Linq;

let wmlNs = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
let mceNs = "http://schemas.openxmlformats.org/markup-compatibility/2006"
let wml = XNamespace.Get wmlNs
let mce = XNamespace.Get mceNs