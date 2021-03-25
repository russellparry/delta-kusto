﻿using DeltaKustoIntegration.Parameterization;
using DeltaKustoLib;
using DeltaKustoLib.CommandModel;
using DeltaKustoLib.KustoModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeltaKustoIntegration.Database
{
    public class ScriptDatabaseProvider : IDatabaseProvider
    {
        private readonly IFileGateway _fileGateway;
        private readonly IImmutableList<SourceFileParametrization> _scripts;

        public ScriptDatabaseProvider(
            IFileGateway fileGateway,
            SourceFileParametrization[] scripts)
        {
            _fileGateway = fileGateway;
            _scripts = scripts.ToImmutableArray();
        }

        async Task<DatabaseModel> IDatabaseProvider.RetrieveDatabaseAsync(
            CancellationToken ct)
        {
            var scriptTasks = _scripts
                .Select(s => LoadScriptsAsync(s, ct));

            await Task.WhenAll(scriptTasks);

            var commands = scriptTasks
                .SelectMany(t => t.Result)
                .SelectMany(s => CommandBase.FromScript(s))
                .ToImmutableArray();
            var database = DatabaseModel.FromCommands(commands);

            return database;
        }

        private async Task<IEnumerable<string>> LoadScriptsAsync(
            SourceFileParametrization fileParametrization,
            CancellationToken ct)
        {
            if (fileParametrization.FilePath != null)
            {
                var script = await _fileGateway.GetFileContentAsync(
                    fileParametrization.FilePath,
                    ct);

                return new[] { script };
            }
            else if (fileParametrization.FolderPath != null)
            {
                var scripts = _fileGateway.GetFolderContentsAsync(
                    fileParametrization.FolderPath,
                    fileParametrization.Extensions,
                    ct);
                var contents = (await scripts.ToEnumerableAsync())
                    .Select(t => t.content);

                return contents;
            }
            else
            {
                throw new InvalidOperationException("We should never get here");
            }
        }
    }
}