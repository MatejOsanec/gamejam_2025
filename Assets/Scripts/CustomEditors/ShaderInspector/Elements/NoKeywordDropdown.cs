namespace BGLib.ShaderInspector {

    using System.Collections.Generic;
    using UnityEngine;

    /// Does exactly the same thing as KeywordDropdown, shortcut just for the clarity of inspectors code
    public class NoKeywordDropdown : KeywordDropdown {

        public class NoKeywordOption : Option {

            public NoKeywordOption(
                string displayName,
                string description = null,
                string documentationUrl = null,
                string documentationButtonLabel = null,
                bool countsTowardsKeywordCount = true,
                List<Element> childElements = null,
                DisplayFilter displayFilter = null,
                string displayFilterErrorMessage = null
            ) : base(
                keyword: string.Empty,
                displayName,
                description,
                documentationUrl,
                documentationButtonLabel,
                countsTowardsKeywordCount,
                childElements,
                displayFilter,
                displayFilterErrorMessage
            ) { }
        }

        public NoKeywordDropdown(
            string propertyName,
            Style style,
            IReadOnlyList<NoKeywordOption> options,
            string displayName = null,
            string tooltip = null,
            string description = null,
            string documentationUrl = null,
            string documentationButtonLabel = null,
            bool? countsTowardsKeywordCount = null,
            List<Element> childElements = null,
            Color? backgroundColor = null,
            DisplayFilter displayFilter = null,
            DisplayFilter enabledFilter = null
        ) : base(
            propertyName,
            style,
            options,
            displayName,
            tooltip,
            description,
            documentationUrl,
            documentationButtonLabel,
            countsTowardsKeywordCount,
            childElements,
            backgroundColor,
            displayFilter,
            enabledFilter
        ) { }
    }
}
