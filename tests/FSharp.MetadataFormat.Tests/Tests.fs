﻿#if INTERACTIVE
#I "../../bin"
#r "FSharp.MetadataFormat.dll"
#r "RazorEngine.dll"
#r "../../packages/NUnit.2.6.3/lib/nunit.framework.dll"
#load "../Common/FsUnit.fs"
#else
module FSharp.MetadataFormat.Tests
#endif

open FsUnit
open System.IO
open NUnit.Framework
open FSharp.MetadataFormat

// --------------------------------------------------------------------------------------
// Run the metadata formatter on sample project 
// --------------------------------------------------------------------------------------

let (@@) a b = Path.Combine(a, b)

let root = __SOURCE_DIRECTORY__ 

let getOutputDir()  = 
  let tempFile = Path.GetTempFileName()
  File.Delete(tempFile)
  Directory.CreateDirectory(tempFile).FullName

let layoutRoots = 
  [ __SOURCE_DIRECTORY__ @@ "../../misc/templates"
    __SOURCE_DIRECTORY__ @@ "../../misc/templates/reference" ]

let info =
  [ "project-name", "FSharp.ProjectScaffold"
    "project-author", "Your Name"
    "project-summary", "A short summary of your project"
    "project-github", "http://github.com/pblasucci/fsharp-project-scaffold"
    "project-nuget", "http://nuget.com/packages/FSharp.ProjectScaffold"
    "root", "http://tpetricek.github.io/FSharp.FSharp.ProjectScaffold" ]

[<Test>]
let ``MetadataFormat works on sample Deedle assembly``() = 
  let library = root @@ "files" @@ "Deedle.dll"
  let output = getOutputDir()
  MetadataFormat.Generate(library, output, layoutRoots, info)
  let files = Directory.GetFiles(output)
  
  let optIndex = files |> Seq.tryFind (fun s -> s.EndsWith "index.html")
  optIndex.IsSome |> shouldEqual true
  
  let optSeriesMod = files |> Seq.tryFind (fun s -> s.Contains "seriesmodule")
  optSeriesMod.IsSome |> shouldEqual true

  #if INTERACTIVE
  System.Diagnostics.Process.Start(output)
  #endif

// Ignore by default to make tests run reasonably fast
[<Test; Ignore>]
let ``MetadataFormat works on sample FAKE assembly``() = 
  let library = root @@ "files" @@ "FAKE" @@ "FakeLib.dll"
  let output = getOutputDir()
  MetadataFormat.Generate(library, output, layoutRoots, info)
  let files = Directory.GetFiles(output)
  files |> Seq.length |> shouldEqual 166

[<Test>]
let ``MetadataFormat works on two sample F# assemblies``() = 
  let libraries = 
    [ root @@ "files/FsLib/bin/Debug" @@ "FsLib1.dll"
      root @@ "files/FsLib/bin/Debug" @@ "FsLib2.dll" ]
  let output = getOutputDir()
  MetadataFormat.Generate(libraries, output, layoutRoots, info)
  let fileNames = Directory.GetFiles(output)
  let files = dict [ for f in fileNames -> Path.GetFileName(f), File.ReadAllText(f) ]
  files.["fslib-class.html"] |> should contain "Readonly int property"
  files.["fslib-record.html"] |> should contain "This is name"
  files.["fslib-record.html"] |> should contain "Additional member"
  files.["fslib-union.html"] |> should contain "Hello of int"
  files.["index.html"] |> should contain "Sample class"
  files.["index.html"] |> should contain "Union sample"
  files.["index.html"] |> should contain "Record sample"
  files.["fslib-nested.html"] |> should contain "Somewhat nested type"
  files.["fslib-nested.html"] |> should contain "Somewhat nested module"
  files.["fslib-nested-nestedtype.html"] |> should contain "Very nested member"
  files.["fslib-nested-submodule.html"] |> should contain "Very nested field"
  #if INTERACTIVE
  System.Diagnostics.Process.Start(output)
  #endif

