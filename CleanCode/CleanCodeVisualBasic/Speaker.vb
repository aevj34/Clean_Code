Public Class Speaker

    Public Property FirstName As String
    Public Property LastName As String
    Public Property Email As String
    Public Property Exp As System.Nullable(Of Integer)
    Public Property HasBlog As Boolean
    Public Property BlogURL As String
    Public Property Browser As WebBrowser
    Public Property Certifications As List(Of String)
    Public Property Employer As String
    Public Property RegistrationFee As Integer
    Public Property Sessions As List(Of Session)

    ''' <summary>
    ''' Register a speaker
    ''' </summary>
    ''' <returns>speakerID</returns>
    Public Function Register(repository As IRepository) As Nullable(Of Integer)

        'lets init some vars
        Dim speakerId As Nullable(Of Integer) = Nothing
        Dim good As Boolean = False
        Dim appr As Boolean = False
        'var nt = new List<string> {"MVC4", "Node.js", "CouchDB", "KendoUI", "Dapper", "Angular"};
        Dim ot = New List(Of String)() From {"Cobol", "Punch Cards", "Commodore", "VBScript"}

        'DEFECT #5274 DA 12/10/2012
        'We weren't filtering out the prodigy domain so I added it.
        Dim domains = New List(Of String)() From {"aol.com", "hotmail.com", "prodigy.com", "CompuServe.com"}

        If Not String.IsNullOrWhiteSpace(FirstName) Then

            If Not String.IsNullOrWhiteSpace(LastName) Then

                If Not String.IsNullOrWhiteSpace(Email) Then

                    'put list of employers in array
                    Dim emps = New List(Of String)() From {"Microsoft", "Google", "Fog Creek Software", "37Signals"}

                    'DFCT #838 Jimmy 
                    'We're now requiring 3 certifications so I changed the hard coded number. Boy, programming is hard.
                    good = ((Exp > 10 OrElse HasBlog OrElse Certifications.Count() > 3 OrElse emps.Contains(Employer)))

                    If Not good Then
                        'need to get just the domain from the email
                        Dim emailDomain As String = Email.Split("@"c).Last()

                        If Not domains.Contains(emailDomain) AndAlso (Not (Browser.Name = WebBrowser.BrowserName.InternetExplorer AndAlso Browser.MajorVersion < 9)) Then
                            good = True
                        End If
                    End If

                    If good Then
                        'DEFECT #5013 CO 1/12/2012
                        'We weren't requiring at least one session
                        If Sessions.Count() <> 0 Then

                            For Each session As Session In Sessions
                                'For Each tech As String In nt
                                '    If session.Title.Contains(tech) Then
                                '        session.Approved = True
                                '        Exit For
                                '    End If
                                'Next

                                For Each tech As String In ot
                                    If session.Title.Contains(tech) OrElse session.Description.Contains(tech) Then
                                        session.Approved = False
                                        Exit For
                                    Else
                                        session.Approved = True
                                        appr = True
                                    End If
                                Next
                            Next
                        Else
                            Throw New ArgumentException("Can't register speaker with no sessions to present.")
                        End If

                        If appr Then
                            'if we got this far, the speaker is approved
                            'let's go ahead and register him/her now.
                            'First, let's calculate the registration fee. 
                            'More experienced speakers pay a lower fee.
                            If Exp <= 1 Then
                                RegistrationFee = 500
                            ElseIf Exp >= 2 AndAlso Exp <= 3 Then
                                RegistrationFee = 250
                            ElseIf Exp >= 4 AndAlso Exp <= 5 Then
                                RegistrationFee = 100
                            ElseIf Exp >= 6 AndAlso Exp <= 9 Then
                                RegistrationFee = 50
                            Else
                                RegistrationFee = 0
                            End If



                            'Now, save the speaker and sessions to the db.
                            Try
                                speakerId = repository.SaveSpeaker(Me)
                                'in case the db call fails 
                            Catch e As Exception
                            End Try
                        Else
                            Throw New NoSessionsApprovedException("No sessions approved.")
                        End If
                    Else
                        Throw New SpeakerDoesntMeetRequirementsException("Speaker doesn't meet our abitrary and capricious standards.")
                    End If
                Else
                    Throw New ArgumentNullException("Email is required.")
                End If
            Else
                Throw New ArgumentNullException("Last name is required.")
            End If
        Else
            Throw New ArgumentNullException("First Name is required")
        End If

        'if we got this far, the speaker is registered.
        Return speakerId

    End Function


    Public Function Register_Clean(repository As IRepository) As Nullable(Of Integer)

        If String.IsNullOrWhiteSpace(FirstName) Then Throw New ArgumentNullException("First Name is required.")
        If String.IsNullOrWhiteSpace(LastName) Then Throw New ArgumentNullException("Last name is required.")
        If String.IsNullOrWhiteSpace(Email) Then Throw New ArgumentNullException("Email is required.")

        Dim isAptToStandards As Boolean = False
        Dim approvedSessions As Boolean = False
        Dim technologys = New List(Of String)() From {"Cobol", "Punch Cards", "Commodore", "VBScript"}
        Dim domains = New List(Of String)() From {"aol.com", "hotmail.com", "prodigy.com", "CompuServe.com"}
        Dim employers = New List(Of String)() From {"Microsoft", "Google", "Fog Creek Software", "37Signals"}

        isAptToStandards = IsValidToStandards(employers)

        If isAptToStandards = False Then
            isAptToStandards = IsValidDomain(domains)
            Throw New SpeakerDoesntMeetRequirementsException("Speaker doesn't meet our abitrary and capricious standards.")
        End If

        If Sessions.Count = 0 Then Throw New ArgumentException("Can't register speaker with no sessions to present.")

        For Each session As Session In Sessions
            session.Approved = Not IsDesapprovedSession(technologys, session)
            approvedSessions = session.Approved
        Next

        If approvedSessions = False Then Throw New NoSessionsApprovedException("No sessions approved.")

        RegistrationFee = GetValueRegistrationFee(Exp)

        Return repository.SaveSpeaker(Me)

    End Function


    Public Function IsValidToStandards(employers As List(Of String)) As Boolean

        Const MinimumExperence As Integer = 10
        Const MinimumCertifications As Integer = 3
        Return ((Exp > MinimumExperence OrElse HasBlog OrElse Certifications.Count() > MinimumCertifications OrElse employers.Contains(Employer)))

    End Function


    Public Function IsValidDomain(domains As List(Of String)) As Boolean

        Const MaximumVersionInternetExplorer As Integer = 9
        Dim emailDomain As String = Email.Split("@").Last()
        If Not domains.Contains(emailDomain) AndAlso (Not (Browser.Name = WebBrowser.BrowserName.InternetExplorer AndAlso Browser.MajorVersion < MaximumVersionInternetExplorer)) Then
            Return True
        End If
        Return False

    End Function


    Public Function IsDesapprovedSession(technologys As List(Of String), session As Session) As Boolean
        Return technologys.Where(Function(tech) session.Title.Contains(tech) Or session.Description.Contains(tech)).Count > 0
    End Function


    Public Function GetValueRegistrationFee(Exp As Integer) As Integer

        Dim ValueRegistrationFee As Integer
        If Exp <= 1 Then
            ValueRegistrationFee = 500
        ElseIf Exp >= 2 AndAlso Exp <= 3 Then
            ValueRegistrationFee = 250
        ElseIf Exp >= 4 AndAlso Exp <= 5 Then
            ValueRegistrationFee = 100
        ElseIf Exp >= 6 AndAlso Exp <= 9 Then
            ValueRegistrationFee = 50
        Else
            ValueRegistrationFee = 0
        End If
        Return ValueRegistrationFee

    End Function


#Region "Custom Exceptions"

    Public Class SpeakerDoesntMeetRequirementsException
        Inherits Exception
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(format As String, ParamArray args As Object())
            MyBase.New(String.Format(format, args))
        End Sub
    End Class

    Public Class NoSessionsApprovedException
        Inherits Exception
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class

#End Region

End Class
