module JTC1.SC34WG4.Main

open ReadAndWriteDocument
open ExtractFields
open LocateSecCitations
open UpdateXDocument

[<EntryPoint>]
let main argv =
    let fileFromSC = argv.[0]
    let fileFromISO = argv.[1]
    let outputFile = argv.[2]
    let (scDoc,scMan) = getDocAndManager  fileFromSC
    let (isoDoc,isoMan) = getDocAndManager fileFromISO
    let dict = analyzeDocAndCreateDictionaryForRefFields scDoc scMan
    let contElemList = getAllSecCitations isoDoc isoMan
    addBRSE contElemList dict
    createDocument fileFromISO outputFile isoDoc

    0 // return an integer exit code
