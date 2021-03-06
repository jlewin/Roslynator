﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Extensions;

namespace Roslynator.CSharp.Refactorings
{
    internal static class AvoidUsageOfTabRefactoring
    {
        public static void Analyze(SyntaxTreeAnalysisContext context)
        {
            SyntaxTree tree = context.Tree;

            SyntaxNode root;
            if (!tree.TryGetRoot(out root))
                return;

            foreach (SyntaxTrivia trivia in root.DescendantTrivia(descendIntoTrivia: true))
            {
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    string text = trivia.ToString();

                    for (int i = 0; i < text.Length; i++)
                    {
                        if (text[i] == '\t')
                        {
                            int index = i;

                            do
                            {
                                i++;

                            } while (i < text.Length && text[i] == '\t');

                            context.ReportDiagnostic(
                                DiagnosticDescriptors.AvoidUsageOfTab,
                                Location.Create(context.Tree, new TextSpan(trivia.SpanStart + index, i - index)));
                        }
                    }
                }
            }
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            TextSpan span,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var textChange = new TextChange(span, new string(' ', span.Length * 4));

            SourceText newSourceText = sourceText.WithChanges(textChange);

            return document.WithText(newSourceText);
        }
    }
}
