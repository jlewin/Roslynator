﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactoring
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(DefaultCodeRefactoringProvider))]
    public class DefaultCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            SyntaxNode root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);
#if DEBUG
            try
            {
#endif
                await ComputeRefactoringsAsync(new RefactoringContext(context, root));
#if DEBUG
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().ToString());
                throw;
            }
#endif
        }

        private static async Task ComputeRefactoringsAsync(RefactoringContext context)
        {
            context.ComputeRefactoringsForNodeInsideTrivia();

            await context.ComputeRefactoringsForNodeAsync();

            context.ComputeRefactoringsForToken();

            context.ComputeRefactoringsForTrivia();
        }
    }
}