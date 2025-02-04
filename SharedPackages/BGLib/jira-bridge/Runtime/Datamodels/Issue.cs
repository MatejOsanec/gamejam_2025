using System;

namespace BGLib.JiraBridge {

    [Serializable]
    public class Issue {

        public IssueFields fields = new("Unnamed Issue", string.Empty);
    }

    [Serializable]
    public class IssueFields {

        public Project project = new();
        public AssigneeOrReporter assignee = null;
        public AssigneeOrReporter reporter = null;
        public string summary = null;
        public string description = null;
        public IssueType issueType = new();

        public IssueFields(string summary, string description) {

            if (string.IsNullOrEmpty(summary)) {
                throw new System.Exception("Summary can't be empty");
            }
            this.summary = summary;
            this.description = description;
        }

        public IssueFields(string summary, string description, AssigneeOrReporter assignee, AssigneeOrReporter reporter) {

            if (string.IsNullOrEmpty(summary)) {
                throw new System.Exception("Summary can't be empty");
            }
            this.summary = summary;
            this.description = description;
            this.assignee = assignee;
            this.reporter = reporter;
        }
    }

    [Serializable]
    public class IssueType {

        public string name = "Bug";
    }

    [Serializable]
    public class AssigneeOrReporter {

        public string id;
    }
}
