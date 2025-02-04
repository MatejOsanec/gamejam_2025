using System.Text;
using System.Threading.Tasks;
using BGLib.JiraBridge;
using NUnit.Framework;
using UnityEngine;

// #define BS_JIRA_BRIDGE_TEST
#if BS_JIRA_BRIDGE_TEST
public class JiraBridge {

    [Test]
    public async Task CreateTicket() {

        JiraClient jiraClient = new();
        LogMonitor logMonitor = new();

        var createResult = await jiraClient.CreateTicket(new IssueFields("Ticket created from test runner", "My cool ticket"));
        Assert.IsNotNull(createResult);
        Debug.Log($"Created ticket with key {createResult.key}. Full Data: {createResult.self}");

        for (int i = 0; i < 100; i++) {
            Debug.Log($"Dummy log for log monitor {i + 1}");
        }

        var attachmentResult = await jiraClient.UploadAttachmentsToTicket(createResult.key, new() {
            { "log.log", new UTF8Encoding().GetBytes(logMonitor.GetLog()) }
        });
        Assert.IsNotNull(attachmentResult);
        Assert.Greater(attachmentResult.Count, 0);
        Debug.Log($"Uploaded {attachmentResult.Count} attachment(s)");
    }

    [Test]
    public async Task GetUsers() {

        JiraClient jiraClient = new();
        var users = await jiraClient.GetUsers();
        Assert.IsNotNull(users);
        Assert.Greater(users.Count, 0);

        Debug.Log("Found the following Jira users:");
        foreach (var user in users) {
            Debug.Log($"- {user.displayName}: {user.emailAddress}");
        }
    }
}
#endif // BS_JIRA_BRIDGE_TEST
