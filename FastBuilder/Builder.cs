using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FastBuilder
{
    public class Builder
    {
        public static BuildResult BuildAndAnalyze(string path, string targetFile)
        {
            var projectName = Path.GetFileNameWithoutExtension(path);
            var result = new BuildResult();
            result.ProjectPath = path;

            var args = new List<string>
            {
                "build",
                path,
                "--verbosity", "d",
                "--configuration", "Debug",
                "--no-incremental",
                "--no-dependencies"
            };

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = Helpers.GetArgsString(args),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            proc.ErrorDataReceived += (sender, args) =>
            {
                result.Success = false;
                result.OutputLines.Add(args.Data);
            };

            result.OutputLines.Add(proc.StartInfo.Arguments);

            proc.Start();

            string proj = null;
            bool nextLineIsCsc = false;
            while(true)
            {
                if(!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine()?.Trim();
                    if(line == null) continue;

                    result.OutputLines.Add(line);

                    if(line.StartsWith(projectName))
                    {
                        result.TargetOutputFilePath = line.Split("->", StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                        result.Success = true;
                        continue;
                    }

                    var parts = line.Split('>', '\"');
                    if(parts.Length > 2 && parts[2] == "CoreCompile" && parts.Length > 6)
                    {
                        proj = parts[6];
                        continue;
                    }

                    if(parts.Length > 1 && parts[0] == "Task " && parts[1] == "Csc")
                    {
                        nextLineIsCsc = true;
                        continue;
                    }

                    if(parts.Length > 1 && parts[1] == "CoreCompile:")
                    {
                        nextLineIsCsc = true;
                        continue;
                    }

                    if(nextLineIsCsc)
                    {
                        nextLineIsCsc = false;
                        if(!line.Contains(" exec", StringComparison.Ordinal))
                        {
                            //Console.WriteLine("----------------------->");
                            continue;
                        }

                        ParseCscLine(result, proj, line);
                        continue;
                    }
                }

                if(proc.HasExited)
                {
                    break;
                }
            }

            if(result.Success)
            {
                result.OutputLines = null;
                CopyResult(result, targetFile);
            }

            return result;
        }

        private static void ParseCscLine(BuildResult mainResult, string proj, string line)
        {
            BuildResult result = null;
            if(mainResult.ProjectPath == proj)
            {
                result = mainResult;
            }
            else
            {
                result = new BuildResult()
                {
                    ProjectPath = proj
                };
                mainResult.SubResults.Add(proj, result);
            }

            result.CscString = line;

            result.OutputStart = line.IndexOf(" /out:", StringComparison.Ordinal) + " /out:".Length;
            result.OutputEnd = line.IndexOf(" /", result.OutputStart, StringComparison.Ordinal);
            result.OutputCount = result.OutputEnd - result.OutputStart;

            result.OutputFilePath = line.Substring(result.OutputStart, result.OutputCount);

            result.ArgsStart = line.IndexOf(" exec", StringComparison.Ordinal);
            result.CscFile = line.Substring(0, result.ArgsStart);
        }

        public static bool BuildFast(BuildResult buildResult, string targetFile)
        {
            var str = buildResult.CscString;

            if(targetFile != null && targetFile != buildResult.OutputFilePath)
            {
                str = str.Remove(buildResult.OutputStart, buildResult.OutputEnd - buildResult.OutputStart);
                str = str.Insert(buildResult.OutputStart, targetFile);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = buildResult.CscFile,
                Arguments = str.Substring(buildResult.ArgsStart + 1),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = buildResult.ProjectDirectory
            };

            var proc = new Process
            {
                StartInfo = startInfo
            };

            bool success = true;
            proc.ErrorDataReceived += (sender, args) =>
            {
                success = false;
                Console.WriteLine(args.Data);
            };

            proc.Start();

            while(true)
            {
                if(!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    //Console.WriteLine(line);
                }

                if(proc.HasExited)
                {
                    break;
                }
            }

            if(success)
            {
                //CopyResult(buildResult, targetFile);
            }

            return success;
        }

        private static void CopyResult(BuildResult buildResult, string targetFile)
        {
            var from = buildResult.OutputFilePath;
            var directory = buildResult.ProjectDirectory;

            if(targetFile == null) return;
            if(!File.Exists(from)) from = Path.Join(directory, from);
            if(!File.Exists(from)) return;
            if(targetFile == from) return;
            File.Move(from, targetFile, true);
        }
    }


    public class BuildResult
    {
        public string ProjectPath;
        public string ProjectName      => Path.GetFileNameWithoutExtension(ProjectPath);
        public string ProjectDirectory => Path.GetDirectoryName(ProjectPath);

        public string   CscString;
        public bool     Success;
        public string[] Sources;

        public string OutputFilePath;
        public int    OutputStart;
        public int    OutputEnd;

        public int    OutputCount;
        public string CscFile;
        public string TargetOutputFilePath;

        public int ArgsStart;

        public Dictionary<string, BuildResult> SubResults = new();

        public List<string> OutputLines = new List<string>(50_000);
    }
}