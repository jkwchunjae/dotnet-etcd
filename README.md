# dotnet-etcd
![](https://raw.githubusercontent.com/shubhamranjan/dotnet-etcd/master/docs/img/etcd-logo-rectangle.png)

A C# .NET (dotnet) GRPC client for etcd v3+

[![Build Status](https://travis-ci.org/shubhamranjan/dotnet-etcd.svg?branch=master)](https://travis-ci.org/shubhamranjan/dotnet-etcd)
![Nuget Version Info](https://img.shields.io/nuget/v/dotnet-etcd.svg)
![Nuget Download Info](https://img.shields.io/nuget/dt/dotnet-etcd.svg)

## Supported .NET Versions

* .NET 5
* .NETCoreApp 3.1
* .NETStandard 2.1

**For older dotnet versions, use lib version < 5.x**


## Installing Package
Nuget package is published on [nuget.org](https://www.nuget.org/packages/dotnet-etcd/) and can be installed in the following ways :
    
### Nuget Package Manager
    
    Install-Package dotnet-etcd

### .NET CLI
    
    dotnet add package dotnet-etcd

### Paket CLI
    
    paket add dotnet-etcd
` The NuGet Team does not provide support for this client. Please contact its maintainers for support.`

## Usage :

Add using statement at the top of your class file
```C#
    using dotnet_etcd;
```
### Client Initialization
    
```C#
    EtcdClient client = new EtcdClient("host1:port1:,...., hostN:portN");
    // E.g.
    EtcdClient etcdClient = new EtcdClient("https://localhost:23790,https://localhost:23791,https://localhost:23792");
```

### Client With user and password
    
```C#
    EtcdClient client = new EtcdClient("https://localhost:23790");
    var authRes = client.Authenticate(new Etcdserverpb.AuthenticateRequest()
    {
        Name = "name",
        Password = "password",
    });
    //  Put key "foo/bar" with value "barfoo" with authenticated token
    client.Put("foo/bar", "barfoo", new Grpc.Core.Metadata() {
        new Grpc.Core.Metadata.Entry("token",authRes.Token)
    });
```


#### Available Constructor Parameters

* handler - Http Handler that can be used by the underlying grpc client. This can be used for variouse use cases, for e.g. configuring client certifcates . Default : null
* ssl - Bool depicting whether to configure a secure or unsecure connection. Default : false.
* useLegacyRpcExceptionForCancellation - Bool setting to revert task cancellations back to throwing gRPC's RpcException with a StatusCode=Cancelled. The default behavior now is to throw an OperationCanceledException, which integrates better with .net Tasks. Default : false.

### Operations

>A lot of methods have been implemented using etcd's default input/output parameters. I am simplifying a lot of methods by including more overloads as I come across use cases. If you have some, please feel free to raise and issue or a PR :)

#### Key-Value Operations
##### Put a key

```C#
    client.Put(<KEY_STRING>,<VALUE_STRING>);
    // E.g Put key "foo/bar" with value "foobar"
    client.Put("foo/bar","barfoo");

    await client.PutAsync(<KEY_STRING>,<VALUE_STRING>);
    // E.g Put key "foo/bar" with value "foobar" in async
    await client.PutAsync("foo/bar","barfoo");
```

##### Get a key
    
```C#
    client.GetVal(<KEY_STRING>);
    // E.g Get key "foo/bar"
    client.GetVal("foo/bar");
    // To get full etcd response
    client.Get("foo/bar");

    await client.GetValAsync(<KEY_STRING>);
    // E.g. Get key "foo/bar" in async
    await client.GetValAsync("foo/bar");
    // To get full etcd response
    await client.GetAsync("foo/bar");
```

##### Get multiple keys with a common prefix

```C#
    client.GetRange(<PREFIX_STRING>);
    // E.g. Get all keys with pattern "foo/*"
    client.GetRange("foo/"); 

    await client.GetRangeAsync(<PREFIX_STRING>);
    // E.g. Get all keys with pattern "foo/*" in async
    await client.GetRangeAsync("foo/");

    // E.g. Get all keys
    await client.GetRangeAsync("");
```

##### Delete a key

```C#
    client.Delete(<KEY_STRING>);
    // E.g. Delete key "foo/bar"
    client.Delete("foo/bar");

    await client.DeleteAsync(<KEY_STRING>);
    // E.g. Delete key "foo/bar" in async
    await client.DeleteAsync("foo/bar");
```

##### Delete multiple keys with a common prefix

```C#
    client.DeleteRange(<PREFIX_STRING>);
    // E.g. Delete all keys with pattern "foo/*"
    client.DeleteRange("foo/"); 

    await client.DeleteRangeAsync(<PREFIX_STRING>);
    // E.g. Delete all keys with pattern "foo/*" in async
    await client.DeleteRangeAsync("foo/");
```

### Watch Operations
##### Watch a key

```C#
    WatchRequest request = new WatchRequest()
    {
        CreateRequest = new WatchCreateRequest()
        {
            Key = ByteString.CopyFromUtf8("foo")
        }
    };
    etcdClient.Watch(request, print);

    // -------------------------------
    // Print function that prints key and value from the watch response
    private static void print(WatchResponse response)
    {   
        if (response.Events.Count == 0)
        {
            Console.WriteLine(response);
        }
        else
        {
            Console.WriteLine($"{response.Events[0].Kv.Key.ToStringUtf8()}:{response.Events .Kv.Value.ToStringUtf8()}");
        }
    }

    // ----------------------------------
    // Print function that prints key and value from the minimal watch
    // response data 
    private static void print(WatchEvent[] response)
    {
        foreach(WatchEvent e1 in response)
        {
            Console.WriteLine($"{e1.Key}:{e1.Value}:{e1.Type}");
        }
    }
```
>Watch also has a simple overload as follows

```C#
    etcdClient.Watch("foo", print);
```

> More overloads are also available. You can check them using IntelliSense (Ctrl+Shift+Space). Detailed documentation coming soon.

### Cluster Operations

#### Add a member into the cluster

```C#
     MemberAddRequest request = new MemberAddRequest();
     request.PeerURLs.Add("http://example.com:2380");
     request.PeerURLs.Add("http://10.0.0.1:2380");
     MemberAddResponse res = etcdClient.MemberAdd(request);

     // Async
     MemberAddResponse res = await etcdClient.MemberAddAsync(request);

     // Do something with response
```

#### Remove an existing member from the cluster

```C#
    MemberRemoveRequest request = new MemberRemoveRequest
    {
        // ID of member to be removed
        ID = 651748107021
    };
    MemberRemoveResponse res = etcdClient.MemberRemove(request);

    // Async
    MemberRemoveResponse res = await etcdClient.MemberRemoveAsync(request);

    // Do something with response
```

### Update the member configuration

```C#
    MemberUpdateRequest request = new MemberUpdateRequest
    {
        // ID of member to be updated
        ID = 651748107021
    };
    request.PeerURLs.Add("http://10.0.0.1:2380");
    MemberUpdateResponse res = etcdClient.MemberUpdate(request);

    // Async
    MemberUpdateResponse res = await etcdClient.MemberUpdateAsync(request);

    // Do something with response
```
###  List all the members in the cluster

```C#
    MemberListRequest request = new MemberListRequest();
    etcdClient.MemberList(request);
    MemberListResponse res = etcdClient.MemberList(request);

    // Async
    MemberListResponse res = await etcdClient.MemberListAsync(request);

    // Do something with response
    foreach(var member in res.Members)
    {
        Console.WriteLine($"{member.ID} - {member.Name} - {member.PeerURLs}");
    }
```

###  Modify data with transaction   https://etcd.io/docs/v3.4.0/learning/api/#transaction
```C#
    var txr = new Etcdserverpb.TxnRequest();
    txr.Success.Add(new Etcdserverpb.RequestOp()
    {
        RequestPut = new Etcdserverpb.PutRequest()
        {
            Key = Google.Protobuf.ByteString.CopyFrom("transaction-key", System.Text.Encoding.UTF8),
            Value = Google.Protobuf.ByteString.CopyFrom("tv", System.Text.Encoding.UTF8),
        }
    });
    client.Transaction(txr);
```
### Canceling an operation

#### Handling cancellations with OperationCanceledException
This is the default behavior selected when creating the EtcdClient with the useLegacyRpcExceptionForCancellation parameter set to false.

```C#
    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken cancellationToken = cts.Token;

    try
    {
        cts.Cancel();
        StatusRequest request = new StatusRequest();
        StatusResponse response = client.Status(request, cancellationToken: cancellationToken);
        bool isOK = response.Errors.Count == 0;
        Console.WriteLine(isOK ? "Status OK" : "Status not OK");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Operation was canceled.");
    }
```

#### Legacy cancellation with RpcException and StatusCode=Cancelled
The next sample shows the behavior when gRPC throws an RpcException with Cancelled StatusCode. To enable this legacy feature, create the EtcdClient with the useLegacyRpcExceptionForCancellation set to true.


```C#
    using EtcdClient client = new EtcdClient(
        "https://localhost:2379",
        useLegacyRpcExceptionForCancellation: true);

    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken cancellationToken = cts.Token;

    try
    {
        cts.Cancel();
        StatusRequest request = new StatusRequest();
        StatusResponse response = client.Status(request, cancellationToken: cancellationToken);
        bool isOK = response.Errors.Count == 0;
        Console.WriteLine(isOK ? "Status OK" : "Status not OK");
    }
    catch (RpcException rpc) // useLegacyRpcExceptionForCancellation: true
    {
        if (rpc.StatusCode == StatusCode.Cancelled)
        {
            Console.WriteLine("Operation was canceled.");
        }
        else throw;
    }
```
