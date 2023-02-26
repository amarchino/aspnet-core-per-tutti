ARG VARIANT=7.0-bullseye
FROM mcr.microsoft.com/devcontainers/dotnet:0-${VARIANT}

# [Choice] Node.js version: lts/*, 16, 14, 12, 10
ARG NODE_VERSION="lts/*"
RUN if [ "${NODE_VERSION}" != "none" ]; then su vscode -c ". /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# .NET Core 2.1.30
RUN wget --no-verbose https://download.visualstudio.microsoft.com/download/pr/5797d98a-8faf-472d-925c-931ac542d3c8/e48942da88f4d9d653a7b5c0790e7724/dotnet-sdk-2.1.818-linux-x64.tar.gz --directory-prefix=/tmp \
    && tar --extract --file=/tmp/dotnet-sdk-2.1.818-linux-x64.tar.gz --directory /usr/share/dotnet \
    && rm /tmp/dotnet-sdk-2.1.818-linux-x64.tar.gz

# .NET Core 2.2.8
RUN wget --no-verbose https://download.visualstudio.microsoft.com/download/pr/022d9abf-35f0-4fd5-8d1c-86056df76e89/477f1ebb70f314054129a9f51e9ec8ec/dotnet-sdk-2.2.207-linux-x64.tar.gz --directory-prefix=/tmp \
    && tar --extract --file=/tmp/dotnet-sdk-2.2.207-linux-x64.tar.gz --directory /usr/share/dotnet \
    && rm /tmp/dotnet-sdk-2.2.207-linux-x64.tar.gz

# .NET Core 3.0.3
RUN wget --no-verbose https://download.visualstudio.microsoft.com/download/pr/43f3a3bd-3df2-41e6-beca-3ec4952ca6c4/30fe7779249607d1bb3bb4b20d61a479/dotnet-sdk-3.0.103-linux-x64.tar.gz --directory-prefix=/tmp \
    && tar --extract --file=/tmp/dotnet-sdk-3.0.103-linux-x64.tar.gz --directory /usr/share/dotnet \
    && rm /tmp/dotnet-sdk-3.0.103-linux-x64.tar.gz

# .NET Core 3.1.32
RUN wget --no-verbose https://download.visualstudio.microsoft.com/download/pr/e89c4f00-5cbb-4810-897d-f5300165ee60/027ace0fdcfb834ae0a13469f0b1a4c8/dotnet-sdk-3.1.426-linux-x64.tar.gz --directory-prefix=/tmp \
    && tar --extract --file=/tmp/dotnet-sdk-3.1.426-linux-x64.tar.gz --directory /usr/share/dotnet \
    && rm /tmp/dotnet-sdk-3.1.426-linux-x64.tar.gz

# .NET 5.0.17
RUN wget --no-verbose https://download.visualstudio.microsoft.com/download/pr/904da7d0-ff02-49db-bd6b-5ea615cbdfc5/966690e36643662dcc65e3ca2423041e/dotnet-sdk-5.0.408-linux-x64.tar.gz --directory-prefix=/tmp \
    && tar --extract --file=/tmp/dotnet-sdk-5.0.408-linux-x64.tar.gz --directory /usr/share/dotnet \
    && rm /tmp/dotnet-sdk-5.0.408-linux-x64.tar.gz

# Install other versions of the runtime if necessary

RUN curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg -o /usr/share/keyrings/yarn-keyring.asc \
  && sed -i '1s;^deb.*$;deb [arch=amd64 signed-by=/usr/share/keyrings/yarn-keyring.asc] https://dl.yarnpkg.com/debian/ stable main;' /etc/apt/sources.list.d/yarn.list \
  && apt update \
  && apt install sqlite3

# Set easier configuration
RUN chmod 777 /usr/share/dotnet/sdk

USER vscode

RUN /usr/bin/dotnet dev-certs https \
  && echo 'export PATH="$PATH:/home/vscode/.dotnet/tools"' >> ~/.bash_profile
