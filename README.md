# A step by step guide to solving cold starts on AWS Lambda and DotNet core using a Custom Framework 

If you’re not sure what a cold start is or why is it important to minimize the time they take, you can read my article on cold starts:

https://medium.com/slalom-build/solving-cold-starts-on-aws-lambda-when-using-dotnet-core-51f244f08f60

In this README, I will show you specific steps to lower the start up time of a Lambda. We’ll go from 2.2 seconds to 0.8 seconds using 256 MB for the whole example, as the memory allocated for a function is a factor on how long it takes to load.

## Runtimes

Lambdas support a variety of runtimes, some of them taking a very short time (such as Node and Python) in cold starts, and some of them taking considerably longer (like dotnet and Java).

And what we’re going to do is create our own. Don’t be scared, I’ll show you step by step how to achieve it and will leave you some working code.

## Tools

For this demo, I will be using a Windows laptop with Visual Studio. In addition, I like using AWS toolkit to save time but it’s 100% optional. You can download it  [here](https://aws.amazon.com/blogs/developer/aws-toolkit-for-visual-studio-now-supports-visual-studio-2019/).

You are also going to need Docker for Windows (or similar for Mac). You’ll see why shortly. You can download it from  [here](https://docs.docker.com/docker-for-windows/).

## Steps

First, we’ll create a new project. We’ll be using a library hosted on the runtime 2.1 for our baseline.

Disclaimer: this is to compare the 2.1 framework with the custom framework. By the end of the article, I write about using the new 3.1 framework with a new Serializer provided by Amazon.

Let’s create a Serverless application:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Create%20a%20new%20project.png)

ON the next screen, we will select `Empty Serverless Application`:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Select%20Blueprint.png)

After that, we’ll publish our application to see what happens. We can do it directly from Visual Studio. As we installed the AWS Toolkit, we can choose to use AWS Explorer:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Use%20AWS%20Explorer.png)

From there, we can add our AWS account credentials:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/AWS%20Account%20Credentials.png)

After our account credentials have been added, we can right click and select `Publish to AWS Lambda`:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Publish%20to%20AWS%20Lambda.png)

We will leave all the options as default but to upload it we will have to create a S3 Bucket (select `New`):

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Create%20New%20Bucket.png)

After creating our container, click `Publish`, and a screen will appear to show us what’s happening behind the scenes:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Publish%20in%20progess.png)

You can see that the status is `CREATE_IN_PROGRESS`, and we’ll wait until it says `CREATE_COMPLETE`:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Progress%20complete.png)

You can see that it displays an AWS Serverless URL. Let’s give it a try!

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Hello%20AWS%20Serverless.png)

Hey, hello to you too AWS Serverless!

By default, we got a function called `Get`, but let’s add another function just to see how it works.

For that, we’re going to open our  `serverless.template`  file, and add a `Get2` function after the `Get`. It looks like this:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Serverless%20template.png)

In the same  `Function.cs`  file, we’ll add another function. It looks like this:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Function_cs.png)

Publish again, adjust the URL to add the `/Get2` and we get this:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Hello%20Serverless%202.png)

Let’s login to our AWS account. We’ll go to the Lambdas page. We’ll see something similar to this:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/AWS%20login%20console.png)

We’re interested in our  `Get2`  function, so we’ll click it and see the following:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/AWS%20Console%20Get2.png)

If we click on the `Monitoring` tab, we can click `View logs in CloudWatch`:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/AWS%20Console%20View%20logs.png)

We will see only one container that was initialized:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/One%20container.png)

On refreshing the page so you can see multiple executions, so when we click the Log Stream, we can see the following:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Multiple%20executions.png)

We can see that the first execution took more than 2100ms (a cold start), and the second took 1ms! That’s our baseline.

## Adding the custom runtime

We’re going to add a  `bootstrap`  file to our project, as a plaintext file: 

This is the script that the Lambda host calls to start the custom `runtime./var/task/AWSServerless2`.

We’ll configure it as `Content` and `Copy Always` To do this, we’ll select the file and press F4 (or right click and select `Properties`).

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime.png)

