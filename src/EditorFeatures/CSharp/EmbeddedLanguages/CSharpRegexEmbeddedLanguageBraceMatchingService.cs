﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Composition;
using Microsoft.CodeAnalysis.BraceMatching;
using Microsoft.CodeAnalysis.CSharp.EmbeddedLanguages.LanguageServices;
using Microsoft.CodeAnalysis.Features.EmbeddedLanguages.Json.LanguageServices;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Editor.CSharp.EmbeddedLanguages
{
    [ExportEmbeddedLanguageBraceMatchingServiceInternal(
        PredefinedEmbeddedLanguageBraceMatchingServiceNames.Regex, LanguageNames.CSharp, supportsUnannotatedAPIs: true, "Regex", "Regexp"), Shared]
    internal sealed class CSharpRegexEmbeddedLanguageBraceMatchingService :
        AbstractJsonEmbeddedLanguageBraceMatchingService
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public CSharpRegexEmbeddedLanguageBraceMatchingService()
            : base(CSharpEmbeddedLanguagesProvider.Info)
        {
        }
    }
}
