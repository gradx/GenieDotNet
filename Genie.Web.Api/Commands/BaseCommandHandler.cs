using Genie.Common;
using Genie.Core;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using System.Linq.Expressions;
using System.Reflection;

namespace Genie.Web.Api.Commands;

public class BaseCommandHandler(GenieContext genieContext)
{
    private static readonly RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager();
    protected GenieContext Context => genieContext;

    public static string? GetBoundary(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            throw new ArgumentNullException(nameof(contentType));

        var elements = new List<string>(contentType.Split(' '));
        string? element = elements.Find(entry => entry.StartsWith("boundary="))!;


        string boundary = element["boundary=".Length..];
        return HeaderUtilities.RemoveQuotes(boundary).Value;
    }

    public class UploadResult
    {
        public IMessage? Grpc { get; set; }
        public string? Error { get; set; }
    }

    public async Task<UploadResult?> ProcessArtifacts(MessageParser parser, HttpRequest httpRequest, CancellationToken cancellationToken)
    {

        string boundary = GetBoundary(httpRequest.ContentType)!;

        MultipartReader reader = new(boundary, httpRequest.Body, 80 * 1024);
        MultipartSection? section;

        var result = new UploadResult();


        while ((section = await reader.ReadNextSectionAsync(cancellationToken)) != null)
        {
            ContentDispositionHeaderValue? contentDispositionHeaderValue = section.GetContentDispositionHeader();

            if (contentDispositionHeaderValue == null)
                continue;

            if (contentDispositionHeaderValue.IsFormDisposition())
            {
                FormMultipartSection? formMultipartSection = section.AsFormDataSection();
                if (formMultipartSection != null)
                {
                    _ = await formMultipartSection.GetValueAsync(cancellationToken);
                }
            }
            else if (contentDispositionHeaderValue.IsFileDisposition())
            {
                var fileMultipartSection = section.AsFileSection()!;

                // handle gRPC 
                if (fileMultipartSection.Name == "grpc" && fileMultipartSection.FileStream != null)
                {
                    using var ms = manager.GetStream();
                    await fileMultipartSection.FileStream.CopyToAsync(ms, cancellationToken);

                    result!.Grpc = parser.ParseFrom(ms.GetReadOnlySequence());

                }
                else if (result is not null && result.Grpc is not null)
                {
                    var nativeType = GrpcClassMapping.GetType(result.Grpc)!;


                }
            }
        }

        return result;
    }
}