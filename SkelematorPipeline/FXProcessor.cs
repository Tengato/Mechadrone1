using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;


namespace SkelematorPipeline
{
    [ContentProcessor(DisplayName = "Skelemator FX Processor")]
    public class FXProcessor : ContentProcessor<EffectContent, CompiledEffectContent>
    {
        const string defaultFXCPath = @"..\..\External\Windows\DirectXUtils\fxc.exe";

        public string PathToFXCompiler { get; set; }

        public override CompiledEffectContent Process(EffectContent input, ContentProcessorContext context)
        {
            string fxFileName = input.Identity.SourceFilename;
            string tempOutputFileName = Path.GetTempFileName();

            byte[] compiledFx;

            try
            {
                List<string> args = new List<string>();
                args.Add("/T fx_2_0");
                args.Add(String.Format("\"{0}\"", fxFileName));
                args.Add(String.Format("/Fo \"{0}\"", tempOutputFileName));

                ProcessStartInfo startInfo = new ProcessStartInfo();

                if (String.IsNullOrEmpty(PathToFXCompiler))
                    PathToFXCompiler = Path.Combine(Environment.CurrentDirectory, defaultFXCPath);

                startInfo.FileName = PathToFXCompiler;
                startInfo.Arguments = String.Join(" ", args);
                context.Logger.LogMessage("Compile command: {0} {1}", startInfo.FileName, startInfo.Arguments);
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardError = true;

                using (Process process = System.Diagnostics.Process.Start(startInfo))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        context.Logger.LogMessage(reader.ReadToEnd());
                    }

                    using (StreamReader reader = process.StandardError)
                    {
                        context.Logger.LogImportantMessage(reader.ReadToEnd());
                    }
                }

                compiledFx = File.ReadAllBytes(tempOutputFileName);
            }
            finally
            {
                File.Delete(tempOutputFileName);
            }

            // This doesn't seem to work for Xbox...
            byte[] xnaHeader = new byte[] { 0xCF, 0x0B, 0xF0, 0xBC, 0x10, 0x00, 0x00, 0x00,
                                            0x38, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00 };

            byte[] fxcPlusXnaHeader = new byte[ xnaHeader.Length + compiledFx.Length ];
            Array.Copy(xnaHeader, fxcPlusXnaHeader, xnaHeader.Length);
            Array.Copy(compiledFx, 0, fxcPlusXnaHeader, xnaHeader.Length, compiledFx.Length);

            CompiledEffectContent cec = new CompiledEffectContent(fxcPlusXnaHeader);
            cec.Identity = input.Identity;
            cec.Name = input.Name;

            return cec;
        }
    }
}