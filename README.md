# Assisted Review Portal

Build status:

[![Build Status](https://dev.azure.com/veritasg11n/glob-auto/_apis/build/status/VeritasOS.assisted-review-portal?branchName=master)](https://dev.azure.com/veritasg11n/glob-auto/_build/latest?definitionId=2&branchName=master)

Docker image:

[`docker pull apoblock/arp`](https://hub.docker.com/r/apoblock/arp)

[arp-uploader](https://www.npmjs.com/package/arp-uploader) tool:

[![npm version](https://badge.fury.io/js/arp-uploader.svg)](https://badge.fury.io/js/arp-uploader)

## Overview

The Assisted Review Portal (ARP) is a web application that stores screenshots and associated incidents in a structured manner. It has been developed to conduct linguistic reviews using screenshots of the product's user interface. It enables users to see the translated strings in context to provide accurate translations. ARP also has an image comparison feature to identify any changes that have occurred between different versions of the product, which can reduce the manual effort in regression testing.

You can access the API directly, or use the [arp-uploader](https://www.npmjs.com/package/arp-uploader) to upload the screenshots.

## Demo

### Video demonstration

[![Watch the demo](https://f1.media.brightcove.com/8/4396107486001/4396107486001_6072569138001_6072572517001-vs.jpg?pubId=4396107486001&videoId=6072572517001)](https://bcove.video/2Q94o1y)

### Demo portal

You can access and play around with a demo portal at: https://arp-demo.azurewebsites.net

Use *arpuser* as a login with *User@123* password.

## Usage

### Prerequisites

* Install Docker - https://docs.docker.com/install/

### Configuration

* All the configuration settings are defined in `ARP/appsettings.json`, and can be overwritten by the environmental variables (see [below](#advanced-setup-with-docker-run))
* Authentication – you can setup your local admin and user accounts, and/or LDAP for authentication

### Basic (demo) setup with docker-compose

Get the [docker-compose.yml](https://github.com/VeritasOS/assisted-review-portal/blob/master/docker-compose.yml) file to local folder, then from the same folder run:

* `docker-compose build` - This will download and build the containers
* `docker-compose up` – This will start the containers
* `http://localhost:8000` – The portal can be accessed at this address, with the user accounts configured in `ARP/appsettings.json` or LDAP (if available)
* *Note:* that in this configuration the database and images storage are inside the docker images and will get reset when you rebuild

### Advanced setup with docker run

* Create an MSSQL database dedicated for ARP, save the connection string
  * *Note:* APR will initialize the database and apply migrations if needed
* Create a volume where you will store the image files, and have it accessible from the Docker host, e.g. `/var/imagestore`
* Create the settings file `env.list` that will overwrite entries from `ARP/appsettings.json`, here are the values you can update:
  * `ConnectionStrings:GarbDatabase=Data Source=my-db-server1;Initial Catalog=ARP;User ID=arb;Password=***DBPassword***` - connection string to the database
  * `StorageHelperSettings:StorageRootFolder=/arpstore/` - folder in the container image, where screenshots will be stored. *Note:* this should be mapped to some persistent/local storage using `-v` option in the `docker run` command
  * `Authentication:LdapServer=ldap.mydomain.org` - address of LDAP server you will use to authenticate users
  * `Authentication:LdapDomain=mydomain.org` - domain used by LDAP
  * `Authentication:LocalAdminName=admin` - local admin account name
  * `Authentication:LocalAdminPwd=***MyAdminPassword***` - local admin'd password
  * `Authentication:LocalReadOnlyName=user` - local read-only account name
  * `Authentication:LocalReadOnlyPwd=***MyReadOnlyPassword***` - local read-only user's password
* Start the image
  * `docker run -d -p 5000:80 -v /var/imagestore:/arpstore --env-file env.list --name myportal apoblock/arp`
  * The Web App will be available on `localhost:5000`
* `docker run` parameters
  * `--name` - name of your local image
  * `-v` - mapping of your local filesystem to the container image filesystem
  * `-p` - mapping of your local port to container image port
  * `-d` - detach - container will be run in the background
  * `--env-file` - file with defined environment variables

## Building

## Building the container

Prerequisites - install following tools:

* Docker

Clone the ARP repository `git clone https://github.com/VeritasOS/assisted-review-portal.git`, then from the root folder:

* Build the ARP WebAPI docker image
  * `docker build -t arpwebapp .`
  * Build will be created in the docker images (multi-stage build)
  * The final image will only contain files required by the runtime
* Run the ARP WebAPI docker image (see [above](#advanced-setup-with-docker-run) for explanation)
  * `docker run -d -p 5000:80 -v /var/imagestore:/arpstore --env-file env.list --name myportal arpwebapp`

## Building/debugging in Visual Studio

Prerequisites - install following tools:

* NodeJS
* Angular CLI 6+
* .NET Core 2.2+
* Visual Studio 2017+

Build:

* Clone the ARP repository `git clone https://github.com/VeritasOS/assisted-review-portal.git`
* Open the `ARP.sln` in Visual Studio
* Build Solution
* Start Debugging
