namespace TheDocManager.AnalyzerAttributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class AvoidUseIfPossibleAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All)]
    public class SoonToBeDeprecatedAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All)]
    public class DoNotUseWithoutAdminApprovalAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All)]
    public class UnsafeCodeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All)]
    public class VulnerabilitiesIdentifiedAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All)]
    public class ProblematicAttribute : Attribute { }
}
