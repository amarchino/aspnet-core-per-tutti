ARG VARIANT=7.0-bullseye
FROM mcr.microsoft.com/devcontainers/dotnet:0-${VARIANT}

# .NET 5.0.17
RUN wget --no-verbose https://download.visualstudio.microsoft.com/download/pr/904da7d0-ff02-49db-bd6b-5ea615cbdfc5/966690e36643662dcc65e3ca2423041e/dotnet-sdk-5.0.408-linux-x64.tar.gz --directory-prefix=/tmp \
    && tar --extract --file=/tmp/dotnet-sdk-5.0.408-linux-x64.tar.gz --directory /usr/share/dotnet \
    && rm /tmp/dotnet-sdk-5.0.408-linux-x64.tar.gz

# .NET 6.0.22
RUN wget --no-verbose https://download.visualstudio.microsoft.com/download/pr/d97d1625-d7ed-444c-a7e9-e7b469842960/d8b97220d0d79119e3026da2b956854e/dotnet-sdk-6.0.414-linux-x64.tar.gz --directory-prefix=/tmp \
    && tar --extract --file=/tmp/dotnet-sdk-6.0.414-linux-x64.tar.gz --directory /usr/share/dotnet \
    && rm /tmp/dotnet-sdk-6.0.414-linux-x64.tar.gz

# Install other versions of the runtime if necessary

RUN curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg -o /usr/share/keyrings/yarn-keyring.asc \
  && sed -i '1s;^deb.*$;deb [arch=amd64 signed-by=/usr/share/keyrings/yarn-keyring.asc] https://dl.yarnpkg.com/debian/ stable main;' /etc/apt/sources.list.d/yarn.list \
  && apt update \
  && apt install sqlite3

# Set easier configuration
#RUN chmod 777 /usr/share/dotnet/sdk

#USER vscode

#RUN echo 'export PATH="$PATH:/home/vscode/.dotnet/tools"' >> ~/.bash_profile
