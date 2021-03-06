﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Extensions;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class RemovePropertyInitializerRefactoring
    {
        public static bool CanRefactor(RefactoringContext context, PropertyDeclarationSyntax propertyDeclaration)
        {
            return propertyDeclaration.Initializer != null
                && propertyDeclaration.Initializer.Span.Contains(context.Span);
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            PropertyDeclarationSyntax propertyDeclaration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            PropertyDeclarationSyntax newNode = propertyDeclaration
                .WithInitializer(null)
                .WithSemicolonToken(NoneToken())
                .WithTriviaFrom(propertyDeclaration)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(propertyDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}
