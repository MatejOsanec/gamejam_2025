public class FileOperationResult { }

public class LoadFileResult : FileOperationResult {

    public string value = null;
}

public class LoadFilesResult : FileOperationResult {

    public string[] values = null;
}

public class FileExistsResult : FileOperationResult {

    public bool fileExists = false;
}

public class FilesExistResult : FileOperationResult {

    public bool[] filesExist = null;
}