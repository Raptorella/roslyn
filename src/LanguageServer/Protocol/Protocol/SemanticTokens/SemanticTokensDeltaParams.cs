﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Roslyn.LanguageServer.Protocol
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Parameters for a request for Edits that can be applied to a previous response
    /// from a semantic tokens Document provider.
    ///
    /// See the <see href="https://microsoft.github.io/language-server-protocol/specifications/specification-current/#semanticTokensDeltaParams">Language Server Protocol specification</see> for additional information.
    /// </summary>
    internal class SemanticTokensDeltaParams : ITextDocumentParams, IPartialResultParams<SemanticTokensDeltaPartialResult>
    {
        /// <summary>
        /// Gets or sets an identifier for the document to fetch semantic tokens from.
        /// </summary>
        [JsonPropertyName("textDocument")]
        public TextDocumentIdentifier TextDocument { get; set; }

        /// <summary>
        /// Gets or sets a property indicating the version of the semantic
        /// tokens Document provider response that the edits will be applied to.
        /// </summary>
        [JsonPropertyName("previousResultId")]
        public string PreviousResultId { get; set; }

        /// <summary>
        /// Gets or sets the value of the Progress instance.
        /// </summary>
        [JsonPropertyName(Methods.PartialResultTokenName)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IProgress<SemanticTokensDeltaPartialResult>? PartialResultToken
        {
            get;
            set;
        }
    }
}
