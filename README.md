# Delegate Learning Guide

This project provides examples for both accessing the Delegate API and creating webhooks compatible with the Delegate Webhook Step.

We explore potential workflows and introduce ideas that can help you use the Delegate Webhook Step as a part of your external processes or to draw inspiration from.

We will also take a look at how you can call into the Delegate APIs to start and check the status of workflows from external processes!

## Using This Project

Feel free to clone this project or download a zip of it to get started. 

Additionally, don't hesitate to browse the project and explore the project! Follow this readme to learn more.

## Project Setup

The examples in this project utilize both Hangfire and a Slack app webhook. Hangfire requires a database so we'll use the LocalDB for that.

We'll cover setting up Slack later in this readme.

First, lets add configuration for the database and the slack endpoint.

Navigate to the `appsettings.Development.json` file. If it doesn't already exist, add it to the root directory and it will appear below appsettings.json. 

Hangfire is configured in `Program.cs` to use the default connection string. So lets add connection string configuration and the default connection string to this file.

```
"ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=[INPUTDATABASENAME];Trusted_Connection=True;MultipleActiveResultSets=true"
  }
```

You will also need to create the database. Connect to your LocalDB and create a database. Replace `[INPUTDATABASENAME]` with the name of the database you create.

Skipping this step will cause the project to fail during startup.

Finally, add a configuration for `SlackEndpoint`.
```
"SlackEndpoint": ""
```

For now leave this blank.

Your config file should look like this.

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=[INPUTDATABASENAME];Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "SlackEndpoint": ""
}

```

Save your project and F5 to start it.

You should automatically navigate to the OpenAPI Swagger docs.

At this point, you will need to update your configuration to utilize the slack endpoints, but the other webhooks are ready for testing.