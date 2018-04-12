Imports VBCodeStandard.Test.TestHelper
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VBCodeStandard.Test
	<TestClass>
	Public Class StringEqualsUnitTests
		Inherits CodeFixVerifier

		<TestMethod>
		Public Sub StringEquals()

			Const test1 = "
Module Module1
	Sub Main()
		Dim a = (""ssdadad"" = ""safhafhafh"")
	End Sub
End Module"

			Const test2 = "
Module Module1
	Sub Main()
		Dim a = (""ssdadad"" = """")
	End Sub
End Module"

			Const test3 = "
Module Module1
	Sub Main()
		Dim a = (""ssdadad"" <> ""safhafhafh"")
	End Sub
End Module"

			Const test4 = "
Module Module1
	Sub Main()
		Dim a = (""ssdadad"" <> """")
	End Sub
End Module"

			Const test5 = "
Module Module1
	Sub Main()
		Dim a = (""ssdadad"" <> String.Empty)
	End Sub
End Module"

			Dim expected = New DiagnosticResult() With {.Id = StringEqualsAnalyzer.DiagnosticId, .Severity = DiagnosticSeverity.Info}

			VerifyBasicDiagnostic(test1, expected)
			VerifyBasicDiagnostic(test2, expected)
			VerifyBasicDiagnostic(test3, expected)
			VerifyBasicDiagnostic(test4, expected)
			VerifyBasicDiagnostic(test5, expected)

			Const fixtest1 = "
Module Module1
    Sub Main()
        Dim a = (String.Equals(""ssdadad"", ""safhafhafh"", StringComparison.Ordinal))
    End Sub
End Module"

			Const fixtest2 = "
Module Module1
    Sub Main()
        Dim a = (String.IsNullOrEmpty(""ssdadad""))
    End Sub
End Module"

			Const fixtest3 = "
Module Module1
    Sub Main()
        Dim a = (Not String.Equals(""ssdadad"", ""safhafhafh"", StringComparison.Ordinal))
    End Sub
End Module"

			Const fixtest4 = "
Module Module1
    Sub Main()
        Dim a = (Not String.IsNullOrEmpty(""ssdadad""))
    End Sub
End Module"

			Const fixtest5 = "
Module Module1
    Sub Main()
        Dim a = (Not String.IsNullOrEmpty(""ssdadad""))
    End Sub
End Module"
			VerifyBasicFix(test1, fixtest1, allowNewCompilerDiagnostics:=True)
			VerifyBasicFix(test2, fixtest2, allowNewCompilerDiagnostics:=True)
			VerifyBasicFix(test3, fixtest3, allowNewCompilerDiagnostics:=True)
			VerifyBasicFix(test4, fixtest4, allowNewCompilerDiagnostics:=True)
			VerifyBasicFix(test5, fixtest5, allowNewCompilerDiagnostics:=True)
		End Sub

		' No diagnostics expected to show up. 
		<TestMethod>
		Public Sub StringEqualsNoDiagnostic()
			Const test = "Dim a = (2 = 1)"
			VerifyBasicDiagnostic(test)
		End Sub

		Protected Overrides Function GetBasicCodeFixProvider() As CodeFixProvider
			Return New StringEqualsCodeFixProvider()
		End Function

		Protected Overrides Function GetBasicDiagnosticAnalyzer() As DiagnosticAnalyzer
			Return New StringEqualsAnalyzer()
		End Function

	End Class
End Namespace