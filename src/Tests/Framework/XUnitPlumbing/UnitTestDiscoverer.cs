using System;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Tests.Framework
{
	public class UnitTestDiscoverer : NestTestDiscoverer
	{
		private bool RunningOnTeamCity { get; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));

		public UnitTestDiscoverer(IMessageSink diagnosticMessageSink)
			: base(diagnosticMessageSink, TestClient.Configuration.RunUnitTests)
		{
		}

		protected override bool SkipMethod(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
		{
			var classOfMethod = Type.GetType(testMethod.TestClass.Class.Name, true, true);
			var method = classOfMethod.GetMethod(testMethod.Method.Name, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public)
			             ?? testMethod.Method.ToRuntimeMethod();

			return !TestClient.Configuration.RunUnitTests
			       || ClassShouldSkipWhenPackageReference(classOfMethod)
			       || ClassIsIntegrationOnly(classOfMethod)
			       || SkipWhenRunOnTeamCity(classOfMethod, method)
			       || SkipWhenNeedingTypedKeys(classOfMethod);
		}


		private static bool ClassShouldSkipWhenPackageReference(Type classOfMethod)
		{
#if TESTINGNUGETPACKAGE
			var attributes = classOfMethod.GetAttributes<ProjectReferenceOnlyAttribute>();
			return (attributes.Any());
#else
			return false;
#endif
		}

		private static bool ClassIsIntegrationOnly(Type classOfMethod)
		{
			var attributes = classOfMethod.GetAttributes<IntegrationOnlyAttribute>();
			return (attributes.Any());
		}

		private bool SkipWhenRunOnTeamCity(Type classOfMethod, MethodInfo info)
		{
			if (!this.RunningOnTeamCity) return false;

			var attributes = classOfMethod.GetAttributes<SkipOnTeamCityAttribute>().Concat(info.GetAttributes<SkipOnTeamCityAttribute>());
			return attributes.Any();
		}
	}
}