Then, we’ll modify our project file to run in Core 3.1; by updating `TargetFramework`, and`OutputType`, and adding a package reference to `Amazon.Lambda.RuntimeSupport` ,   In the Solution Explorer window, click the project and it’s going to open the editor.

```<Project Sdk="Microsoft.NET.Sdk">  
 <PropertyGroup>  
  <TargetFramework>netcoreapp3.1</TargetFramework>
  <GenerateRuntimeConfigurationFiles>true</GenerateRuntimeConfigurationFiles>  
  <AWSProjectType>Lambda</AWSProjectType>  
  <OutputType>Exe</OutputType>
  <LangVersion>latest</LangVersion>  
 </PropertyGroup>  
 <ItemGroup>  
 <PackageReference Include="Amazon.Lambda.Core" Version="1.1.0" />  
  <PackageReference Include="Amazon.Lambda.RuntimeSupport" Version="1.1.0" />  
  <PackageReference Include="Amazon.Lambda.Serialization.Json" Version="1.5.0" />  
  <PackageReference Include="Amazon.Lambda.APIGatewayEvents" Version="1.2.0" />  
 </ItemGroup>  
</Project>
```

As we’re using our own framework, we’ll also need to add a startup. It’s mandatory that we do this as we’re creating an Executable project now. So let’s add a`Startup.cs`  class:

``` 
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AWSServerless2
{
    class Startup
    {
        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
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
        private static LambdaExecuter<TResponse, TRequest> GetHandler<TResponse, TRequest>()
        {
            string handlerName = Environment.GetEnvironmentVariable("_HANDLER");
            return new LambdaExecuter<TResponse, TRequest>(handlerName);
        }

        private static void Initialize()
        {
            //for a future module
        }
    }
}
  ```

It’s worth explaining what we’re doing in line 69:

`string handlerName = Environment.GetEnvironmentVariable("_HANDLER");`

AWS sends us the name of the handler that should process the current request in the environment variable named `_Handler`. 

Also, in line 44 we are using the  `LambdaBootstrap`  class. This class can be found in  [this Github link](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.RuntimeSupport/Bootstrap/LambdaBootstrap.cs).

After that, we’ll add the  `LambdaExecuter`:

```
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
                Console.WriteLine($"ERROR FOUND CREATING HANDLER!!!! {ex.Message}\r\n{ex.StackTrace}");
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
```

It’s worth explaining what we are doing on line 21:

`string[] handlerSections = handler.Split("::");`

The signature of the handler will be in this format: `_Assembly::Namespace.ClassName::MethodName”._`

We’ll change the runtime in our template (specifically noting the `Runtime` value of `provided`):

```
{
	"AWSTemplateFormatVersion" : "2010-09-09",
	"Transform" : "AWS::Serverless-2016-10-31",
	"Description" : "An AWS Serverless Application.",
	"Resources" : 
	{
		"Get" : 
		{
			"Type" : "AWS::Serverless::Function",
			"Properties": 
			{
				"Handler": "AWSServerless2::AWSServerless2.Functions::Get",
				"Runtime": "provided",
				"CodeUri": "",
				"MemorySize": 256,
				"Timeout": 30,
				"Role": null,
				"Policies": [ "AWSLambdaBasicExecutionRole" ],
				"Events": 
				{
					"RootGet": 
					{
						"Type": "Api",
						"Properties": 
						{
							"Path": "/",
							"Method": "GET"
						}
					}
				}
			}
		},
		"Get2" : 
		{
			"Type" : "AWS::Serverless::Function",
			"Properties": 
			{
				"Handler": "AWSServerless2::AWSServerless2.Functions::Get2",
				"Runtime": "provided",
				"CodeUri": "",
				"MemorySize": 256,
				"Timeout": 30,
				"Role": null,
				"Policies": [ "AWSLambdaBasicExecutionRole" ],
				"Events": 
				{
					"RootGet": 
					{
						"Type": "Api",
						"Properties": 
						{
							"Path": "/GET2",
							"Method": "GET"
						}
					}
				}
			}
		}
	},

	"Outputs" : 
	{
		"ApiURL" : 
		{
			"Description" : "API endpoint URL for Prod environment",
			"Value" : { "Fn::Sub" : "https://${ServerlessRestApi}.execute-api.${AWS::Region}.amazonaws.com/Prod/" }
		}
	}
}
```

