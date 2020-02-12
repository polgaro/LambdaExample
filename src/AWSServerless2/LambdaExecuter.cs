using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AWSServerless2
{
    public class LambdaExecuter<TResponse, TRequest>
    {
        private string handler;
        private string className;
        private string methodName;

        private object objFunctionExecuter;

        public LambdaExecuter(string handler)
        {
            this.handler = handler;
            string[] handlerSections = handler.Split("::");

            if (handlerSections.Length != 3)
            {
                string errorMessage = $"Handler is in the wrong format.Please use[xx::xx::xx], found[{handler}]";
                Console.WriteLine(errorMessage);
                throw new Exception(errorMessage);
            }

            className = handlerSections[1];
            methodName = handlerSections[2];
            Console.WriteLine($"Handler ${className}");

            try
            {
                Type t = System.Reflection.Assembly.GetExecutingAssembly().GetType(className);
                objFunctionExecuter = Activator.CreateInstance(t);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR FOUND!!!! {ex.Message}\r\n{ex.StackTrace}");
            }
        }

        public TResponse HandleRequest(TRequest input, ILambdaContext context)
        {
            if(objFunctionExecuter == null || string.IsNullOrEmpty(methodName))
            {
                throw new Exception("Object not initialized correctly. Invalid handler.");
            }

            MethodInfo mi = objFunctionExecuter.GetType().GetMethod(methodName);
            return (TResponse)mi.Invoke(objFunctionExecuter, new object[] { input, context } );
        }
    }
}
