module CodeGen

open FsAst
open FSharp.Compiler.XmlDoc
open FSharp.Compiler.SyntaxTree
open Fantomas
open FSharp.Compiler.Text
open Fantomas.FormatConfig

let createNamespace (names: seq<string>) declarations =
    let nameParts =
        names
        |> Seq.collect (fun name ->
            if name.Contains "."
            then name.Split('.')
            else [| name |]
        )

    let xmlDoc = PreXmlDoc.Create [ ]
    SynModuleOrNamespace.SynModuleOrNamespace([ for name in nameParts -> Ident.Create name ], true, SynModuleOrNamespaceKind.DeclaredNamespace,declarations,  xmlDoc, [ ], None, range.Zero)

let createQualifiedModule (idens: seq<string>) declarations =
    let nameParts =
        idens
        |> Seq.collect (fun name ->
            if name.Contains "."
            then name.Split('.')
            else [| name |]
        )

    let xmlDoc = PreXmlDoc.Create [ ]
    SynModuleOrNamespace.SynModuleOrNamespace([ for ident in nameParts -> Ident.Create ident ], true, SynModuleOrNamespaceKind.NamedModule,declarations,  xmlDoc, [ SynAttributeList.Create [ SynAttribute.RequireQualifiedAccess()  ]  ], None, range.Zero)

let createFile modules =
    let qualfiedNameOfFile = QualifiedNameOfFile.QualifiedNameOfFile(Ident.Create "IrrelevantFileName")
    ParsedImplFileInput.ParsedImplFileInput("IrrelevantFileName", false, qualfiedNameOfFile, [], [], modules, (false, false))

let formatAstInternal ast =
    let cfg = {
        FormatConfig.Default
            with
                StrictMode = true
                DisableElmishSyntax = false
                IndentSize = 4
                MaxIfThenElseShortWidth = 4
    }

    CodeFormatter.FormatASTAsync(ast, "temp.fsx", [], None, cfg)

let stringEnumAttr = """namespace Fable.Core
type StringEnumAttribute() =
    inherit System.Attribute()"""

let formatAst file =
    formatAstInternal (ParsedInput.ImplFile file)
    |> Async.RunSynchronously