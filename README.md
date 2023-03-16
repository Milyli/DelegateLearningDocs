# Delegate API & Webhook Learning Guide

This sample project provides examples for accessing the Delegate API and creating webhooks compatible with the Delegate Webhook Step.

We'll explore potential workflows and introduce ideas to help you use the Delegate Webhook Step as a part of your external Relativity processes. We hope this project inspires your team to find creative ways to tie together non-Relativity systems and Relativity!

# Using This Project

This project is "F5-able." This means it's ready to go out of the box whether you are using Visual Studio or Rider. Feel free to download a zip of the project from GitHub or use the provided URL to clone it.

This README contains details to help you get started and familiar with using the project.

# Project Setup

## Configuration

After cloning or downloading the project, our first step is to fill out the configuration. We recommend keeping your learning project in source control. In the following examples, a Development environment app settings file and ensuring that this file is not committed to your source control.

Add a new file to the project named `appsettings.Development.json`. It will appear under your `appsettings.json` file.

![AppSettings Image](https://i.imgur.com/RY9rpI8.png)

Open this file and copy the contents of the `appsettings.json` file into it. 

Your `appsettings.Development.json` should look like this:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": ""
  },
  "SlackEndpoint": "",
  "RelativityBaseUri": "",
  "RelativityOAuthClientId": "",
  "RelativityOAuthClientSecret": ""
}
```

Below we've provided details for each of the five different configuration settings.

| Configuration | Details |
|--|--|
| ConnectionStrings:Default | This sample project uses Hangfire and the Hangfire dashboard to show how to spin off long-running processes without blocking Delegate. Hangfire requires a database. The project will not run without this setting configured. For early local development, utilizing LocalDB is recommended: `Server=(localdb)\\mssqllocaldb;Database=[INPUTDATABASENAME];Trusted_Connection=True;MultipleActiveResultSets=true` |
| SlackEndpoint| We'll cover setting up a Slack endpoint to deliver messages to a slack channel. The project will run without this setting, but the two Slack Webhooks will not work. |
| RelativityBaseUri | This is the base address for your Relativity instance with the trailing `/Relativity`. For example, `http://RELATIVITYDEVVM/`. |
| RelativityOAuthClientId | Delegate's API is hosted on the Delegate custom page. To make API calls to custom pages from outside Relativity, an OAuth2 client is required. For more information on setting one up, please take a look at the [Relativity Documentation](https://help.relativity.com/Server2022/Content/Relativity/Authentication/OAuth2_clients.htm).  |
| RelativityOAuthClientSecret | The Client Secret for your Relativity OAuth2 client. |

As with all software, we strongly recommend following best practices regarding securing configuration information.

## Swagger

This project uses Swagger to provide you with the tools for testing out of the box. When running the project locally, you will automatically navigate to the swagger index, where you can execute each API call and see schema definitions for the models in the project.

This project demonstrates utilizing an intermediary API for hosting Webhooks and calling the Delegate API. The API section below will include specific requirements for calling the Delegate API.

