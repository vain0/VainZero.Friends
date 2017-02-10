﻿namespace VainZero.Friends.Core

open System
open Basis.Core
open FParsec
open VainZero.Collections

module Parsing =
  module internal Internal =
    type BinaryTree<'x> =
      | Leaf
        of 'x
      | Node
        of BinaryTree<'x> * BinaryTree<'x>

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module BinaryTree =
      let rec toNonemptyList =
        function
        | Leaf x ->
          (x, [])
        | Node (l, r) ->
          let (lh, lt) = l |> toNonemptyList
          let (rh, rt) = r |> toNonemptyList
          (lh, List.append lt (rh :: rt))

    type Parser<'x> = Parser<'x, unit>

    let list1Parser p op =
      parse {
        let separatorParser =
          attempt (op |>> (fun () l r -> Node(l, r)))
        let! tree = chainl1 (p |>> Leaf) separatorParser
        return tree |> BinaryTree.toNonemptyList
      }

    let hagamoParser: Parser<unit> =
      skipAnyOf "はがも"

    let identifierParser: Parser<string> =
      many1Chars (letter <|> digit <|> pchar '_')

    let (termParser: Parser<Term>, termParserRef) =
      createParserForwardedToRef ()

    let atomOrVarTermParser =
      let predefinedVarNames =
        [|
          "あなた"; "きみ"; "だれ"
          "なに"; "あれ"; "これ"; "これら"; "それ"; "それら"
        |]
        |> set
      parse {
        let! name = identifierParser
        return
          if predefinedVarNames |> Set.contains name || name.StartsWith("_")
          then VarTerm (Variable.Create(name))
          else AtomTerm (Atom name)
      }

    let naturalTermParser =
      parse {
        let! digits = many1Chars digit
        do! notFollowedBy (letter <|> pchar '_')
        match Int32.TryParse(digits) with
        | (true, n) ->
          return Term.ofNatural n
        | (false, _) ->
          return! fail "Too large numeric literal."
      }

    let atomicTermParser =
      attempt naturalTermParser
      <|> atomOrVarTermParser

    let appTermParser =
      parse {
        let separatorParser = spaces1 >>. skipChar 'の' >>. spaces1
        let! (term, terms) = list1Parser atomicTermParser separatorParser
        let rec functors terms =
          parse {
            match terms with
            | [] ->
              return []
            | (AtomTerm atom :: terms) ->
              let! tail = functors terms
              return atom :: tail
            | _ ->
              return! fail "Functor must be an atom."
          }
        let! atoms = functors terms
        return atoms |> Seq.fold (fun term atom -> AppTerm (atom, term)) term
      }

    let listTermParser =
      parse {
        let separatorParser = spaces1 >>. skipChar 'と' >>. spaces1
        let! (term, terms) = list1Parser appTermParser separatorParser
        let! endsWithTail =
          (spaces1 >>. skipString "とか") |> attempt |> opt
          |>> Option.isSome
        return
          if terms |> List.isEmpty then
            term
          else if endsWithTail then
            let (terms, tailTerm) = (term, terms) |> NonemptyList.decomposeLast
            Term.listWithTailFromSeq tailTerm terms
          else
            (term :: terms) |> Term.listFromSeq
      }

    termParserRef :=
      listTermParser

    let atomicPropositionParser =
      parse {
        let! term = termParser
        do! spaces1 >>. hagamoParser >>. spaces1
        let! predicateName = identifierParser
        do! spaces1 >>. skipString "フレンズ"
        return (Predicate predicateName).[term]
      }

    let andPropositionParser =
      parse {
        let elementParser =
          parse {
            let! prop = atomicPropositionParser
            return AtomicProposition prop
          }
        let separatorParser =
          spaces1 >>. skipString "で" >>. spaces1
        let! (prop, props) = list1Parser elementParser separatorParser
        return
          if props |> List.isEmpty
          then prop
          else AndProposition (Vector.ofList (prop :: props))
      }

    let propositionParser =
      andPropositionParser

    let axiomRuleParser =
      parse {
        let! prop = atomicPropositionParser
        do! spaces >>. skipString "なんだね！"
        return AxiomRule prop
      }

    let inferRuleParser =
      parse {
        let! bodyProp = propositionParser
        do! spaces >>. skipString "なら" >>. spaces1
        let! headProp = atomicPropositionParser
        do! spaces >>. skipString "なんだね！"
        return InferRule (headProp, bodyProp)
      }

    let ruleParser =
      parse {
        do! skipString "すごーい！" >>. spaces1
        return! attempt axiomRuleParser <|> inferRuleParser
      }

    let queryParser =
      parse {
        let! prop = propositionParser
        do! spaces >>. skipString "なんだっけ？"
        return Query prop
      }

    let statementParser =
      attempt (ruleParser |>> Rule)
      <|> queryParser

    let inputParser =
      parse {
        do! spaces
        let! statement = statementParser
        do! spaces >>. eof
        return statement
      }

    let run source parser =
      match runParserOnString parser () "にゅうりょく" source with
      | Success (statement, (), _) ->
        statement |> Result.Success
      | Failure (message, _, _) ->
        message |> Result.Failure

  open Internal

  let parseTerm source =
    termParser |> run source

  let parseStatement source =
    inputParser |> run source
