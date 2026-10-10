using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Esbuild.Util.Abstract;
using Soenneker.Esbuild.Util.Dtos;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.JavaScript.Minifier.Abstract;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Abstract;

namespace Soenneker.JavaScript.Minifier;

public sealed class JavaScriptMinifier : IJavaScriptMinifier
{
    private readonly IEsbuildUtil _esbuildUtil;
    private readonly IFileUtil _fileUtil;
    private readonly IDirectoryUtil _directoryUtil;

    public JavaScriptMinifier(IEsbuildUtil esbuildUtil, IFileUtil fileUtil, IDirectoryUtil directoryUtil)
    {
        _esbuildUtil = esbuildUtil;
        _fileUtil = fileUtil;
        _directoryUtil = directoryUtil;
    }

    public ValueTask<string> Minify(string? javaScript, CancellationToken cancellationToken = default) =>
        MinifyCore(javaScript, null, cancellationToken);

    public async ValueTask<string> MinifyFile(string filePath, bool log = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        string javaScript = await _fileUtil.Read(filePath, log, cancellationToken).NoSync();
        return await MinifyCore(javaScript, filePath, cancellationToken).NoSync();
    }

    public async ValueTask SaveMinifiedFile(string sourcePath, string? destinationPath = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        if (destinationPath != null) ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        string minified = await MinifyFile(sourcePath, log, cancellationToken).NoSync();
        string targetPath = Path.GetFullPath(destinationPath ?? sourcePath);
        await _directoryUtil.Create(Path.GetDirectoryName(targetPath)!, log, cancellationToken).NoSync();
        await _fileUtil.Write(targetPath, minified, log, cancellationToken).NoSync();
    }

    private async ValueTask<string> MinifyCore(string? javaScript, string? sourcefile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(javaScript)) return string.Empty;

        EsbuildCommandResult result = await _esbuildUtil.Transform(javaScript, new EsbuildTransformOptions
        {
            Loader = "js",
            Minify = true,
            Sourcefile = sourcefile
        }, cancellationToken).NoSync();
        return result.StandardOutput;
    }
}
