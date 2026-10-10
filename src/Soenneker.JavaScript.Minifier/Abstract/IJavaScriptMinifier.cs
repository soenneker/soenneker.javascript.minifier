using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.JavaScript.Minifier.Abstract;

/// <summary>Minifies JavaScript through esbuild without bundling imports or changing the module format.</summary>
public interface IJavaScriptMinifier
{
    /// <summary>Minifies JavaScript whitespace, identifiers, and syntax. Null or whitespace input returns an empty string.</summary>
    /// <param name="javaScript">JavaScript source, including scripts or ES modules.</param>
    /// <param name="cancellationToken">Cancels installation or minification.</param>
    /// <returns>Minified JavaScript. License comments are retained according to esbuild's default behavior.</returns>
    /// <remarks>Uses the latest esbuild through IEsbuildUtil. Invalid JavaScript propagates EsbuildException with CLI diagnostics.</remarks>
    ValueTask<string> Minify(string? javaScript, CancellationToken cancellationToken = default);

    /// <summary>Reads a JavaScript file and returns minified content without modifying the file.</summary>
    /// <param name="filePath">Path to the source file; also included in esbuild diagnostics.</param>
    /// <param name="log">Whether file operations are logged.</param>
    /// <param name="cancellationToken">Cancels reading or minification.</param>
    /// <returns>The minified JavaScript.</returns>
    ValueTask<string> MinifyFile(string filePath, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>Minifies a source file and saves it after successful transformation, creating destination directories as needed.</summary>
    /// <param name="sourcePath">Path to the source JavaScript file.</param>
    /// <param name="destinationPath">Output path. Null overwrites the source file.</param>
    /// <param name="log">Whether file operations are logged.</param>
    /// <param name="cancellationToken">Cancels file operations or minification.</param>
    /// <returns>A task that completes when the minified file has been written.</returns>
    ValueTask SaveMinifiedFile(string sourcePath, string? destinationPath = null, bool log = true, CancellationToken cancellationToken = default);
}
