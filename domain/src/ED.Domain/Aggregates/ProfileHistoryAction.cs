namespace ED.Domain
{
    public enum ProfileHistoryAction
    {
        CreateProfile,
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
