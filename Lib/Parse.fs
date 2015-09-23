﻿namespace Zander.Internal
open System
type ColumnRecognizer=(NumberOf*BlockType) list
type RowRecognizer =(NumberOf*ColumnRecognizer*string)
type BlockRecognizer=RowRecognizer list

module Parse=

    type Token={ value:string; block:BlockType }
        with
            override self.ToString()=
                match self.block with
                    | Empty -> "Empty"
                    | Const c -> sprintf "'%s'" c
                    | Value v -> sprintf "@%s = %s" v self.value
            static member tryValue (t:Token)=
                match t.block with
                    | Value (name) -> Some (t.value)
                    | _ -> None
            static member tryKeyValue (t:Token)=
                match t.block with
                    | Value (name) -> Some (name, t.value)
                    | _ -> None


    type Result=
        | Ok of Token
        | WrongConstant of (string * string)
        | UnRecognized of string
        | Missing 
        with
            static member isOk result=
                match result with
                    | Ok v -> true
                    | _ -> false
            static member tryValue result=
                match result with
                    | Ok v -> Some v
                    | _ -> None
            static member value result=
                match result with
                    | Ok v -> v
                    | _ -> failwithf "Not ok result %s!" (result.ToString())

    type RecognizedBlock=((Result list*string) list)
    type ColumnsAndPosition = InputAndPosition<string list>
    type ResultAndLength = (Result*int)
    open Zander.Internal.Matches

    let expression (expr:ColumnRecognizer) row : Result list=
        let columnMatch (columnExpr:BlockType) column=
            let value = { value=column;block= columnExpr }
            match columnExpr, column with
                | Empty, "" -> Ok( value )
                | Const v1,v2 -> 
                    if v1=v2 then 
                        Ok( value )
                    else
                        WrongConstant (v1, v2)
                | Value n, v -> Ok( value )
                | _, v-> UnRecognized v
        let toResult matchResult = 
            match matchResult with
                | MatchOk v -> v
                | MatchEmptyList -> Missing
                | MatchFailure -> failwith "match failure"

        matches columnMatch Result.isOk expr row |> List.map toResult

    let block (expr:BlockRecognizer) index blocks : RecognizedBlock=

        let rec bmatch idx eidx : (Result list*string) list=
            let erow = expr |> List.tryItem eidx
            let row = blocks |> List.tryItem (index + idx)

            let is_ok (r : (Result list*string) list)=
                r |> List.collect fst
                  |> List.forall Result.isOk

            let rest_of c e n : (Result list*string) list=
                // List.takeWhile ?
                // TODO: !
                let isRepeat = match c with | One -> false ; | _ -> true
                let r = bmatch (idx+1) eidx 
                if isRepeat && is_ok r then
                    r
                else
                    bmatch (idx+1) (eidx+1)

            match erow, row with
                | Some (count,column,name), Some r -> 
                    let h = [((expression column r), name)]
                    let r = rest_of count column name
                    h @ r
                | None, None -> []
                | Some (_,_,n), None -> [[Missing], n]
                | None, Some r -> []
        
        bmatch 0 0

    let rowsOf v = 
        let valuesOf v' =
            v'
            |> List.choose Result.tryValue
            |> List.choose Token.tryValue

        v |> List.map (
             fun (row,name)-> (valuesOf row) , name
             )