Lastly, we need some changes in the  `aws-lambda-tools-defaults` by updating `framework` and `msbuild-parameters`. This is only because we’re using Lambda Tools. Later when we use Docker, we will not need this file:

```
{  
 "Information" :   
 [  
  "This file provides default values for the deployment wizard inside Visual Studio and the AWS Lambda commands added to the .NET Core CLI.",  
  "To learn more about the Lambda commands with the .NET Core CLI execute the following command at the command line in the project root directory.",  
  "dotnet lambda help",  
  "All the command line options for the Lambda command can be specified in this file."  
 ],  
 "profile" : "lambda",  
 "region" : "us-west-2",  
 "configuration" : "Release",  
 "framework" : "netcoreapp3.1",
 "s3-prefix" : "AWSServerless2/",  
 "template" : "serverless.template",  
 "template-parameters" : "",  
 "s3-bucket" : "dgarber-serverless-demo-bucket",  
 "stack-name": "dgarber-stack",  
 "msbuild-parameters": "--self-contained true"
}
```

Note that the “`--self-contained true`” is necessary to create a package that contains the Core Framework. That’s mandatory as we will not use the framework that AWS provides.

At this point, our project should look like this:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%202.png)

So far we switched to 3.1, switched to a `provided` framework, and created a package that includes the full framework.

Now we can publish and see what happens. The results are not staggering:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%203.png)

Everything changes when we start compiling Ahead of Time (AoT). With Core 3.0, Microsoft added ReadyToRun.

Internally, ReadyToRun calls  `crossgen`. If you run your dotnet publish verbose, you can actually see that it’s running crossgen internally. What it does is doing an Ahead of Time compilation (AoT) instead of Just In Time compilation (JIT).

Please be mindful that at the time of this writing, ReadyToRun does not support cross platform. This means that you can only compile for Windows using Windows, Linux using Linux, and so on…

**For R2R compatible with AWS Lambdas, we need to be compiling using Linux.**

So, we’ll need Docker! We will add a  `dockerfile` to our project:

We will also change our `aws-lambda-tools-default`.

Our new MSBuild parameters will be:

`"msbuild-parameters" : "--self-contained true /p:PublishReadyToRun=true"`

Now, we’ll right click our project and click `Open Folder in File Explorer`:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%204.png)

And we’ll open a command line prompt by writing `cmd` and pressing enter:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%205.png)

It’s time to build our container:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%206.png)

After the container is created, we can run it:

`docker run --rm -v %cd%:/app diegobuild`

We’ll see something like this when it finishes.

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%207.png)

Do you see the file that got generated (called  `app.zip`)? We’ll upload it to our function as that’s our new binaries, created and compiled by our Linux container.

We’ll go back to our Lambda in our AWS console, scroll down to the function code, and click `Upload` to upload our new app.zip:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%208.png)

We’ll select our app.zip and upload. It’ll take a while now that our package is larger as it contains the whole framework. Note that the app.zip is 31MB.

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%209.png)

So now, we execute it and…

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%2010.png)

Bingo! total time under 1.2 seconds! Not bad huh? Think we can beat that? You bet!

First, let’s reduce the size of the package by changing again our MSBuild parameters:

`"msbuild-parameters" : "--self-contained true /p:PreserveCompilationContext=false /p:PublishTrimmed=true /p:PublishReadyToRun=true"`

Note that we’re using  `PublishTrimmed`, please take into account that it will detect dependencies that are not being used and  **not**  include them into the package. This could be a problem if you’re using assemblies by reflection. In this case, you can indicate into the csproj that you do not want your assemblies to be removed from the output to avoid errors.

