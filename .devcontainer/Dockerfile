ARG VARIANT=7.0-bullseye
FROM mcr.microsoft.com/devcontainers/dotnet:0-${VARIANT}

# [Choice] Node.js version: lts/*, 16, 14, 12, 10
ARG NODE_VERSION="lts/*"
RUN if [ "${NODE_VERSION}" != "none" ]; then su vscode -c ". /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# Set easier configuration
RUN chmod 777 /usr/share/dotnet/sdk

# Version 2.1 of the runtime
RUN wget --no-verbose https://download.visualstudio.microsoft.com/download/pr/5797d98a-8faf-472d-925c-931ac542d3c8/e48942da88f4d9d653a7b5c0790e7724/dotnet-sdk-2.1.818-linux-x64.tar.gz --directory-prefix=/tmp \
    && tar --extract --file=/tmp/dotnet-sdk-2.1.818-linux-x64.tar.gz --directory /usr/share/dotnet \
    && rm /tmp/dotnet-sdk-2.1.818-linux-x64.tar.gz

# Install other versions of the runtime if necessary
