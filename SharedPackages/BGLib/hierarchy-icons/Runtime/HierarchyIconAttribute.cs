using System;
using UnityEngine;

namespace BGLib.HierarchyIcons {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HierarchyIconAttribute : Attribute {

        public readonly string gameObjectTooltip;
        public readonly string gameObjectIconPath;
        public readonly Color gameObjectIconTint;

        public readonly string parentTooltip;
        public readonly string parentIconPath;
        public readonly Color parentIconTint;

        public HierarchyIconAttribute(
            string gameObjectTooltip,
            string gameObjectIconPath,
            string? gameObjectIconHex = null,
            string? parentTooltip = null,
            string? parentIconPath = null,
            string? parentIconHex = null
        ) {
            this.gameObjectTooltip = gameObjectTooltip;
            this.gameObjectIconPath = gameObjectIconPath;
            ColorUtility.TryParseHtmlString(gameObjectIconHex ?? "#ffffff", out this.gameObjectIconTint);
            this.parentTooltip = parentTooltip ?? string.Empty;
            this.parentIconPath = parentIconPath ?? string.Empty;
            ColorUtility.TryParseHtmlString(parentIconHex ?? "#ffffff", out this.parentIconTint);
        }

        public HierarchyIconAttribute(
            string gameObjectTooltip,
            Icon gameObjectIconPath = Icon.None,
            string? gameObjectIconHex = null,
            string? parentTooltip = null,
            Icon parentIconPath = Icon.None,
            string? parentIconHex = null
        ) {
            this.gameObjectTooltip = gameObjectTooltip;
            this.gameObjectIconPath = Defines.iconDatabase[gameObjectIconPath] ?? string.Empty;
            ColorUtility.TryParseHtmlString(gameObjectIconHex ?? "#ffffff", out this.gameObjectIconTint);
            this.parentTooltip = parentTooltip ?? string.Empty;
            if (!Defines.iconDatabase.TryGetValue((Icon)parentIconPath!, out this.parentIconPath)) {
                this.parentIconPath = string.Empty;
            }
            ColorUtility.TryParseHtmlString(parentIconHex ?? "#ffffff", out this.parentIconTint);
        }
    }
}
