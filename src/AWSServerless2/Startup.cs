using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AWSServerless2
{
    class Startup
    {
        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static async Task Main(string[] args)
        {
            try
            {
                Initialize();

                StartHandlers();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR FOUND STARTING THE APPLICATION!!!! {ex.Message}\r\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Registers FunctionHandler as the callback for each lambda call
        /// </summary>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static void StartHandlers()
        {
            Func<APIGatewayProxyRequest, ILambdaContext, APIGatewayProxyResponse> functionHandler =
                (APIGatewayProxyRequest req, ILambdaContext context) => 
                FunctionHandler< APIGatewayProxyResponse, APIGatewayProxyRequest>(req, context);

            using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper(
                functionHandler, 
                new Amazon.Lambda.Serialization.Json.JsonSerializer()))
            
            using (var bootstrap = new LambdaBootstrap(handlerWrapper))
            {
                Console.Write("RunAsync Started");
                bootstrap.RunAsync().Wait();
            }
        }

        /// <summary>
        /// Executed for each Lambda call.
        /// It will create a LambdaExecuter for it to handle the request.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static TResponse FunctionHandler
                <TResponse, TRequest>(TRequest input, ILambdaContext context)
        {
            Console.WriteLine("FunctionHandler started");
            LambdaExecuter<TResponse, TRequest> handler = GetHandler<TResponse, TRequest>();
            Console.WriteLine("Handler created");
            return handler.HandleRequest(input, context);
        }

        /// <summary>
        /// Gets the handler from the Environment Variable and creates a new LambdaExecuter
        /// </summary>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static LambdaExecuter<TResponse, TRequest> GetHandler<TResponse, TRequest>()
        {
            string handlerName = Environment.GetEnvironmentVariable("_HANDLER");
            return new LambdaExecuter<TResponse, TRequest>(handlerName);
        }

        /// <summary>
        /// Initializes the JsonConvert with a small payload
        /// </summary>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static void Initialize()
        {
            string sandwich = JsonConvert.SerializeObject(new { sandwich = 1 });
        }
    }
}
