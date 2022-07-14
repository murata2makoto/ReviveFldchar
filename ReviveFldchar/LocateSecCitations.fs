module JTC1.SC34WG4.LocateSecCitations

open System.Xml.Linq;
open System.Xml;
open System.Xml.XPath;

let getAllSecCitations (doc: XDocument) (man: XmlNamespaceManager) =
    let citationXPath = "//w:r[.//w:rStyle[@w:val='citesec' or @w:val='citeapp']]"
    [for secCitation in doc.XPathSelectElements(citationXPath, man) ->
        let content = secCitation.Value
        let index = 
            if content.StartsWith("Clause") then content.Substring(7)
            elif content.StartsWith("Subclause") then content.Substring(10)
            elif content.Contains("Annex") then content.Substring(6)
            else content
        (index, secCitation)]


