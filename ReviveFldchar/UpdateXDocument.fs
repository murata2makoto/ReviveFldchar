module JTC1.SC34WG4.UpdateXDocument

open System.Collections.Generic
open System.Xml.Linq;

let addBRSE (ceList:(string * XElement) list) 
    (dict: Dictionary<string, XElement * XElement * XElement * XElement>) =
    let mutable covered = Set.empty
    for (index, secCitation) in ceList do
        match dict.TryGetValue(index) with 
        | true, (b,r,s,e) ->
            printfn "Rewriting cross reference %s" index
            covered <- covered.Add(index)
            secCitation.AddAfterSelf([|e|])
            secCitation.AddBeforeSelf([|b;r;s|])
        | false, _ -> printfn "Undefined cross reference %s" index
    Set.difference (Set.ofSeq(dict.Keys)) covered
    |> Set.iter (fun x -> printfn "Cross referene to %s is not rewritten" x)
