#r "FakeLib.dll"
open Fake
let buildDir  = "./build/"
let appReferences  =
    !! "HGOG/**/*.csproj"
      ++ "HOG/**/*.fsproj"


Target "BuildApp" (fun _ ->
    // compile all projects below src/app/
    MSBuildRelease buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)


RunTargetOrDefault "BuildApp"