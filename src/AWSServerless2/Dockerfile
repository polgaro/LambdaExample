FROM mcr.microsoft.com/dotnet/core/sdk:3.1

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        zip \
		curl \
    && rm -rf /var/lib/apt/lists/* \
	&& dotnet tool install -g Amazon.Lambda.Tools

ENV PATH="$PATH:/root/dotnet:/root/.dotnet/tools"

WORKDIR /app
ENTRYPOINT ["dotnet-lambda", "package", "--config-file aws-lambda-tools-defaults.json"]