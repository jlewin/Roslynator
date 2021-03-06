﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Extensions;
using Roslynator.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ExpandEventRefactoring
    {
        public static bool CanRefactor(EventFieldDeclarationSyntax eventDeclaration)
        {
            return eventDeclaration.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration)
                && eventDeclaration.Declaration?.Variables.Count == 1;
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            EventFieldDeclarationSyntax eventDeclaration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            EventDeclarationSyntax newNode = ExpandEvent(eventDeclaration)
                .WithTriviaFrom(eventDeclaration)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(eventDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static EventDeclarationSyntax ExpandEvent(EventFieldDeclarationSyntax eventDeclaration)
        {
            AccessorListSyntax accessorList = AccessorList(
                AccessorDeclaration(SyntaxKind.AddAccessorDeclaration, Block()),
                AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration, Block()));

            accessorList = Remover.RemoveWhitespaceOrEndOfLine(accessorList)
                .WithCloseBraceToken(accessorList.CloseBraceToken.WithLeadingTrivia(NewLineTrivia()));

            VariableDeclaratorSyntax declarator = eventDeclaration.Declaration.Variables[0];

            return EventDeclaration(
                eventDeclaration.AttributeLists,
                eventDeclaration.Modifiers,
                eventDeclaration.Declaration.Type,
                default(ExplicitInterfaceSpecifierSyntax),
                declarator.Identifier,
                accessorList);
        }
    }
}
