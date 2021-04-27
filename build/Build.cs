using System;
using System.Linq;
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

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

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
        });
    Target Publish => _ => _
        .DependsOn(PublishTgBot, PublishSite)
        .Executes(() =>
        {
        });
}
