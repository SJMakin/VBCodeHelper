<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class VariableDeclarationAnalyzer
    Inherits VbCodingStandardsAnalyzerBase

    Public Const DiagnosticId = DiagnosticIdProvider.VariableDeclaration

	Private Const Category = "Performance"

	Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.VariableDeclarationRuleAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.VariableDeclarationAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.VariableDeclarationAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)


	Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(Rule)
		End Get
	End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf Analyze, SyntaxKind.FieldDeclaration, SyntaxKind.LocalDeclarationStatement)
    End Sub

    Private Function IsConst(m As SyntaxToken) As Boolean
		Return m.Kind() = SyntaxKind.ConstKeyword
	End Function

    Private Sub CheckDeclartor(declarators As SeparatedSyntaxList(Of VariableDeclaratorSyntax), context As SyntaxNodeAnalysisContext, localDeclaration As Boolean)
        For Each declarator In declarators
            ' This only makes sense for simple as clauses (as opposed to As New syntax).
            Dim simpleAs = TryCast(declarator.AsClause, SimpleAsClauseSyntax)
            If (simpleAs Is Nothing) Then Exit Sub

            ' Get info about type.
            Dim initInfo = context.SemanticModel.GetTypeInfo(simpleAs.Type)
            If (initInfo.Type Is Nothing) Then Exit Sub

            If initInfo.Type.IsValueType Then
                ' Check it has an equals clause. 
                If (declarator.Initializer Is Nothing) Then Exit Sub
                ' Try and get the init value.
                Dim initValue = context.SemanticModel.GetConstantValue(declarator.Initializer.Value)

                ' If there is a value and it is a value type and the value is the default. 
                If initValue.Value IsNot Nothing AndAlso initValue.Value.Equals(Activator.CreateInstance(initValue.Value.GetType())) Then
                    Dim diag = Diagnostic.Create(Rule, declarator.GetLocation())
                    context.ReportDiagnostic(diag)
                End If
            ElseIf localDeclaration AndAlso initInfo.Type.IsReferenceType AndAlso declarator.Initializer Is Nothing Then
                Dim diag = Diagnostic.Create(Rule, declarator.GetLocation())
                context.ReportDiagnostic(diag)
            End If

            If initInfo.Type.IsReferenceType AndAlso declarator.Initializer IsNot Nothing AndAlso TypeOf declarator.Initializer.Value Is ObjectCreationExpressionSyntax Then
                Dim diag = Diagnostic.Create(Rule, declarator.GetLocation())
                context.ReportDiagnostic(diag)
            End If
        Next
    End Sub

    Private Shared ReadOnly _loopSyntaxBlockKinds As New List(Of SyntaxKind) From {SyntaxKind.ForBlock,
																				   SyntaxKind.ForEachBlock,
																				   SyntaxKind.WhileBlock,
																				   SyntaxKind.DoUntilLoopBlock,
																				   SyntaxKind.DoWhileLoopBlock,
																				   SyntaxKind.DoLoopUntilBlock,
																				   SyntaxKind.DoLoopWhileBlock,
																				   SyntaxKind.SimpleDoLoopBlock}

	Private Shared Function IsInLoop(node As LocalDeclarationStatementSyntax) As Boolean
		Dim test As SyntaxNode = node.Parent
		While True
			If _loopSyntaxBlockKinds.Contains(test.Kind()) Then Return True
			If (test.Parent Is Nothing) Then Exit While
			test = test.Parent
		End While

		Return False
	End Function

    Protected Overrides Sub CheckSyntax(context As SyntaxNodeAnalysisContext)
        Dim localDeclaration = TryCast(context.Node, LocalDeclarationStatementSyntax)
        If localDeclaration IsNot Nothing Then
            If localDeclaration.Modifiers.Any(AddressOf IsConst) Then Exit Sub

            If IsInLoop(localDeclaration) Then Exit Sub

            CheckDeclartor(localDeclaration.Declarators, context, True)
        End If

        Dim fieldDeclaration = TryCast(context.Node, FieldDeclarationSyntax)
        If fieldDeclaration IsNot Nothing Then
            If fieldDeclaration.Modifiers.Any(AddressOf IsConst) Then Exit Sub

            CheckDeclartor(fieldDeclaration.Declarators, context, False)
        End If
    End Sub
End Class