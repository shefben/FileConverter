using System.Diagnostics;
using FileConverter.CustomConverters;

namespace FileConverter.ConversionJobs
{
    public class ConversionJob_Custom : ConversionJob
    {
        private readonly CustomConverterDefinition definition;

        public ConversionJob_Custom(ConversionPreset preset, string inputPath, CustomConverterDefinition def)
            : base(preset, inputPath)
        {
            this.definition = def;
        }

        protected override void Convert()
        {
            string args = this.definition.Arguments ?? string.Empty;
            args = args.Replace("{input}", this.InputFilePath).Replace("{output}", this.OutputFilePath);

            if (this.definition.Variables != null)
            {
                foreach (var kv in this.definition.Variables)
                {
                    args = args.Replace("{" + kv.Key + "}", kv.Value ?? string.Empty);
                }
            }

            if (this.definition.Options != null)
            {
                foreach (var opt in this.definition.Options)
                {
                    if (!opt.EvaluateCondition(this.Preset))
                    {
                        args = args.Replace("{" + opt.Name + "}", string.Empty);
                        continue;
                    }

                    string value = this.Preset.GetSettingsValue(opt.Name) ?? string.Empty;
                    args = args.Replace("{" + opt.Name + "}", value);
                }
            }

            var psi = new ProcessStartInfo(this.definition.ProgramPath, args)
            {
                UseShellExecute = false,
                RedirectStandardError = !this.definition.ShowProgram,
                RedirectStandardOutput = !this.definition.ShowProgram,
                CreateNoWindow = !this.definition.ShowProgram,
            };

            Process proc = Process.Start(psi);
            string stdOut = null;
            string stdErr = null;
            if (!this.definition.ShowProgram)
            {
                stdOut = proc.StandardOutput.ReadToEnd();
                stdErr = proc.StandardError.ReadToEnd();
            }
            proc.WaitForExit();

            if (proc.ExitCode == 0 && System.IO.File.Exists(this.OutputFilePath))
            {
                if (!string.IsNullOrEmpty(this.definition.PostProcessCommand))
                {
                    try
                    {
                        var postPsi = new ProcessStartInfo(this.definition.PostProcessCommand)
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        };
                        var postProc = Process.Start(postPsi);
                        postProc.WaitForExit();
                    }
                    catch (System.Exception ex)
                    {
                        Diagnostics.Debug.Log($"Post process failed: {ex.Message}");
                    }
                }
                this.OnConversionSucceed();
            }
            else
            {
                string errorMsg = $"External converter failed with code {proc.ExitCode}";
                if (!string.IsNullOrEmpty(stdErr))
                {
                    errorMsg += ": " + stdErr.Trim();
                }
                this.ConversionFailed(errorMsg);
            }
        }
    }
}
