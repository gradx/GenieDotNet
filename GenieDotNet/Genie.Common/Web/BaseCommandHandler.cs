using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Linq.Expressions;
using System.Reflection;

namespace Genie.Common.Web;

public class BaseCommandHandler(GenieContext genieContext)
{
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
                    using var ms = new MemoryStream();
                    await fileMultipartSection.FileStream.CopyToAsync(ms, cancellationToken);

                    result!.Grpc = parser.ParseFrom(ms.ToArray());

                }
                else if (result is not null && result.Grpc is not null)
                {
                    var nativeType = GrpcClassMapping.GetType(result.Grpc)!;

                    FindArtifact(result, fileMultipartSection, nativeType, out ParameterExpression msg, out Grpc.Artifact artifact);
                    if (artifact == null)
                        return new UploadResult { Error = "Artifact Name not matched" };

                    string sub_folder = artifact.Category switch
                    {
                        Grpc.Artifact.Types.Category.Header => "header",
                        Grpc.Artifact.Types.Category.DirectMessage => "directmessage",
                        Grpc.Artifact.Types.Category.Kiosk => "kiosk",
                        Grpc.Artifact.Types.Category.Message => "message",
                        Grpc.Artifact.Types.Category.Profile => "profile",
                        Grpc.Artifact.Types.Category.Resource => "resource",
                        _ => throw new NullReferenceException("Category cannot be null")
                    };


                    var folderLambda = GetFolder(nativeType, msg);
                    _ = await folderLambda.Compile().Invoke(result.Grpc, genieContext, sub_folder, cancellationToken);

                    //string fileName = StorageAdapter.GenerateFilename(fileMultipartSection.FileName, artifact.SealedEnvelope?.Key.IsPrivate ?? false);

                    //using var f = File.Create(Path.Combine(Context.Kafka.MountPath, fileName));
                    //await fileMultipartSection.FileStream!.CopyToAsync(f, cancellationToken);
                }
            }
        }

        return result;
    }



    private static void ProcessArtifact(GenieContext genieContext, UploadResult? result, FileMultipartSection fileMultipartSection, Grpc.Artifact artifact, (string Id, string Folder) folder, string fileName)
    {
        artifact.Container = folder.Folder;
        artifact.Filename = fileName;
        artifact.ContentType = fileMultipartSection.Section.ContentType;
        artifact.ServerUri = genieContext.Azure.Storage.Server + "/" + genieContext.Azure.Storage.Share;
    }

    private static Expression<Func<IMessage, GenieContext, string, CancellationToken, Task<(string Id, string Folder)>>> GetFolder(Type nativeType, ParameterExpression msg)
    {
        var folderMethodInfo = nativeType.GetMethod("GetFolderName", BindingFlags.Static | BindingFlags.Public)!;
        var gc = Expression.Parameter(typeof(GenieContext));
        var sf = Expression.Parameter(typeof(string));
        var ct = Expression.Parameter(typeof(CancellationToken));

        var folderExp = Expression.Call(folderMethodInfo, msg, gc, sf, ct);

        var folderLambda = Expression.Lambda<Func<IMessage, GenieContext, string, CancellationToken, Task<(string Id, string Folder)>>>(folderExp, msg, gc, sf, ct);
        return folderLambda;
    }

    private static void FindArtifact(UploadResult? result, FileMultipartSection fileMultipartSection, Type nativeType, out ParameterExpression msg, out Grpc.Artifact artifact)
    {
        // public static GRPC.Artifact? FindArtifact(IMessage message, string name)
        msg = Expression.Parameter(typeof(IMessage));
        var name = Expression.Parameter(typeof(string));
        var methodInfo = nativeType.GetMethod("FindArtifact", BindingFlags.Static | BindingFlags.Public)!;
        var exp = Expression.Call(methodInfo, msg, name);
        var lambda = Expression.Lambda<Func<IMessage, string, Grpc.Artifact>>(exp, msg, name);


        // if (result.Grpc is GRPC.ChannelRequest channelRequest && fileMultipartSection.Name.StartsWith("member/"))
        artifact = lambda.Compile().Invoke(result!.Grpc!, fileMultipartSection.Name);
    }
}