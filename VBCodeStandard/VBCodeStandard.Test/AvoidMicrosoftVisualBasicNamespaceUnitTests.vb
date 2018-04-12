Imports VBCodeStandard.Test.TestHelper
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VBCodeStandard.Test
	<TestClass>
	Public Class AvoidMicrosoftVisualBasicNamespaceUnitTests
		Inherits DiagnosticVerifier


		' No diagnostics expected to show up. 
		<TestMethod>
		Public Sub AvoidMicrosoftVisualBasicNamespaceNoDiagnostic()
			Const test = "
Module Module1
    Sub Main()
        Dim s As String = ""0123456789""
        s = s.Substring(1, 3)
    End Sub
End Module
"
			VerifyBasicDiagnostic(test)
		End Sub

		' Diagnostic And CodeFix both triggered And checked for. 
		<TestMethod>
		Public Sub AvoidMicrosoftVisualBasicNamespace()

			Const test1 = "
Imports Microsoft.VisualBasic
Module Module1
    Sub Main()
        Dim s As String = ""0123456789""
        s = Mid(s, 1, 3)
    End Sub
End Module
"

			Const test2 = "
Imports Microsoft.VisualBasic
Module Module1
    Sub Main()
        Dim s As String = ""0123456789""
        s = Format(s, ""0.0000"")
    End Sub
End Module
"

			Dim expected = New DiagnosticResult() With {.Id = AvoidMicrosoftVisualBasicNamespaceAnalyzer.DiagnosticWithoutFixId, .Severity = DiagnosticSeverity.Info}

#Disable Warning CCS0001
			AdditionalReferences.Add(MetadataReference.CreateFromFile(GetType(Strings).Assembly.Location))
#Enable Warning CCS0001

			VerifyBasicDiagnostic(test1, expected)
			VerifyBasicDiagnostic(test2, expected)
		End Sub

		Protected Overrides Function GetBasicDiagnosticAnalyzer() As DiagnosticAnalyzer
			Return New AvoidMicrosoftVisualBasicNamespaceAnalyzer()
		End Function

	End Class
End Namespace