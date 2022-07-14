module JTC1.SC34WG4.ReadAndWriteDocument
open WellKnownNamespaces
open System.IO.Compression
open System.IO
open System.Xml.Linq;
open System.Xml;


let getDocAndManager (fileName: string) =
    if fileName.EndsWith("docx") then
        use strm = new FileStream(fileName, FileMode.Open)
        use zipArchive = new ZipArchive(strm, ZipArchiveMode.Read)
        let documentStream = zipArchive.GetEntry "word/document.xml"
        use str = documentStream.Open() 
        use aReader = XmlReader.Create(str)
        let doc = XDocument.Load(aReader)
        let man = new XmlNamespaceManager (aReader.NameTable)
        man.AddNamespace("w", wmlNs)
        man.AddNamespace("mc", WellKnownNamespaces.mceNs)
        (doc, man)
    else failwith "strange input file"



let createDocument (inputFileName: string) (outputFileName: string) (doc: XDocument) =
    if inputFileName.EndsWith("docx")  &&
        outputFileName.EndsWith("docx") then
        File.Delete(outputFileName)
        use istrm = new FileStream(inputFileName, FileMode.Open)
        use iarchive = new ZipArchive(istrm, ZipArchiveMode.Read)
        use ostrm = new FileStream(outputFileName, FileMode.CreateNew)
        use oarchive = new ZipArchive(ostrm, ZipArchiveMode.Create)
        for entry in iarchive.Entries do
            let copyEnt = oarchive.CreateEntry(entry.FullName)
            use sw = new StreamWriter(copyEnt.Open())
            if entry.FullName = "word/document.xml" then
                    use xw =  XmlWriter.Create(sw)
                    doc.WriteTo(xw)
            else
                use sr = new StreamReader(entry.Open())
                sw.Write(sr.ReadToEnd())
    else failwith "strange input or output file"

