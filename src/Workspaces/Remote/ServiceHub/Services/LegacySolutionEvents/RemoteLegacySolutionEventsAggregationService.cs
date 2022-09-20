﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.LegacySolutionEvents;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.Remote
{
    internal sealed class RemoteLegacySolutionEventsAggregationService : BrokeredServiceBase, IRemoteLegacySolutionEventsAggregationService
    {
        internal sealed class Factory : FactoryBase<IRemoteLegacySolutionEventsAggregationService>
        {
            protected override IRemoteLegacySolutionEventsAggregationService CreateService(in ServiceConstructionArguments arguments)
                => new RemoteLegacySolutionEventsAggregationService(arguments);
        }

        public RemoteLegacySolutionEventsAggregationService(in ServiceConstructionArguments arguments)
            : base(arguments)
        {
        }

        public ValueTask OnTextDocumentOpenedAsync(Checksum solutionChecksum, DocumentId documentId, CancellationToken cancellationToken)
        {
            return RunServiceAsync(solutionChecksum, async solution =>
            {
                var aggregationService = solution.Services.GetRequiredService<ILegacySolutionEventsAggregationService>();
                await aggregationService.OnTextDocumentOpenedAsync(
                    new TextDocumentEventArgs(solution.GetRequiredDocument(documentId)), cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        public ValueTask OnTextDocumentClosedAsync(Checksum solutionChecksum, DocumentId documentId, CancellationToken cancellationToken)
        {
            return RunServiceAsync(solutionChecksum, async solution =>
            {
                var aggregationService = solution.Services.GetRequiredService<ILegacySolutionEventsAggregationService>();
                await aggregationService.OnTextDocumentClosedAsync(
                    new TextDocumentEventArgs(solution.GetRequiredDocument(documentId)), cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        public ValueTask OnWorkspaceChangedAsync(
            Checksum oldSolutionChecksum,
            Checksum newSolutionChecksum,
            WorkspaceChangeKind kind,
            ProjectId? projectId,
            DocumentId? documentId,
            CancellationToken cancellationToken)
        {
            return RunServiceAsync(oldSolutionChecksum, newSolutionChecksum,
                async (oldSolution, newSolution) =>
                {
                    var aggregationService = oldSolution.Services.GetRequiredService<ILegacySolutionEventsAggregationService>();
                    await aggregationService.OnWorkspaceChangedAsync(
                        new WorkspaceChangeEventArgs(kind, oldSolution, newSolution, projectId, documentId), cancellationToken).ConfigureAwait(false);
                }, cancellationToken);
        }
    }
}
