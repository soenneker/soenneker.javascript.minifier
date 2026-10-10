using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Esbuild.Util;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.JavaScript.Minifier.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.JavaScript.Minifier.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class JavaScriptMinifierTests : HostedUnitTest
{
    private readonly IJavaScriptMinifier _util;

    public JavaScriptMinifierTests(Host host) : base(host) => _util = Resolve<IJavaScriptMinifier>(true);

    [Test]
    public async Task Minify_reduces_module_source()
    {
        const string source = "export function add(firstValue, secondValue) { /* remove this */ return firstValue + secondValue; }";
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        string result = await _util.Minify(source, timeout.Token).NoSync();
        Check(result.Length < source.Length && result.Contains("export") && !result.Contains("remove this"), "JavaScript was not minified as a module.");
    }

    [Test]
    public async Task Empty_input_and_cancellation()
    {
        Check(await _util.Minify(null).NoSync() == string.Empty, "Null input should be empty.");
        Check(await _util.Minify(" \r\n\t").NoSync() == string.Empty, "Whitespace input should be empty.");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        try
        {
            await _util.Minify("", cancellation.Token).NoSync();
            throw new Exception("Cancellation was ignored.");
        }
        catch (OperationCanceledException) { }
    }

    [Test]
    public async Task Files_minify_and_save_only_after_success()
    {
        string directory = Path.Combine(Path.GetTempPath(), "javascript minifier " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        CancellationToken ct = timeout.Token;
        try
        {
            string sourcePath = Path.Combine(directory, "input.js");
            string outputPath = Path.Combine(directory, "nested", "output.min.js");
            const string source = "function sum(first, second) { /* remove */ return first + second; } console.log(sum(1, 2));";
            await File.WriteAllTextAsync(sourcePath, source, ct).NoSync();
            string result = await _util.MinifyFile(sourcePath, false, ct).NoSync();
            Check(result.Length < source.Length, "File was not minified.");
            Check(await File.ReadAllTextAsync(sourcePath, ct).NoSync() == source, "MinifyFile changed its input.");
            await _util.SaveMinifiedFile(sourcePath, outputPath, false, ct).NoSync();
            Check(await File.ReadAllTextAsync(outputPath, ct).NoSync() == result, "Destination content differs.");
            await _util.SaveMinifiedFile(sourcePath, log: false, cancellationToken: ct).NoSync();
            Check(await File.ReadAllTextAsync(sourcePath, ct).NoSync() == result, "In-place save failed.");
            const string invalid = "const = ;";
            await File.WriteAllTextAsync(sourcePath, invalid, ct).NoSync();
            try
            {
                await _util.SaveMinifiedFile(sourcePath, outputPath, false, ct).NoSync();
                throw new Exception("Invalid JavaScript unexpectedly succeeded.");
            }
            catch (EsbuildException ex)
            {
                Check(ex.Result.ExitCode != 0 && ex.Result.StandardError.Contains("input.js"), "Source diagnostics missing.");
            }
            Check(await File.ReadAllTextAsync(sourcePath, ct).NoSync() == invalid, "Invalid input was modified.");
            Check(await File.ReadAllTextAsync(outputPath, ct).NoSync() == result, "Failure overwrote destination.");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
