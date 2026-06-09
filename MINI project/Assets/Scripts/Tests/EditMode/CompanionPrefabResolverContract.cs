internal static class CompanionPrefabResolverContract
{
    public static string DealerResourcePath()
    {
        return CompanionPrefabResolver.ToResourcePath("Dealer/CPN_Dealer_T01_Kael");
    }

    public static string AlreadyRootedResourcePath()
    {
        return CompanionPrefabResolver.ToResourcePath(
            "Companions/Dealer/CPN_Dealer_T01_Kael.prefab"
        );
    }
}
