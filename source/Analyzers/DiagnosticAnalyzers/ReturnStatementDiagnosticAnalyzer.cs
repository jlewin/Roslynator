﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Analyzers;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReturnStatementDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.MergeLocalDeclarationWithReturnStatement,
                    DiagnosticDescriptors.MergeLocalDeclarationWithReturnStatementFadeOut,
                    DiagnosticDescriptors.ReplaceReturnStatementWithExpressionStatement,
                    DiagnosticDescriptors.ReplaceReturnStatementWithExpressionStatementFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.RegisterSyntaxNodeAction(f => AnalyzeReturnStatement(f), SyntaxKind.ReturnStatement);
        }

        private void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
        {
            if (GeneratedCodeAnalyzer?.IsGeneratedCode(context) == true)
                return;

            var returnStatement = (ReturnStatementSyntax)context.Node;

            MergeLocalDeclarationWithReturnStatementAnalyzer.Analyze(context);

            if (ReplaceReturnStatementWithExpressionStatementRefactoring.CanRefactor(returnStatement, context.SemanticModel, context.CancellationToken)
                && !returnStatement.ContainsDirectives(TextSpan.FromBounds(returnStatement.ReturnKeyword.Span.End, returnStatement.Expression.Span.Start)))
            {
                context.ReportDiagnostic(DiagnosticDescriptors.ReplaceReturnStatementWithExpressionStatement, returnStatement.GetLocation(), "return");

                context.FadeOutToken(DiagnosticDescriptors.ReplaceReturnStatementWithExpressionStatementFadeOut, returnStatement.ReturnKeyword);
            }
        }
    }
}