![Swagger UI](https://i.imgur.com/rQoQWdt.png)

# API

This section outlines using the `Start Workflow` and `Workflow Status` APIs. The Delegate API activity flow is detailed in the diagram below.

![API Activity Diagram](https://i.imgur.com/yNXzrdJ.png)

## OAuth2 Client

To work with APIs hosted on custom pages in Relativity, you must utilize an OAuth2 client with `Flow Grant Type` set to `Client Credential`. Delegate has built-in security that validates if the user configured in the Client Credential can access the workspace.

After creating and configuring your client, Relativity requires your requests to be decorated with an Authorization header with the value `Bearer [token]` as its value.

A helper method is included in the `RelativityController.cs` file that connects to the Relativity token service and retrieves the bearer token for your OAuth2 client.

```
var formParameters = new Dictionary<string, string>
{
    { "client_id", _configuration.GetValue<string>("RelativityOAuthClientId") },
    { "client_secret", _configuration.GetValue<string>("RelativityOAuthClientSecret") },
    { "scope", "SystemUserInfo" },
    { "grant_type", "client_credentials" }
};

var url = $"Relativity/Identity/connect/token";

var httpClient = _httpClientFactory.CreateClient("UnsecureRelativityClient");
httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("RelativityBaseUri"));

var httpResponseMessage = await httpClient.PostAsync(url, new FormUrlEncodedContent(formParameters));

if (!httpResponseMessage.IsSuccessStatusCode)
{
    var errorMessage = await httpResponseMessage.Content.ReadAsStringAsync();
    throw new Exception("Failed to retrieve token: " + errorMessage);
}
else
{
    var responseContent = JsonConvert.DeserializeObject<ExpandoObject>(await httpResponseMessage.Content.ReadAsStringAsync());
    var responseJson = JToken.Parse(JsonConvert.SerializeObject(responseContent));
    var token = responseJson["access_token"].ToString().Trim(new char[] { '{', '}' });
    return token;
}
```

The HTTP Client in this scenario ignores development server certificates and should not be used for production. 

## Start Workflow

Workflows can be started by submitting a POST request to the workflow start endpoint. Workflows are started by name. If more than one workflow exists with the name, all workflows matching that name will be run.

### URL
`<host>/Relativity/CustomPages/66e2c574-c9d9-46f5-8581-98db7c016464/api/v1/{WorkspaceId}/workflow/start`

### Verb

POST

### Headers

| Header | Value |
|--|--|
| Content-Type | application/json |
| X-CSRF-Header | - |
| Authorization | Bearer [token] |

### Request Body

```
{
   "workflowName": "Name",
   "isRestart": false
}
```

The IsRestart property is optional and is false by default. Setting the value to true allows API calls to restart a failed or paused workflow from the beginning.

### Response Body

```
[{
      success: true,
      Errors: [],
      workflowId,
      startedOn,
      startedStep,
      StatusUrl
},
{
   ...
}]
``` 

## Workflow Status

Workflow status can be checked by submitting a GET request to the workflow status endpoint. Workflow status can be checked at any time.

### URL

`<host>/Relativity/CustomPages/66e2c574-c9d9-46f5-8581-98db7c016464/api/v1/{WorkspaceId}/workflow/status?workspaceId={WorkspaceId}&workflowName={WorkflowName}`

### Verb

GET

### Headers

| Header | Value |
|--|--|
| Content-Type | application/json |
| X-CSRF-Header | - |
| Authorization | Bearer [token] |

### Response Body

```
[{
   success: false,
   workflowId = {workflowId},
   startedOn = null,
   StartedStep = null,
   StatusUrl = null
   Errors: [error1, error2],
   
},
{
   ...
}]
```

# Webhook Step

Delegate 3.2 adds a Webhook Step. The Webhook Step issues a post request to a configured URL with some basic information about the workflow in the request body. The provided URL must be HTTPS.

**Important Note** - The Delegate Webhook step will wait up to 30 seconds to receive a response from your webhook. It is essential to design your webhooks so they can begin their work and respond in a timely fashion. An example in this project demonstrates utilizing Hangfire to accomplish this requirement. A response is issued to Delegate after the Hangfire task is successfully enqueued.

![Delegate Webhook Step](https://i.imgur.com/Q0widAk.png)

To help jumpstart building your webhooks, the following model works great alongside [.NET 7 API Controller model binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-7.0).

```
    public class WebhookStep
    {
        public string? InstanceId { get; set; }
        public string? FriendlyInstanceName { get; set; }
        public int? WorkspaceId { get; set; }
        public string? WorkflowName { get; set; }
        public string? RelativityInstanceURL { get; set; }
    }
```

## Basic Webhooks

Several basic webhooks are provided in the WebhookController that provide simple responses. These can help you get an understanding of the communication that takes place between Delegate and the tools you are creating. To use these, run the Delegate learning application, create a new Delegate workflow, add a Webhook step, and set the URL.

![Simple Webhook Examples](https://i.imgur.com/G7jitDV.png)

## Slack Message

The included slack webhooks take advantage of Slack applications' *Incoming Webhooks* feature. Follow the [Sending messages using Incoming Webhooks](https://api.slack.com/messaging/webhooks) guide in the Slack documentation.

Following the guide will provide you with an URL that can be added to the `appsettings.Development.json` configuration. The included Slack webhooks can then be utilized in your Delegate workflows.

# Contact

As always, we are here to help! If you have questions, don't hesitate to get in touch with us at [support@milyli.com](mailto:support@milyli.com). The tech support team will forward your question to the Milyli dev team, and we will try to get you an answer promptly.
