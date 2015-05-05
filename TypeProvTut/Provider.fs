module Ttl.Intl.TypeProviders

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open FileParser
open System
open System.IO

type Variant = { Name: string; Test: string; Weight: int; }

[<TypeProvider>]
type MultivariateProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let baseNamespace = "Multivariate.Test"
    let asm = Assembly.GetExecutingAssembly()
    let multivariateProvider = 
        ProvidedTypeDefinition(
            asm, 
            baseNamespace, 
            "MultivariateProvider", 
            Some(typeof<obj>))
    let parameters = [ProvidedStaticParameter("PathToNickl", typeof<string>)]

    do multivariateProvider.DefineStaticParameters(parameters, fun typeName args ->
        let pathToNickl = 
            let local = args.[0] :?> string
            Path.Combine([| config.ResolutionFolder ; local |])

        let provider =
            ProvidedTypeDefinition(
                asm,
                baseNamespace,
                typeName,
                Some typeof<obj>,
                HideObjectMethods = true
            )

        let constructorEmptyParameters exp =
            ProvidedConstructor(
                [], 
                InvokeCode = fun args -> <@@ %exp @@>
            )

        let configuredTests = parseFile pathToNickl

        let ``testsWithVariantsGreaterThan100%`` = 
            configuredTests 
            |> List.map (fun (name, variants) ->
                (name, variants |> List.map snd |> List.sum > 100)
            )
            |> List.filter snd

        let testsWithSameName =
            configuredTests
            |> Seq.groupBy fst
            |> Seq.filter (fun (name, tests) ->
                tests |> Seq.length > 1
            )

        let testsWithSameVariantNames =
            configuredTests
            |> Seq.filter(fun (_, variants) ->
                variants
                |> Seq.groupBy fst
                |> Seq.exists (fun (_, variants) ->
                    variants |> Seq.length > 1
                )
            )

        let errorGuard tests msg =
            if not(tests |> Seq.isEmpty)
            then failwith (sprintf msg (System.String.Join(", ", tests |> Seq.map fst)))

        errorGuard ``testsWithVariantsGreaterThan100%`` "tests %s have variants totalling more than 100 percent" 
        errorGuard testsWithSameName "duplicate names for tests %s" 
        errorGuard testsWithSameVariantNames "duplicate variants for tests %s" 

        let addIndividualTests() =
            for (name, variants) in configuredTests do
                let testtype = ProvidedTypeDefinition(name, Some typeof<obj>)
                testtype.HideObjectMethods <- true

                for (variant, weight) in variants do
                    testtype.AddMember(
                        ProvidedProperty(
                            variant, 
                            typeof<Variant>, 
                            IsStatic = true,
                            GetterCode = (fun args -> 
                                <@@ { Name = variant; Test = name; Weight = weight } @@>)
                        )
                    )

                provider.AddMember testtype

        let addAllVariantsProperty() =
            provider.AddMember(
                ProvidedProperty(
                    "AllVariants",
                    typeof<Variant list>,
                    IsStatic = true,
                    GetterCode = fun args -> 
                        <@@ parseFile pathToNickl
                            |> List.map(fun (test,variants) -> 
                                variants 
                                |> List.map(fun (variant,weight) -> 
                                    {   
                                        Name = variant; 
                                        Test = test;
                                        Weight = weight }
                                )
                            )
                            |> List.concat @@>
                )
            )

        addIndividualTests()
        addAllVariantsProperty()

        provider
    )

    do this.AddNamespace(baseNamespace, [multivariateProvider])

[<assembly:TypeProviderAssembly>]
do ()