using System;
using System.Linq;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Blobs;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class DomainService : Blobs.Blob.BlobBase
    {
        private readonly IServiceProvider serviceProvider;
        public DomainService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<GetMyProfileBlobsResponse> GetMyProfileBlobs(
            GetMyProfileBlobsRequest request,
            ServerCallContext context)
        {
            if (!await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        "Login does not have access to profile"));
            }

            var blobs = await this.serviceProvider
                .GetRequiredService<IBlobListQueryRepository>()
                .GetMyBlobsAsync(
                    request.ProfileId,
                    request.MaxFileSize,
                    request.AllowedFileTypes.ToArray(),
                    request.Offset,
                    request.Limit,
                    context.CancellationToken);

            return new GetMyProfileBlobsResponse
            {
                Length = blobs.Length,
                Result =
                {
                    blobs.Result.ProjectToType<GetMyProfileBlobsResponse.Types.Blob>()
                }
            };
        }

        public override async Task<GetProfileFreeBlobsResponse> GetProfileFreeBlobs(
            GetProfileFreeBlobsRequest request,
            ServerCallContext context)
        {
            if (!await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        "Login does not have access to profile"));
            }

            TableResultVO<IBlobListQueryRepository.GetFreeBlobsVO> blobs =
                await this.serviceProvider
                    .GetRequiredService<IBlobListQueryRepository>()
                    .GetFreeBlobsAsync(
                        request.ProfileId,
                        request.Offset,
                        request.Limit,
                        request.FileName,
                        request.Author,
                        request.FromDate?.ToLocalDateTime(),
                        request.ToDate?.ToLocalDateTime(),
                        context.CancellationToken);

            IBlobListQueryRepository.GetStorageInfoVO storage =
                await this.serviceProvider
                    .GetRequiredService<IBlobListQueryRepository>()
                    .GetStorageInfoAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetProfileFreeBlobsResponse
            {
                Length = blobs.Length,
                Result =
                {
                    blobs.Result.ProjectToType<GetProfileFreeBlobsResponse.Types.Blob>()
                },
                StorageQuota = storage.Quota,
                UsedStorageSpace = storage.UsedSpace,
            };
        }

        public override async Task<GetProfileInboxBlobsResponse> GetProfileInboxBlobs(
            GetProfileInboxBlobsRequest request,
            ServerCallContext context)
        {
            if (!await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        "Login does not have access to profile"));
            }

            var blobs = await this.serviceProvider
                .GetRequiredService<IBlobListQueryRepository>()
                .GetInboxBlobsAsync(
                    request.ProfileId,
                    request.Offset,
                    request.Limit,
                    request.FileName,
                    request.MessageSubject,
                    request.FromDate?.ToLocalDateTime(),
                    request.ToDate?.ToLocalDateTime(),
                    context.CancellationToken);

            return new GetProfileInboxBlobsResponse
            {
                Length = blobs.Length,
                Result =
                {
                    blobs.Result.ProjectToType<GetProfileInboxBlobsResponse.Types.Blob>()
                }
            };
        }

        public override async Task<GetProfileOutboxBlobsResponse> GetProfileOutboxBlobs(
            GetProfileOutboxBlobsRequest request,
            ServerCallContext context)
        {
            if (!await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        "Login does not have access to profile"));
            }

            var blobs = await this.serviceProvider
                .GetRequiredService<IBlobListQueryRepository>()
                .GetOutboxBlobsAsync(
                    request.ProfileId,
                    request.Offset,
                    request.Limit,
                    request.FileName,
                    request.MessageSubject,
                    request.FromDate?.ToLocalDateTime(),
                    request.ToDate?.ToLocalDateTime(),
                    context.CancellationToken);

            return new GetProfileOutboxBlobsResponse
            {
                Length = blobs.Length,
                Result =
                {
                    blobs.Result.ProjectToType<GetProfileOutboxBlobsResponse.Types.Blob>()
                }
            };
        }

        public async override Task<Empty> DeleteProfileBlob(
            DeleteProfileBlobRequest request,
            ServerCallContext context)
        {
            if (!await this.serviceProvider
                .GetRequiredService<IAuthorizationService>()
                .HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        "Login does not have access to profile"));
            }

            await this.serviceProvider.GetRequiredService<IMediator>()
                .Send(
                    new RemoveProfileBlobCommand(
                        request.ProfileId,
                        request.BlobId
                    ),
                    context.CancellationToken);

            return new Empty();
        }
    }
}
