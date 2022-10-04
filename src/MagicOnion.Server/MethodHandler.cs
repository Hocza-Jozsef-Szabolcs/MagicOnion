using Grpc.Core;
using MessagePack;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using MagicOnion.Internal;
using MagicOnion.Server.Filters;
using MagicOnion.Server.Filters.Internal;
using MagicOnion.Server.Diagnostics;
using MagicOnion.Server.Internal;

namespace MagicOnion.Server;

public class MethodHandler : IEquatable<MethodHandler>
{
    // reflection cache
    static readonly MethodInfo Helper_CreateService = typeof(ServiceProviderHelper).GetMethod(nameof(ServiceProviderHelper.CreateService), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
    static readonly MethodInfo Helper_TaskToEmptyValueTask = typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.TaskToEmptyValueTask), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
    static readonly MethodInfo Helper_NewEmptyValueTask = typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.NewEmptyValueTask), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
    static readonly MethodInfo Helper_SetTaskUnaryResult = typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.SetTaskUnaryResult), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
    static readonly MethodInfo Helper_SetUnaryResult = typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.SetUnaryResult), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
    static readonly MethodInfo Helper_SerializeTaskClientStreamingResult = typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.SerializeTaskClientStreamingResult), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
    static readonly MethodInfo HelperSerializeClientStreamingResult = typeof(MethodHandlerResultHelper).GetMethod(nameof(MethodHandlerResultHelper.SerializeClientStreamingResult), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
    static readonly PropertyInfo ServiceContext_Request = typeof(ServiceContext).GetProperty("Request", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    static int methodHandlerIdBuild = 0;

    readonly int methodHandlerId;
    readonly bool isStreamingHub;
    readonly MessagePackSerializerOptions serializerOptions;
    readonly Func<ServiceContext, ValueTask> methodBody;
    
    // options
    readonly bool enableCurrentContext;
    
    internal IMagicOnionLogger Logger { get; }
    internal bool IsReturnExceptionStackTraceInErrorDetail { get; }

    public string ServiceName { get; }
    public string MethodName { get; }
    public Type ServiceType { get; }
    public MethodInfo MethodInfo { get; }
    public MethodType MethodType { get; }

    public ILookup<Type, Attribute> AttributeLookup { get; }

    // use for request handling.
    public Type RequestType { get; }
    public Type UnwrappedResponseType { get; }

    public MethodHandler(Type classType, MethodInfo methodInfo, string methodName, MethodHandlerOptions handlerOptions, IServiceProvider serviceProvider, IMagicOnionLogger logger, bool isStreamingHub)
    {
        this.methodHandlerId = Interlocked.Increment(ref methodHandlerIdBuild);

        var serviceInterfaceType = classType.GetInterfaces().First(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IService<>)).GetGenericArguments()[0];

        this.ServiceType = classType;
        this.ServiceName = serviceInterfaceType.Name;
        this.MethodInfo = methodInfo;
        this.MethodName = methodName;

        this.UnwrappedResponseType = UnwrapResponseType(methodInfo, out var mt, out var responseIsTask, out var requestType);
        this.MethodType = mt;
        this.serializerOptions = handlerOptions.SerializerOptions;
        this.isStreamingHub = isStreamingHub;

        var parameters = methodInfo.GetParameters();
        if (requestType == null)
        {
            var resolver = this.serializerOptions.Resolver;
            requestType = MagicOnionMarshallers.CreateRequestTypeAndSetResolver(classType.Name + "/" + methodInfo.Name, parameters, ref resolver);
            this.serializerOptions = this.serializerOptions.WithResolver(resolver);
        }

        this.RequestType = requestType;

        this.AttributeLookup = classType.GetCustomAttributes(true)
            .Concat(methodInfo.GetCustomAttributes(true))
            .Cast<Attribute>()
            .ToLookup(x => x.GetType());

        var filters = FilterHelper.GetFilters(handlerOptions.GlobalFilters, classType, methodInfo);

        // options
        this.IsReturnExceptionStackTraceInErrorDetail = handlerOptions.IsReturnExceptionStackTraceInErrorDetail;
        this.Logger = logger;
        this.enableCurrentContext = handlerOptions.EnableCurrentContext;

        // prepare lambda parameters
        var createServiceMethodInfo = Helper_CreateService.MakeGenericMethod(classType, serviceInterfaceType);
        var contextArg = Expression.Parameter(typeof(ServiceContext), "context");
        var instance = Expression.Call(createServiceMethodInfo, contextArg);

        switch (MethodType)
        {
            case MethodType.Unary:
            case MethodType.ServerStreaming:
                // (ServiceContext context) =>
                // {
                //      var request = (TRequest)context.Request;
                //      var result = new FooService() { Context = context }.Bar(request.Item1, request.Item2);
                //      return MethodHandlerResultHelper.SetUnaryResult(result, context);
                // };
                try
                {
                    var requestArg = Expression.Parameter(RequestType, "request");
                    var contextRequest = Expression.Property(contextArg, ServiceContext_Request);
                    var assignRequest = Expression.Assign(requestArg, Expression.Convert(contextRequest, RequestType));

                    Expression[] arguments = new Expression[parameters.Length];
                    if (parameters.Length == 1)
                    {
                        arguments[0] = requestArg;
                    }
                    else
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            arguments[i] = Expression.Field(requestArg, "Item" + (i + 1));
                        }
                    }

                    var callBody = Expression.Call(instance, methodInfo, arguments);

                    if (MethodType == MethodType.ServerStreaming)
                    {
                        var finalMethod = (responseIsTask)
                            ? Helper_TaskToEmptyValueTask.MakeGenericMethod(MethodInfo.ReturnType.GetGenericArguments()[0]) // Task<ServerStreamingResult<TResponse>>
                            : Helper_NewEmptyValueTask.MakeGenericMethod(MethodInfo.ReturnType); // ServerStreamingResult<TResponse>
                        callBody = Expression.Call(finalMethod, callBody);
                    }
                    else
                    {
                        var finalMethod = (responseIsTask)
                            ? Helper_SetTaskUnaryResult.MakeGenericMethod(UnwrappedResponseType)
                            : Helper_SetUnaryResult.MakeGenericMethod(UnwrappedResponseType);
                        callBody = Expression.Call(finalMethod, callBody, contextArg);
                    }

                    var body = Expression.Block(new[] { requestArg }, assignRequest, callBody);
                    var compiledBody = Expression.Lambda(body, contextArg).Compile();

                    this.methodBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (Func<ServiceContext, ValueTask>)compiledBody);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Can't create handler. Path:{ToString()}", ex);
                }
                break;
            case MethodType.ClientStreaming:
            case MethodType.DuplexStreaming:
                if (parameters.Length != 0)
                {
                    throw new InvalidOperationException($"{MethodType} does not support method parameters. If you need to send initial parameter, use header instead. Path:{ToString()}");
                }

                // (ServiceContext context) => new FooService() { Context = context }.Bar();
                try
                {
                    var callBody = Expression.Call(instance, methodInfo);

                    if (MethodType == MethodType.ClientStreaming)
                    {
                        var finalMethod = (responseIsTask)
                            ? Helper_SerializeTaskClientStreamingResult.MakeGenericMethod(RequestType, UnwrappedResponseType)
                            : HelperSerializeClientStreamingResult.MakeGenericMethod(RequestType, UnwrappedResponseType);
                        callBody = Expression.Call(finalMethod, callBody, contextArg);
                    }
                    else
                    {
                        var finalMethod = (responseIsTask)
                            ? Helper_TaskToEmptyValueTask.MakeGenericMethod(MethodInfo.ReturnType.GetGenericArguments()[0])
                            : Helper_NewEmptyValueTask.MakeGenericMethod(MethodInfo.ReturnType);
                        callBody = Expression.Call(finalMethod, callBody);
                    }

                    var compiledBody = Expression.Lambda(callBody, contextArg).Compile();

                    this.methodBody = FilterHelper.WrapMethodBodyWithFilter(serviceProvider, filters, (Func<ServiceContext, ValueTask>)compiledBody);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Can't create handler. Path:{ToString()}", ex);
                }
                break;
            default:
                throw new InvalidOperationException("Unknown MethodType:" + MethodType + $"Path:{ToString()}");
        }
    }

    // non-filtered.
    public byte[] BoxedSerialize(object requestValue)
    {
        return MessagePackSerializer.Serialize(RequestType, requestValue, serializerOptions);
    }

    public object BoxedDeserialize(byte[] responseValue)
    {
        return MessagePackSerializer.Deserialize(UnwrappedResponseType, responseValue, serializerOptions);
    }

    static Type UnwrapResponseType(MethodInfo methodInfo, out MethodType methodType, out bool responseIsTask, out Type? requestTypeIfExists)
    {
        var t = methodInfo.ReturnType;
        if (!t.GetTypeInfo().IsGenericType) throw new Exception($"Invalid return type, path:{methodInfo.DeclaringType!.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");

        // Task<Unary<T>>
        if (t.GetGenericTypeDefinition() == typeof(Task<>))
        {
            responseIsTask = true;
            t = t.GetGenericArguments()[0];
        }
        else
        {
            responseIsTask = false;
        }

        // Unary<T>
        var returnType = t.GetGenericTypeDefinition();
        if (returnType == typeof(UnaryResult<>))
        {
            methodType = MethodType.Unary;
            requestTypeIfExists = default;
            return t.GetGenericArguments()[0];
        }
        else if (returnType == typeof(ClientStreamingResult<,>))
        {
            methodType = MethodType.ClientStreaming;
            var genArgs = t.GetGenericArguments();
            requestTypeIfExists = genArgs[0];
            return genArgs[1];
        }
        else if (returnType == typeof(ServerStreamingResult<>))
        {
            methodType = MethodType.ServerStreaming;
            requestTypeIfExists = default;
            return t.GetGenericArguments()[0];
        }
        else if (returnType == typeof(DuplexStreamingResult<,>))
        {
            methodType = MethodType.DuplexStreaming;
            var genArgs = t.GetGenericArguments();
            requestTypeIfExists = genArgs[0];
            return genArgs[1];
        }
        else
        {
            throw new Exception($"Invalid return type, path:{methodInfo.DeclaringType!.Name + "/" + methodInfo.Name} type:{methodInfo.ReturnType.Name}");
        }
    }

    internal void BindHandler(ServiceBinderBase binder)
    {
        // NOTE: ServiceBinderBase.AddMethod has `class` generic constraint.
        //       We need to box an instance of the value type.
        var rawRequestType = RequestType.IsValueType ? typeof(Box<>).MakeGenericType(RequestType) : RequestType;
        var rawResponseType = UnwrappedResponseType.IsValueType ? typeof(Box<>).MakeGenericType(UnwrappedResponseType) : UnwrappedResponseType;

        typeof(MethodHandler)
            .GetMethod(nameof(BindHandlerTyped), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(RequestType, UnwrappedResponseType, rawRequestType, rawResponseType)
            .Invoke(this, new [] { binder });
    }

    void BindHandlerTyped<TRequest, TResponse, TRawRequest, TRawResponse>(ServiceBinderBase binder)
        where TRawRequest : class
        where TRawResponse : class
    {
        var handlerBinder = MagicOnionMethodHandlerBinder<TRequest, TResponse, TRawRequest, TRawResponse>.Instance;
        switch (this.MethodType)
        {
            case MethodType.Unary:
                if (this.MethodInfo.GetParameters().Any())
                {
                    handlerBinder.BindUnary(binder, UnaryServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                }
                else
                {
                    handlerBinder.BindUnaryPalameterless(binder, UnaryServerMethod<Nil, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                }
                break;
            case MethodType.ClientStreaming:
                handlerBinder.BindClientStreaming(binder, ClientStreamingServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                break;
            case MethodType.ServerStreaming:
                handlerBinder.BindServerStreaming(binder, ServerStreamingServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                break;
            case MethodType.DuplexStreaming:
                if (isStreamingHub)
                {
                    handlerBinder.BindStreamingHub(binder, DuplexStreamingServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                }
                else
                {
                    handlerBinder.BindDuplexStreaming(binder, DuplexStreamingServerMethod<TRequest, TResponse>, this, this.ServiceName, this.MethodName, this.serializerOptions);
                }
                break;
            default:
                throw new InvalidOperationException("Unknown RegisterType:" + this.MethodType);
        }
    }

    async Task<TResponse?> UnaryServerMethod<TRequest, TResponse>(TRequest request, ServerCallContext context)
    {
        var isErrorOrInterrupted = false;
        var serviceContext = new ServiceContext(ServiceType, MethodInfo, AttributeLookup, this.MethodType, context, serializerOptions, Logger, this, context.GetHttpContext().RequestServices);
        serviceContext.SetRawRequest(request);

        TResponse? response = default;
        try
        {
            Logger.BeginInvokeMethod(serviceContext, typeof(TRequest));
            if (enableCurrentContext)
            {
                ServiceContext.currentServiceContext.Value = serviceContext;
            }
            await this.methodBody(serviceContext).ConfigureAwait(false);
            if (serviceContext.Result is not null)
            {
                response = (TResponse?)serviceContext.Result;
            }
        }
        catch (ReturnStatusException ex)
        {
            isErrorOrInterrupted = true;
            context.Status = ex.ToStatus();
            response = default;

            // WORKAROUND: Grpc.AspNetCore.Server throws a `Cancelled` status exception when it receives `null` response.
            //             To return the status code correctly, we needs to rethrow the exception here.
            //             https://github.com/grpc/grpc-dotnet/blob/d4ee8babcd90666fc0727163a06527ab9fd7366a/src/Grpc.AspNetCore.Server/Internal/CallHandlers/UnaryServerCallHandler.cs#L50-L56
            var rpcException = new RpcException(ex.ToStatus());
#if NET6_0_OR_GREATER
                if (ex.StackTrace is not null)
                {
                    ExceptionDispatchInfo.SetRemoteStackTrace(rpcException, ex.StackTrace);
                }
#endif
            throw rpcException;
        }
        catch (Exception ex)
        {
            isErrorOrInterrupted = true;
            if (IsReturnExceptionStackTraceInErrorDetail)
            {
                // Trim data.
                var msg = ex.ToString();
                var lineSplit = msg.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < lineSplit.Length; i++)
                {
                    if (!(lineSplit[i].Contains("System.Runtime.CompilerServices")
                          || lineSplit[i].Contains("直前に例外がスローされた場所からのスタック トレースの終わり")
                          || lineSplit[i].Contains("End of stack trace from the previous location where the exception was thrown")
                        ))
                    {
                        sb.AppendLine(lineSplit[i]);
                    }
                    if (sb.Length >= 5000)
                    {
                        sb.AppendLine("----Omit Message(message size is too long)----");
                        break;
                    }
                }
                var str = sb.ToString();

                context.Status = new Status(StatusCode.Unknown, str);
                Logger.Error(ex, context);
                response = default;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            Logger.EndInvokeMethod(serviceContext, typeof(TResponse), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
        }

        return response;
    }

    async Task<TResponse?> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context)
    {
        var isErrorOrInterrupted = false;
        var serviceContext = new StreamingServiceContext<TRequest, Nil /* Dummy */>(
            ServiceType,
            MethodInfo,
            AttributeLookup,
            this.MethodType,
            context,
            serializerOptions,
            Logger,
            this,
            context.GetHttpContext().RequestServices,
            requestStream,
            default
        );

        TResponse? response;
        try
        {
            using (requestStream as IDisposable)
            {
                Logger.BeginInvokeMethod(serviceContext, typeof(Nil));
                if (enableCurrentContext)
                {
                    ServiceContext.currentServiceContext.Value = serviceContext;
                }
                await this.methodBody(serviceContext).ConfigureAwait(false);
                response = serviceContext.Result is TResponse r ? r : default;
            }
        }
        catch (ReturnStatusException ex)
        {
            isErrorOrInterrupted = true;
            context.Status = ex.ToStatus();
            response = default;
        }
        catch (Exception ex)
        {
            isErrorOrInterrupted = true;
            if (IsReturnExceptionStackTraceInErrorDetail)
            {
                context.Status = new Status(StatusCode.Unknown, ex.ToString());
                Logger.Error(ex, context);
                response = default;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            Logger.EndInvokeMethod(serviceContext, typeof(TResponse), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
        }

        return response;
    }

    async Task ServerStreamingServerMethod<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
    {
        var isErrorOrInterrupted = false;
        var serviceContext = new StreamingServiceContext<Nil /* Dummy */, TResponse>(
            ServiceType,
            MethodInfo,
            AttributeLookup,
            this.MethodType,
            context,
            serializerOptions, 
            Logger,
            this,
            context.GetHttpContext().RequestServices,
            default,
            responseStream
        );
        serviceContext.SetRawRequest(request);
        try
        {
            Logger.BeginInvokeMethod(serviceContext, typeof(TRequest));
            if (enableCurrentContext)
            {
                ServiceContext.currentServiceContext.Value = serviceContext;
            }
            await this.methodBody(serviceContext).ConfigureAwait(false);
            return;
        }
        catch (ReturnStatusException ex)
        {
            isErrorOrInterrupted = true;
            context.Status = ex.ToStatus();
            return;
        }
        catch (Exception ex)
        {
            isErrorOrInterrupted = true;
            if (IsReturnExceptionStackTraceInErrorDetail)
            {
                context.Status = new Status(StatusCode.Unknown, ex.ToString());
                Logger.Error(ex, context);
                return;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            Logger.EndInvokeMethod(serviceContext, typeof(Nil), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
        }
    }

    async Task DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
    {
        var isErrorOrInterrupted = false;
        var serviceContext = new StreamingServiceContext<TRequest, TResponse>(
            ServiceType,
            MethodInfo,
            AttributeLookup,
            this.MethodType,
            context,
            serializerOptions,
            Logger,
            this,
            context.GetHttpContext().RequestServices,
            requestStream,
            responseStream
        );
        try
        {
            Logger.BeginInvokeMethod(serviceContext, typeof(Nil));
            using (requestStream as IDisposable)
            {
                if (enableCurrentContext)
                {
                    ServiceContext.currentServiceContext.Value = serviceContext;
                }
                await this.methodBody(serviceContext).ConfigureAwait(false);

                return;
            }
        }
        catch (ReturnStatusException ex)
        {
            isErrorOrInterrupted = true;
            context.Status = ex.ToStatus();
            return;
        }
        catch (Exception ex)
        {
            isErrorOrInterrupted = true;
            if (IsReturnExceptionStackTraceInErrorDetail)
            {
                context.Status = new Status(StatusCode.Unknown, ex.ToString());
                Logger.Error(ex, context);
                return;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            Logger.EndInvokeMethod(serviceContext, typeof(Nil), (DateTime.UtcNow - serviceContext.Timestamp).TotalMilliseconds, isErrorOrInterrupted);
        }
    }

    public override string ToString()
    {
        return ServiceName + "/" + MethodName;
    }

    public override int GetHashCode()
    {
        return ServiceName.GetHashCode() ^ MethodInfo.Name.GetHashCode() << 2;
    }

    public bool Equals(MethodHandler? other)
    {
        return other != null && ServiceName.Equals(other.ServiceName) && MethodInfo.Name.Equals(other.MethodInfo.Name);
    }

    public class UniqueEqualityComparer : IEqualityComparer<MethodHandler>
    {
        public bool Equals(MethodHandler? x, MethodHandler? y)
        {
            return (x == null && y == null) || (x != null && y != null && x.methodHandlerId.Equals(y.methodHandlerId));
        }

        public int GetHashCode(MethodHandler obj)
        {
            return obj.methodHandlerId.GetHashCode();
        }
    }
}

/// <summary>
/// Options for MethodHandler construction.
/// </summary>
public class MethodHandlerOptions
{
    public IList<MagicOnionServiceFilterDescriptor> GlobalFilters { get; }

    public bool IsReturnExceptionStackTraceInErrorDetail { get; }

    public bool EnableCurrentContext { get; }

    public MessagePackSerializerOptions SerializerOptions { get; }

    public MethodHandlerOptions(MagicOnionOptions options)
    {
        GlobalFilters = options.GlobalFilters;
        IsReturnExceptionStackTraceInErrorDetail = options.IsReturnExceptionStackTraceInErrorDetail;
        EnableCurrentContext = options.EnableCurrentContext;
        SerializerOptions = options.SerializerOptions;
    }
}

internal class MethodHandlerResultHelper
{
    static readonly ValueTask CopmletedValueTask = new ValueTask();

    public static ValueTask NewEmptyValueTask<T>(T result)
    {
        // ignore result.
        return CopmletedValueTask;
    }

    public static async ValueTask TaskToEmptyValueTask<T>(Task<T> result)
    {
        // wait and ignore result.
        await result;
    }

    public static async ValueTask SetUnaryResult<T>(UnaryResult<T> result, ServiceContext context)
    {
        if (result.hasRawValue)
        {
            context.Result = (result.rawTaskValue != null) ? await result.rawTaskValue.ConfigureAwait(false) : result.rawValue;
        }
    }

    public static async ValueTask SetTaskUnaryResult<T>(Task<UnaryResult<T>> taskResult, ServiceContext context)
    {
        var result = await taskResult.ConfigureAwait(false);
        if (result.hasRawValue)
        {
            context.Result = (result.rawTaskValue != null) ? await result.rawTaskValue.ConfigureAwait(false) : result.rawValue;
        }
    }

    public static ValueTask SerializeClientStreamingResult<TRequest, TResponse>(ClientStreamingResult<TRequest, TResponse> result, ServiceContext context)
    {
        if (result.hasRawValue)
        {
            context.Result = result.rawValue;
        }

        return default(ValueTask);
    }

    public static async ValueTask SerializeTaskClientStreamingResult<TRequest, TResponse>(Task<ClientStreamingResult<TRequest, TResponse>> taskResult, ServiceContext context)
    {
        var result = await taskResult.ConfigureAwait(false);
        if (result.hasRawValue)
        {
            context.Result = result.rawValue;
        }
    }
}