Now our package is smaller:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%2011.png)

And the results?

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%2012.png)

1.1 seconds? That’s half of the original time. Can we keep going? Yes we can!

Remember the Startup.cs where it said “for a future module”? Well, let’s replace that with:

`private static void Initialize(){ string sandwich = JsonConvert.SerializeObject(new { sandwich = 1 });}`

Even though we performed an Ahead of Time compilation, the compiler will keep optimizing our code. The more complex the payload, the longer it will take to optimize it. At some point (called steady-state), the code will run faster, but it’s not very likely that we will care about it given the time it takes to compile.

So, let’s optimize the  `JsonConvert` (a class provided by `NewtonSoft.Json`), as it’s utilized by the AWS Runtime

We check the time again and…

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%2013.png)

One second! Great!

Now, let’s change a setting in our project, we’ll set  `<Optimize>false</Optimize>`:

Also, we’ll add some hints to the compiler to tell it NOT to optimize our code. You can see this when we’re using  `[MethodImpl(MethodImplOptions.NoOptimization)]`

And the results:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/Adding%20custom%20runtime%2014.png)

Oh, yes! 800ms!

## Let’s try using the 3.1 framework

Amazon recently [introduced support for Dotnet Core 3.1](https://aws.amazon.com/blogs/compute/announcing-aws-lambda-supports-for-net-core-3-1/), that opened the doors to use ReadyToRun without having to create your own Runtime.

Please be  **extremely**  careful with the suggestion to use the new AWS Serializer as suggested here. The new Serializer uses the namespace System.Text.Json and this library does  **not** support everything that the current library (Newtonsoft.Json)  [supports](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to). The biggest problem, in my opinion, is the lack of support for loops. So, use it at your own risk.

You can find this example also in my Github. It’s in the same place, but under the branch `core31-aws`.

### Step 1:

We’ll update the serverless.template.

We’ll update the Runtime to `dotnetcore3.1`.

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/3.1%20framework%201.png)

### Step 2:

Let’s modify our csproj file to have a simpler version.

We’ll use the latest template provided by Amazon. This mean removing NewtonSoft.Json and updating the AWS libraries to the latest version:

### Step 3:

Remove the Startup.cs and Lambdaexecuter.cs as they are remnants from the custom Framework project

### Step 4:

Update the aws-lambda-tools-defaults.json with the instructions given by AWS, namely updated `msbuild-parameters`:

```
{  
 "Information" :   
 [  
  "This file provides default values for the deployment wizard inside Visual Studio and the AWS Lambda commands added to the .NET Core CLI.",  
  "To learn more about the Lambda commands with the .NET Core CLI execute the following command at the command line in the project root directory.",  
  "dotnet lambda help",  
  "All the command line options for the Lambda command can be specified in this file."  
 ],  
 "profile" : "lambda",  
 "region" : "us-west-2",  
 "configuration" : "Release"**,**  
 "framework" : "netcoreapp3.1",  
 "s3-prefix" : "AWSServerless2/",  
 "template" : "serverless.template",  
 "template-parameters" : "",  
 "s3-bucket" : "dgarber-serverless-demo-bucket",  
 "stack-name": "dgarber-stack",  
 "msbuild-parameters": "/p:PublishReadyToRun=true --self-contained false"  
}
```

### Step 5:

Update your Function.cs to use the new Serializer:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/3.1%20framework%202.png)

### Step 6:

Compile and deploy!

We don’t need to redo our dockerfile or recompile our image, so we only need to run:

`docker run --rm -v %cd%:/app diegobuild`

You’ll see something like this when your package is complete:

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/3.1%20framework%203.png)

Now, we deploy it just like we did before and we try again.

We get…

![Image for post](https://github.com/zzaman/LambdaExample/blob/feature/README/media/3.1%20framework%204.png)

Wait… 321ms??? YES.

## But, there MUST be a drawback, right?

Well, yes!

To get these numbers, we rely on using the new Serializer that uses System.Text.Json that doesn’t need to be initialized but this can be called a drawback to this method.
