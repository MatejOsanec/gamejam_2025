using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using BGLib.PackagesCore.Editor;
using NUnit.Framework;

public class CodeFileHandlerTests {

    [Test]
    public void ExtractDefineSymbols_CapturesIfAndElseSymbols() {

        var mockFileSystem = new MockFileSystem();
        const string filePath = "test_code.cs";
        mockFileSystem.AddFile(
            filePath,
            new MockFileData(
                @"
    public class Test {
        public Test() {
        #if MY_SYMBOL
            Debug.Log(""My symbol is present"");
        #elif OTHER_SYMBOL
            Debug.Log(""Other symbol is present"");
        #endif
        }
    }
"
            )
        );
        var codeFileHandler = new CodeFileHandler(mockFileSystem);
        var symbols = codeFileHandler.ExtractDefineSymbols(filePath).ToArray();
        Assert.AreEqual(new[] { "MY_SYMBOL", "OTHER_SYMBOL" }, symbols);
    }

    [Test]
    public void ExtractDefineSymbols_IgnoresLocalSymbols() {

        var mockFileSystem = new MockFileSystem();
        const string filePath = "test_code.cs";
        mockFileSystem.AddFile(
            filePath,
            new MockFileData(
                @"
    public class Test {
        // #define LOCAL_SYMBOL
        public Test() {
 # if LOCAL_SYMBOL
        Debug.Log(""""My symbol is present"""");
#  elif OTHER_SYMBOL
        Debug.Log(""""Other symbol is present"""");
   #   endif //LOCAL_SYMBOL
        }
    }
"
            )
        );
        var codeFileHandler = new CodeFileHandler(mockFileSystem);
        var symbols = codeFileHandler.ExtractDefineSymbols(filePath).ToArray();
        Assert.AreEqual(new[] { "OTHER_SYMBOL" }, symbols);
    }

    [Test]
    public void ExtractDefineSymbols_CapturesMultipleSymbolsInOneLine() {

        var mockFileSystem = new MockFileSystem();
        const string filePath = "test_code.cs";
        mockFileSystem.AddFile(
            filePath,
            new MockFileData(
                @"
    public class Test {
        // #define LOCAL_SYMBOL
        public Test() {
  # if LOCAL_SYMBOL&&SYMBOL_A || SYMBOL_B   &&   ! SYMBOL_C&&(LOCAL_SYMBOL&&SYMBOL_D||(!SYMBOL_E))
        Debug.Log(""""My symbol is present"""");
# elif OTHER_SYMBOL && !LOCAL_SYMBOL || ((((SYMBOL_F))))
        Debug.Log(""""Other symbol is present"""");
 #endif //LOCAL_SYMBOL&&SYMBOL_A || SYMBOL_B   &&   ! SYMBOL_C&&(LOCAL_SYMBOL&&SYMBOL_D||(!SYMBOL_E))
        }
    }
"
            )
        );
        var codeFileHandler = new CodeFileHandler(mockFileSystem);
        var symbols = codeFileHandler.ExtractDefineSymbols(filePath).ToArray();
        Assert.AreEqual(
            new[] {
                "SYMBOL_A",
                "SYMBOL_B",
                "SYMBOL_C",
                "SYMBOL_D",
                "SYMBOL_E",
                "OTHER_SYMBOL",
                "SYMBOL_F"
            },
            symbols
        );
    }

    [Test]
    public void ExtractDefineSymbols_CapturesConditionalAssemblies() {

        var mockFileSystem = new MockFileSystem();
        const string filePath = "test_code.cs";
        mockFileSystem.AddFile(
            filePath,
            new MockFileData(
                @"
    public class Test {
        // #define LOCAL_SYMBOL
        [Conditional(""SYMBOL_A""),Diagnostics.Conditional(""SYMBOL_B"") , Preserve(), Test,  System.Diagnostics.Conditional(""SYMBOL_C"")]
        public void MethodA() {
#if SYMBOL_D&&SYMBOL_E
        Debug.Log(""""My symbol is present"""");
#endif
        }
        [System.Diagnostics.Conditional(""SYMBOL_F"")][  Conditional(""SYMBOL_G"")  ]  [Diagnostics.Conditional(""SYMBOL_H"")]
        public void MethodB() {
        }
    }
"
            )
        );
        var codeFileHandler = new CodeFileHandler(mockFileSystem);
        var symbols = codeFileHandler.ExtractDefineSymbols(filePath).ToArray();
        Assert.AreEqual(
            new[] {
                "SYMBOL_A",
                "SYMBOL_B",
                "SYMBOL_C",
                "SYMBOL_D",
                "SYMBOL_E",
                "SYMBOL_F",
                "SYMBOL_G",
                "SYMBOL_H"
            },
            symbols
        );
    }
}


