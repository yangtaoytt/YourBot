using System.Diagnostics;
using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using YourBot.Config.Implement.Level1.Service.Group.Command;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;

public class JmProcessor : IProcessorCore<GroupCommandData, NullSharedDataWrapper<JmConfig>, JmConfig> {
    public static IServiceAttribute<GroupCommandData> GetServiceAttribute() {
        return ReadGroupCommandAttribute.GetInstance();
    }

    public static NullSharedDataWrapper<JmConfig> Init(JmConfig initData) {
        return new NullSharedDataWrapper<JmConfig>(initData);
    }

    public static void Final(NullSharedDataWrapper<JmConfig> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(GroupCommandData data, NullSharedDataWrapper<JmConfig> sharedData,
        Logger2Event? logger) {
        await Task.CompletedTask;
        var groupUin = data.GroupUin;

        var config = sharedData.Execute(reference => reference.Value);

        if (!YourBotUtil.CheckSimpleGroupPermission(config, groupUin)) {
            return [];
        }

        var commandHandler = data.CommandHandler;

        if (commandHandler.Command != "jm") {
            return [];
        }

        int jmId;
        int? commandBlockSize = null;
        try {
            jmId = int.Parse(commandHandler.Next().Command);
            if (int.TryParse(commandHandler.TryNext()?.Command, out var tempBlockSize)) {
                commandBlockSize = tempBlockSize;
            }
        } catch (Exception) {
            return [YourBotUtil.SendToGroupMessage(groupUin, config.Priority, "wrong parameters")];
        }
        var basePath = Path.Combine(config.ImageDir, $"{jmId}");

        var blockSizeStr = commandBlockSize != null ? commandBlockSize.ToString() : "auto";
        var mosaicPath = Path.Combine(config.ImageDir, $"{jmId}.{blockSizeStr}");
        try {
            if (!Directory.Exists(basePath)) {
                var (isSuccess, errorMsg) =
                    await FetchImages(jmId, config.OptionFilePath, Environment.CurrentDirectory);
                if (!isSuccess && Directory.Exists(basePath)) {
                    Directory.Delete(basePath, true);
                }

                if (!isSuccess) {
                    return [YourBotUtil.SendToGroupMessage(groupUin, config.Priority, "jm:" + errorMsg)];
                }
            }
            
            if (!Directory.Exists(mosaicPath)) {
                var files = new DirectoryInfo(basePath).GetFiles();
                foreach (var file in files) {
                    int blockSize = 0;
                    if (commandBlockSize != null) {
                        blockSize = commandBlockSize.Value;
                    } else {
                        var imageInfo = await Image.IdentifyAsync(file.FullName);
                        blockSize = CalculateOptimalBlockSize(imageInfo.Width,
                            imageInfo.Height);
                    }
                    ApplyMosaic(file.FullName, file.FullName.Replace($"{jmId}", $"{jmId}.{blockSizeStr}"), blockSize);
                }
            }
        } catch (Exception e) {
            return [
                YourBotUtil.SendToGroupMessage(groupUin, config.Priority,
                    "Encountered an issue while process image: " + e.Message)
            ];
        }

        var messageChains = new List<MessageChain>();
        var dir = new DirectoryInfo(mosaicPath);
        try {
            var files = dir.GetFiles();
            messageChains.AddRange(files.OrderBy(f => f.Name)
                .Select(file => MessageBuilder.Group(groupUin).Image(file.FullName).Build()));
        } catch (Exception e) {
            return [
                YourBotUtil.SendToGroupMessage(groupUin, config.Priority,
                    "Encountered an issue while send image: " + e.Message)
            ];
        }

        var message = MessageBuilder.Group(groupUin).MultiMsg(messageChains.ToArray()).Build();
        return [
            CanSendGroupMessageAttribute.GetInstance()
                .GetCertificate(new SendToGroupMessageData(new Priority(config.Priority, PriorityStrategy.Share),
                    message))
        ];
    }

    public static int CalculateOptimalBlockSize(int imageWidth, int imageHeight, float density = 0.3f) {
        if (density <= 0 || density > 1)
            throw new ArgumentException("Density must be between 0.1 and 1.0");
        
        float geometricMean = MathF.Sqrt(imageWidth * imageHeight);

        float baseSize = geometricMean / (20f * density);

        int blockSize = (int)MathF.Round(baseSize);
        blockSize = Math.Max(4, blockSize);
        blockSize = Math.Min(100, blockSize);
        
        return blockSize % 2 == 0 ? blockSize + 1 : blockSize;
    }
    
    private static async Task<(bool isSuccess, string errorMsg)> FetchImages(int jmId, string optionFilePath,
        string workingDir) {
        var downloadSuccess = false;
        var downloadCompletedEvent = new ManualResetEvent(false);
        var lockObj = new object();

        using var pythonProcess = new Process();
        var startInfo = new ProcessStartInfo {
            FileName = "jmcomic",
            Arguments = $" --option \"{optionFilePath}\" {jmId}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false,
            WorkingDirectory = workingDir
        };

        pythonProcess.StartInfo = startInfo;

        pythonProcess.OutputDataReceived += (sender, e) => {
            if (string.IsNullOrEmpty(e.Data)) {
                return;
            }

            if (!e.Data.Contains("本子下载完成")) {
                return;
            }

            lock (lockObj) {
                downloadSuccess = true;
            }

            downloadCompletedEvent.Set();
        };

        pythonProcess.ErrorDataReceived += (sender, e) => {
            if (!string.IsNullOrEmpty(e.Data)) {
                downloadCompletedEvent.Set();
            }
        };

        try {
            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();


            var processTask = Task.Run(() => {
                pythonProcess.WaitForExit();
                return pythonProcess.ExitCode;
            });

            var completedTask = await Task.WhenAny(
                processTask,
                Task.Delay(TimeSpan.FromMinutes(5))
            );

            if (completedTask == processTask) {
                var exitCode = await processTask;
                bool success;
                lock (lockObj) {
                    success = downloadSuccess || exitCode == 0;
                }

                if (success) {
                    return (true, string.Empty);
                }

                return (false, $"jmImageDown Error, Exit Code: {exitCode}");
            }

            pythonProcess.Kill();
            return (false, "download timeout");
        } catch (Exception ex) {
            return (false, $"jmImageDown Error: {ex.Message}");
        }
    }

    
    private static void ApplyMosaic(string inputPath, string outputPath, int blockSize) {
        if (blockSize < 1) {
            throw new ArgumentException("Block size must be at least 1");
        }

        using var image = Image.Load<Rgba32>(inputPath);
        for (var y = 0; y < image.Height; y += blockSize) {
            for (var x = 0; x < image.Width; x += blockSize) {
                var currentBlockWidth = Math.Min(blockSize, image.Width - x);
                var currentBlockHeight = Math.Min(blockSize, image.Height - y);
                
                var averageColor = CalculateAverageColor(image, x, y, currentBlockWidth, currentBlockHeight);
                
                FillBlock(image, x, y, currentBlockWidth, currentBlockHeight, averageColor);
            }
        }

        var outDir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outDir)) {
            Directory.CreateDirectory(outDir!);
        }
        image.Save(outputPath);
    }

    private static Rgba32 CalculateAverageColor(
        Image<Rgba32> image,
        int startX,
        int startY,
        int width,
        int height) {
        long totalR = 0, totalG = 0, totalB = 0;
        var pixelCount = width * height;

        for (var y = startY; y < startY + height; y++) {
            for (var x = startX; x < startX + width; x++) {
                var pixel = image[x, y];
                totalR += pixel.R;
                totalG += pixel.G;
                totalB += pixel.B;
            }
        }

        return new Rgba32(
            (byte)(totalR / pixelCount),
            (byte)(totalG / pixelCount),
            (byte)(totalB / pixelCount));
    }

    private static void FillBlock(
        Image<Rgba32> image,
        int startX,
        int startY,
        int width,
        int height,
        Rgba32 color) {
        for (var y = startY; y < startY + height; y++) {
            for (var x = startX; x < startX + width; x++) {
                image[x, y] = color;
            }
        }
    }
}