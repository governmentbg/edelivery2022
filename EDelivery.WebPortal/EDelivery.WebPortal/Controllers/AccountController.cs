using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

using ED.DomainServices.Profiles;

using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Extensions;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Utils;
using EDelivery.WebPortal.Utils.Exceptions;

using EDeliveryResources;

using JWT;
using JWT.Serializers;

using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace EDelivery.WebPortal.Controllers
{
    public class AccountController : BaseController
    {
        private const int SystemLoginId = 1;
        private const string KepIdentifier = "KepIdentifier";

        private readonly Lazy<Profile.ProfileClient> profileClient;

        private EDeliverySignInManager signInManager;
        private EDeliveryUserManager userManager;

        public AccountController()
        {
            this.profileClient = new Lazy<Profile.ProfileClient>(
                () => Grpc.GrpcClientFactory.CreateProfileClient(), isThreadSafe: false);
        }

        public EDeliverySignInManager SignInManager
        {
            get
            {
                return signInManager
                    ?? HttpContext.GetOwinContext().Get<EDeliverySignInManager>();
            }
            private set
            {
                signInManager = value;
            }
        }

        public EDeliveryUserManager UserManager
        {
            get
            {
                return userManager
                    ?? HttpContext.GetOwinContext().GetUserManager<EDeliveryUserManager>();
            }
            private set
            {
                userManager = value;
            }
        }

        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult CertificateAuthV2(
            bool login = true,
            string returnUrl = null)
        {
            CertAuthViewModel model = new CertAuthViewModel();

            X509Certificate2 cert = SAML2.Config.Saml2Section
                .GetConfig()
                .ServiceProvider
                .SigningCertificate
                .GetCertificate();

            System.Xml.XmlDocument authnRequest =
                SamlHelper.GenerateKEPAuthnRequest(model);

            string signedXml = SamlHelper.SignXmlDocument(authnRequest, cert);

            string encodedStr = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(signedXml));

            model.EncodedRequest = encodedStr;

            string relayState = login
                ? CertAuthViewModel.RelayStateLogin
                : string.Empty;

            if (!string.IsNullOrEmpty(returnUrl))
            {
                relayState += ";" + returnUrl;
            }

            model.EncodedRelayState = !string.IsNullOrEmpty(relayState)
                ? Convert.ToBase64String(Encoding.UTF8.GetBytes(relayState))
                : string.Empty;

            return View("CertificateAuth", model);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult AuthenticateCertificate()
        {
            StringViewModel vm =
                new StringViewModel(ErrorMessages.ErrorSystemGeneral);

            if (TempData["ErrorMessage"] is string error)
            {
                vm = new StringViewModel(error);
            }

            return View(vm);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> AuthenticateCertificate(
            string SamlResponse,
            string RelayState)
        {
            if (string.IsNullOrEmpty(SamlResponse))
            {
                TempData["ErrorMessage"] = ErrorMessages.ErrorLoginKEP;

                return RedirectToAction(
                    nameof(AccountController.AuthenticateCertificate));
            }

            string decodedStr = Encoding.UTF8.GetString(
                Convert.FromBase64String(SamlResponse));

            CertificateAuthResponse response = new CertificateAuthResponse();

            switch (SamlHelper.SamlConfiguration.SamlVersion)
            {
                case 1:
                    response = SamlHelper.ParseSaml2CertificateResult(decodedStr);
                    break;
                case 2:
                    response = SamlHelper.ParseSaml2CertificateResultV2(decodedStr);
                    break;
            }

            string relayState = string.Empty;
            string returnUrl = string.Empty;
            if (!string.IsNullOrEmpty(RelayState))
            {
                relayState = Encoding.UTF8.GetString(
                    Convert.FromBase64String(RelayState));

                string[] relayStateParts = relayState
                    .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (relayStateParts.Length > 1)
                {
                    returnUrl = relayStateParts[1];
                }
            }

            switch (response.ResponseStatus)
            {
                case eCertResponseStatus.Success:
                    GetRegisteredIndividualResponse individual;

                    try
                    {
                        individual =
                            await this.profileClient.Value.GetRegisteredIndividualAsync(
                                new GetRegisteredIndividualRequest
                                {
                                    Identifier = response.EGN
                                },
                                cancellationToken: Response.ClientDisconnectedToken);
                    }
                    catch (Exception ex)
                    {
                        ElmahLogger.Instance.Error(
                            ex,
                            $"AuthenticateCertificate Can not get person by provided EGN {response.EGN}");

                        TempData["ErrorMessage"] = ErrorMessages.ErrorSystemGeneral;

                        return RedirectToAction(
                            nameof(AccountController.AuthenticateCertificate));
                    }

                    if (Guid.TryParse(individual.Profile?.Guid, out Guid subjectGuidObj))
                    {
                        return LoginInternalKEP(subjectGuidObj, returnUrl);
                    }
                    else
                    {
                        KEPRegistrationModel regModel =
                            new KEPRegistrationModel(response);

                        GetRegixPersonInfoResponse regixResp =
                            await this.profileClient.Value.GetRegixPersonInfoAsync(
                                new GetRegixPersonInfoRequest()
                                {
                                    Identifier = regModel.CertInfo.EGN
                                });

                        if (regixResp.Result != null && regixResp.Result.Success)
                        {
                            regModel.LockNames = true;
                            regModel.FirstName = regixResp.Result.FirstName;
                            regModel.MiddleName = regixResp.Result.SurName;
                            regModel.LastName = regixResp.Result.FamilyName;
                        }

                        this.Session[KepIdentifier] = regModel.CertInfo.EGN;
                        this.SetTempModel(regModel, false);

                        return RedirectToAction(
                            nameof(AccountController.RegisterPersonWithKEP));
                    }
                case eCertResponseStatus.InvalidSignature:
                case eCertResponseStatus.InvalidResponseXML:
                case eCertResponseStatus.MissingEGN:
                    ElmahLogger.Instance.Error(
                        $"AuthenticateCertificate responseStatus is: {response.ResponseStatus}, message {response.ResponseStatusMessage}");

                    TempData["ErrorMessage"] = string.Format(
                        ErrorMessages.ErrorKepFailedInvalidResponse,
                        response.ResponseStatusMessage);

                    return RedirectToAction(
                        nameof(AccountController.AuthenticateCertificate));
                case eCertResponseStatus.CanceledByUser:
                    return RedirectToAction("Index", "Home");
                case eCertResponseStatus.AuthenticationFailed:
                default:
                    ElmahLogger.Instance.Error(
                        $"AuthenticateCertificate responseStatus is: {response.ResponseStatus}, message {response.ResponseStatusMessage}");

                    TempData["ErrorMessage"] = string.Format(
                        ErrorMessages.ErrorKepAuthenticationFailed,
                        response.ResponseStatusMessage);

                    return RedirectToAction(
                        nameof(AccountController.AuthenticateCertificate));
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> AuthenticateNoiPikRegisterLegal(
            string jwt)
        {
            return await AuthenticateNOIPIK(
                jwt,
                Url.Action(
                    nameof(AccountController.RegisterLegalEntity1),
                    "Account"));
        }

        [AllowAnonymous]
        public async Task<ActionResult> AuthenticateNOIPIK(
            string jwt,
            string returnUrl = null)
        {
            NoiUserDetails tokenObject = null;
            if (!string.IsNullOrWhiteSpace(jwt))
            {
                tokenObject =
                    GetJWTDetails<NoiUserDetails>(
                        jwt,
                        true,
                        WebConfigurationManager.AppSettings["NOIAuthSharedSecret"]);
            }

            if (tokenObject == null)
            {
                TempData["ErrorMessage"] = ErrorMessages.NotAuthenticatedByThirdParty;

                return RedirectToAction(
                    nameof(AccountController.AuthenticateCertificate));
            }

            GetRegisteredIndividualResponse individual =
                await this.profileClient.Value.GetRegisteredIndividualAsync(
                    new GetRegisteredIndividualRequest
                    {
                        Identifier = tokenObject.EGN
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (Guid.TryParse(individual.Profile?.Guid, out Guid subjectId))
            {
                UserStore.Login user = await UserManager.FindByESubjectIdAsync(subjectId);

                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        TempData["ErrorMessage"] = ErrorMessages.ErrorLoginIsDeactivated;

                        return RedirectToAction(
                            nameof(AccountController.AuthenticateCertificate));
                    }

                    await SignInManager.SignInAsync(user, false, false);

                    return string.IsNullOrEmpty(returnUrl)
                        ? RedirectToLocal("Profile/Index")
                        : Redirect(returnUrl);
                }
            }

            //the user does not exists -> register it
            //return a form for filling additional fields
            PIKRegistrationModel regModel =
                new PIKRegistrationModel(jwt, tokenObject);

            //try get person names from regixt
            GetRegixPersonInfoResponse regixResp =
                await this.profileClient.Value.GetRegixPersonInfoAsync(
                    new GetRegixPersonInfoRequest()
                    {
                        Identifier = tokenObject.EGN
                    });

            if (regixResp.Result != null && regixResp.Result.Success)
            {
                regModel.LockNames = true;
                regModel.FirstName = regixResp.Result.FirstName;
                regModel.MiddleName = regixResp.Result.SurName;
                regModel.LastName = regixResp.Result.FamilyName;
            }

            this.SetTempModel(regModel, false);

            return RedirectToAction(
                nameof(AccountController.RegisterPersonWithPIK));
        }

        // keep it as there are 3rd party systems using this url
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Profile");
            }

            return Redirect("/");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            this.HttpContext.ClearCachedUserData();

            AuthenticationManager.SignOut();

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ChooseRegisterType(ChooseRegistrationModel model)
        {
            if (model.TargetGroupId == TargetGroupId.Individual)
            {
                switch (model.RegistrationType)
                {
                    case eRegistrationType.PIK:
                        string noiPikAuthUrl = string.Format(
                            "{0}{1}",
                            ConfigurationManager.AppSettings["NOIAuthUrl"],
                            HttpUtility.UrlEncode(
                                Url.Action(
                                    "AuthenticateNoiPik",
                                    "Account",
                                    null,
                                    this.Request.Url.Scheme)));

                        return Redirect(noiPikAuthUrl);
                    case eRegistrationType.Certificate:
                        return RedirectToAction(
                            SamlHelper.SamlConfiguration.LoginUrl,
                            new { login = false });
                }
            }

            throw new ArgumentException(
                $"{nameof(model.TargetGroupId)} has invalid value.");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult RegisterPersonWithKEP()
        {
            KEPRegistrationModel vm = this.GetTempModel<KEPRegistrationModel>(true);
            if (vm != null)
            {
                return View(vm);
            }

            string redirectToEAuth = Url.Action(
                SamlHelper.SamlConfiguration.LoginUrl,
                "Account",
                new { login = true });

            return Redirect(redirectToEAuth);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterPersonWithKEP(
            KEPRegistrationModel model)
        {
            try
            {
                if (this.Session[KepIdentifier] is string kepIdentifier)
                {
                    if (kepIdentifier != model.CertInfo.EGN)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            ErrorMessages.ErrorInvalidEGNParam);
                    }
                }

                if (!ModelState.IsValid)
                {
                    this.SetTempModel(model, true);

                    return RedirectToAction(
                        nameof(AccountController.RegisterPersonWithKEP));
                }

                CheckIndividualUniquenessResponse resp =
                    await profileClient.Value.CheckIndividualUniquenessAsync(
                        new CheckIndividualUniquenessRequest
                        {
                            Identifier = model.CertInfo.EGN,
                            Email = model.EmailAddress,
                        });

                if (!resp.IsUniqueEmail || !resp.IsUniqueIdentifier)
                {
                    if (!resp.IsUniqueIdentifier)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            ErrorMessages.UsernameAlredyRegistered);
                    }

                    if (!resp.IsUniqueEmail)
                    {
                        ModelState.AddModelError(
                            nameof(KEPRegistrationModel.EmailAddress),
                            ErrorMessages.EmailAlredyRegistered);
                    }

                    this.SetTempModel(model, true);

                    return RedirectToAction(
                        nameof(AccountController.RegisterPersonWithKEP));
                }

                if (model.PhoneNotifications && model.ViberNotifications)
                {
                    model.PhoneNotifications = false;
                }

                return await RegisterPersonInternal(
                    model.FirstName,
                    model.MiddleName,
                    model.LastName,
                    model.CertInfo.EGN,
                    model.EmailAddress,
                    model.PhoneNumber,
                    model.Address,
                    model.CreateProfile,
                    model.EmailNotifications,
                    model.PhoneNotifications,
                    model.ViberNotifications);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Unsuccessful registration with KEP");

                ModelState.AddModelError(
                    string.Empty,
                    ErrorMessages.ErrorSystemGeneral);

                this.SetTempModel(model, true);

                return RedirectToAction(
                    nameof(AccountController.RegisterPersonWithKEP));
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult RegisterPersonWithPIK()
        {
            PIKRegistrationModel vm = this.GetTempModel<PIKRegistrationModel>(true);
            if (vm != null)
            {
                return View(vm);
            }

            string NOIAuthUrl = ConfigurationManager.AppSettings["NOIAuthUrl"];

            string encodedUrl = HttpUtility.UrlEncode(
                Url.Action(
                    "AuthenticateNoiPik",
                    "Account",
                    null,
                    this.Request.Url.Scheme));

            string redirectToNOI = $"{NOIAuthUrl}{encodedUrl}";

            return Redirect(redirectToNOI);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterPersonWithPIK(
            PIKRegistrationModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    this.SetTempModel(model, true);

                    return RedirectToAction(
                        nameof(AccountController.RegisterPersonWithPIK));
                }

                if (string.IsNullOrEmpty(model.Token))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        ErrorMessages.NotAuthenticatedByThirdParty);

                    this.SetTempModel(model, true);

                    return RedirectToAction(
                        nameof(AccountController.RegisterPersonWithPIK));
                }

                NoiUserDetails userDetails =
                    GetJWTDetails<NoiUserDetails>(
                        model.Token,
                        false,
                        WebConfigurationManager.AppSettings["NOIAuthSharedSecret"]);
                if (userDetails == null || model.EGN != userDetails.EGN)
                {
                    ElmahLogger.Instance.Error(
                        $"RegisterPersonWithPIK called without invalid token {model.Token}");

                    ModelState.AddModelError(
                        string.Empty,
                        ErrorMessages.NotAuthenticatedByThirdParty);

                    this.SetTempModel(model, true);

                    return RedirectToAction(
                        nameof(AccountController.RegisterPersonWithPIK));
                }

                CheckIndividualUniquenessResponse resp =
                    await profileClient.Value.CheckIndividualUniquenessAsync(
                        new CheckIndividualUniquenessRequest
                        {
                            Identifier = userDetails.EGN,
                            Email = model.EmailAddress,
                        });

                if (!resp.IsUniqueEmail || !resp.IsUniqueIdentifier)
                {
                    if (!resp.IsUniqueIdentifier)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            ErrorMessages.UsernameAlredyRegistered);
                    }

                    if (!resp.IsUniqueEmail)
                    {
                        ModelState.AddModelError(
                            nameof(PIKRegistrationModel.EmailAddress),
                            ErrorMessages.EmailAlredyRegistered);
                    }

                    this.SetTempModel(model, true);

                    return RedirectToAction(
                        nameof(AccountController.RegisterPersonWithPIK));
                }

                if (model.PhoneNotifications && model.ViberNotifications)
                {
                    model.PhoneNotifications = false;
                }

                return await RegisterPersonInternal(
                    model.FirstName,
                    model.MiddleName,
                    model.LastName,
                    model.EGN,
                    model.EmailAddress,
                    model.PhoneNumber,
                    model.Address,
                    model.CreateProfile,
                    model.EmailNotifications,
                    model.PhoneNotifications,
                    model.ViberNotifications);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Unsuccessful registration with PIK");

                ModelState.AddModelError(
                    string.Empty,
                    ErrorMessages.ErrorSystemGeneral);

                this.SetTempModel(model, true);

                return RedirectToAction(
                    nameof(AccountController.RegisterPersonWithPIK));
            }
        }

        [AllowAnonymous]
        [Route("Register/LegalEntity")]
        [HttpGet]
        public ActionResult RegisterLegalEntity1()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            RegisterLegalEntityViewModel vm =
                this.GetTempModel<RegisterLegalEntityViewModel>(true)
                    ?? new RegisterLegalEntityViewModel();

            return View("RegisterLegalEntity", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterLegalEntity(
            RegisterLegalEntityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.SetTempModel(model, true);

                return RedirectToAction(
                    nameof(AccountController.RegisterLegalEntity1));
            }

            string identifier = string.Empty;

            try
            {
                ParseRegistrationDocumentResponse parsedReponse =
                    await this.profileClient.Value.ParseRegistrationDocumentAsync(
                        new ParseRegistrationDocumentRequest
                        {
                            BlobId = model.FileId.Value
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                if (!parsedReponse.IsSuccessful)
                {
                    throw new ModelStateException(
                        nameof(RegisterLegalEntityViewModel.FileId),
                        ErrorMessages.ErrorInvalidPdfFormData);
                }

                identifier = parsedReponse.Result.Identifier;

                if (model.PhoneNotifications && model.ViberNotifications)
                {
                    model.PhoneNotifications = false;
                }

                CreateRegisterRequestResponse response =
                    await this.profileClient.Value.CreateRegisterRequestAsync(
                        new CreateRegisterRequestRequest
                        {
                            RegistrationEmail = model.EmailAddress,
                            RegistrationPhone = model.PhoneNumber,
                            RegistrationIsEmailNotificationEnabled = model.EmailNotifications,
                            RegistrationIsSmsNotificationEnabled = model.PhoneNotifications,
                            RegistrationIsViberNotificationEnabled = model.ViberNotifications,
                            Name = parsedReponse.Result.Name,
                            Identifier = parsedReponse.Result.Identifier,
                            Phone = parsedReponse.Result.Phone,
                            Email = parsedReponse.Result.Email,
                            Residence = parsedReponse.Result.Residence,
                            City = parsedReponse.Result.City,
                            State = parsedReponse.Result.State,
                            Country = parsedReponse.Result.Country,
                            TargetGroupId = (int)TargetGroupId.LegalEntity,
                            BlobId = model.FileId.Value,
                            LoginId = this.UserData.LoginId,
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                if (response.IsSuccessful)
                {
                    return RedirectToAction(nameof(AccountController.RegistrationSuccess));
                }
                else
                {
                    throw new ModelStateException(
                        nameof(RegisterLegalEntityViewModel.FileId),
                        string.Format(
                            ErrorMessages.ErrorEIKAlreadyRegistered,
                            identifier));
                }
            }
            catch (ModelStateException re)
            {
                ModelState.AddModelError(re.Key, re.Message);

                ElmahLogger.Instance.Error(
                    re,
                    $"Error registering legal with identifier [{identifier}] with data from pdf");

                this.SetTempModel(model, true);

                return RedirectToAction(
                    nameof(AccountController.RegisterLegalEntity1));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ErrorMessages.ErrorCanNotCreateUser);

                ElmahLogger.Instance.Error(
                    ex,
                    $"Error registering legal with certificate! EIK: {identifier}");

                this.SetTempModel(model, true);

                return RedirectToAction(
                    nameof(AccountController.RegisterLegalEntity1));
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult RegistrationSuccess()
        {
            return View();
        }

        #region Redirects

        [HttpGet]
        public ActionResult Register()
        {
            return Redirect("/");
        }

        #endregion

        private async Task<ActionResult> RegisterPersonInternal(
            string firstName,
            string middleName,
            string lastName,
            string identifier,
            string email,
            string phone,
            string residence,
            bool createProfile,
            bool emailNotifications,
            bool phoneNotifications,
            bool viberNotifications)
        {
            try
            {
                CreateOrUpdateIndividualResponse createdProfile =
                    await profileClient.Value.CreateOrUpdateIndividualAsync(
                        new CreateOrUpdateIndividualRequest
                        {
                            FistName = firstName,
                            MiddleName = middleName,
                            LastName = lastName,
                            Identifier = identifier,
                            Email = email,
                            Phone = phone,
                            Residence = residence,
                            IsPassive = !createProfile,
                            IsEmailNotificationEnabled = emailNotifications,
                            IsSmsNotificationEnabled = phoneNotifications,
                            IsViberNotificationEnabled = viberNotifications,
                            ActionLoginId = SystemLoginId,
                            Ip = this.Request.UserHostAddress,
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                // TODO: move in form with security
                this.Session.Remove(KepIdentifier);

                string error = LoginInternal(
                    Guid.Parse(createdProfile.ProfileGuid));

                if (!string.IsNullOrEmpty(error))
                {
                    TempData["ErrorMessage"] = error;

                    return RedirectToAction(
                        nameof(AccountController.AuthenticateCertificate));
                }

                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(ex, "Error registering person!");

                throw;
            }
        }

        private ActionResult LoginInternalKEP(
            Guid electronicSubjectId,
            string returnUrl = null)
        {
            try
            {
                string error = LoginInternal(electronicSubjectId);

                if (!string.IsNullOrEmpty(error))
                {
                    TempData["ErrorMessage"] = error;

                    return RedirectToAction(
                        nameof(AccountController.AuthenticateCertificate));
                }

                return string.IsNullOrEmpty(returnUrl)
                    ? RedirectToLocal("Profile/Index")
                    : Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Error logging existing user with KEP");

                TempData["ErrorMessage"] = ErrorMessages.ErrorLoginKEP;

                return RedirectToAction(
                    nameof(AccountController.AuthenticateCertificate));
            }
        }

        private string LoginInternal(Guid subjectId)
        {
            if (subjectId == Guid.Empty)
            {
                return ErrorMessages.ErrorSystemGeneral;
            }

            UserStore.Login user =
                this.UserManager.FindByESubjectId(subjectId);
            if (!user.IsActive)
            {
                return ErrorMessages.ErrorLoginIsDeactivated;
            }

            SignInManager.SignIn(user, false, false);

            return string.Empty;
        }

        private T GetJWTDetails<T>(
            string jwt,
            bool validate,
            string sharedSecret)
        {
            try
            {
                JsonNetSerializer serializer = new JsonNetSerializer();
                UtcDateTimeProvider dateTimeProvider = new UtcDateTimeProvider();
                JwtBase64UrlEncoder encoder = new JwtBase64UrlEncoder();
                JwtValidator validator =
                    new JwtValidator(serializer, dateTimeProvider);
                JwtDecoder decoder =
                    new JwtDecoder(serializer, validator, encoder);

                T tokenObject = decoder.DecodeToObject<T>(
                    jwt,
                    sharedSecret,
                    validate);

                return tokenObject;
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    $"Get Json Token failed for token {jwt}");

                return default;
            }
        }

        // TODO: remove, useless and confusing
        protected ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Profile");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (userManager != null)
                {
                    userManager.Dispose();
                    userManager = null;
                }

                if (signInManager != null)
                {
                    signInManager.Dispose();
                    signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
