namespace ED.Domain
{
    public enum ProfileHistoryAction
    {
        CreateProfile, // TODO: not really used, instead of this ProfileActivated is used
        AccessProfile,
        ProfileActivated,
        ProfileDeactivated,
        GrantAccessToProfile,
        RemoveAccessToProfile,
        ProfileUpdated,
        BringProfileInForce,
        MarkAsReadonly,
        MarkAsNonReadonly
    }
}
