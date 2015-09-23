﻿namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

[<TestFixture>] 
module NumberedMatchTests = 
    open Zander.Internal.Matches

    let zeroToThree = (fun m a -> 
                                match m, a with
                                    | '0', 0->Some "0"
                                    | '1', 1->Some "1"
                                    | '2', 2->Some "2"
                                    | '3', 3->Some "3"
                                    | _,_ -> None
                                )
    let someIsOk = Option.isSome

    [<Test>] 
    let ``Match 1 on 1`` ()=
        let l = matches zeroToThree someIsOk [ (One, '1'); (One,'2'); (One,'3'); ]
                              [1;2;3] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "2");(Some "3")]

    [<Test>] 
    let ``Match many 1`` ()=
        let l = matches zeroToThree someIsOk [ (Many, '1'); ]
                              [1;1;1] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "1");(Some "1")]

    [<Test>] 
    let ``Match zero or many 1`` ()=
        let l = matches zeroToThree someIsOk [ (ZeroOrMany, '1'); ]
                              [1;1;1] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "1");(Some "1")]

    [<Test>] 
    let ``Match zero or many 1 when empty`` ()=
        let l = matches zeroToThree someIsOk [ (ZeroOrMany, '1'); ]
                              []
        l |> should equal []