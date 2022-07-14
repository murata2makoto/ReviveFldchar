module JTC1.SC34WG4.ExtractFields

open System.Linq;
open System.Xml.Linq;
open System.Xml;
open System.Xml.XPath;
open System.Collections.Generic
open WellKnownNamespaces

let private getAllParagraphsContainingFields 
                (doc: XDocument) (man: XmlNamespaceManager) =
    let intro =
        doc.XPathSelectElement(".//w:p[w:r[w:t[text()='Introduction']]]", man)

    intro.XPathSelectElements("following::w:p[.//w:r[.//w:fldChar]]", man)

let private getAllSiblingRsContainingFields 
                (man: XmlNamespaceManager) (paras: XElement seq) =
    [for p in paras do
        let rs = 
            p.XPathSelectElements(".//w:r[.//w:fldChar]", man) |> List.ofSeq
        if not(rs.IsEmpty) then 
            yield rs]

let rec private  check (siblings: XElement list) =
    match siblings with
    | [_] | [] -> ()
    | first::second::tail ->
            if first.ElementsAfterSelf().Contains(second)
                then check siblings.Tail
                else failwith "inversion!"

let rec private divideAsMuchAsPossible
                    (man: XmlNamespaceManager)  (siblingRs: XElement list list)=
    let getFldCharType (s: XElement) =
        let fldChar = s.XPathSelectElement(".//w:fldChar[@w:fldCharType]", man)
        fldChar.Attribute(wml + "fldCharType").Value

    let rec help (s: XElement list): XElement list list  = 
        match s with
        | [] -> []
        | [_] | [_;_] -> 
            [s]
        | s1::s2::s3::tail ->
            match getFldCharType s1, getFldCharType s2, getFldCharType s3 with
            | "end", _, _ -> [s1]::(help s.Tail)
            | "separate", "begin", _ -> [s1]::(help s.Tail)
            | "separate", "end", _ -> [s1; s2]::(help (s3::tail))
            | "separate", "separate", "end" -> [s1;s2;s3]::(help tail)
            | "separate", "separate", "begin" -> [s1;s2]::(help  (s3::tail))
            | "begin", "begin", _ -> [s1]::(help s.Tail)
            | "begin", "end", _ -> [s1; s2]::(help (s3::tail))
            | "begin", "separate", "end" -> [s1; s2; s3]::(help tail)
            | "begin", "separate", "begin" -> [s1; s2]::(help (s3::tail))
            | "begin", "separate", "separate" -> failwith "hen"
            | "separate", "separate", "separate"  -> failwith "hen"
            | _ -> failwith "hen"

    [for siblings in siblingRs do
        check siblings
        for s in help siblings do
            yield s];;
 
 let private getRefFields
                (man: XmlNamespaceManager)  (runListList: XElement list list) =
    [for runList in runListList do
        match runList with
        | [] | [_] | [_; _] -> ()
        | [b;s;e]  ->
            let refCands1 = 
                b.XPathSelectElements(
                    "following-sibling::w:r[
                        w:instrText[contains(., 'REF') 
                                    and not(contains(., 'PAGEREF'))]]", 
                    man)
            let refCands2 = 
                s.XPathSelectElements(
                    "preceding-sibling::w:r[
                        w:instrText[contains(., 'REF') 
                                    and not(contains(., 'PAGEREF'))]]", 
                    man)
            if (refCands1 |> Seq.length > 0)
               && (refCands2 |> Seq.length > 0) 
               && refCands1.First() = refCands2.Last() then 
                let content = 
                    e.XPathSelectElements(
                        "preceding-sibling::w:r[w:t]/w:t", man).Last().Value
                yield (b, refCands1.First(), s, e, content)
            else 
                ()
        | _ -> 
            () ];;

let private constructDictionaryForRefFields 
                (b_ref_s_e_c_list: 
                    (XElement * XElement * XElement * XElement * string) list ) =
    let dict = new Dictionary<string, XElement * XElement * XElement * XElement>();
    for (b, ref, s, e, content) in b_ref_s_e_c_list do
        let index = 
            if content.StartsWith("Clause") then content.Substring(7)
            elif content.StartsWith("Subclause") then content.Substring(10)
            elif content.Contains("Annex") then content.Substring(6)
            else content
        dict.[index] <- (b, ref, s, e)
    dict;;

let analyzeDocAndCreateDictionaryForRefFields
        (doc: XDocument) (man: XmlNamespaceManager): 
    Dictionary<string, XElement * XElement * XElement * XElement> = 

    getAllParagraphsContainingFields doc man
    |> getAllSiblingRsContainingFields man
    |> divideAsMuchAsPossible man
    |> getRefFields man
    |> constructDictionaryForRefFields