#r @"packages\FAKE\tools\FakeLib.dll"
open Fake
let version = "0.2"
let publishDir = "./publish/"
let buildDir  = "./build/"
let appReferences  =
    !! "HGOG/**/*.csproj"
      ++ "HOG/**/*.fsproj"


Target "Build" (fun _ ->
    // compile all projects below src/app/
    MSBuildRelease buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

Target "ZipSources" (fun _ ->
    CreateDir publishDir
    !! ("/**/*.*")
        -- "*.zip"
        -- "packages/**/*.*"
        -- "**/obj/**/*.*"
        -- "**/bin/**/*.*"
        -- "**/.paket/**/*.*"
        -- (sprintf "%s**/*.*" publishDir)
        -- (sprintf "%s**/*.*" buildDir)
        |> Zip "." (publishDir + "HOG-sources." + version + ".zip")
)

Target "ZipBinaries" (fun _ ->
    !!  (sprintf "%s**/*.*" buildDir)
        ++ "README.MD"
        ++ "Changelog.txt"
        ++ "examples/**/*.*"
        |> Zip "." (publishDir + "HOG-exe." + version + ".zip")
)

Target "Publish" (fun _ ->
    printf "TODO: publish binaries to website"
)

Target "ZipAll" (fun _ ->
    printf "Zipping sources and binaries"
)

"ZipSources"
  ==> "Build"

"ZipBinaries"
  ==> "Build"

"ZipAll"
  ==> "ZipSources"
  ==> "ZipBinaries"

"ZipAll"
  ==> "ZipSources"

"ZipAll"
  ==> "ZipBinaries"


RunTargetOrDefault "Build"