// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Etcdserverpb;
using Google.Protobuf;

namespace dotnet_etcd.helper
{
    public static class WatchPrefixRequestHelper
    {
        public static WatchRequest ToWatchRequest(this WatchPrefixRequest request)
        {
            var prefix = request.CreateRequest.Prefix?.ToStringUtf8() ?? string.Empty;
            var rangeEnd = EtcdClient.GetRangeEnd(prefix);
            return new WatchRequest
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = request.CreateRequest.Prefix,
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd),
                    Fragment = request.CreateRequest.Fragment,
                    PrevKv = request.CreateRequest.PrevKv,
                    ProgressNotify = request.CreateRequest.ProgressNotify,
                    StartRevision = request.CreateRequest.StartRevision,
                    WatchId = request.CreateRequest.WatchId,
                },
                CancelRequest = request.CancelRequest,
                ProgressRequest = request.ProgressRequest,
            };
        }

        public static WatchRequest[] ToWatchRequest(this WatchPrefixRequest[] prefixRequests)
            => prefixRequests.Select(req => req.ToWatchRequest()).ToArray();
    }
}
