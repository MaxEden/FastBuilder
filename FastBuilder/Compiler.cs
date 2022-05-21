using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FastBuilder
{
    public class Compiler
    {
        private readonly Dictionary<string, CompilerProjectInfo>  _projectInfos = new();
        private readonly Dictionary<string, CompilerDocumentInfo> _docInfos     = new();

        public string BuildProject(string path, string targetFile = null, bool noDependencies = true)
        {
            path = Path.GetFullPath(path);
            
            if(!_projectInfos.TryGetValue(path, out var pi))
            {
                var result = Builder.BuildAndAnalyze(path, targetFile);

                if(!result.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"FastBuilder: {Path.GetFileName(path)} has failed!");
                    foreach(var outputLine in result.OutputLines)
                    {
                        Console.Error.WriteLine(outputLine);
                    }
                    Console.ResetColor();
                    _projectInfos.Remove(path);
                    return null;
                }

                AddResult(result);
                foreach(var subResult in result.SubResults.Values)
                {
                    AddResult(subResult);
                }

                Console.WriteLine($"FastBuilder: -> {result.TargetOutputFilePath}");
                return result.TargetOutputFilePath;
            }
            else
            {
                pi = _projectInfos[path];
                if(pi.OutputFilePath == null) pi.OutputFilePath = targetFile;
                if(pi.OutputFilePath == null) pi.OutputFilePath = pi.BuildResult.TargetOutputFilePath;

                Builder.BuildFast(pi.BuildResult, pi.OutputFilePath);
                Console.WriteLine($"FastBuilder: fast -> { pi.OutputFilePath}");
                return pi.OutputFilePath;
            }
        }

        private void AddResult(BuildResult result)
        {
            var pi = new CompilerProjectInfo()
            {
                Path = result.ProjectPath,
                Directory = Path.GetDirectoryName(result.ProjectPath),
                BuildResult = result
            };

            result.Sources = Helpers.SearchDown(pi.Directory, "*.cs", "bin", "obj");

            foreach(var source in result.Sources)
            {
                _docInfos[source] = new CompilerDocumentInfo()
                {
                    ProjInfo = pi
                };
            }

            _projectInfos[pi.Path] = pi;
        }

        public void FileChanged(string path)
        {
            if(!_docInfos.TryGetValue(path, out var docInfo))
            {
                var projInfo = _projectInfos.Values.FirstOrDefault(p => path.StartsWith(p.Directory));
                if(projInfo == null) return;
                ProjectChanged(projInfo.Path);
            }
        }

        public void ProjectChanged(string path)
        {
            if(!_projectInfos.TryGetValue(path, out var projInfo)) return;

            _projectInfos.Remove(projInfo.Path);

            foreach(var source in projInfo.BuildResult.Sources)
            {
                _docInfos.Remove(source);
            }
        }
    }

    class CompilerProjectInfo
    {
        public string      Directory;
        public string      Path;
        public string      OutputFilePath;
        public BuildResult BuildResult;
    }

    class CompilerDocumentInfo
    {
        public CompilerProjectInfo ProjInfo;
    }
}