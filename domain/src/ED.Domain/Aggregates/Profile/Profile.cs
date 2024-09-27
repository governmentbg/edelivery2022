using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class Profile
    {
        public const int SystemProfileId = 1;

        // EF constructor
        private Profile()
        {
            this.ElectronicSubjectName = null!;
            this.EmailAddress = null!;
            this.Phone = null!;
            this.Identifier = null!;
        }

        // todo: maybe all constructors should be replaced with static methods with description
        public Profile(
            string firstName,
            string middleName,
            string lastName,
            string identifier,
            string phone,
            string email,
            string residence,
            bool isPassive,
            bool licenceAgreed,
            bool gdprAgreed,
            int loginId,
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt)
        {
            DateTime now = DateTime.Now;

            this.IsActivated = true;
            this.ActivatedDate = now;
            this.ProfileType = ProfileType.Individual;
            this.ElectronicSubjectName = $"{firstName} {middleName} {lastName}";
            this.EmailAddress = email;
            this.Phone = phone;
            this.Identifier = identifier;
            this.DateCreated = now;
            this.CreatedBy = loginId;
            this.ModifyDate = now;
            this.ModifiedBy = loginId;
            this.IsReadOnly = false;
            this.IsPassive = isPassive;
            this.HideAsRecipient = false;
            this.LicenceAgreed = licenceAgreed;
            this.GDPRAgreed = gdprAgreed;

            this.Individual = new(firstName, middleName, lastName);

            this.Address = new(residence);

            this.keys.Add(
                new ProfileKey(
                    provider,
                    keyName,
                    oaepPadding,
                    issuedAt,
                    expiresAt));
        }

        public Profile(
            string firstName,
            string middleName,
            string lastName,
            string identifier,
            string phone,
            string email,
            string residence,
            int loginId,
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt)
        {
            DateTime now = DateTime.Now;

            this.IsActivated = false;
            this.ProfileType = ProfileType.Individual;
            this.ElectronicSubjectName = $"{firstName} {middleName} {lastName}";
            this.EmailAddress = email;
            this.Phone = phone;
            this.Identifier = identifier;
            this.DateCreated = now;
            this.CreatedBy = loginId;
            this.ModifyDate = now;
            this.ModifiedBy = loginId;
            this.IsReadOnly = false;
            this.IsPassive = true;
            this.HideAsRecipient = false;
            this.LicenceAgreed = false;
            this.GDPRAgreed = false;

            this.Individual = new(firstName, middleName, lastName);

            this.Address = new(residence);

            this.keys.Add(
                new ProfileKey(
                    provider,
                    keyName,
                    oaepPadding,
                    issuedAt,
                    expiresAt));
        }

        public Profile(
            string name,
            string identifier,
            string phone,
            string email,
            string residence,
            string? city,
            string? state,
            string? country,
            int loginId,
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt,
            bool isEmailNotificationEnabled,
            bool isEmailNotificationOnDeliveryEnabled,
            bool isPhoneNotificationEnabled,
            bool isPhoneNotificationOnDeliveryEnabled,
            string registrationEmail,
            string registrationPhone,
            (LoginProfilePermissionType permission, int? templateId)[] permissions)
        {
            DateTime now = DateTime.Now;

            this.IsActivated = false;
            this.ActivatedDate = now;
            this.ProfileType = ProfileType.LegalEntity;
            this.ElectronicSubjectName = name;
            this.EmailAddress = email;
            this.Phone = phone;
            this.Identifier = identifier;
            this.DateCreated = now;
            this.CreatedBy = loginId;
            this.ModifyDate = now;
            this.ModifiedBy = loginId;
            this.IsReadOnly = false;
            this.IsPassive = false;
            this.HideAsRecipient = false;
            this.LicenceAgreed = false;
            this.GDPRAgreed = false;

            this.LegalEntity = new(name);

            this.Address = new(
                residence,
                city,
                state,
                country);

            this.keys.Add(
                new ProfileKey(
                    provider,
                    keyName,
                    oaepPadding,
                    issuedAt,
                    expiresAt));

            this.logins.Add(
                new LoginProfile(
                    loginId,
                    false,
                    isEmailNotificationEnabled,
                    isEmailNotificationOnDeliveryEnabled,
                    isPhoneNotificationEnabled,
                    isPhoneNotificationOnDeliveryEnabled,
                    registrationEmail,
                    registrationPhone,
                    loginId));

            this.loginPermissions.AddRange(
                permissions
                    .Select(e => new LoginProfilePermission(
                        loginId,
                        e.permission,
                        e.templateId)));
        }

        public Profile(
            string name,
            string identifier,
            string phone,
            string email,
            string residence,
            int adminUserId,
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt)
        {
            DateTime now = DateTime.Now;

            this.IsActivated = true;
            this.ActivatedDate = now;
            this.ProfileType = ProfileType.LegalEntity;
            this.ElectronicSubjectName = name;
            this.EmailAddress = email;
            this.Phone = phone;
            this.Identifier = identifier;
            this.DateCreated = now;
            this.CreatedBy = Login.SystemLoginId;
            this.ModifyDate = now;
            this.ModifiedBy = Login.SystemLoginId;
            this.IsReadOnly = false;
            this.IsPassive = false;
            this.HideAsRecipient = false;
            this.LicenceAgreed = false;
            this.GDPRAgreed = false;

            this.CreatedByAdminUserId = adminUserId;
            this.ActivatedByAdminUserId = adminUserId;

            this.LegalEntity = new(name);
            this.keys.Add(
                new ProfileKey(
                    provider,
                    keyName,
                    oaepPadding,
                    issuedAt,
                    expiresAt));

            this.Address = new(residence);
        }

        public static Profile CreateInstanceEsbPassiveIndividualRegistration(
            string identifier,
            string firstName,
            string middleName,
            string lastName,
            string phone,
            string email,
            string residence,
            string? city,
            string? state,
            string? countryIso,
            int loginId,
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt)
        {
            DateTime now = DateTime.Now;

            Profile profile = new()
            {
                IsActivated = false,
                ActivatedDate = null,
                ProfileType = ProfileType.Individual,
                ElectronicSubjectName = $"{firstName} {middleName} {lastName}",
                EmailAddress = email,
                Phone = phone,
                Identifier = identifier,
                DateCreated = now,
                CreatedBy = loginId,
                ModifyDate = now,
                ModifiedBy = loginId,
                IsReadOnly = false,
                IsPassive = true,
                HideAsRecipient = false,
                Individual = new(firstName, middleName, lastName),
                Address = new(residence, city, state, countryIso),
            };

            profile.keys.Add(
                new ProfileKey(
                    provider,
                    keyName,
                    oaepPadding,
                    issuedAt,
                    expiresAt));

            return profile;
        }

        public static Profile CreateInstanceEsbTicketPassiveIndividualRegistration(
            string identifier,
            string firstName,
            string middleName,
            string lastName,
            string? phone,
            string? email,
            int loginId,
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt)
        {
            DateTime now = DateTime.Now;

            Profile profile = new()
            {
                IsActivated = false,
                ActivatedDate = null,
                ProfileType = ProfileType.Individual,
                ElectronicSubjectName = $"{firstName} {middleName} {lastName}",
                EmailAddress = email ?? string.Empty,
                Phone = phone ?? string.Empty,
                Identifier = identifier,
                DateCreated = now,
                CreatedBy = loginId,
                ModifyDate = now,
                ModifiedBy = loginId,
                IsReadOnly = false,
                IsPassive = true,
                HideAsRecipient = false,
                Individual = new(firstName, middleName, lastName),
                Address = null,
            };

            profile.keys.Add(
                new ProfileKey(
                    provider,
                    keyName,
                    oaepPadding,
                    issuedAt,
                    expiresAt));

            return profile;
        }

        public static Profile CreateInstanceEsbIndividualRegistration(
            string identifier,
            string firstName,
            string middleName,
            string lastName,
            string phone,
            string email,
            string residence,
            string? city,
            string? state,
            string? countryIso,
            bool isFullFeatured,
            int loginId,
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt)
        {

            DateTime now = DateTime.Now;

            Profile profile = new()
            {
                IsActivated = true,
                ActivatedDate = now,
                ProfileType = ProfileType.Individual,
                ElectronicSubjectName = $"{firstName} {middleName} {lastName}",
                EmailAddress = email,
                Phone = phone,
                Identifier = identifier,
                DateCreated = now,
                CreatedBy = loginId,
                ModifyDate = now,
                ModifiedBy = loginId,
                IsReadOnly = false,
                IsPassive = !isFullFeatured,
                HideAsRecipient = false,
                Individual = new(firstName, middleName, lastName),
                Address = new(residence, city, state, countryIso),
            };

            profile.keys.Add(
                new ProfileKey(
                    provider,
                    keyName,
                    oaepPadding,
                    issuedAt,
                    expiresAt));

            return profile;
        }

        public static Profile CreateInstanceEsbLegalEntityRegistration(
            string identifier,
            string name,
            string phone,
            string email,
            string residence,
            string? city,
            string? state,
            string? countryIso,
            (int LoginId, string Email, string Phone)[] ownersData,
            int actionLoginId,
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt)
        {

            DateTime now = DateTime.Now;

            Profile profile = new()
            {
                IsActivated = true,
                ActivatedDate = now,
                ProfileType = ProfileType.LegalEntity,
                ElectronicSubjectName = name,
                EmailAddress = email,
                Phone = phone,
                Identifier = identifier,
                DateCreated = now,
                CreatedBy = actionLoginId,
                ModifyDate = now,
                ModifiedBy = actionLoginId,
                IsReadOnly = false,
                IsPassive = false,
                HideAsRecipient = false,
                LegalEntity = new(name),
                Address = new(residence, city, state, countryIso),
            };

            profile.keys.Add(
                new ProfileKey(
                    provider,
                    keyName,
                    oaepPadding,
                    issuedAt,
                    expiresAt));

            var permissions = new (LoginProfilePermissionType permission, int? templateId)[]
            {
                (LoginProfilePermissionType.FullMessageAccess, null),
                (LoginProfilePermissionType.AdministerProfileAccess, null),
            };

            foreach (var ownerData in ownersData)
            {
                profile.logins.Add(
                    new LoginProfile(
                        ownerData.LoginId,
                        false,
                        emailNotificationActive: false,
                        emailNotificationOnDeliveryActive: false,
                        phoneNotificationActive: false,
                        phoneNotificationOnDeliveryActive: false,
                        ownerData.Email,
                        ownerData.Phone,
                        actionLoginId));

                profile.loginPermissions.AddRange(
                    permissions
                        .Select(e => new LoginProfilePermission(
                            ownerData.LoginId,
                            e.permission,
                            e.templateId)));
            }

            return profile;
        }

        public int Id { get; set; }

        /// <summary>
        /// when false indicates that the profile cannot be logged into or be part of any new actions,
        /// can be changed from the admin panel
        /// </summary>
        public bool IsActivated { get; set; }

        public ProfileType ProfileType { get; set; }

        public Guid ElectronicSubjectId { get; set; }

        public string ElectronicSubjectName { get; set; }

        public string EmailAddress { get; set; }

        public string Phone { get; set; }

        public DateTime DateCreated { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? DateDeleted { get; set; }

        public int? DeletedByAdminUserId { get; set; }

        public string Identifier { get; set; }

        /// <summary>
        /// readonly profiles can receive messages, but can't send,
        /// can be changed from the admin panel
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// switch (IsActivated, IsPassive)
        ///     case (true, false): registered profile
        ///      case (true, true): registered profile with option NOT to create a personal profile (can't send/receive messages)
        ///      case (false, false): profile is registered through registration request and waiting for confirmation/rejection
        ///      case (false, true): passive registration of profile by 3rd party
        /// </summary>
        public bool IsPassive { get; set; }

        public bool? EnableMessagesWithCode { get; set; }

        public DateTime? ModifyDate { get; set; }

        public int? ModifiedBy { get; set; }

        public int? ModifiedByAdminUserId { get; set; }

        public DateTime? ActivatedDate { get; set; }

        public int? ActivatedByAdminUserId { get; set; }

        public int? AddressId { get; set; }

        public int? CreatedByAdminUserId { get; set; }

        private List<ProfileKey> keys = new();

        public IReadOnlyCollection<ProfileKey> Keys =>
            this.keys.AsReadOnly();

        public Individual? Individual { get; set; }

        public LegalEntity? LegalEntity { get; set; }

        private List<LoginProfile> logins = new();

        public IReadOnlyCollection<LoginProfile> Logins =>
            this.logins.AsReadOnly();

        public Address? Address { get; set; }

        private List<LoginProfilePermission> loginPermissions = new();

        public IReadOnlyCollection<LoginProfilePermission> LoginPermissions =>
            this.loginPermissions.AsReadOnly();

        public ProfileQuota? Quota { get; set; }

        public bool HideAsRecipient { get; set; }

        public bool LicenceAgreed { get; set; }

        public bool GDPRAgreed {  get; set; }

        public void UpdateIndividual(
            string firstName,
            string middleName,
            string lastName,
            string phone,
            string emailAddress,
            bool isPassive,
            string residence,
            bool licenceAgreed,
            bool gdprAgreed,
            int modifiedBy)
        {
            DateTime now = DateTime.Now;

            this.ModifyDate = now;
            this.ModifiedBy = modifiedBy;
            this.ElectronicSubjectName = $"{firstName} {middleName} {lastName}";
            this.EmailAddress = emailAddress;
            this.Phone = phone;
            this.IsActivated = true;
            this.ActivatedDate = now; // todo: something fishy here
            this.IsPassive = isPassive;
            this.LicenceAgreed = licenceAgreed;
            this.GDPRAgreed = gdprAgreed;

            this.Individual!.Update(firstName, middleName, lastName);

            if (this.Address != null)
            {
                this.Address.Update(residence);
            }
            else
            {
                this.Address = new(residence);
            }
        }

        public void UpdateIndividual(
            string firstName,
            string middleName,
            string lastName,
            string phone,
            string emailAddress,
            bool isPassive,
            string residence,
            string? city,
            string? state,
            string? countryIso,
            int modifiedBy)
        {
            DateTime now = DateTime.Now;

            this.ModifyDate = now;
            this.ModifiedBy = modifiedBy;
            this.ElectronicSubjectName = $"{firstName} {middleName} {lastName}";
            this.EmailAddress = emailAddress;
            this.Phone = phone;
            this.IsActivated = true;
            this.ActivatedDate = now;
            this.IsPassive = isPassive;

            this.Individual!.Update(firstName, middleName, lastName);

            if (this.Address != null)
            {
                this.Address.Update(residence, city, state, countryIso);
            }
            else
            {
                this.Address = new(residence, city, state, countryIso);
            }
        }

        public void GrantLoginAccess(
            int loginId,
            bool isDefault,
            bool isEmailNotificationEnabled,
            bool isEmailNotificationOnDeliveryEnabled,
            bool isPhoneNotificationEnabled,
            bool isPhoneNotificationOnDeliveryEnabled,
            string email,
            string phone,
            int actionLoginId,
            (LoginProfilePermissionType permission, int? templateId)[] permissions)
        {
            this.AssertActive();

            this.logins.Add(
                new LoginProfile(
                    loginId,
                    isDefault,
                    isEmailNotificationEnabled,
                    isEmailNotificationOnDeliveryEnabled,
                    isPhoneNotificationEnabled,
                    isPhoneNotificationOnDeliveryEnabled,
                    email,
                    phone,
                    actionLoginId));

            this.loginPermissions.AddRange(
                permissions
                    .Select(e => new LoginProfilePermission(
                        loginId,
                        e.permission,
                        e.templateId)));
        }

        public void GrantLoginAccessByAdmin(
            int loginId,
            bool isDefault,
            bool isEmailNotificationEnabled,
            bool isEmailNotificationOnDeliveryEnabled,
            bool isPhoneNotificationEnabled,
            bool isPhoneNotificationOnDeliveryEnabled,
            string email,
            string phone,
            int adminUserId,
            (LoginProfilePermissionType permission, int? templateId)[] permissions)
        {
            this.AssertActive();

            this.logins.Add(
                new LoginProfile(
                    loginId,
                    isDefault,
                    isEmailNotificationEnabled,
                    isEmailNotificationOnDeliveryEnabled,
                    isPhoneNotificationEnabled,
                    isPhoneNotificationOnDeliveryEnabled,
                    email,
                    phone,
                    Login.SystemLoginId)
                {
                    AccessGrantedByAdminUserId = adminUserId
                });

            this.loginPermissions.AddRange(
                permissions
                    .Select(e => new LoginProfilePermission(
                    loginId,
                    e.permission,
                    e.templateId)));
        }

        public void RevokeAccess(
            int loginId)
        {
            this.AssertActive();

            LoginProfile login = this.Logins.Single(e => e.LoginId == loginId);

            if (login.IsDefault)
            {
                throw new ArgumentException("Can not remove access to your own profile.");
            }

            this.logins.Remove(login);

            this.loginPermissions.RemoveAll(e => e.LoginId == loginId);
        }

        public void UpdateSettings(
            int loginId,
            bool emailNotificationActive,
            bool emailNotificationOnDeliveryActive,
            bool phoneNotificationActive,
            bool phoneNotificationOnDeliveryActive,
            string email,
            string phone)
        {
            this.AssertActive();

            LoginProfile login = this.Logins.First(e => e.LoginId == loginId);

            login.Update(
                emailNotificationActive,
                emailNotificationOnDeliveryActive,
                phoneNotificationActive,
                phoneNotificationOnDeliveryActive,
                email,
                phone);
        }

        public void Update(
            string email,
            string phone,
            string residence,
            bool sync,
            int actionLoginId,
            StringBuilder updatesLog)
        {
            this.AssertActive();

            this.UpdateInternal(
                this.Identifier,
                email,
                phone,
                this.Address?.Country,
                this.Address?.State,
                this.Address?.City,
                residence,
                actionLoginId,
                updatesLog);

            if (sync)
            {
                LoginProfile? login = this.Logins.SingleOrDefault(e => e.IsDefault);
                login?.Sync(email, phone);
            }
        }

        private void UpdateInternal(
            string identifier,
            string emailAddress,
            string phone,
            string? addressCountryCode,
            string? addressState,
            string? addressCity,
            string? addressResidence,
            int actionLoginId,
            StringBuilder updatesLog)
        {
            if (this.Identifier != identifier)
            {
                updatesLog.Append($"{this.Identifier}-{identifier};");
                this.Identifier = identifier;
            }

            if (this.EmailAddress != emailAddress)
            {
                updatesLog.Append($"{this.EmailAddress}-{emailAddress};");
                this.EmailAddress = emailAddress;
            }

            if (this.Phone != phone)
            {
                updatesLog.Append($"{this.Phone}-{phone};");
                this.Phone = phone;
            }

            if (this.Address == null)
            {
                if (addressResidence != null)
                {
                    this.Address = new Address(
                        addressResidence,
                        addressCity,
                        addressState,
                        addressCountryCode);

                    updatesLog.Append($"_-{addressResidence};");

                    if (addressCity != null)
                    {
                        updatesLog.Append($"_-{addressCity};");
                    }

                    if (addressState != null)
                    {
                        updatesLog.Append($"_-{addressState};");
                    }

                    if (addressCountryCode != null)
                    {
                        updatesLog.Append($"_-{addressCountryCode};");
                    }
                }
            }
            else
            {
                if (this.Address.Country != addressCountryCode)
                {
                    updatesLog.Append($"{this.Address.Country}-{addressCountryCode};");
                    this.Address.Country = addressCountryCode;
                }

                if (this.Address.State != addressState)
                {
                    updatesLog.Append($"{this.Address.State}-{addressState};");
                    this.Address.State = addressState;
                }

                if (this.Address.City != addressCity)
                {
                    updatesLog.Append($"{this.Address.City}-{addressCity};");
                    this.Address.City = addressCity;
                }

                if (this.Address.Residence != addressResidence)
                {
                    updatesLog.Append($"{this.Address.Residence}-{addressResidence};");
                    this.Address.Residence = addressResidence;
                }
            }

            this.ModifyDate = DateTime.Now;
            this.ModifiedBy = actionLoginId;
        }

        public void UpdateIndividualName(
            string firstName,
            string middleName,
            string lastName,
            int actionLoginId,
            StringBuilder updatesLog)
        {
            this.AssertActive();

            if (this.Individual == null)
            {
                throw new Exception("Individual should not be null");
            }

            this.Individual.Update(
                firstName,
                middleName,
                lastName);

            string profileName = $"{firstName} {middleName} {lastName}";

            if (this.ElectronicSubjectName != profileName)
            {
                updatesLog.Append($"{this.ElectronicSubjectName}-{profileName};");
                this.ElectronicSubjectName = profileName;
            }

            this.ModifyDate = DateTime.Now;
            this.ModifiedBy = actionLoginId;
        }

        // private as this is not an operation of the aggregate
        private void UpdateLegalEntityName(
            string name,
            int actionLoginId,
            StringBuilder updatesLog)
        {
            if (this.LegalEntity == null)
            {
                throw new Exception("LegalEntity should not be null");
            }

            this.LegalEntity.Update(name);

            if (this.ElectronicSubjectName != name)
            {
                updatesLog.Append($"{this.ElectronicSubjectName}-{name};");
                this.ElectronicSubjectName = name;
            }

            this.ModifyDate = DateTime.Now;
            this.ModifiedBy = actionLoginId;
        }

        public void UpdateIndividualNameByAdmin(
            string firstName,
            string middleName,
            string lastName,
            int adminUserId,
            StringBuilder updatesLog)
        {
            this.ModifiedByAdminUserId = adminUserId;

            if (this.Individual == null)
            {
                throw new Exception("Individual should not be null");
            }

            this.Individual.Update(
                firstName,
                middleName,
                lastName);

            string profileName = $"{firstName} {middleName} {lastName}";

            if (this.ElectronicSubjectName != profileName)
            {
                updatesLog.Append($"{this.ElectronicSubjectName}-{profileName};");
                this.ElectronicSubjectName = profileName;
            }

            this.ModifyDate = DateTime.Now;
            this.ModifiedBy = Login.SystemLoginId;
        }

        public void UpdateLegalEntityNameByAdmin(
            string name,
            int adminUserId,
            StringBuilder updatesLog)
        {
            this.ModifiedByAdminUserId = adminUserId;
            this.UpdateLegalEntityName(
                name,
                Login.SystemLoginId,
                updatesLog);
        }

        public void UpdateDataByAdmin(
            string identifier,
            string email,
            string phone,
            string? addressCountryCode,
            string? addressState,
            string? addressCity,
            string? addressResidence,
            bool? enableMessagesWithCode,
            bool hideAsRecipient,
            int adminUserId,
            StringBuilder updatesLog)
        {
            this.ModifiedByAdminUserId = adminUserId;

            this.EnableMessagesWithCode = enableMessagesWithCode;
            this.HideAsRecipient = hideAsRecipient;

            this.UpdateInternal(
                identifier,
                email,
                phone,
                addressCountryCode,
                addressState,
                addressCity,
                addressResidence,
                Login.SystemLoginId,
                updatesLog);
        }

        public ProfileKey AddProfileKey(
            string provider,
            string keyName,
            string oaepPadding,
            DateTime issuedAt,
            DateTime expiresAt)
        {
            this.AssertActive();

            ProfileKey newKey = new(
                provider,
                keyName,
                oaepPadding,
                issuedAt,
                expiresAt);

            this.keys.Add(newKey);

            return newKey;
        }

        public void UpdateLoginAccess(
            int loginId,
            (LoginProfilePermissionType permission, int? templateId)[] permissions)
        {
            this.AssertActive();

            this.loginPermissions.RemoveAll(e => e.LoginId == loginId);

            this.loginPermissions.AddRange(
                permissions
                    .Select(e => new LoginProfilePermission(
                        loginId,
                        e.permission,
                        e.templateId)));
        }

        public void ConfirmRegistration(
            int adminUserId)
        {
            this.AssertInactive();

            this.ActivatedDate = DateTime.Now;
            this.ActivatedByAdminUserId = adminUserId;
            this.IsPassive = false;
            this.IsActivated = true;
        }

        public void Activate(int adminUserId)
        {
            this.AssertInactive();

            this.IsActivated = true;
            this.ActivatedByAdminUserId = adminUserId;
            this.ActivatedDate = DateTime.Now;
        }

        public void Deactivate(int adminUserId)
        {
            this.AssertActive();

            this.IsActivated = false;
            this.DeletedByAdminUserId = adminUserId;
            this.DateDeleted = DateTime.Now;
        }

        public void MarkAsNonReadonly(int adminUserId)
        {
            this.IsReadOnly = false;
            this.ModifyDate = DateTime.Now;
            this.ModifiedByAdminUserId = adminUserId;
        }

        public void MarkAsReadOnly(int adminUserId)
        {
            this.IsReadOnly = true;
            this.ModifyDate = DateTime.Now;
            this.ModifiedByAdminUserId = adminUserId;
        }

        public void BringInForce()
        {
            this.AssertActive();

            this.IsPassive = false;
        }

        public void UpdateSettingsByAdmin(
            int loginId,
            bool emailNotificationActive,
            bool emailNotificationOnDeliveryActive,
            bool phoneNotificationActive,
            bool phoneNotificationOnDeliveryActive,
            string email,
            string phone,
            int adminUserId)
        {
            LoginProfile login = this.Logins.First(e => e.LoginId == loginId);

            login.UpdateByAdmin(
                emailNotificationActive,
                emailNotificationOnDeliveryActive,
                phoneNotificationActive,
                phoneNotificationOnDeliveryActive,
                email,
                phone,
                adminUserId);
        }

        public void CreateOrUpdateQuotas(
            int? storageQuotaInMb,
            int adminUserId)
        {
            if (this.Quota == null)
            {
                this.Quota = new ProfileQuota(storageQuotaInMb, adminUserId);
            }
            else
            {
                this.Quota.Update(storageQuotaInMb, adminUserId);
            }
        }

        private void AssertActive()
        {
            if (!this.IsActivated)
            {
                throw new DomainValidationException($"The operation cannot be performed on profiles with {nameof(this.IsActivated)}=false");
            }
        }

        private void AssertInactive()
        {
            if (this.IsActivated)
            {
                throw new DomainValidationException($"The operation cannot be performed on profiles with {nameof(this.IsActivated)}=true");
            }
        }
    }

    class ProfileMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "Profiles";

            var builder = modelBuilder.Entity<Profile>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.ElectronicSubjectId).ValueGeneratedOnAdd();

            builder.HasMany(e => e.Keys)
                .WithOne()
                .HasForeignKey(e => e.ProfileId);

            builder.HasOne(e => e.Individual)
                .WithOne()
                .HasPrincipalKey<Profile>(e => e.ElectronicSubjectId)
                .HasForeignKey<Individual>(e => e.IndividualId)
                .IsRequired(false);

            builder.HasOne(e => e.LegalEntity)
                .WithOne()
                .HasPrincipalKey<Profile>(e => e.ElectronicSubjectId)
                .HasForeignKey<LegalEntity>(e => e.LegalEntityId)
                .IsRequired(false);

            builder.HasMany(e => e.Logins)
                .WithOne()
                .HasForeignKey(e => e.ProfileId);

            builder.HasOne(e => e.Address)
                .WithMany()
                .HasForeignKey(e => e.AddressId)
                .IsRequired(false);

            builder.HasMany(e => e.LoginPermissions)
                .WithOne()
                .HasForeignKey(e => e.ProfileId);

            builder.HasOne(e => e.Quota)
                .WithOne()
                .HasPrincipalKey<Profile>(e => e.Id)
                .HasForeignKey<ProfileQuota>(e => e.ProfileId)
                .IsRequired(false);

            // add relations for entities that do not reference each other
            builder.HasOne(typeof(Login))
                .WithMany()
                .HasForeignKey(nameof(Profile.CreatedBy))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
