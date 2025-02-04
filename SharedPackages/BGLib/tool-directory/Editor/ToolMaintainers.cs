
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public enum ToolMaintainer {

    Unknown,
    RandySchouten,
    JoaoBertolino,
    DannyDeBruijne,
    TechArt,
    AlexEfremov,
    ToursFrontend,
    KarenHovhannisyan,
    FilipHauptfleish,
}

public struct ToolMaintainerData {

    public ToolMaintainer type;
    public string name;
    public string link;
    public bool linkIsEmail;
}

public static class ToolMaintainers {

    private static readonly Dictionary<ToolMaintainer, ToolMaintainerData> _maintainerDatas = new ToolMaintainerData[] {
        new() {
            type = ToolMaintainer.Unknown,
            name = "Beat Games",
            link = "https://fb.workplace.com/groups/1544469759230063", // Beat Games Development group
            linkIsEmail = false,
        },
        new() {
            type = ToolMaintainer.RandySchouten,
            name = "Randy Schouten",
            link = "rschouten@meta.com",
            linkIsEmail = true,
        },
        new() {
            type = ToolMaintainer.JoaoBertolino,
            name = "João Bertolino",
            link = "joaoneto@meta.com",
            linkIsEmail = true,
        },
        new() {
            type = ToolMaintainer.DannyDeBruijne,
            name = "Danny de Bruijne",
            link = "ddebruijne@meta.com",
            linkIsEmail = true,
        },
        new() {
            type = ToolMaintainer.TechArt,
            name = "Tech art",
            link = "https://fb.workplace.com/groups/1544469759230063", // Beat Games Development group
            linkIsEmail = false,
        },
        new() {
            type = ToolMaintainer.AlexEfremov,
            name = "Alex Efremov",
            link = "jofr@meta.com",
            linkIsEmail = true,
        },
        new() {
            type = ToolMaintainer.ToursFrontend,
            name = "Tours Frontend",
            link = "https://fb.workplace.com/groups/869884164448457", // Beat Saber Tours group,
            linkIsEmail = false,
        },
        new() {
            type = ToolMaintainer.KarenHovhannisyan,
            name = "Karen Hovhannisyan",
            link = "khovhannisyan@meta.com",
            linkIsEmail = true,
        },
        new() {
            type = ToolMaintainer.FilipHauptfleish,
            name = "Filip Hauptfleish",
            link = "filipha@meta.com",
            linkIsEmail = true,
        }
    }.ToDictionary(maintainer => maintainer.type);

    public static bool TryGetMaintainer(ToolMaintainer maintainer, out ToolMaintainerData maintainerData) =>
        _maintainerDatas.TryGetValue(maintainer, out maintainerData);
}
