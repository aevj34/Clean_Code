using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    /// <summary>
    /// Represents a single speaker
    /// </summary>
    public class Speaker
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? Exp { get; set; }
        public bool HasBlog { get; set; }
        public string BlogURL { get; set; }
        public WebBrowser Browser { get; set; }
        public List<string> Certifications { get; set; }
        public string Employer { get; set; }
        public int RegistrationFee { get; set; }
        public List<BusinessLayer.Session> Sessions { get; set; }

        /// <summary>
        /// Register a speaker
        /// </summary>
        /// <returns>speakerID</returns>
        public int? Register(IRepository repository)
        {
            //lets init some vars
            int? speakerId = null;
            bool good = false;
            bool appr = false;
            //var nt = new List<string> {"MVC4", "Node.js", "CouchDB", "KendoUI", "Dapper", "Angular"};
            var ot = new List<string>() { "Cobol", "Punch Cards", "Commodore", "VBScript" };

            //DEFECT #5274 DA 12/10/2012
            //We weren't filtering out the prodigy domain so I added it.
            var domains = new List<string>() { "aol.com", "hotmail.com", "prodigy.com", "CompuServe.com" };

            if (!string.IsNullOrWhiteSpace(FirstName))
            {
                if (!string.IsNullOrWhiteSpace(LastName))
                {
                    if (!string.IsNullOrWhiteSpace(Email))
                    {
                        //put list of employers in array
                        var emps = new List<string>() { "Microsoft", "Google", "Fog Creek Software", "37Signals" };

                        //DFCT #838 Jimmy 
                        //We're now requiring 3 certifications so I changed the hard coded number. Boy, programming is hard.
                        good = ((Exp > 10 || HasBlog || Certifications.Count() > 3 || emps.Contains(Employer)));

                        if (!good)
                        {
                            //need to get just the domain from the email
                            string emailDomain = Email.Split('@').Last();

                            if (!domains.Contains(emailDomain) && (!(Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < 9)))
                            {
                                good = true;
                            }
                        }

                        if (good)
                        {
                            //DEFECT #5013 CO 1/12/2012
                            //We weren't requiring at least one session
                            if (Sessions.Count() != 0)
                            {
                                foreach (var session in Sessions)
                                {
                                    //foreach (var tech in nt)
                                    //{
                                    //    if (session.Title.Contains(tech))
                                    //    {
                                    //        session.Approved = true;
                                    //        break;
                                    //    }
                                    //}

                                    foreach (var tech in ot)
                                    {
                                        if (session.Title.Contains(tech) || session.Description.Contains(tech))
                                        {
                                            session.Approved = false;
                                            break;
                                        }
                                        else
                                        {
                                            session.Approved = true;
                                            appr = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new ArgumentException("Can't register speaker with no sessions to present.");
                            }

                            if (appr)
                            {
                                //if we got this far, the speaker is approved
                                //let's go ahead and register him/her now.
                                //First, let's calculate the registration fee. 
                                //More experienced speakers pay a lower fee.
                                if (Exp <= 1)
                                {
                                    RegistrationFee = 500;
                                }
                                else if (Exp >= 2 && Exp <= 3)
                                {
                                    RegistrationFee = 250;
                                }
                                else if (Exp >= 4 && Exp <= 5)
                                {
                                    RegistrationFee = 100;
                                }
                                else if (Exp >= 6 && Exp <= 9)
                                {
                                    RegistrationFee = 50;
                                }
                                else
                                {
                                    RegistrationFee = 0;
                                }



                                //Now, save the speaker and sessions to the db.
                                try
                                {
                                    speakerId = repository.SaveSpeaker(this);
                                }
                                catch (Exception e)
                                {
                                    //in case the db call fails 
                                }
                            }
                            else
                            {
                                throw new NoSessionsApprovedException("No sessions approved.");
                            }
                        }
                        else
                        {
                            throw new SpeakerDoesntMeetRequirementsException("Speaker doesn't meet our abitrary and capricious standards.");
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException("Email is required.");
                    }
                }
                else
                {
                    throw new ArgumentNullException("Last name is required.");
                }
            }
            else
            {
                throw new ArgumentNullException("First Name is required");
            }

            //if we got this far, the speaker is registered.
            return speakerId;
        }


        public int? Register_Clean(IRepository repository)
        {
            if (string.IsNullOrWhiteSpace(FirstName)) throw new ArgumentNullException("First Name is required");
            if (string.IsNullOrWhiteSpace(LastName)) throw new ArgumentNullException("Last name is required.");
            if (string.IsNullOrWhiteSpace(Email)) throw new ArgumentNullException("Email is required.");

            bool isAptToStandards = false;
            bool approvedSessions = false;
            var technologys = new List<string>() { "Cobol", "Punch Cards", "Commodore", "VBScript" };
            var domains = new List<string>() { "aol.com", "hotmail.com", "prodigy.com", "CompuServe.com" };
            var employers = new List<string>() { "Microsoft", "Google", "Fog Creek Software", "37Signals" };

            isAptToStandards = IsValidToStandards(employers);

            if (!isAptToStandards)
            {
                isAptToStandards = IsValidDomain(domains);
                throw new SpeakerDoesntMeetRequirementsException("Speaker doesn't meet our abitrary and capricious standards.");
            }

            if (Sessions.Count == 0) throw new ArgumentException("Can't register speaker with no sessions to present.");

            foreach (var session in Sessions)
            {
                session.Approved = !(IsDesapprovedSession(technologys,session));
                approvedSessions = session.Approved;
            }

            if (!approvedSessions) throw new NoSessionsApprovedException("No sessions approved.");

            RegistrationFee = GetValueRegistrationFee(Exp);

            return repository.SaveSpeaker(this);
        }


        public bool IsDesapprovedSession(List<string> technologys, Session session)
        {
            return technologys.Where(tech => session.Title.Contains(tech) || session.Description.Contains(tech)).Count() > 0;
        }


        public bool IsValidDomain(List<string> domains)
        {
            const int MaxVersionInternetExplorer = 9;
            string emailDomain = Email.Split('@').Last();

            if (!domains.Contains(emailDomain) && (!(Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < MaxVersionInternetExplorer)))
            {
                return true;
            }
            return false;
        }

        public bool IsValidToStandards(List<string> employers)
        {
            const int MinExperence = 10;
            const int MinCertifications = 3;

            return (Exp > MinExperence || HasBlog || Certifications.Count() > MinCertifications || employers.Contains(Employer));
        }

        public int GetValueRegistrationFee(int? Exp)
        {
            int ValueRegistrationFee;

            if (Exp <= 1)
            {
                ValueRegistrationFee = 500;
            }
            else if (Exp >= 2 && Exp <= 3)
            {
                ValueRegistrationFee = 250;
            }
            else if (Exp >= 4 && Exp <= 5)
            {
                ValueRegistrationFee = 100;
            }
            else if (Exp >= 6 && Exp <= 9)
            {
                ValueRegistrationFee = 50;
            }
            else
            {
                ValueRegistrationFee = 0;
            }

            return ValueRegistrationFee;
        }



        #region Custom Exceptions
        public class SpeakerDoesntMeetRequirementsException : Exception
        {
            public SpeakerDoesntMeetRequirementsException(string message)
                : base(message)
            {
            }

            public SpeakerDoesntMeetRequirementsException(string format, params object[] args)
                : base(string.Format(format, args)) { }
        }

        public class NoSessionsApprovedException : Exception
        {
            public NoSessionsApprovedException(string message)
                : base(message)
            {
            }
        }
        #endregion
    }
}
