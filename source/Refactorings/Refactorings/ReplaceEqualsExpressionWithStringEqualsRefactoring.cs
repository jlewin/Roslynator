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
    internal static class ReplaceEqualsExpressionWithStringEqualsRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression))
            {
                ExpressionSyntax left = binaryExpression.Left;

                if (left?.IsMissing == false
                    && !left.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    ExpressionSyntax right = binaryExpression.Right;

                    if (right?.IsMissing == false
                        && !right.IsKind(SyntaxKind.NullLiteralExpression))
                    {
                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                        ITypeSymbol leftSymbol = semanticModel.GetTypeInfo(left, context.CancellationToken).ConvertedType;

                        if (leftSymbol?.IsString() == true)
                        {
                            ITypeSymbol rightSymbol = semanticModel.GetTypeInfo(right, context.CancellationToken).ConvertedType;

                            if (rightSymbol?.IsString() == true)
                            {
                                string title = (binaryExpression.IsKind(SyntaxKind.EqualsExpression))
                                    ? "Replace == with string.Equals"
                                    : "Replace != with !string.Equals";

                                context.RegisterRefactoring(
                                    title,
                                    cancellationToken => RefactorAsync(context.Document, binaryExpression, cancellationToken));
                            }
                        }
                    }
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            INamedTypeSymbol symbol = semanticModel.GetTypeByMetadataName(MetadataNames.System_StringComparison);

            IFieldSymbol fieldSymbol = GetDefaultFieldSymbol(symbol);

            ExpressionSyntax newNode = SimpleMemberInvocationExpression(
                StringType(),
                "Equals",
                ArgumentList(
                    Argument(binaryExpression.Left),
                    Argument(binaryExpression.Right),
                    Argument(
                        SimpleMemberAccessExpression(
                            ParseName(MetadataNames.System_StringComparison).WithSimplifierAnnotation(),
                            IdentifierName(fieldSymbol.Name)))));

            if (binaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
                newNode = LogicalNotExpression(newNode);

            newNode = newNode
                .WithTriviaFrom(binaryExpression)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(binaryExpression, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static IFieldSymbol GetDefaultFieldSymbol(INamedTypeSymbol symbol)
        {
            foreach (IFieldSymbol fieldSymbol in symbol.GetFields())
            {
                if (fieldSymbol.HasConstantValue)
                {
                    object constantValue = fieldSymbol.ConstantValue;

                    if (constantValue is int)
                    {
                        var value = (int)constantValue;

                        if (value == 0)
                            return fieldSymbol;
                    }
                }
            }

            return null;
        }
    }
}