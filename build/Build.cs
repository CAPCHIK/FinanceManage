using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using BuildConfiguration;
using Microsoft.Build.Tasks;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Logger;


[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter]
    string BuildId { get; set; } = "no-build-id";

    private DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;
    private BuildInfo BuildInfo => new BuildInfo(StartTime, BuildId);

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    static AbsolutePath SourceDirectory => RootDirectory / "FinanceManage";
    static AbsolutePath TgBotOutputDirectory => RootDirectory / "deploy" / "tg_bot" / "build";
    static AbsolutePath SiteOutputDirectory => RootDirectory / "deploy" / "site" / "build";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(TgBotOutputDirectory);
            EnsureCleanDirectory(SiteOutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });
    Target PublishTgBot => _ => _
        .DependsOn(Restore, Clean)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject("FinanceManage.TelegramBot"))
                .SetConfiguration(Configuration)
                .SetOutput(TgBotOutputDirectory)
                .EnableNoRestore());
            CreateBuildInfo(TgBotOutputDirectory / "build.json");
            CopyFile(TgBotOutputDirectory / "build.json", TgBotOutputDirectory / "old.json");
        });
    Target PublishSite => _ => _
        .DependsOn(Restore, Clean)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject("FinanceManage.Site.Server"))
                .SetConfiguration(Configuration)
                .SetOutput(SiteOutputDirectory)
                .EnableNoRestore());
            CreateBuildInfo(SiteOutputDirectory / "build.json");
            InsertBuildInfoInAppSettings(SiteOutputDirectory / "wwwroot" / "appsettings.json");
        });


    Target Publish => _ => _
        .DependsOn(PublishTgBot, PublishSite)
        .Executes(() =>
        {
        });

    private void CreateBuildInfo(AbsolutePath pathToFole)
    {
        File.WriteAllText(pathToFole, JsonSerializer.Serialize(new { BuildInfo }));
    }
    private void InsertBuildInfoInAppSettings(AbsolutePath filePath)
    {
        var oldText = "{}";
        if (FileExists(filePath))
        {
            oldText = File.ReadAllText(filePath);
            DeleteFile(filePath);
        }
        var document = JsonSerializer.Deserialize<JsonDocument>(oldText);

        using var fileRewrite = File.OpenWrite(filePath);
        var options = new JsonWriterOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        using var utf8Writer = new Utf8JsonWriter(fileRewrite, options);

        var buildInfoExists = false;
        utf8Writer.WriteStartObject();
        foreach (var item in document.RootElement.EnumerateObject())
        {
            if (item.Name == nameof(BuildInfo))
            {
                continue;
            }
            utf8Writer.WritePropertyName(item.Name);
            item.Value.WriteTo(utf8Writer);
        }

        if (!buildInfoExists)
        {
            utf8Writer.WritePropertyName(nameof(BuildInfo));
            var buildInfoJson = JsonSerializer.Serialize(BuildInfo);
            JsonSerializer.Deserialize<JsonDocument>(buildInfoJson).WriteTo(utf8Writer);
        }

        utf8Writer.WriteEndObject();
        utf8Writer.Flush();
    }
}
