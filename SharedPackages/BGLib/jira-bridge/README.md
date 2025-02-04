# Jira Bridge
Proof-of-concept of how jira tickets can be created through a panic button in Unity.
It could benefit internal playtests and QA workflows

Note that this does not contain the UI side, elsewhere you would need to listen to the panic button event, display a pop-up with summary/desc fields and initiate the ticket creation. Or, do this automated.

As a sample on how to use, you could have a look at the tests.
To make the extension work, you need a `jira-settings.json` file in your `Application.persistentDataPath`. On Windows, for example with Beat Saber, this is `C:\Users\ddebruijne\AppData\LocalLow\Hyperbolic Magnetism\Beat Saber`

Its contents are noted `Settings.cs`, but this is a template for convenience:
```json
{
    "apiUrl": "https://beatgames.atlassian.net/rest/api/2",
    "apiKeyUser": "ddebruijne@meta.com",
    "apiKey": "YOUR_ATLASSIAN_API_KEY",
    "projectKey": "BS"
}
```

An exception will be thrown if the file is not present, so it's recommended to initialize the JiraClient with a try/catch:
```c#

JiraClient jiraClient = null;

public void Initialize() {
	try {
		jiraClient = new();
	}
	catch {
		Debug.Log("Jira bridge not initialized.");
	}
}

```

Features:
- Panic button: press 5 times to fire event
- Log monitor intercepts all messages to attach to the ticket
- Function to get all users in a jira instance; this is handy since by default the reporter is always the API key user.
  - EG in beat saber, we display a dropdown
- Creating a jira ticket
- Adds save data, logs, screenshot and system information.

### Research and resources
- intercepting log messages: https://docs.unity3d.com/ScriptReference/Application-logMessageReceivedThreaded.html
- creating api tokens: https://id.atlassian.com/manage-profile/security/api-tokens
- auth: https://developer.atlassian.com/cloud/jira/platform/basic-auth-for-rest-apis/
- null assignee: https://confluence.atlassian.com/jirakb/how-to-set-assignee-to-unassigned-via-rest-api-in-jira-744721880.html
- api docs
	- https://developer.atlassian.com/server/jira/platform/jira-rest-api-examples/
	- https://confluence.atlassian.com/jirakb/how-to-add-an-attachment-to-a-jira-issue-using-rest-api-699957734.html
	- https://docs.atlassian.com/software/jira/docs/api/REST/8.22.6/#issue/{issueIdOrKey}/attachments-addAttachment
